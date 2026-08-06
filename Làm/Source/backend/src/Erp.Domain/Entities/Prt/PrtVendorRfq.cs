using Erp.Domain.Base;

namespace Erp.Domain.Entities.Prt;

/// <summary>Thực thể portal nhà cung cấp & đại lý (UC_PRT_013, 028, 032, 033, 034).</summary>
public class PrtVendorRfq : TenantEntity
{
    public Guid VendorId { get; set; }
    public string RfqCode { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // Draft | Sent | Quoted | Closed
    public bool IsSubscribedNewsletter { get; set; } = true;
    public bool DeliveryReadyAlert { get; set; } = false;
    public decimal TotalOutstandingBalance { get; set; } = 0m;
}
