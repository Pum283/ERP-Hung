using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Giao dịch nộp tiền / rút tiền ca bán hàng (UC_POS_044).</summary>
public class PosShiftCashTransaction : TenantEntity
{
    public Guid ShiftId { get; set; }
    /// <summary>CashIn | CashOut</summary>
    public string TransactionType { get; set; } = "CashIn";
    public decimal AmountVnd { get; set; }
    public string Reason { get; set; } = "";
    public Guid PerformedByUserId { get; set; }
    public DateTimeOffset TransactionTime { get; set; } = DateTimeOffset.UtcNow;
}
