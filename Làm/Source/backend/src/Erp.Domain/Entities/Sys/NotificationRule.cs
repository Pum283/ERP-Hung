using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class NotificationRule : TenantEntity
{
    public string EventType { get; set; } = "";
    public string TitleTemplate { get; set; } = "";
    public string BodyTemplate { get; set; } = "";
    public bool IsEnabled { get; set; } = true;
}
