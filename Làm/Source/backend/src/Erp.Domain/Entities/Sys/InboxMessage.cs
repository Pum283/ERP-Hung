using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

/// <summary>Inbox chống xử lý trùng theo eventId + consumer.</summary>
public class InboxMessage : TenantEntity
{
    public Guid EventId { get; set; }
    public string Consumer { get; set; } = "";
    public string EventType { get; set; } = "";
    /// <summary>Processed | Failed</summary>
    public string Status { get; set; } = "Processed";
    public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? ResultNote { get; set; }
}
