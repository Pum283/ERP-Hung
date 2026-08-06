using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Bảng giá thành lệnh SX (UC_MFG_027, 029, 031).</summary>
public class MfgCostSheet : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid WorkOrderId { get; set; }
    /// <summary>Draft · Calculated · Pushed · Void</summary>
    public string Status { get; set; } = "Draft";
    public decimal MaterialCost { get; set; }
    public decimal LaborCost { get; set; }
    public decimal OverheadCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal GoodQty { get; set; }
    public decimal UnitCost { get; set; }
    public Guid? InvSkuId { get; set; }
    public string? InvSkuCode { get; set; }
    public Guid? FinJournalId { get; set; }
    public string? FinJournalCode { get; set; }
    public DateTimeOffset? CalculatedAt { get; set; }
    public DateTimeOffset? PushedAt { get; set; }
    public Guid CalculatedByUserId { get; set; }
    public string? Note { get; set; }
}

public class MfgCostSheetLine : TenantEntity
{
    public Guid CostSheetId { get; set; }
    public Guid? MaterialIssueId { get; set; }
    public Guid ItemId { get; set; }
    /// <summary>Material · Labor · Overhead</summary>
    public string Source { get; set; } = "Material";
    public decimal Qty { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}
