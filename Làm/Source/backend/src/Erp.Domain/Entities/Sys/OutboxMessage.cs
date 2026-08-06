using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

/// <summary>Outbox sự kiện liên module (INT-05) — ghi cùng txn nghiệp vụ.</summary>
public class OutboxMessage : TenantEntity
{
    public string EventType { get; set; } = "";
    public string SourceModule { get; set; } = "";
    public Guid? CorrelationId { get; set; }
    public string PayloadJson { get; set; } = "{}";
    /// <summary>Pending | Published | Failed | Blocked | Dead</summary>
    public string Status { get; set; } = "Pending";
    public int AttemptCount { get; set; }
    public DateTimeOffset? NextAttemptAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string? LastError { get; set; }
}
