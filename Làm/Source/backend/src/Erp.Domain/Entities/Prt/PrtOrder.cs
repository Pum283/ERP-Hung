using Erp.Domain.Base;

namespace Erp.Domain.Entities.Prt;

/// <summary>Đơn hàng portal KH (UC_PRT_007–008).</summary>
public class PrtOrder : TenantEntity
{
    public Guid AccountId { get; set; }
    public string Code { get; set; } = "";
    public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>Draft · Confirmed · Shipping · Delivered · Cancelled</summary>
    public string Status { get; set; } = "Confirmed";
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Note { get; set; }
}
