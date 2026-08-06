using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class IntegrationCallLog : TenantEntity
{
    public string Kind { get; set; } = "Webhook";
    public string Target { get; set; } = "";
    public int StatusCode { get; set; }
    public string? RequestSummary { get; set; }
    public string? ResponseSummary { get; set; }
    public DateTimeOffset CalledAt { get; set; } = DateTimeOffset.UtcNow;
}
