using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Bảng giá dịch vụ hiện trường & đơn giá giờ công kỹ thuật (UC_FSM_004).</summary>
public class FsmServicePriceRate : TenantEntity
{
    public string ServiceCode { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string ServiceCategory { get; set; } = "Bảo Trì Định Kỳ";
    public decimal BaseHourlyRateVnd { get; set; } = 250000;
    public decimal StandardTravelFeeVnd { get; set; } = 150000;
    public decimal EmergencySurchargePct { get; set; } = 30;
    public bool IsActive { get; set; } = true;
}
