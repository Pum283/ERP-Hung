using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Chương trình khuyến mại (UC_CRM_032, 033, 035, 037).</summary>
public class CrmPromotion : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    /// <summary>Percentage · FixedAmount · BuyXGetY · FreeShipping</summary>
    public string DiscountType { get; set; } = "Percentage";
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderValue { get; set; }
    /// <summary>Draft · Active · Expired · Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    /// <summary>Tổng lượt cho phép sử dụng (UC_CRM_035).</summary>
    public int? MaxUsageTotal { get; set; }
    /// <summary>Giới hạn mỗi khách hàng.</summary>
    public int? MaxUsagePerCustomer { get; set; }
    public int CurrentUsageCount { get; set; }
    public Guid? CampaignId { get; set; }
}
