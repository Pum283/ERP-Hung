using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Điều kiện khuyến mại (UC_CRM_033).</summary>
public class CrmPromotionCondition : TenantEntity
{
    public Guid PromotionId { get; set; }
    /// <summary>Product · Category · CustomerSegment · MinQty · MinAmount</summary>
    public string ConditionType { get; set; } = "";
    /// <summary>ID sản phẩm / nhóm SP / segment hoặc giá trị min.</summary>
    public string ConditionValue { get; set; } = "";
    /// <summary>Equals · GreaterThan · In · Between</summary>
    public string Operator { get; set; } = "Equals";
}
