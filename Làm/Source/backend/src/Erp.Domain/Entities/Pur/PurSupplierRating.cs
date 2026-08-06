using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Đánh giá nhà cung cấp (UC_PUR).</summary>
public class PurSupplierRating : TenantEntity
{
    public Guid VendorId { get; set; }
    public decimal QualityScore { get; set; }  // 0 - 100
    public decimal DeliveryScore { get; set; } // 0 - 100
    public decimal PriceScore { get; set; }    // 0 - 100
    public decimal CompositeScore { get; set; }
    public string RatingPeriod { get; set; } = "Q3-2026";
}
