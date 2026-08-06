using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Hạng mục WBS dự án (UC_PJM_011–012).</summary>
public class PjmWbsItem : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? ParentItemId { get; set; }
    public Guid? AssigneeUserId { get; set; }
    public string? AssigneeName { get; set; }
    /// <summary>Open · InProgress · Done · Cancelled</summary>
    public string Status { get; set; } = "Open";
    public int SortOrder { get; set; }
    public string? Note { get; set; }

    /// <summary>UC_PJM_013 — 0–100.</summary>
    public decimal PercentComplete { get; set; }
    /// <summary>UC_PJM_014.</summary>
    public bool IsMilestone { get; set; }
    /// <summary>UC_PJM_014 / 017 — deadline hạng mục.</summary>
    public DateTimeOffset? DueDate { get; set; }
}
