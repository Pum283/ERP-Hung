using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Cấu hình ca giao hàng (UC_LOG_005).</summary>
public class LogDeliveryShift : TenantEntity
{
    public string ShiftCode { get; set; } = "";
    public string ShiftName { get; set; } = "";
    public string StartTime { get; set; } = "08:00";
    public string EndTime { get; set; } = "12:00";
    public int MaxOrdersCapacity { get; set; } = 30;
    public bool IsActive { get; set; } = true;
}
