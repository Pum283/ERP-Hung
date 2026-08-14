using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Nhật ký đồng bộ dữ liệu làm việc offline (UC_FSM_043).</summary>
public class FsmOfflineSyncAuditLog : TenantEntity
{
    public Guid TechnicianUserId { get; set; }
    public string TechnicianName { get; set; } = "";
    public string DeviceIdentifier { get; set; } = "SM-A536B-ANDROID";
    public int SyncedOperationsCount { get; set; } = 8;
    public string SyncStatus { get; set; } = "Success"; // Success | ConflictResolved | Failed
    public DateTimeOffset OfflineSessionStartedAt { get; set; } = DateTimeOffset.UtcNow.AddHours(-4);
    public DateTimeOffset SyncedAt { get; set; } = DateTimeOffset.UtcNow;
}
