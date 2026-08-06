using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Định biên nhân sự (UC_HRM_075–080).</summary>
public class HeadcountPlan : TenantEntity
{
    /// <summary>OrgUnit | Department | Shift</summary>
    public string ScopeType { get; set; } = "OrgUnit";
    public Guid OrgUnitId { get; set; }
    public Guid? DepartmentId { get; set; }
    /// <summary>Mã ca (Morning/Afternoon/Night/…) khi ScopeType=Shift.</summary>
    public string? ShiftCode { get; set; }
    public int PlannedHeadcount { get; set; }
    /// <summary>Draft | Pending | Approved | Rejected | Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string? Note { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid? DecidedByUserId { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
}
