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
/// Unit tests cho Bước 23:
///   UC_HRM_076 — Khai báo định biên theo ca (Shift-based Headcount Plan)
///   UC_HRM_077 — Khai báo định biên theo bộ phận (Department-based Headcount Plan)
///   UC_HRM_078 — So sánh thực tế vs định biên (Headcount Plan vs Actual Comparison)
///   UC_HRM_079 — Cảnh báo thiếu người (Headcount Shortage Alert)
/// 16 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmHeadcountStep23PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmHeadcountService _headcountSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _user       = Guid.NewGuid();
    private readonly Guid _approver   = Guid.NewGuid();
    private readonly Guid _orgUnitId  = Guid.NewGuid();
    private readonly Guid _deptId     = Guid.NewGuid();

    public HrmHeadcountStep23PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-headcount-step23-" + Guid.NewGuid())
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
            Code = "ORG_HC23", Name = "Phòng Định Biên 23", UnitType = "Department", Path = "/1"
        });
        _db.Departments.Add(new Department
        {
            Id = _deptId, TenantId = _tenant, OrgUnitId = _orgUnitId,
            Code = "DEPT_DEV23", Name = "Bộ Phận Phát Triển Phần Mềm"
        });
        _db.Users.Add(new AppUser
        {
            Id = _user, TenantId = _tenant, Username = "hc_user23", DisplayName = "Phạm Định Biên 23"
        });
        _db.Users.Add(new AppUser
        {
            Id = _approver, TenantId = _tenant, Username = "approver23", DisplayName = "Trần Trưởng Phòng 23"
        });
        _db.SaveChanges();

        _headcountSvc = new HrmHeadcountService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_076: Khai báo định biên theo ca (Shift-based Headcount Plan)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC076_UpsertShiftHeadcount_ValidShift_CreatesPlanSuccessfully()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);

        var plan = await _headcountSvc.UpsertAsync(_tenant, _user,
            new HeadcountPlanUpsertRequest(null, "Shift", _orgUnitId, null, "SH_MORNING", 8, from, null, "Định biên ca sáng", false));

        Assert.NotNull(plan);
        Assert.Equal("Shift", plan.ScopeType);
        Assert.Equal("SH_MORNING", plan.ShiftCode);
        Assert.Equal(8, plan.PlannedHeadcount);
        Assert.Equal("Draft", plan.Status);
    }

    [Fact]
    public async Task UC076_UpsertShiftHeadcount_EmptyShiftCode_ThrowsAppException()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _headcountSvc.UpsertAsync(_tenant, _user,
                new HeadcountPlanUpsertRequest(null, "Shift", _orgUnitId, null, "   ", 8, from, null, null, false)));

        Assert.Contains("mã ca", ex.Message);
    }

    [Fact]
    public async Task UC076_UpsertShiftHeadcount_ShiftCodeTooLong_ThrowsAppException()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _headcountSvc.UpsertAsync(_tenant, _user,
                new HeadcountPlanUpsertRequest(null, "Shift", _orgUnitId, null, new string('S', 41), 8, from, null, null, false)));

        Assert.Contains("40 ký tự", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_077: Khai báo định biên theo bộ phận (Department-based Headcount Plan)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC077_UpsertDepartmentHeadcount_ValidDept_CreatesPlanSuccessfully()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);

        var plan = await _headcountSvc.UpsertAsync(_tenant, _user,
            new HeadcountPlanUpsertRequest(null, "Department", _orgUnitId, _deptId, null, 12, from, null, "Định biên dev", false));

        Assert.NotNull(plan);
        Assert.Equal("Department", plan.ScopeType);
        Assert.Equal(_deptId, plan.DepartmentId);
        Assert.Equal("Bộ Phận Phát Triển Phần Mềm", plan.DepartmentName);
        Assert.Equal(12, plan.PlannedHeadcount);
    }

    [Fact]
    public async Task UC077_UpsertDepartmentHeadcount_MissingDepartmentId_ThrowsAppException()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _headcountSvc.UpsertAsync(_tenant, _user,
                new HeadcountPlanUpsertRequest(null, "Department", _orgUnitId, null, null, 12, from, null, null, false)));

        Assert.Contains("DepartmentId", ex.Message);
    }

    [Fact]
    public async Task UC077_UpsertDepartmentHeadcount_DepartmentNotBelongToOrg_ThrowsAppException()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);
        var otherOrgId = Guid.NewGuid();
        _db.OrgUnits.Add(new OrgUnit { Id = otherOrgId, TenantId = _tenant, Code = "ORG_OTHER23", Name = "Org Khác" });
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _headcountSvc.UpsertAsync(_tenant, _user,
                new HeadcountPlanUpsertRequest(null, "Department", otherOrgId, _deptId, null, 12, from, null, null, false)));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_078: So sánh thực tế vs định biên (Headcount Compare)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC078_CompareHeadcount_ApprovedPlan_CalculatesActualAndGapCorrectly()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);

        // Create approved plan for Department with planned = 10
        var plan = await _headcountSvc.UpsertAsync(_tenant, _user,
            new HeadcountPlanUpsertRequest(null, "Department", _orgUnitId, _deptId, null, 10, from, null, "Định biên dev", true));
        await _headcountSvc.DecideAsync(_tenant, _approver, plan.Id, approve: true);

        // Add 3 active employees in this department
        for (int i = 1; i <= 3; i++)
        {
            _db.Employees.Add(new Employee
            {
                TenantId = _tenant, EmployeeCode = $"EMP_DEPT_{i}", FullName = $"NV Dept {i}",
                OrgUnitId = _orgUnitId, DepartmentId = _deptId, Status = "Active",
                HireDate = from
            });
        }
        await _db.SaveChangesAsync();

        var result = await _headcountSvc.CompareAsync(_tenant);

        Assert.NotEmpty(result);
        var row = result.FirstOrDefault(x => x.DepartmentId == _deptId);
        Assert.NotNull(row);
        Assert.Equal(10, row.Planned);
        Assert.Equal(3, row.Actual);
        Assert.Equal(7, row.Gap); // 10 - 3 = 7 thiếu
        Assert.True(row.Shortage);
    }

    [Fact]
    public async Task UC078_CompareHeadcount_ExcludesDraftOrPendingPlans()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);

        // Plan 1: Draft
        await _headcountSvc.UpsertAsync(_tenant, _user,
            new HeadcountPlanUpsertRequest(null, "OrgUnit", _orgUnitId, null, null, 10, from, null, "Draft plan", false));

        var result = await _headcountSvc.CompareAsync(_tenant);

        Assert.Empty(result);
    }

    [Fact]
    public async Task UC078_CompareHeadcount_ExcludesExpiredPlans()
    {
        var pastFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
        var pastTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        // Plan: Approved but expired
        var plan = await _headcountSvc.UpsertAsync(_tenant, _user,
            new HeadcountPlanUpsertRequest(null, "OrgUnit", _orgUnitId, null, null, 10, pastFrom, pastTo, "Expired plan", true));
        await _headcountSvc.DecideAsync(_tenant, _approver, plan.Id, approve: true);

        var result = await _headcountSvc.CompareAsync(_tenant);

        Assert.Empty(result);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_079: Cảnh báo thiếu người (Headcount Shortages Alert)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC079_Shortages_OnlyReturnsRowsWithPositiveGap()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);

        // Plan 1: Shortage (Planned = 10, Actual = 2 -> Gap = 8)
        var p1 = await _headcountSvc.UpsertAsync(_tenant, _user,
            new HeadcountPlanUpsertRequest(null, "Department", _orgUnitId, _deptId, null, 10, from, null, "Plan shortage", true));
        await _headcountSvc.DecideAsync(_tenant, _approver, p1.Id, approve: true);

        _db.Employees.Add(new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP_S1", FullName = "NV Shortage 1",
            OrgUnitId = _orgUnitId, DepartmentId = _deptId, Status = "Active", HireDate = from
        });
        _db.Employees.Add(new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP_S2", FullName = "NV Shortage 2",
            OrgUnitId = _orgUnitId, DepartmentId = _deptId, Status = "Active", HireDate = from
        });
        await _db.SaveChangesAsync();

        var shortages = await _headcountSvc.ShortagesAsync(_tenant);

        Assert.Single(shortages);
        Assert.Equal(8, shortages[0].Gap);
        Assert.True(shortages[0].Shortage);
    }

    [Fact]
    public async Task UC079_Shortages_ReturnsEmptyWhenSufficientStaffed()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow);

        // Plan: Planned = 2, Actual = 2 -> Gap = 0
        var p = await _headcountSvc.UpsertAsync(_tenant, _user,
            new HeadcountPlanUpsertRequest(null, "Department", _orgUnitId, _deptId, null, 2, from, null, "Plan sufficient", true));
        await _headcountSvc.DecideAsync(_tenant, _approver, p.Id, approve: true);

        _db.Employees.Add(new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP_F1", FullName = "NV Full 1",
            OrgUnitId = _orgUnitId, DepartmentId = _deptId, Status = "Active", HireDate = from
        });
        _db.Employees.Add(new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP_F2", FullName = "NV Full 2",
            OrgUnitId = _orgUnitId, DepartmentId = _deptId, Status = "Active", HireDate = from
        });
        await _db.SaveChangesAsync();

        var shortages = await _headcountSvc.ShortagesAsync(_tenant);

        Assert.Empty(shortages);
    }
}
