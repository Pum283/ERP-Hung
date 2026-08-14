using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Chuyển kho một bước trực tiếp (UC_INV_034).</summary>
public class InvOneStepTransfer : TenantEntity
{
    public string TransferNumber { get; set; } = "";
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public string TransferReason { get; set; } = "Cân bằng tồn kho nội bộ";
    public DateTimeOffset ExecutedAt { get; set; } = DateTimeOffset.UtcNow;
}
