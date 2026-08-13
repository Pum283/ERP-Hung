using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Phân loại nhóm nhà cung cấp (UC_PUR_002).</summary>
public class PurSupplierCategory : TenantEntity
{
    public string CategoryCode { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
