using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Đơn bán POS (UC_POS_026–040).</summary>
public class PosSale : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid ShiftId { get; set; }
    public Guid StoreId { get; set; }
    public Guid? TerminalId { get; set; }
    /// <summary>Open · Held · Paid · Cancelled · Returned</summary>
    public string Status { get; set; } = "Open";
    public string? AreaName { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ReturnedAmount { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? ReceiptPrintedAt { get; set; }
    public string? Note { get; set; }

    /// <summary>None · Promotion · Voucher · Manual (UC_POS_021–024).</summary>
    public string DiscountSource { get; set; } = "None";
    public Guid? PromotionId { get; set; }
    public Guid? VoucherId { get; set; }
    public string? AppliedVoucherCode { get; set; }
    /// <summary>Percent · Amount — dùng cho giảm tay.</summary>
    public string? ManualDiscountType { get; set; }
    public decimal ManualDiscountValue { get; set; }
    /// <summary>None · Pending · Approved · Rejected</summary>
    public string DiscountApprovalStatus { get; set; } = "None";
    public string? DiscountNote { get; set; }
    public Guid? DiscountDecidedByUserId { get; set; }
    public DateTimeOffset? DiscountDecidedAt { get; set; }
}
