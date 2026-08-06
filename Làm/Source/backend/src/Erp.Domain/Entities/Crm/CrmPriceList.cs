using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Bảng giá / chính sách giá CRM (UC_CRM_072).</summary>
public class CrmPriceList : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
