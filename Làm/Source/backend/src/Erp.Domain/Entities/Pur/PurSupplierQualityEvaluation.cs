using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Đánh giá chất lượng nhà cung cấp (UC_PUR_005).</summary>
public class PurSupplierQualityEvaluation : TenantEntity
{
    public Guid SupplierId { get; set; }
    public string Period { get; set; } = "Q3-2026";
    public double OnTimeDeliveryScore { get; set; } // Max 100
    public double QualityComplianceScore { get; set; } // Max 100
    public double PriceCompetitivenessScore { get; set; } // Max 100
    public double OverallRatingScore { get; set; } // Average
    public string RatingGrade { get; set; } = "A"; // A | B | C | D
    public string Comments { get; set; } = "";
    public Guid EvaluatedByUserId { get; set; }
    public DateTimeOffset EvaluatedAt { get; set; } = DateTimeOffset.UtcNow;
}
