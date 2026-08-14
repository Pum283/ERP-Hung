using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Báo cáo đúng hạn giao hàng OTD (On-Time Delivery) (UC_PUR_049).</summary>
public class PurVendorOtdReport : TenantEntity
{
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = "";
    public int TotalOrdersCount { get; set; }
    public int OnTimeOrdersCount { get; set; }
    public int LateOrdersCount { get; set; }
    public double OnTimeDeliveryPercentage { get; set; }
    public DateTimeOffset PeriodStartDate { get; set; }
    public DateTimeOffset PeriodEndDate { get; set; }
}
