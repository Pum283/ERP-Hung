using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Chuyển chi phí dịch vụ kỹ thuật sang sổ cái Tài chính FIN (UC_FSM_032).</summary>
public class FsmFinanceCostTransfer : TenantEntity
{
    public string TransferVoucherNumber { get; set; } = "";
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = "";
    public decimal TransferredAmountVnd { get; set; } = 1000000;
    public string DebitAccount { get; set; } = "627"; // Chi phí sản xuất chung / DV ngoài
    public string CreditAccount { get; set; } = "154"; // Chi phí dở dang
    public string JournalEntryStatus { get; set; } = "Posted"; // Posted | Draft | Error
    public DateTimeOffset TransferredAt { get; set; } = DateTimeOffset.UtcNow;
}
