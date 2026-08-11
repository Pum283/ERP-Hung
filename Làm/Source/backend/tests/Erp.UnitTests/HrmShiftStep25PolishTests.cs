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
/// Unit tests cho Bước 25:
///   UC_HRM_084 — Đổi ca giữa nhân viên (Shift Swap Between Employees)
///   UC_HRM_085 — Hủy lịch ca (Shift Assignment Cancellation)
///   UC_HRM_086 — Xem lịch ca theo đơn vị (Org-unit Shift Schedule Overview)
///   UC_HRM_087 — Xem lịch ca cá nhân trên APP (Personal Shift Roster on Mobile / Self-service)
/// 16 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmShiftStep25PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmShiftService _shiftSvc;

    private readonly Guid _tenant      = Guid.NewGuid();
    private readonly Guid _user1       = Guid.NewGuid();
    private readonly Guid _user2       = Guid.NewGuid();
    private readonly Guid _userNoEmp   = Guid.NewGuid();
    private readonly Guid _orgUnitId   = Guid.NewGuid();
    private readonly Guid _jobTitleId  = Guid.NewGuid();

    private Guid _empId1;
    private Guid _empId2;
    private Guid _shiftIdA;
    private Guid _shiftIdB;

    public HrmShiftStep25PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-shift-step25-" + Guid.NewGuid())
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
            Id = _orgUnitId, TenantId = _tenant,
            Code = "ORG_SHIFT25", Name = "Phòng Đổi Ca 25", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_OPER25", Name = "Nhân Viên Vận Hành"
        });
        _db.Users.Add(new AppUser
        {
            Id = _user1, TenantId = _tenant, Username = "emp_user25_1", DisplayName = "Nguyễn Văn Đổi Ca 1"
        });
        _db.Users.Add(new AppUser
        {
            Id = _user2, TenantId = _tenant, Username = "emp_user25_2", DisplayName = "Lê Văn Đổi Ca 2"
        });
        _db.Users.Add(new AppUser
        {
            Id = _userNoEmp, TenantId = _tenant, Username = "no_emp_user25", DisplayName = "User Không Hồ Sơ"
        });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _user1, EmployeeCode = "EMP_S25_1", FullName = "Nguyễn Văn Đổi Ca 1",
            OrgUnitId = _orgUnitId, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        var emp2 = new Employee
        {
            TenantId = _tenant, UserId = _user2, EmployeeCode = "EMP_S25_2", FullName = "Lê Văn Đổi Ca 2",
            OrgUnitId = _orgUnitId, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.AddRange(emp1, emp2);

        var shiftA = new WorkShift
        {
            TenantId = _tenant, Code = "SH_MORNING25", Name = "Ca Sáng 8h",
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(16, 0), BreakMinutes = 60,
            IsOvernight = false, IsActive = true
        };
        var shiftB = new WorkShift
        {
            TenantId = _tenant, Code = "SH_AFTERNOON25", Name = "Ca Chiều 14h",
            StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(22, 0), BreakMinutes = 60,
            IsOvernight = false, IsActive = true
        };
        _db.WorkShifts.AddRange(shiftA, shiftB);
        _db.SaveChanges();

        _empId1 = emp1.Id;
        _empId2 = emp2.Id;
        _shiftIdA = shiftA.Id;
        _shiftIdB = shiftB.Id;

        _shiftSvc = new HrmShiftService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_084: Đổi ca giữa nhân viên (Shift Swap)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC084_SwapShifts_ValidSameDateAssignments_SwapsWorkShiftIdsSuccessfully()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var assignA = await _shiftSvc.AssignAsync(_tenant, _user1, new ShiftAssignRequest(_empId1, _shiftIdA, today, "Ca sáng A"));
        var assignB = await _shiftSvc.AssignAsync(_tenant, _user2, new ShiftAssignRequest(_empId2, _shiftIdB, today, "Ca chiều B"));

        await _shiftSvc.SwapAsync(_tenant, _user1, new ShiftSwapRequest(assignA.Id, assignB.Id));

        var updatedA = await _db.ShiftAssignments.FirstAsync(x => x.Id == assignA.Id);
        var updatedB = await _db.ShiftAssignments.FirstAsync(x => x.Id == assignB.Id);

        Assert.Equal(_shiftIdB, updatedA.WorkShiftId);
        Assert.Equal(_shiftIdA, updatedB.WorkShiftId);
    }

    [Fact]
    public async Task UC084_SwapShifts_DifferentDates_ThrowsAppException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tomorrow = today.AddDays(1);

        var assignA = await _shiftSvc.AssignAsync(_tenant, _user1, new ShiftAssignRequest(_empId1, _shiftIdA, today, null));
        var assignB = await _shiftSvc.AssignAsync(_tenant, _user2, new ShiftAssignRequest(_empId2, _shiftIdB, tomorrow, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.SwapAsync(_tenant, _user1, new ShiftSwapRequest(assignA.Id, assignB.Id)));

        Assert.Contains("cùng ngày", ex.Message);
    }

    [Fact]
    public async Task UC084_SwapShifts_CancelledAssignment_ThrowsAppException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var assignA = await _shiftSvc.AssignAsync(_tenant, _user1, new ShiftAssignRequest(_empId1, _shiftIdA, today, null));
        var assignB = await _shiftSvc.AssignAsync(_tenant, _user2, new ShiftAssignRequest(_empId2, _shiftIdB, today, null));
        await _shiftSvc.CancelAsync(_tenant, _user2, assignB.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.SwapAsync(_tenant, _user1, new ShiftSwapRequest(assignA.Id, assignB.Id)));

        Assert.Contains("Scheduled", ex.Message);
    }

    [Fact]
    public async Task UC084_SwapShifts_NonExistentAssignment_ThrowsAppException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var assignA = await _shiftSvc.AssignAsync(_tenant, _user1, new ShiftAssignRequest(_empId1, _shiftIdA, today, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.SwapAsync(_tenant, _user1, new ShiftSwapRequest(assignA.Id, Guid.NewGuid())));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC084_SwapShifts_LockedPeriod_ThrowsAppException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var periodKey = today.ToString("yyyy-MM");

        var assignA = await _shiftSvc.AssignAsync(_tenant, _user1, new ShiftAssignRequest(_empId1, _shiftIdA, today, null));
        var assignB = await _shiftSvc.AssignAsync(_tenant, _user2, new ShiftAssignRequest(_empId2, _shiftIdB, today, null));

        await _shiftSvc.LockPeriodAsync(_tenant, _user1, new ShiftLockRequest(_orgUnitId, periodKey, "Khóa lịch ca"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.SwapAsync(_tenant, _user1, new ShiftSwapRequest(assignA.Id, assignB.Id)));

        Assert.Contains("đã khóa sổ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_085: Hủy lịch ca (Shift Assignment Cancellation)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC085_CancelShift_ValidAssignment_UpdatesStatusToCancelled()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var assign = await _shiftSvc.AssignAsync(_tenant, _user1, new ShiftAssignRequest(_empId1, _shiftIdA, today, "Lịch ca mẫu"));

        await _shiftSvc.CancelAsync(_tenant, _user1, assign.Id);

        var dbItem = await _db.ShiftAssignments.FirstAsync(x => x.Id == assign.Id);
        Assert.Equal("Cancelled", dbItem.Status);
    }

    [Fact]
    public async Task UC085_CancelShift_NonExistentAssignment_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.CancelAsync(_tenant, _user1, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC085_CancelShift_LockedPeriod_ThrowsAppException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var periodKey = today.ToString("yyyy-MM");

        var assign = await _shiftSvc.AssignAsync(_tenant, _user1, new ShiftAssignRequest(_empId1, _shiftIdA, today, null));
        await _shiftSvc.LockPeriodAsync(_tenant, _user1, new ShiftLockRequest(_orgUnitId, periodKey, "Khóa sổ"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.CancelAsync(_tenant, _user1, assign.Id));

        Assert.Contains("đã khóa sổ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_086: Xem lịch ca theo đơn vị (Org-unit Shift Schedule Overview)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC086_ListAssignments_OrgUnitFilter_ReturnsAssignmentsInDateRange()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await _shiftSvc.AssignAsync(_tenant, _user1, new ShiftAssignRequest(_empId1, _shiftIdA, today, null));
        await _shiftSvc.AssignAsync(_tenant, _user2, new ShiftAssignRequest(_empId2, _shiftIdB, today, null));

        var list = await _shiftSvc.ListAssignmentsAsync(_tenant, _orgUnitId, null, today, today);

        Assert.Equal(2, list.Count);
        Assert.All(list, x => Assert.Equal(_orgUnitId, x.OrgUnitId));
    }

    [Fact]
    public async Task UC086_ListAssignments_EmployeeFilter_ReturnsOnlyTargetEmployee()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await _shiftSvc.AssignAsync(_tenant, _user1, new ShiftAssignRequest(_empId1, _shiftIdA, today, null));
        await _shiftSvc.AssignAsync(_tenant, _user2, new ShiftAssignRequest(_empId2, _shiftIdB, today, null));

        var list = await _shiftSvc.ListAssignmentsAsync(_tenant, null, _empId1, today, today);

        Assert.Single(list);
        Assert.Equal(_empId1, list[0].EmployeeId);
    }

    [Fact]
    public async Task UC086_ListAssignments_DateRangeFilter_ExcludesOutsideDates()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);
        var tomorrow = today.AddDays(1);

        await _shiftSvc.AssignAsync(_tenant, _user1, new ShiftAssignRequest(_empId1, _shiftIdA, yesterday, null));
        await _shiftSvc.AssignAsync(_tenant, _user1, new ShiftAssignRequest(_empId1, _shiftIdA, today, null));
        await _shiftSvc.AssignAsync(_tenant, _user1, new ShiftAssignRequest(_empId1, _shiftIdA, tomorrow, null));

        var list = await _shiftSvc.ListAssignmentsAsync(_tenant, null, _empId1, today, today);

        Assert.Single(list);
        Assert.Equal(today, list[0].WorkDate);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_087: Xem lịch ca cá nhân trên APP (Personal Shift Roster)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC087_MyAssignments_ValidUserWithEmployee_ReturnsPersonalRoster()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await _shiftSvc.AssignAsync(_tenant, _user1, new ShiftAssignRequest(_empId1, _shiftIdA, today, "Lịch ca cá nhân"));

        var myRoster = await _shiftSvc.MyAssignmentsAsync(_tenant, _user1, today, today);

        Assert.Single(myRoster);
        Assert.Equal(_empId1, myRoster[0].EmployeeId);
        Assert.Equal("Nguyễn Văn Đổi Ca 1", myRoster[0].EmployeeName);
    }

    [Fact]
    public async Task UC087_MyAssignments_UserWithNoEmployeeProfile_ThrowsAppException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.MyAssignmentsAsync(_tenant, _userNoEmp, today, today));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("hồ sơ nhân viên", ex.Message);
    }
}
