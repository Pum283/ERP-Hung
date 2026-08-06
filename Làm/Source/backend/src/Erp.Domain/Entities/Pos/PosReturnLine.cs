using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

public class PosReturnLine : TenantEntity
{
    public Guid ReturnId { get; set; }
    public Guid? SaleLineId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal Quantity { get; set; } = 1;
    public decimal LineAmount { get; set; }
}
