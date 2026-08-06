using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Đơn vị tính (UC_INV_003).</summary>
public class InvUnitOfMeasure : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
