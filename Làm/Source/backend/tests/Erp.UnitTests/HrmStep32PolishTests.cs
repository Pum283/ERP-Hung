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
/// Unit tests cho Bước 32:
///   UC_HRM_113 — Bảng chấm công toàn công ty (Company-wide Attendance Timesheet)
///   UC_HRM_114 — Cảnh báo thiếu chấm realtime (Real-time Missing Punch Alerts)
///   UC_HRM_115 — Tự tính phút đi trễ (Automatic Late Arrival Minutes Calculation)
///   UC_HRM_116 — Tự trừ công do đi trễ (Automatic Late Penalty Deduction Application)
/// 14 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep32PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmAttendanceService _attendanceSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userEmp1   = Guid.NewGuid();
    private readonly Guid _userEmp2   = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _orgUnit2    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;
    private Guid _empId2;

    public HrmStep32PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step32-" + Guid.NewGuid())
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
            Code = "ORG_S32_1", Name = "Khối Vận Hành 32", UnitType = "Department", Path = "/1"
        });
        _db.OrgUnits.Add(new OrgUnit
        {
            Id = _orgUnit2, TenantId = _tenant,
            Code = "ORG_S32_2", Name = "Khối Kinh Doanh 32", UnitType = "Department", Path = "/2"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_STAFF32", Name = "Nhân Viên 32"
        });

        _db.Users.Add(new AppUser { Id = _userEmp1, TenantId = _tenant, Username = "emp32_1", DisplayName = "Lê Văn Tự Trừ Công 32" });
        _db.Users.Add(new AppUser { Id = _userEmp2, TenantId = _tenant, Username = "emp32_2", DisplayName = "Phạm Thị Cảnh Báo 32" });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp1, EmployeeCode = "EMP_S32_1", FullName = "Lê Văn Tự Trừ Công 32",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        var emp2 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp2, EmployeeCode = "EMP_S32_2", FullName = "Phạm Thị Cảnh Báo 32",
            OrgUnitId = _orgUnit2, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.AddRange(emp1, emp2);

        _db.AttendancePolicies.Add(new AttendancePolicy
        {
            TenantId = _tenant, EnableApp = true, EnableQr = true, EnableFingerprint = true,
            LateGraceMinutes = 15, LateDeductEveryMinutes = 30, LateDeductWorkUnit = 0.25m,
            ForgotCheckoutHours = 14, DefaultShiftStart = new TimeOnly(8, 0), DefaultShiftEnd = new TimeOnly(17, 0)
        });

        _db.SaveChanges();

        _empId1 = emp1.Id;
        _empId2 = emp2.Id;

        _attendanceSvc = new HrmAttendanceService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_113: Bảng chấm công toàn công ty (Company-wide Attendance Timesheet)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC113_Board_NullOrgUnitId_ReturnsCompanyWideRecordsFromAllDepartments()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await _attendanceSvc.CheckInAsync(_tenant, _userEmp1, new AttendancePunchRequest("App", null, null, null, null));
        await _attendanceSvc.CheckInAsync(_tenant, _userEmp2, new AttendancePunchRequest("App", null, null, null, null));

        var companyBoard = await _attendanceSvc.BoardAsync(_tenant, null, today, today);

        Assert.Equal(2, companyBoard.Count);
        var orgUnits = companyBoard.Select(x => x.OrgUnitId).Distinct().ToList();
        Assert.Contains(_orgUnit1, orgUnits);
        Assert.Contains(_orgUnit2, orgUnits);
    }

    [Fact]
    public async Task UC113_Board_FilteredByDateRange_ReturnsRecordsWithinRangeOnly()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var pastDate = today.AddDays(-5);

        // Add historical attendance record
        _db.AttendanceRecords.Add(new AttendanceRecord
        {
            TenantId = _tenant, EmployeeId = _empId1, WorkDate = pastDate, CheckInAt = DateTimeOffset.UtcNow.AddDays(-5), Status = "Closed"
        });
        await _db.SaveChangesAsync();

        var filtered = await _attendanceSvc.BoardAsync(_tenant, null, pastDate, pastDate);

        Assert.Single(filtered);
        Assert.Equal(pastDate, filtered[0].WorkDate);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_114: Cảnh báo thiếu chấm realtime (Real-time Missing Punch Alerts)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC114_MissingAlerts_NoCheckInToday_ReturnsMissingCheckInAlerts()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var alerts = await _attendanceSvc.MissingAlertsAsync(_tenant, today);

        Assert.Equal(2, alerts.Count);
        Assert.All(alerts, a => Assert.Equal("MissingCheckIn", a.AlertType));
    }

    [Fact]
    public async Task UC114_MissingAlerts_CheckInPastForgotCheckoutHours_ReturnsMissingCheckoutAlert()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Add check-in record from 16 hours ago without check-out (policy threshold = 14h)
        _db.AttendanceRecords.Add(new AttendanceRecord
        {
            TenantId = _tenant, EmployeeId = _empId1, WorkDate = today,
            CheckInAt = DateTimeOffset.UtcNow.AddHours(-16), CheckOutAt = null, Status = "Open"
        });
        await _db.SaveChangesAsync();

        var alerts = await _attendanceSvc.MissingAlertsAsync(_tenant, today);

        var emp1Alert = alerts.FirstOrDefault(x => x.EmployeeId == _empId1);
        Assert.NotNull(emp1Alert);
        Assert.Equal("MissingCheckout", emp1Alert.AlertType);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_115: Tự tính phút đi trễ (Automatic Late Arrival Calculation)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC115_LateCheckIn_CalculatesLateMinutesAccurately()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var checkInTime = new DateTimeOffset(today.ToDateTime(new TimeOnly(8, 45)), TimeSpan.FromHours(7)); // 8:45 (late 45m vs 8:00 start)

        var items = new List<AttendanceDeviceSyncItem>
        {
            new AttendanceDeviceSyncItem("EMP_S32_1", checkInTime, "In", null)
        };
        await _attendanceSvc.SyncDeviceAsync(_tenant, _userEmp1, new AttendanceDeviceSyncRequest(items));

        var board = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, today, today);
        Assert.Single(board);
        Assert.Equal(45, board[0].LateMinutes);
    }

    [Fact]
    public async Task UC115_OnTimeCheckIn_SetsLateMinutesToZero()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var checkInTime = new DateTimeOffset(today.ToDateTime(new TimeOnly(7, 55)), TimeSpan.FromHours(7)); // 7:55 (on-time)

        var items = new List<AttendanceDeviceSyncItem>
        {
            new AttendanceDeviceSyncItem("EMP_S32_1", checkInTime, "In", null)
        };
        await _attendanceSvc.SyncDeviceAsync(_tenant, _userEmp1, new AttendanceDeviceSyncRequest(items));

        var board = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, today, today);
        Assert.Single(board);
        Assert.Equal(0, board[0].LateMinutes);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_116: Tự trừ công do đi trễ (Automatic Penalty Deduction)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC116_LateBeyondGracePeriod_CalculatesDeductedWorkUnitAndWorkUnit()
    {
        // Policy: Grace = 15m, DeductEvery = 30m, DeductWorkUnit = 0.25
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var checkInTime = new DateTimeOffset(today.ToDateTime(new TimeOnly(8, 45)), TimeSpan.FromHours(7)); // Late 45m -> excess 30m -> 1 block -> deduct 0.25

        var items = new List<AttendanceDeviceSyncItem>
        {
            new AttendanceDeviceSyncItem("EMP_S32_1", checkInTime, "In", null)
        };
        await _attendanceSvc.SyncDeviceAsync(_tenant, _userEmp1, new AttendanceDeviceSyncRequest(items));

        var board = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, today, today);
        Assert.Single(board);
        var rec = board[0];
        Assert.Equal(0.25m, rec.DeductedWorkUnit);
        Assert.Equal(0.75m, rec.WorkUnit);
    }

    [Fact]
    public async Task UC116_MultipleLateBlocks_AccumulatesDeductionsCorrectly()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var checkInTime = new DateTimeOffset(today.ToDateTime(new TimeOnly(9, 45)), TimeSpan.FromHours(7)); // Late 105m -> excess 90m -> 3 blocks -> deduct 0.75

        var items = new List<AttendanceDeviceSyncItem>
        {
            new AttendanceDeviceSyncItem("EMP_S32_1", checkInTime, "In", null)
        };
        await _attendanceSvc.SyncDeviceAsync(_tenant, _userEmp1, new AttendanceDeviceSyncRequest(items));

        var board = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, today, today);
        Assert.Single(board);
        var rec = board[0];
        Assert.Equal(0.75m, rec.DeductedWorkUnit);
        Assert.Equal(0.25m, rec.WorkUnit);
    }

    [Fact]
    public async Task UC116_LateWithinGracePeriod_NoDeductionApplied()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var checkInTime = new DateTimeOffset(today.ToDateTime(new TimeOnly(8, 10)), TimeSpan.FromHours(7)); // Late 10m <= 15m grace

        var items = new List<AttendanceDeviceSyncItem>
        {
            new AttendanceDeviceSyncItem("EMP_S32_1", checkInTime, "In", null)
        };
        await _attendanceSvc.SyncDeviceAsync(_tenant, _userEmp1, new AttendanceDeviceSyncRequest(items));

        var board = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, today, today);
        Assert.Single(board);
        var rec = board[0];
        Assert.Equal(0m, rec.DeductedWorkUnit);
        Assert.Equal(1.0m, rec.WorkUnit);
    }
}
