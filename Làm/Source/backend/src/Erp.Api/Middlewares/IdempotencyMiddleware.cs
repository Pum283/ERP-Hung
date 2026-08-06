using System.Security.Claims;
using System.Text;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Api.Middlewares;

/// <summary>
/// Replay response khi client gửi header <c>Idempotency-Key</c> trên API mutating M1
/// (leave-requests, WF act). Không có header → bỏ qua.
/// </summary>
public sealed class IdempotencyMiddleware
{
    public const string HeaderName = "Idempotency-Key";
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (!ShouldHandle(context))
        {
            await _next(context);
            return;
        }

        var key = context.Request.Headers[HeaderName].ToString().Trim();
        if (key.Length is 0 or > 120)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Idempotency-Key không hợp lệ (1–120 ký tự)."
            });
            return;
        }

        var tenantClaim = context.User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantClaim, out var tenantId))
        {
            await _next(context);
            return;
        }

        var existing = await db.IdempotencyRecords.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == key && !x.IsDeleted, context.RequestAborted);

        if (existing is not null)
        {
            context.Response.StatusCode = existing.ResponseStatus;
            context.Response.ContentType = "application/json";
            context.Response.Headers["X-Idempotency-Replayed"] = "true";
            await context.Response.WriteAsync(existing.ResponseBody ?? "{}", Encoding.UTF8);
            return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);

            buffer.Position = 0;
            var bodyText = await new StreamReader(buffer, Encoding.UTF8, leaveOpen: true).ReadToEndAsync();
            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);

            if (context.Response.StatusCode is >= 200 and < 300)
            {
                var userClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? context.User.FindFirstValue("sub");
                _ = Guid.TryParse(userClaim, out var userId);

                db.IdempotencyRecords.Add(new IdempotencyRecord
                {
                    TenantId = tenantId,
                    Key = key,
                    RequestPath = context.Request.Path.Value ?? "",
                    ResponseStatus = context.Response.StatusCode,
                    ResponseBody = bodyText.Length > 8000 ? bodyText[..8000] : bodyText,
                    CreatedBy = userId == Guid.Empty ? null : userId
                });

                try
                {
                    await db.SaveChangesAsync(context.RequestAborted);
                }
                catch (DbUpdateException)
                {
                    // race: key vừa được insert bởi request song song — bỏ qua
                }
            }
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private static bool ShouldHandle(HttpContext context)
    {
        if (context.Request.Method is not ("POST" or "PUT" or "PATCH"))
            return false;
        if (!context.Request.Headers.ContainsKey(HeaderName))
            return false;

        var path = context.Request.Path.Value ?? "";
        return path.StartsWith("/api/hrm/leave-requests", StringComparison.OrdinalIgnoreCase)
               || (path.StartsWith("/api/wf/tasks/", StringComparison.OrdinalIgnoreCase)
                   && path.EndsWith("/act", StringComparison.OrdinalIgnoreCase));
    }
}
