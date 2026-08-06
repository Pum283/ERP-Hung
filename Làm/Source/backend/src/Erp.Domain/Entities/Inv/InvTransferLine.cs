using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

public class InvTransferLine : TenantEntity
{
    public Guid TransferId { get; set; }
    public Guid SkuId { get; set; }
    public string SkuCode { get; set; } = "";
    public string SkuName { get; set; } = "";
    public decimal Qty { get; set; }
    public string? LotCode { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}
