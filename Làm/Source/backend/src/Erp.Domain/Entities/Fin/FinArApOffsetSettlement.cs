using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Biên bản đối trừ bù trừ công nợ giữa Phải Thu (AR) và Phải Trả (AP) cùng đối tác (UC_FIN_033).</summary>
public class FinArApOffsetSettlement : TenantEntity
{
    public string SettlementNumber { get; set; } = "BT-2026-0814";
    public string PartnerName { get; set; } = "Công Ty TNHH Thiết Bị Điện Miền Nam (Vừa là NCC vừa là Khách hàng)";
    public decimal ArAmountToOffsetVnd { get; set; } = 65000000;
    public decimal ApAmountToOffsetVnd { get; set; } = 65000000;
    public decimal NetSettlementAmountVnd { get; set; } = 0;
    public string OffsetJournalVoucherNo { get; set; } = "PKT-BT-0012";
    public string Status { get; set; } = "Approved"; // Draft | Approved | Posted
    public DateTimeOffset SettledAt { get; set; } = DateTimeOffset.UtcNow;
}
