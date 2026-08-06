using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Khóa sổ lịch ca theo kỳ (UC_HRM_090).</summary>
public class ShiftPeriodLock : TenantEntity
{
    public Guid OrgUnitId { get; set; }
    /// <summary>yyyy-MM</summary>
    public string PeriodKey { get; set; } = string.Empty;
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public Guid LockedByUserId { get; set; }
    public DateTimeOffset LockedAt { get; set; }
    public string? Note { get; set; }
}