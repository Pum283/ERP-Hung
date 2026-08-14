using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Báo cáo tiết kiệm chi phí từ đàm phán RFQ (UC_PUR_050).</summary>
public class PurRfqSavingsReport : TenantEntity
{
    public Guid RfqId { get; set; }
    public string RfqNumber { get; set; } = "";
    public string Title { get; set; } = "";
    public decimal InitialBudgetVnd { get; set; }
    public decimal AwardedAmountVnd { get; set; }
    public decimal SavingsAmountVnd { get; set; }
    public double SavingsPercentage { get; set; }
    public DateTimeOffset CalculatedAt { get; set; } = DateTimeOffset.UtcNow;
}
