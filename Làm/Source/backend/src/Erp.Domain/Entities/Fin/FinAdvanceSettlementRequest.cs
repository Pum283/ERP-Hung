using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Đề nghị tạm ứng / hoàn ứng chi phí công tác, mua sắm (UC_FIN_021).</summary>
public class FinAdvanceSettlementRequest : TenantEntity
{
    public string RequestNumber { get; set; } = "TU-2026-0814";
    public string EmployeeName { get; set; } = "Kỹ Sư Trưởng Nguyễn Văn An";
    public string Purpose { get; set; } = "Tạm ứng tiền vé máy bay và lưu trú công tác hiện trường dự án Solar FPT";
    public decimal AdvanceAmountVnd { get; set; } = 15000000;
    public decimal SettledAmountVnd { get; set; } = 14200000;
    public decimal RemainingRefundVnd { get; set; } = 800000;
    public string Status { get; set; } = "Settled"; // Requested | Advanced | Settled | Refunded
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
