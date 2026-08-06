using Erp.Domain.Base;

namespace Erp.Domain.Entities.Ast;

/// <summary>Thẻ tài sản cố định (UC_AST_002–004, 014).</summary>
public class AstAsset : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? GroupId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? DepreciationMethodId { get; set; }
    public decimal OriginalCost { get; set; }
    public DateTimeOffset? CapitalizeDate { get; set; }
    public int UsefulLifeMonths { get; set; } = 36;
    public decimal DepreciationRatePercent { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    /// <summary>Draft · Active · Disposed</summary>
    public string Status { get; set; } = "Draft";
    public Guid? AssignedEmployeeId { get; set; }
    public string? AssignedEmployeeName { get; set; }
    public DateTimeOffset? DisposedAt { get; set; }
    public decimal? DisposalAmount { get; set; }
    public string? PurchaseRef { get; set; }
    public string? Note { get; set; }
}
