using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Lịch sử nhắc nợ công nợ phải thu tự động qua Email/SMS (UC_FIN_034).</summary>
public class FinDebtDunningNotification : TenantEntity
{
    public string InvoiceNumber { get; set; } = "INV-2026-0814";
    public string CustomerName { get; set; } = "Công Ty CP Xây Lắp Điện Hải Phòng";
    public decimal OverdueAmountVnd { get; set; } = 42500000;
    public int OverdueDays { get; set; } = 15;
    public string DunningLevel { get; set; } = "Level1_Reminder"; // Level1_Reminder | Level2_Warning | Level3_LegalNotice
    public string DeliveryChannel { get; set; } = "Email"; // Email | SMS | ZaloZNS
    public string RecipientContact { get; set; } = "ketoan@haiphong-power.vn";
    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
}
