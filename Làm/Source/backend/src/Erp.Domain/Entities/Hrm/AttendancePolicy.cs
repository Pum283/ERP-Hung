using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Cấu hình chấm công tenant (UC_HRM_098–108).</summary>
public class AttendancePolicy : TenantEntity
{
    public bool EnableFingerprint { get; set; }
    public bool EnableApp { get; set; } = true;
    public bool EnableQr { get; set; } = true;
    public bool EnableGeoFence { get; set; }
    /// <summary>Phút ân hạn trước khi tính đi trễ.</summary>
    public int LateGraceMinutes { get; set; } = 5;
    /// <summary>Mỗi X phút trễ trừ Y công (WorkUnit).</summary>
    public int LateDeductEveryMinutes { get; set; } = 30;
    public decimal LateDeductWorkUnit { get; set; } = 0.25m;
    /// <summary>Tự đánh dấu quên check-out sau N giờ kể từ check-in.</summary>
    public int ForgotCheckoutHours { get; set; } = 14;
    /// <summary>Số ngày được xin điều chỉnh sau ngày công.</summary>
    public int AdjustDeadlineDays { get; set; } = 3;
    public bool EnableOt { get; set; } = true;
    /// <summary>OT bắt đầu sau N phút vượt end ca.</summary>
    public int OtAfterMinutes { get; set; } = 30;
    public bool EnableNightShiftRule { get; set; }
    public bool EnableHolidayRule { get; set; }
    /// <summary>Giờ bắt đầu mặc định nếu không có lịch ca (HH:mm).</summary>
    public TimeOnly DefaultShiftStart { get; set; } = new(8, 0);
    public TimeOnly DefaultShiftEnd { get; set; } = new(17, 0);
}
