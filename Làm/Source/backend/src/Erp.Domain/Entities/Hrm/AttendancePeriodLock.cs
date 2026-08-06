using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Khóa bảng công theo kỳ (UC_HRM_126–127).</summary>
public class AttendancePeriodLock : TenantEntity
{
    public string PeriodKey { get; set; } = string.Empty;
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public bool IsLocked { get; set; } = true;
    public Guid LockedByUserId { get; set; }
    public DateTimeOffset LockedAt { get; set; }
    public Guid? UnlockedByUserId { get; set; }
    public DateTimeOffset? UnlockedAt { get; set; }
    public string? Note { get; set; }
}
