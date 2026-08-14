using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Nộp quyết toán tiền mặt & chi phí kỹ thuật trong ngày (UC_FSM_044).</summary>
public class FsmDailyExpenseSettlement : TenantEntity
{
    public string SettlementVoucherNumber { get; set; } = "";
    public Guid TechnicianUserId { get; set; }
    public string TechnicianName { get; set; } = "";
    public decimal TotalCashCollectedVnd { get; set; }
    public decimal TotalOutboundExpenseVnd { get; set; }
    public decimal NetSettlementAmountVnd { get; set; }
    public string Status { get; set; } = "Submitted"; // Submitted | Approved | Audited
    public DateTimeOffset SettlementDate { get; set; } = DateTimeOffset.UtcNow;
}
