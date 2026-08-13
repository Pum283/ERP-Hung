using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Kiểm soát điều kiện hoàn thành bài đào tạo trước ca làm việc (UC_LMS_059).</summary>
public class LmsShiftTrainingGate : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public string ShiftId { get; set; } = "";
    public DateTime ShiftDate { get; set; } = DateTime.UtcNow.Date;
    public DateTimeOffset ShiftStartTime { get; set; }
    public Guid CourseId { get; set; }
    public bool IsMandatoryCompleted { get; set; }
    public bool IsWorkEntryBlocked { get; set; }
    public string GateStatus { get; set; } = "Pending"; // Passed | Blocked | Pending
}
