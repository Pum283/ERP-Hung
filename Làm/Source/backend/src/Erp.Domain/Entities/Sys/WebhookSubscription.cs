using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class WebhookSubscription : TenantEntity
{
    public string Name { get; set; } = "";
    public string TargetUrl { get; set; } = "";
    public string EventTypes { get; set; } = "*";
    public string? Secret { get; set; }
    public bool IsActive { get; set; } = true;
}
