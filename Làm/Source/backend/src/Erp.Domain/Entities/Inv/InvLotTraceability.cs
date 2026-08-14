using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Truy vết lô xuôi/ngược (UC_INV_047).</summary>
public class InvLotTraceability : TenantEntity
{
    public string LotNumber { get; set; } = "";
    public Guid ProductId { get; set; }
    public string Direction { get; set; } = "Forward"; // Forward (Nhà cung cấp -> Khách hàng) | Backward (Khách hàng -> Nhà cung cấp)
    public string OriginSupplierOrPO { get; set; } = "";
    public string ProductionBatchNumber { get; set; } = "";
    public string CustomerSalesOrderNumber { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
}
