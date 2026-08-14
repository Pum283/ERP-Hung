using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Hàng chậm luân chuyển (UC_INV_066).</summary>
public class InvSlowMovingAnalysis : TenantEntity
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal CurrentStockQuantity { get; set; }
    public int DaysWithoutIssueMovement { get; set; }
    public decimal TiedUpCapitalVnd { get; set; }
    public string RiskLevel { get; set; } = "HighRisk"; // HighRisk (>180 days) | MediumRisk (90-180 days) | LowRisk (<90 days)
    public DateTimeOffset AnalyzedAt { get; set; } = DateTimeOffset.UtcNow;
}
