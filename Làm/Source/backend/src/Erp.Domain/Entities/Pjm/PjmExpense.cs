using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Chi phí phát sinh dự án (UC_PJM_022).</summary>
public class PjmExpense : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string Code { get; set; } = "";
    public string Category { get; set; } = "Other";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTimeOffset ExpenseDate { get; set; } = DateTimeOffset.UtcNow;
    public Guid? WbsItemId { get; set; }
    /// <summary>Draft | Posted</summary>
    public string Status { get; set; } = "Draft";
    public DateTimeOffset? PostedAt { get; set; }
    public string? Note { get; set; }
}
