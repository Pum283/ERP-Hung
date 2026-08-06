using System.Security.Claims;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Api.Middlewares;

/// <summary>
/// Chặn API theo module license: path /api/{module}/... cần license_module enabled.
/// Auth & sys luôn cho qua (trừ khi muốn chặn sys.license riêng).
/// </summary>
public sealed class LicenseModuleMiddleware
{
    private static readonly HashSet<string> AlwaysAllowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "auth", "sys", "health", "swagger", "favicon.ico"
    };

    private readonly RequestDelegate _next;

    public LicenseModuleMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var path = context.Request.Path.Value ?? "";
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        // api / {module} / ...
        if (segments.Length < 2)
        {
            await _next(context);
            return;
        }

        var module = segments[1];
        if (AlwaysAllowed.Contains(module))
        {
            await _next(context);
            return;
        }

        var tenantClaim = context.User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantClaim, out var tenantId))
        {
            // chưa login — để [Authorize] xử lý
            await _next(context);
            return;
        }

        var enabled = await db.LicenseModules.AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId
                           && x.ModuleCode == module.ToUpperInvariant()
                           && x.IsEnabled
                           && !x.IsDeleted);

        // cũng chấp nhận đúng casing seed (SYS/HRM…)
        if (!enabled)
        {
            enabled = await (
                from l in db.Licenses.AsNoTracking()
                join lm in db.LicenseModules.AsNoTracking() on l.Id equals lm.LicenseId
                where l.TenantId == tenantId && l.Status == "Active"
                      && lm.IsEnabled && !lm.IsDeleted
                      && lm.ModuleCode.ToLower() == module.ToLower()
                select lm.Id
            ).AnyAsync();
        }

        if (!enabled)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = $"Module `{module.ToUpperInvariant()}` chưa được license hoặc đã tắt."
            });
            return;
        }

        await _next(context);
    }
}
