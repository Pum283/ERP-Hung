using Erp.Domain.Base;

namespace Erp.Domain.Entities.Ast;

/// <summary>Kỳ tính khấu hao (UC_AST_010–012).</summary>
public class AstDepreciationRun : TenantEntity
{
    public string Code { get; set; } = "";
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTimeOffset PeriodStart { get; set; }
    public DateTimeOffset PeriodEnd { get; set; }
    /// <summary>Draft · Posted · Pushed</summary>
    public string Status { get; set; } = "Draft";
    public decimal TotalAmount { get; set; }
    public int LineCount { get; set; }
    public Guid? FinJournalId { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
}
