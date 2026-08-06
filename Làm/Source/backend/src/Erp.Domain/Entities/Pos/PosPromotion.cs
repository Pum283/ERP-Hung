using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Chương trình khuyến mại quầy (UC_POS_021).</summary>
public class PosPromotion : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Percent · Amount</summary>
    public string DiscountType { get; set; } = "Percent";
    public decimal DiscountValue { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}

/// <summary>Mã voucher gắn CTKM (UC_POS_022).</summary>
public class PosVoucher : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid PromotionId { get; set; }
    public int MaxUses { get; set; } = 1;
    public int UsedCount { get; set; }
    /// <summary>Active · Inactive · Exhausted</summary>
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
