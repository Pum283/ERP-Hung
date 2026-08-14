using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Bảng giá cước vận chuyển (UC_LOG_003).</summary>
public class LogFreightPricingRate : TenantEntity
{
    public string RateCode { get; set; } = "";
    public string VehicleType { get; set; } = "Truck-2.5T";
    public decimal BasePriceVnd { get; set; } = 300000;
    public decimal PricePerKmFirst10Km { get; set; } = 25000;
    public decimal PricePerKmAfter10Km { get; set; } = 18000;
    public decimal LoadingUnloadingFeeVnd { get; set; } = 100000;
    public bool IsActive { get; set; } = true;
}
