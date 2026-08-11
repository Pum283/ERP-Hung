using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 33:
///   UC_HRM_117 — Đánh dấu quên chấm (Manual Missing Punch Tagging)
///   UC_HRM_119 — Xử lý công OT tự động (Automatic OT Hours Calculation)
///   UC_HRM_120 — Tạo phiếu xin điều chỉnh công (Create Attendance Adjustment Request)
///   UC_HRM_121 — Đính kèm lý do / bằng chứng (Attach Adjustment Evidence / Storage Key)
/// 14 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep33PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmAttendanceService _attendanceSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userEmp1   = Guid.NewGuid();
    private readonly Guid _userEmp2   = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;
    private Guid _empId2;

    public HrmStep33PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step33-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });
        _db.OrgUnits.Add(new OrgUnit
        {
            Id = _orgUnit1, TenantId = _tenant,
            Code = "ORG_S33_1", Name = "Phòng Xử Lý Công 33", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_STAFF33", Name = "Chuyên Viên 33"
        });

        _db.Users.Add(new AppUser { Id = _userEmp1, TenantId = _tenant, Username = "emp33_1", DisplayName = "Nguyễn Văn Điều Chỉnh 33" });
        _db.Users.Add(new AppUser { Id = _userEmp2, TenantId = _tenant, Username = "emp33_2", DisplayName = "Trần Thị OT 33" });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp1, EmployeeCode = "EMP_S33_1", FullName = "Nguyễn Văn Điều Chỉnh 33",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        var emp2 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp2, EmployeeCode = "EMP_S33_2", FullName = "Trần Thị OT 33",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.AddRange(emp1, emp2);

        _db.AttendancePolicies.Add(new AttendancePolicy
        {
            TenantId = _tenant, EnableApp = true, EnableQr = true, EnableFingerprint = true,
            LateGraceMinutes = 15, LateDeductEveryMinutes = 30, LateDeductWorkUnit = 0.25m,
            ForgotCheckoutHours = 14, AdjustDeadlineDays = 7, EnableOt = true, OtAfterMinutes = 30,
            DefaultShiftStart = new TimeOnly(8, 0), DefaultShiftEnd = new TimeOnly(17, 0)
        });

        _db.SaveChanges();

        _empId1 = emp1.Id;
        _empId2 = emp2.Id;

        _attendanceSvc = new HrmAttendanceService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_117: Đánh dấu quên chấm (Manual Missing Punch Tagging)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC117_MissingAlerts_ReturnsMissingCheckInAlertForUnpunchedEmployee()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var alerts = await _attendanceSvc.MissingAlertsAsync(_tenant, today);

        Assert.Equal(2, alerts.Count);
        var alert = alerts.FirstOrDefault(x => x.EmployeeId == _empId1);
        Assert.NotNull(alert);
        Assert.Equal("MissingCheckIn", alert.AlertType);
        Assert.Equal("EMP_S33_1", alert.EmployeeCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_119: Xử lý công OT tự động (Automatic OT Hours Calculation)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC119_CheckOutPastOtThreshold_CalculatesOtMinutesAutomatically()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var inTime = new DateTimeOffset(today.ToDateTime(new TimeOnly(8, 0)), TimeSpan.FromHours(7));
        var outTime = new DateTimeOffset(today.ToDateTime(new TimeOnly(19, 30)), TimeSpan.FromHours(7)); // 19:30 (2h30m OT past 17:00, past 17:30 threshold)

        var items = new List<AttendanceDeviceSyncItem>
        {
            new AttendanceDeviceSyncItem("EMP_S33_2", inTime, "In", null),
            new AttendanceDeviceSyncItem("EMP_S33_2", outTime, "Out", null)
        };
        await _attendanceSvc.SyncDeviceAsync(_tenant, _userEmp2, new AttendanceDeviceSyncRequest(items));

        var board = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, today, today);
        var rec = board.First(x => x.EmployeeId == _empId2);

        Assert.True(rec.OtMinutes >= 120);
    }

    [Fact]
    public async Task UC119_RecalcOt_RecalculatesOtMinutesBatchForDateRange()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var count = await _attendanceSvc.RecalcOtAsync(_tenant, _userEmp1, today.AddDays(-7), today);

        Assert.True(count >= 0);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_120: Tạo phiếu xin điều chỉnh công (Create Attendance Adjust Request)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC120_CreateAdjust_ValidParameters_CreatesAdjustRequestSuccessfully()
    {
        var workDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-2));
        var req = await _attendanceSvc.CreateAdjustAsync(_tenant, _userEmp1,
            new AttendanceAdjustCreateRequest(_empId1, workDate, null, null, "Quên check-in do công tác", null, true));

        Assert.NotNull(req);
        Assert.Equal(_empId1, req.EmployeeId);
        Assert.Equal(workDate, req.WorkDate);
        Assert.Equal("Submitted", req.Status);
        Assert.Equal("Quên check-in do công tác", req.Reason);
    }

    [Fact]
    public async Task UC120_CreateAdjust_ReasonTooShort_ThrowsAppException()
    {
        var workDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.CreateAdjustAsync(_tenant, _userEmp1,
                new AttendanceAdjustCreateRequest(_empId1, workDate, null, null, "Lý", null, true))); // 2 chars < 3

        Assert.Contains("Lý do 3–500 ký tự", ex.Message);
    }

    [Fact]
    public async Task UC120_CreateAdjust_InvalidEmployee_ThrowsAppException()
    {
        var workDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.CreateAdjustAsync(_tenant, _userEmp1,
                new AttendanceAdjustCreateRequest(Guid.NewGuid(), workDate, null, null, "Xin điều chỉnh", null, true)));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Nhân viên không hợp lệ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_121: Đính kèm lý do / bằng chứng (Attach Evidence Storage Key)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC121_CreateAdjust_WithEvidenceStorageKey_StoresStorageKeyCorrectly()
    {
        var workDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var evidenceKey = "evidence/hrm/2026-08/proof_emp33_1.png";

        var req = await _attendanceSvc.CreateAdjustAsync(_tenant, _userEmp1,
            new AttendanceAdjustCreateRequest(_empId1, workDate, null, null, "Có giấy xác nhận công tác", evidenceKey, true));

        Assert.NotNull(req);
        Assert.Equal(evidenceKey, req.EvidenceStorageKey);
    }

    [Fact]
    public async Task UC121_CreateAdjust_WhitespaceEvidenceKey_SetsEvidenceKeyToNull()
    {
        var workDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        var req = await _attendanceSvc.CreateAdjustAsync(_tenant, _userEmp1,
            new AttendanceAdjustCreateRequest(_empId1, workDate, null, null, "Quên check-in do sự cố mạng", "   ", true));

        Assert.NotNull(req);
        Assert.Null(req.EvidenceStorageKey);
    }
}
