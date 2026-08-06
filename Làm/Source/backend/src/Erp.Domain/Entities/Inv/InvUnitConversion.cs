using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Quy đổi ĐVT: FromQty * Factor = ToQty (UC_INV_003).</summary>
public class InvUnitConversion : TenantEntity
{
    public Guid FromUnitId { get; set; }
    public Guid ToUnitId { get; set; }
    public decimal Factor { get; set; }
}
