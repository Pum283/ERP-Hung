using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Kỳ lương (UC_HRM_163+).</summary>
public class PayrollPeriod : TenantEntity
{
    /// <summary>yyyy-MM</summary>
    public string PeriodKey { get; set; } = string.Empty;
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    /// <summary>Draft | Calculated | Confirmed | Locked</summary>
    public string Status { get; set; } = "Draft";
    public string? Note { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public Guid? ConfirmedByUserId { get; set; }
    public DateTimeOffset? LockedAt { get; set; }
    public Guid? LockedByUserId { get; set; }
}
