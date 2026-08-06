using Erp.Application.Common;

namespace Erp.Api.Middlewares;

/// <summary>Gắn X-Correlation-Id vào mọi request (INT-04 / G4.5).</summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _log;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var raw = context.Request.Headers[HeaderName].FirstOrDefault();
        var id = Guid.TryParse(raw, out var parsed) ? parsed : Guid.NewGuid();
        CorrelationContext.Current = id;
        context.Items[HeaderName] = id;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = id.ToString("D");
            return Task.CompletedTask;
        });

        using (_log.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id.ToString("D") }))
        {
            try
            {
                await _next(context);
            }
            finally
            {
                CorrelationContext.Current = null;
            }
        }
    }
}
