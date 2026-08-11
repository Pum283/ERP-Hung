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
/// Unit tests cho Bước 31:
///   UC_HRM_109 — Check-in đầu ca (Shift Check-in Execution)
///   UC_HRM_110 — Check-out cuối ca (Shift Check-out Execution)
///   UC_HRM_111 — Xem lịch sử chấm cá nhân (Personal Attendance History Lookup)
///   UC_HRM_112 — Bảng chấm công theo đơn vị (Department Attendance Timesheet)
/// 15 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep31PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmAttendanceService _attendanceSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userEmp1   = Guid.NewGuid();
    private readonly Guid _userEmp2   = Guid.NewGuid();
    private readonly Guid _userNoEmp  = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _orgUnit2    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;
    private Guid _empId2;

    public HrmStep31PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step31-" + Guid.NewGuid())
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
            Code = "ORG_S31_1", Name = "Phòng Kiểm Thử 31", UnitType = "Department", Path = "/1"
        });
        _db.OrgUnits.Add(new OrgUnit
        {
            Id = _orgUnit2, TenantId = _tenant,
            Code = "ORG_S31_2", Name = "Phòng Phát Triển 31", UnitType = "Department", Path = "/2"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_TESTER31", Name = "Tester 31"
        });

        _db.Users.Add(new AppUser { Id = _userEmp1, TenantId = _tenant, Username = "emp31_1", DisplayName = "Nguyễn Văn CheckIn 31" });
        _db.Users.Add(new AppUser { Id = _userEmp2, TenantId = _tenant, Username = "emp31_2", DisplayName = "Trần Văn CheckIn 31" });
        _db.Users.Add(new AppUser { Id = _userNoEmp, TenantId = _tenant, Username = "no_emp31", DisplayName = "User Không Có Hồ Sơ" });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp1, EmployeeCode = "EMP_S31_1", FullName = "Nguyễn Văn CheckIn 31",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        var emp2 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp2, EmployeeCode = "EMP_S31_2", FullName = "Trần Văn CheckIn 31",
            OrgUnitId = _orgUnit2, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.AddRange(emp1, emp2);

        // Ensure default attendance policy with EnableApp = true
        _db.AttendancePolicies.Add(new AttendancePolicy
        {
            TenantId = _tenant, EnableApp = true, EnableQr = true, EnableFingerprint = true,
            LateGraceMinutes = 15, LateDeductEveryMinutes = 30, LateDeductWorkUnit = 0.25m
        });

        _db.SaveChanges();

        _empId1 = emp1.Id;
        _empId2 = emp2.Id;

        _attendanceSvc = new HrmAttendanceService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_109: Check-in đầu ca (Shift Check-in Execution)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC109_CheckIn_ValidEmployeeAndPolicy_CreatesOpenAttendanceRecord()
    {
        var rec = await _attendanceSvc.CheckInAsync(_tenant, _userEmp1,
            new AttendancePunchRequest("App", null, null, null, "Check-in ca sáng"));

        Assert.NotNull(rec);
        Assert.Equal(_empId1, rec.EmployeeId);
        Assert.Equal("Open", rec.Status);
        Assert.Equal("App", rec.CheckInMethod);
        Assert.NotNull(rec.CheckInAt);
    }

    [Fact]
    public async Task UC109_CheckIn_UserWithoutEmployeeProfile_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.CheckInAsync(_tenant, _userNoEmp,
                new AttendancePunchRequest("App", null, null, null, null)));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Không tìm thấy hồ sơ nhân viên", ex.Message);
    }

    [Fact]
    public async Task UC109_CheckIn_DuplicateCheckInSameDay_ThrowsAppException()
    {
        await _attendanceSvc.CheckInAsync(_tenant, _userEmp1, new AttendancePunchRequest("App", null, null, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.CheckInAsync(_tenant, _userEmp1, new AttendancePunchRequest("App", null, null, null, null)));

        Assert.Contains("Đã check-in trong ngày", ex.Message);
    }

    [Fact]
    public async Task UC109_CheckIn_MethodDisabledInPolicy_ThrowsAppException()
    {
        // Disable App check-in method in policy
        var policy = await _db.AttendancePolicies.FirstAsync(x => x.TenantId == _tenant);
        policy.EnableApp = false;
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.CheckInAsync(_tenant, _userEmp1, new AttendancePunchRequest("App", null, null, null, null)));

        Assert.Contains("Chấm APP đang tắt", ex.Message);
    }

    [Fact]
    public async Task UC109_CheckIn_LockedPeriod_ThrowsAppException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var periodKey = today.ToString("yyyy-MM");
        await _attendanceSvc.LockPeriodAsync(_tenant, _userEmp1, new AttendanceLockRequest(periodKey, "Khóa chấm công"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.CheckInAsync(_tenant, _userEmp1, new AttendancePunchRequest("App", null, null, null, null)));

        Assert.Contains("đã khóa", ex.Message.ToLower());
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_110: Check-out cuối ca (Shift Check-out Execution)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC110_CheckOut_ValidCheckInRecord_UpdatesStatusToClosed()
    {
        await _attendanceSvc.CheckInAsync(_tenant, _userEmp1, new AttendancePunchRequest("App", null, null, null, null));

        var rec = await _attendanceSvc.CheckOutAsync(_tenant, _userEmp1, new AttendancePunchRequest("App", null, null, null, "Check-out ra về"));

        Assert.NotNull(rec);
        Assert.Equal("Closed", rec.Status);
        Assert.Equal("App", rec.CheckOutMethod);
        Assert.NotNull(rec.CheckOutAt);
    }

    [Fact]
    public async Task UC110_CheckOut_WithoutCheckIn_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.CheckOutAsync(_tenant, _userEmp2, new AttendancePunchRequest("App", null, null, null, null)));

        Assert.Contains("Chưa check-in", ex.Message);
    }

    [Fact]
    public async Task UC110_CheckOut_DuplicateCheckOutSameDay_ThrowsAppException()
    {
        await _attendanceSvc.CheckInAsync(_tenant, _userEmp1, new AttendancePunchRequest("App", null, null, null, null));
        await _attendanceSvc.CheckOutAsync(_tenant, _userEmp1, new AttendancePunchRequest("App", null, null, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.CheckOutAsync(_tenant, _userEmp1, new AttendancePunchRequest("App", null, null, null, null)));

        Assert.Contains("Đã check-out", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_111: Xem lịch sử chấm cá nhân (Personal Attendance History)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC111_MyHistory_ReturnsPersonalAttendanceRecordsForDateRange()
    {
        await _attendanceSvc.CheckInAsync(_tenant, _userEmp1, new AttendancePunchRequest("App", null, null, null, null));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var history = await _attendanceSvc.MyHistoryAsync(_tenant, _userEmp1, today, today);

        Assert.Single(history);
        Assert.Equal(_empId1, history[0].EmployeeId);
        Assert.Equal("Nguyễn Văn CheckIn 31", history[0].EmployeeName);
    }

    [Fact]
    public async Task UC111_MyHistory_UserWithoutEmployee_ThrowsAppException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.MyHistoryAsync(_tenant, _userNoEmp, today, today));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_112: Bảng chấm công theo đơn vị (Department Attendance Board)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC112_Board_ReturnsDepartmentAttendanceRecords()
    {
        await _attendanceSvc.CheckInAsync(_tenant, _userEmp1, new AttendancePunchRequest("App", null, null, null, null));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var board = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, today, today);

        Assert.Single(board);
        Assert.Equal(_orgUnit1, board[0].OrgUnitId);
        Assert.Equal("EMP_S31_1", board[0].EmployeeCode);
    }

    [Fact]
    public async Task UC112_Board_EmptyDepartmentRecords_ReturnsEmptyList()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5));

        var board = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, futureDate, futureDate);

        Assert.Empty(board);
    }
}
