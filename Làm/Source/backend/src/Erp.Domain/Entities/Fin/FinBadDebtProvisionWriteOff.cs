using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Trích lập dự phòng nợ phải thu khó đòi và xóa sổ nợ xấu (UC_FIN_037).</summary>
public class FinBadDebtProvisionWriteOff : TenantEntity
{
    public string DebtRecordNumber { get; set; } = "NX-2026-0814";
    public string CustomerName { get; set; } = "Công Ty Cơ Khí Hoàng Gia (Đã giải thể)";
    public decimal OriginalDebtAmountVnd { get; set; } = 30000000;
    public decimal ProvisionAmountVnd { get; set; } = 30000000;
    public double ProvisionRatePct { get; set; } = 100.0;
    public string ActionType { get; set; } = "WriteOff"; // Provision | WriteOff | Recovery
    public string CouncilApprovalDoc { get; set; } = "Nghị quyết HĐQT số 18/2026/NQ-HDQT duyệt xóa nợ xấu";
    public DateTimeOffset ActionDate { get; set; } = DateTimeOffset.UtcNow;
}
