using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Ghi nhận giờ công làm việc timesheet theo dự án (UC_PJM_020).</summary>
public class PjmProjectTimesheetEntry : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; } = "PRJ-2026-088";
    public Guid EmployeeUserId { get; set; }
    public string EmployeeName { get; set; } = "Kỹ Sư Nguyễn Văn Hùng";
    public string TaskDescription { get; set; } = "Đấu nối tủ biến áp và chạy thử ATS";
    public decimal HoursSpent { get; set; } = 8.0m;
    public decimal OvertimeHours { get; set; } = 2.0m;
    public string Status { get; set; } = "Approved"; // Draft | Submitted | Approved | Rejected
    public DateTimeOffset WorkDate { get; set; } = DateTimeOffset.UtcNow;
}
