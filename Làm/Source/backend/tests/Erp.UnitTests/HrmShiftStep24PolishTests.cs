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
/// Unit tests cho Bước 24:
///   UC_HRM_080 — Duyệt thay đổi định biên (Approve / Reject Headcount Plan Changes)
///   UC_HRM_081 — Tạo mẫu ca làm việc (Work Shift Template Creation)
///   UC_HRM_082 — Xếp lịch ca nhân viên (Employee Single Shift Assignment)
///   UC_HRM_083 — Xếp lịch ca theo tuần / tháng (Weekly / Monthly Shift Roster Range Assignment)
/// 16 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmShiftStep24PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmHeadcountService _headcountSvc;
    private readonly HrmShiftService _shiftSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _user       = Guid.NewGuid();
    private readonly Guid _approver   = Guid.NewGuid();
    private readonly Guid _orgUnitId  = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;
    private Guid _empId2;
    private Guid _shiftId1;

    public HrmShiftStep24PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-shift-step24-" + Guid.NewGuid())
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
            Code = "ORG_SHIFT24", Name = "Phòng Xếp Ca 24", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_DEV24", Name = "Kỹ Sư Vận Hành"
        });
        _db.Users.Add(new AppUser
        {
            Id = _user, TenantId = _tenant, Username = "shift_user24", DisplayName = "Phạm Lịch Ca 24"
        });
        _db.Users.Add(new AppUser
        {
            Id = _approver, TenantId = _tenant, Username = "approver24", DisplayName = "Trần Duyệt Ca 24"
        });

        var emp1 = new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP_S24_1", FullName = "Nguyễn Văn Ca 1",
            OrgUnitId = _orgUnitId, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        var emp2 = new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP_S24_2", FullName = "Lê Văn Ca 2",
            OrgUnitId = _orgUnitId, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.AddRange(emp1, emp2);

        var shift1 = new WorkShift
        {
            TenantId = _tenant, Code = "SH_HC8", Name = "Ca Hành Chính 8h",
            StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(17, 0), BreakMinutes = 60,
            IsOvernight = false, IsActive = true
        };
        _db.WorkShifts.Add(shift1);
        _db.SaveChanges();

        _empId1 = emp1.Id;
        _empId2 = emp2.Id;
        _shiftId1 = shift1.Id;

        _headcountSvc = new HrmHeadcountService(_db);
        _shiftSvc = new HrmShiftService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_080: Duyệt thay đổi định biên
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC080_DecideHeadcountPlan_PendingPlan_Approve_UpdatesToApproved()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = await _headcountSvc.UpsertAsync(_tenant, _user,
            new HeadcountPlanUpsertRequest(null, "OrgUnit", _orgUnitId, null, null, 15, from, null, "Gửi duyệt định biên", true));

        Assert.Equal("Pending", plan.Status);

        var approved = await _headcountSvc.DecideAsync(_tenant, _approver, plan.Id, approve: true);

        Assert.Equal("Approved", approved.Status);
        Assert.Equal(_approver, approved.DecidedByUserId);
        Assert.NotNull(approved.DecidedAt);
    }

    [Fact]
    public async Task UC080_DecideHeadcountPlan_PendingPlan_Reject_UpdatesToRejected()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = await _headcountSvc.UpsertAsync(_tenant, _user,
            new HeadcountPlanUpsertRequest(null, "OrgUnit", _orgUnitId, null, null, 15, from, null, "Gửi duyệt định biên", true));

        var rejected = await _headcountSvc.DecideAsync(_tenant, _approver, plan.Id, approve: false);

        Assert.Equal("Rejected", rejected.Status);
    }

    [Fact]
    public async Task UC080_DecideHeadcountPlan_DraftPlan_ThrowsAppException()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);
        var plan = await _headcountSvc.UpsertAsync(_tenant, _user,
            new HeadcountPlanUpsertRequest(null, "OrgUnit", _orgUnitId, null, null, 15, from, null, "Lưu nháp", false));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _headcountSvc.DecideAsync(_tenant, _approver, plan.Id, approve: true));

        Assert.Contains("Pending", ex.Message);
    }

    [Fact]
    public async Task UC080_DecideHeadcountPlan_NonExistentPlan_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _headcountSvc.DecideAsync(_tenant, _approver, Guid.NewGuid(), approve: true));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_081: Tạo mẫu ca làm việc
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC081_UpsertWorkShift_ValidTemplate_CreatesShiftSuccessfully()
    {
        var shift = await _shiftSvc.UpsertTemplateAsync(_tenant, _user,
            new WorkShiftUpsertRequest(null, "SH_NIGHT24", "Ca Đêm 22h-6h", new TimeOnly(22, 0), new TimeOnly(6, 0), 45, true, true, "Ca xoay đêm"));

        Assert.NotNull(shift);
        Assert.Equal("SH_NIGHT24", shift.Code);
        Assert.Equal("Ca Đêm 22h-6h", shift.Name);
        Assert.True(shift.IsOvernight);
        Assert.True(shift.IsActive);
    }

    [Fact]
    public async Task UC081_UpsertWorkShift_DuplicateCode_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.UpsertTemplateAsync(_tenant, _user,
                new WorkShiftUpsertRequest(null, "SH_HC8", "Ca Trùng Mã", new TimeOnly(8, 0), new TimeOnly(17, 0), 60, false, true, null)));

        Assert.Contains("đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC081_UpsertWorkShift_EmptyCode_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.UpsertTemplateAsync(_tenant, _user,
                new WorkShiftUpsertRequest(null, "   ", "Ca Thiếu Mã", new TimeOnly(8, 0), new TimeOnly(17, 0), 60, false, true, null)));

        Assert.Contains("Mã ca", ex.Message);
    }

    [Fact]
    public async Task UC081_UpsertWorkShift_InvalidBreakMinutes_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.UpsertTemplateAsync(_tenant, _user,
                new WorkShiftUpsertRequest(null, "SH_ERR", "Ca Lỗi Nghỉ", new TimeOnly(8, 0), new TimeOnly(17, 0), 700, false, true, null)));

        Assert.Contains("Giờ nghỉ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_082: Xếp lịch ca nhân viên (Single Shift Assignment)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC082_AssignShift_ValidRequest_CreatesAssignmentSuccessfully()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var assign = await _shiftSvc.AssignAsync(_tenant, _user,
            new ShiftAssignRequest(_empId1, _shiftId1, today, "Trực ca chính"));

        Assert.NotNull(assign);
        Assert.Equal(_empId1, assign.EmployeeId);
        Assert.Equal(_shiftId1, assign.WorkShiftId);
        Assert.Equal(today, assign.WorkDate);
        Assert.Equal("Scheduled", assign.Status);
    }

    [Fact]
    public async Task UC082_AssignShift_NonExistentEmployee_ThrowsAppException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.AssignAsync(_tenant, _user,
                new ShiftAssignRequest(Guid.NewGuid(), _shiftId1, today, null)));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC082_AssignShift_InactiveShift_ThrowsAppException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Create inactive shift
        var inactiveShift = await _shiftSvc.UpsertTemplateAsync(_tenant, _user,
            new WorkShiftUpsertRequest(null, "SH_INACTIVE", "Ca Ngừng HĐ", new TimeOnly(8, 0), new TimeOnly(17, 0), 60, false, false, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.AssignAsync(_tenant, _user,
                new ShiftAssignRequest(_empId1, inactiveShift.Id, today, null)));

        Assert.Contains("đã ngưng", ex.Message);
    }

    [Fact]
    public async Task UC082_AssignShift_LockedPeriod_ThrowsAppException()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var periodKey = today.ToString("yyyy-MM");

        // Lock period for current month
        await _shiftSvc.LockPeriodAsync(_tenant, _user,
            new ShiftLockRequest(_orgUnitId, periodKey, "Khóa lịch ca tháng"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.AssignAsync(_tenant, _user,
                new ShiftAssignRequest(_empId1, _shiftId1, today, null)));

        Assert.Contains("đã khóa sổ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_083: Xếp lịch ca theo tuần / tháng (Roster Range Assignment)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC083_AssignRange_Valid7Days_CreatesAssignmentsForMultipleEmployees()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = from.AddDays(6); // 7 days

        var list = await _shiftSvc.AssignRangeAsync(_tenant, _user,
            new ShiftAssignRangeRequest(new[] { _empId1, _empId2 }, _shiftId1, from, to, null, "Xếp ca tuần"));

        Assert.Equal(14, list.Count); // 2 employees * 7 days
    }

    [Fact]
    public async Task UC083_AssignRange_InvalidDateRange_ThrowsAppException()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = from.AddDays(-1); // To < From

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.AssignRangeAsync(_tenant, _user,
                new ShiftAssignRangeRequest(new[] { _empId1 }, _shiftId1, from, to, null, null)));

        Assert.Contains("không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC083_AssignRange_Exceeds62Days_ThrowsAppException()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = from.AddDays(70); // > 62 days

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.AssignRangeAsync(_tenant, _user,
                new ShiftAssignRangeRequest(new[] { _empId1 }, _shiftId1, from, to, null, null)));

        Assert.Contains("tối đa 62 ngày", ex.Message);
    }

    [Fact]
    public async Task UC083_AssignRange_EmptyEmployees_ThrowsAppException()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = from.AddDays(6);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _shiftSvc.AssignRangeAsync(_tenant, _user,
                new ShiftAssignRangeRequest(Array.Empty<Guid>(), _shiftId1, from, to, null, null)));

        Assert.Contains("ít nhất một nhân viên", ex.Message);
    }
}
