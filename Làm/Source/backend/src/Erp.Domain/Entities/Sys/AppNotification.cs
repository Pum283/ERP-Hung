using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class AppNotification : TenantEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string? Link { get; set; }
    public string? EventType { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
