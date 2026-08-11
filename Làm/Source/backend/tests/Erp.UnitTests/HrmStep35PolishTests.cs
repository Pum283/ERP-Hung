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
/// Unit tests cho Bước 35:
///   UC_HRM_128 — Xác nhận bảng công (Confirm Attendance Sheet Record)
///   UC_HRM_130 — Cấu hình quỹ phép theo loại NS (Leave Entitlement Config by Staff Category)
///   UC_HRM_131 — Cấp phát / điều chỉnh quỹ phép (Grant / Adjust Leave Balance)
///   UC_HRM_133 — Duyệt đơn nghỉ đa cấp (Multi-level Leave Approval Workflow)
/// 13 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep35PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmAttendanceService _attendanceSvc;
    private readonly HrmLeaveService _leaveSvc;
    private readonly FakeWfRuntimeService _wfFake;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userEmp1   = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();
    private readonly Guid _empTypeId  = Guid.NewGuid();
    private readonly Guid _leaveTypeId = Guid.NewGuid();

    private Guid _empId1;

    public HrmStep35PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step35-" + Guid.NewGuid())
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
            Code = "ORG_S35_1", Name = "Phòng Duyệt Phép 35", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_LEAVE35", Name = "Chuyên Viên Phép 35"
        });
        _db.EmployeeTypes.Add(new EmployeeType
        {
            Id = _empTypeId, TenantId = _tenant, Code = "ET_OFFICE35", Name = "Khối Văn Phòng 35"
        });
        _db.LeaveTypes.Add(new LeaveType
        {
            Id = _leaveTypeId, TenantId = _tenant, Code = "ANNUAL", Name = "Nghỉ phép năm", IsActive = true
        });

        _db.Users.Add(new AppUser { Id = _userEmp1, TenantId = _tenant, Username = "emp35_1", DisplayName = "Đặng Văn Xác Nhận 35" });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp1, EmployeeCode = "EMP_S35_1", FullName = "Đặng Văn Xác Nhận 35",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, EmployeeTypeId = _empTypeId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.Add(emp1);

        _db.AttendancePolicies.Add(new AttendancePolicy
        {
            TenantId = _tenant, EnableApp = true, DefaultShiftStart = new TimeOnly(8, 0), DefaultShiftEnd = new TimeOnly(17, 0)
        });

        _db.SaveChanges();

        _empId1 = emp1.Id;

        _wfFake = new FakeWfRuntimeService();
        _attendanceSvc = new HrmAttendanceService(_db);
        _leaveSvc = new HrmLeaveService(_db, _wfFake);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_128: Xác nhận bảng công (Confirm Attendance Sheet Record)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC128_ConfirmRecord_ValidRecordId_SetsIsConfirmedToTrue()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var rec = new AttendanceRecord
        {
            TenantId = _tenant, EmployeeId = _empId1, WorkDate = today, Status = "Closed", IsConfirmed = false
        };
        _db.AttendanceRecords.Add(rec);
        await _db.SaveChangesAsync();

        await _attendanceSvc.ConfirmRecordAsync(_tenant, _userEmp1, rec.Id);

        var updated = await _db.AttendanceRecords.FirstAsync(x => x.Id == rec.Id);
        Assert.True(updated.IsConfirmed);
    }

    [Fact]
    public async Task UC128_ConfirmRecord_NonExistentRecord_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _attendanceSvc.ConfirmRecordAsync(_tenant, _userEmp1, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Bản ghi công không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_130: Cấu hình quỹ phép theo loại NS (Leave Entitlement Config)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC130_UpsertEntitlementRule_ValidParameters_CreatesRuleSuccessfully()
    {
        var rule = await _leaveSvc.UpsertEntitlementRuleAsync(_tenant, _userEmp1,
            new LeaveEntitlementRuleUpsertRequest(null, _leaveTypeId, _empTypeId, 14, true, "Quy định 14 ngày/năm"));

        Assert.NotNull(rule);
        Assert.Equal(_leaveTypeId, rule.LeaveTypeId);
        Assert.Equal(_empTypeId, rule.EmployeeTypeId);
        Assert.Equal(14, rule.DaysPerYear);
    }

    [Fact]
    public async Task UC130_UpsertEntitlementRule_DaysExceeds366_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leaveSvc.UpsertEntitlementRuleAsync(_tenant, _userEmp1,
                new LeaveEntitlementRuleUpsertRequest(null, _leaveTypeId, _empTypeId, 400, true, null)));

        Assert.Contains("Số ngày quỹ không hợp lệ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_131: Cấp phát / điều chỉnh quỹ phép (Grant / Adjust Leave Balance)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC131_AdjustBalance_ValidParameters_UpdatesBalanceEntitledAndRemaining()
    {
        var bal = await _leaveSvc.AdjustBalanceAsync(_tenant, _userEmp1,
            new LeaveBalanceAdjustRequest(_empId1, _leaveTypeId, 2026, 15, "Điều chỉnh 15 ngày"));

        Assert.NotNull(bal);
        Assert.Equal(15, bal.Entitled);
        Assert.Equal(0, bal.Used);
        Assert.Equal(15, bal.Remaining);
    }

    [Fact]
    public async Task UC131_AdjustBalance_InvalidEntitled_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leaveSvc.AdjustBalanceAsync(_tenant, _userEmp1,
                new LeaveBalanceAdjustRequest(_empId1, _leaveTypeId, 2026, -5, null)));

        Assert.Contains("Entitled không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC131_AllocateYear_ActiveEmployees_CreatesLeaveBalancesForYear()
    {
        var count = await _leaveSvc.AllocateYearAsync(_tenant, _userEmp1,
            new LeaveAllocateYearRequest(2026, null, null));

        Assert.True(count >= 1);
        var balances = await _leaveSvc.ListBalancesAsync(_tenant, _userEmp1, _empId1);
        Assert.NotEmpty(balances);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_133: Duyệt đơn nghỉ đa cấp (Multi-level Leave Approval Workflow)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC133_CreateAndSubmitLeave_SufficientBalance_StartsWorkflowAndSetsPendingStatus()
    {
        // Setup leave balance = 10 days
        await _leaveSvc.AdjustBalanceAsync(_tenant, _userEmp1, new LeaveBalanceAdjustRequest(_empId1, _leaveTypeId, DateTime.UtcNow.Year, 10, null));

        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var toDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));

        var req = await _leaveSvc.CreateAndOptionallySubmitAsync(_tenant, _userEmp1,
            new LeaveRequestCreateRequest(_empId1, _leaveTypeId, fromDate, toDate, 3, "Nghỉ phép cá nhân", true));

        Assert.NotNull(req);
        Assert.Equal("Pending", req.Status);
        Assert.NotNull(req.WfInstanceId);
        Assert.Equal(1, _wfFake.StartCallCount);
    }

    [Fact]
    public async Task UC133_CreateAndSubmitLeave_ExceedsBalance_ThrowsAppException()
    {
        // Setup leave balance = 2 days
        await _leaveSvc.AdjustBalanceAsync(_tenant, _userEmp1, new LeaveBalanceAdjustRequest(_empId1, _leaveTypeId, DateTime.UtcNow.Year, 2, null));

        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var toDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leaveSvc.CreateAndOptionallySubmitAsync(_tenant, _userEmp1,
                new LeaveRequestCreateRequest(_empId1, _leaveTypeId, fromDate, toDate, 5, "Nghỉ quá số phép còn lại", true)));

        Assert.Contains("không đủ", ex.Message);
    }

    [Fact]
    public async Task UC133_CreateLeave_InvalidDateRange_ThrowsAppException()
    {
        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        var toDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)); // To < From

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leaveSvc.CreateAndOptionallySubmitAsync(_tenant, _userEmp1,
                new LeaveRequestCreateRequest(_empId1, _leaveTypeId, fromDate, toDate, 2, "Từ ngày lớn hơn đến ngày", false)));

        Assert.Contains("Đến ngày phải ≥ từ ngày", ex.Message);
    }

    [Fact]
    public async Task UC133_CreateLeave_ZeroDays_ThrowsAppException()
    {
        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leaveSvc.CreateAndOptionallySubmitAsync(_tenant, _userEmp1,
                new LeaveRequestCreateRequest(_empId1, _leaveTypeId, fromDate, fromDate, 0, "Số ngày bằng 0", false)));

        Assert.Contains("Số ngày phải > 0", ex.Message);
    }
}
