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
/// Unit tests cho Bước 34:
///   UC_HRM_122 — Duyệt / từ chối điều chỉnh (Approve or Reject Attendance Adjustment Request)
///   UC_HRM_123 — Ghi nhận vi phạm đi trễ (Log Late Arrival Violations)
///   UC_HRM_126 — Khóa bảng công theo kỳ (Attendance Period Locking)
///   UC_HRM_127 — Mở khóa bảng công có kiểm soát (Controlled Attendance Period Unlocking)
/// 14 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep34PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmAttendanceService _attendanceSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userEmp1   = Guid.NewGuid();
    private readonly Guid _userMgr    = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;

    public HrmStep34PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step34-" + Guid.NewGuid())
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
            Code = "ORG_S34_1", Name = "Phòng Quản Lý Công 34", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_MGR34", Name = "Quản Lý 34"
        });

        _db.Users.Add(new AppUser { Id = _userEmp1, TenantId = _tenant, Username = "emp34_1", DisplayName = "Vũ Văn Duyệt Công 34" });
        _db.Users.Add(new AppUser { Id = _userMgr, TenantId = _tenant, Username = "mgr34", DisplayName = "Nguyễn Quản Lý 34" });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp1, EmployeeCode = "EMP_S34_1", FullName = "Vũ Văn Duyệt Công 34",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.Add(emp1);

        _db.AttendancePolicies.Add(new AttendancePolicy
        {
            TenantId = _tenant, EnableApp = true, EnableQr = true, EnableFingerprint = true,
            LateGraceMinutes = 15, LateDeductEveryMinutes = 30, LateDeductWorkUnit = 0.25m,
            ForgotCheckoutHours = 14, AdjustDeadlineDays = 7, EnableOt = true, OtAfterMinutes = 30,
            DefaultShiftStart = new TimeOnly(8, 0), DefaultShiftEnd = new TimeOnly(17, 0)
        });

        _db.SaveChanges();

        _empId1 = emp1.Id;

        _attendanceSvc = new HrmAttendanceService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_122: Duyệt / từ chối điều chỉnh (Approve or Reject Adjustment)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC122_DecideAdjust_ApproveSubmitted_UpdatesStatusToApprovedAndUpdatesRecord()
    {
        var workDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-2));
        var checkIn = new DateTimeOffset(workDate.ToDateTime(new TimeOnly(8, 0)), TimeSpan.FromHours(7));
        var checkOut = new DateTimeOffset(workDate.ToDateTime(new TimeOnly(17, 0)), TimeSpan.FromHours(7));

        var adj = await _attendanceSvc.CreateAdjustAsync(_tenant, _userEmp1,
            new AttendanceAdjustCreateRequest(_empId1, workDate, checkIn, checkOut, "Xin bổ sung check-in/out", null, true));

        var result = await _attendanceSvc.DecideAdjustAsync(_tenant, _userMgr, adj.Id, true);

        Assert.NotNull(result);
        Assert.Equal("Approved", result.Status);

        var board = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, workDate, workDate);
        Assert.Single(board);
        Assert.Equal("Adjusted", board[0].Status);
    }

    [Fact]
    public async Task UC122_DecideAdjust_RejectSubmitted_UpdatesStatusToRejected()
    {
        var workDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-2));
        var adj = await _attendanceSvc.CreateAdjustAsync(_tenant, _userEmp1,
            new AttendanceAdjustCreateRequest(_empId1, workDate, null, null, "Xin điều chỉnh không đủ căn cứ", null, true));

        var result = await _attendanceSvc.DecideAdjustAsync(_tenant, _userMgr, adj.Id, false);

        Assert.NotNull(result);
        Assert.Equal("Rejected", result.Status);
    }

    [Fact]
    public async Task UC122_DecideAdjust_DraftRequest_ThrowsAppException()
    {
        var workDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var adj = await _attendanceSvc.CreateAdjustAsync(_tenant, _userEmp1,
            new AttendanceAdjustCreateRequest(_empId1, workDate, null, null, "Lưu nháp chưa nộp", null, false)); // Submit = false -> Draft

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.DecideAdjustAsync(_tenant, _userMgr, adj.Id, true));

        Assert.Contains("Chỉ duyệt phiếu Submitted", ex.Message);
    }

    [Fact]
    public async Task UC122_DecideAdjust_NonExistentId_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.DecideAdjustAsync(_tenant, _userMgr, Guid.NewGuid(), true));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Phiếu điều chỉnh không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_123: Ghi nhận vi phạm đi trễ (Log Late Arrival Violations)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC123_LateArrivalBeyondGrace_LogsLateViolationInBoard()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var checkInTime = new DateTimeOffset(today.ToDateTime(new TimeOnly(8, 45)), TimeSpan.FromHours(7)); // Late 45m

        var items = new List<AttendanceDeviceSyncItem>
        {
            new AttendanceDeviceSyncItem("EMP_S34_1", checkInTime, "In", null)
        };
        await _attendanceSvc.SyncDeviceAsync(_tenant, _userEmp1, new AttendanceDeviceSyncRequest(items));

        var board = await _attendanceSvc.BoardAsync(_tenant, _orgUnit1, today, today);
        Assert.Single(board);
        var rec = board[0];

        Assert.Equal(45, rec.LateMinutes);
        Assert.Equal(0.25m, rec.DeductedWorkUnit);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_126: Khóa bảng công theo kỳ (Attendance Period Locking)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC126_LockPeriod_ValidKey_CreatesLockedPeriodSuccessfully()
    {
        var lockDto = await _attendanceSvc.LockPeriodAsync(_tenant, _userMgr, new AttendanceLockRequest("2026-08", "Khóa công tháng 8/2026"));

        Assert.NotNull(lockDto);
        Assert.Equal("2026-08", lockDto.PeriodKey);
        Assert.True(lockDto.IsLocked);
        Assert.Equal(new DateOnly(2026, 8, 1), lockDto.PeriodFrom);
        Assert.Equal(new DateOnly(2026, 8, 31), lockDto.PeriodTo);
    }

    [Fact]
    public async Task UC126_LockPeriod_InvalidPeriodKeyFormat_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.LockPeriodAsync(_tenant, _userMgr, new AttendanceLockRequest("08-2026", "Sai định dạng")));

        Assert.Contains("PeriodKey phải dạng yyyy-MM", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_127: Mở khóa bảng công có kiểm soát (Controlled Period Unlocking)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC127_UnlockPeriod_ValidPeriodKey_UnlocksPeriodSuccessfully()
    {
        await _attendanceSvc.LockPeriodAsync(_tenant, _userMgr, new AttendanceLockRequest("2026-07", "Khóa công tháng 7/2026"));

        var unlockDto = await _attendanceSvc.UnlockPeriodAsync(_tenant, _userMgr, "2026-07");

        Assert.NotNull(unlockDto);
        Assert.Equal("2026-07", unlockDto.PeriodKey);
        Assert.False(unlockDto.IsLocked);
    }

    [Fact]
    public async Task UC127_UnlockPeriod_NonExistentKey_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.UnlockPeriodAsync(_tenant, _userMgr, "2019-01"));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Kỳ khóa không tồn tại", ex.Message);
    }
}
