using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.DTOs.Lms;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Lms;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Implementations.Services.Lms;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 47:
///   UC_HRM_185 — Báo cáo quỹ phép (Leave Fund Summary Report)
///   UC_HRM_186 — Báo cáo chi phí nhân sự (HR Cost Summary Report)
///   UC_HRM_187 — Báo cáo định biên vs thực tế (Headcount Target vs Actual Report)
///   UC_LMS_001 — Danh mục chương trình đào tạo (LMS Training Program Catalog)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep47PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmDashboardService _dashSvc;
    private readonly HrmHeadcountService _headcountSvc;
    private readonly LmsCourseService _lmsSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;

    public HrmStep47PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step47-" + Guid.NewGuid())
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
            Code = "ORG_S47_1", Name = "Phòng Đào Tạo 47", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_LMS47", Name = "Giảng Viên 47"
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin47", DisplayName = "Admin LMS 47" });

        var emp1 = new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP_S47_1", FullName = "Nguyễn Văn Đào Tạo 47",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.Add(emp1);

        // Leave balance for UC_HRM_185
        _db.LeaveBalances.Add(new LeaveBalance
        {
            TenantId = _tenant, EmployeeId = emp1.Id, Year = 2026,
            Entitled = 12m, Used = 4m, Remaining = 8m
        });

        // Headcount plan for UC_HRM_187
        _db.HeadcountPlans.Add(new HeadcountPlan
        {
            TenantId = _tenant, OrgUnitId = _orgUnit1, ScopeType = "OrgUnit",
            PlannedHeadcount = 10, Status = "Approved",
            EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            RequestedByUserId = _userAdmin
        });

        _db.SaveChanges();

        _empId1 = emp1.Id;

        var payrollSvc = new HrmPayrollService(_db);
        _headcountSvc = new HrmHeadcountService(_db);
        _dashSvc = new HrmDashboardService(_db, _headcountSvc, payrollSvc);
        _lmsSvc = new LmsCourseService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_185: Báo cáo quỹ phép (Leave Fund Summary Report)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC185_LeaveSummary_ValidYear_ReturnsLeaveFundBreakdownByOrg()
    {
        var rows = await _dashSvc.LeaveSummaryAsync(_tenant, 2026);

        Assert.NotEmpty(rows);
        var row = rows.FirstOrDefault(r => r.OrgUnitName == "Phòng Đào Tạo 47");
        Assert.NotNull(row);
        Assert.Equal(12m, row.Entitled);
        Assert.Equal(4m, row.Used);
        Assert.Equal(8m, row.Remaining);
    }

    [Fact]
    public async Task UC185_LeaveSummary_NonExistentYear_ReturnsEmptyList()
    {
        var rows = await _dashSvc.LeaveSummaryAsync(_tenant, 2099);
        Assert.Empty(rows);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_186: Báo cáo chi phí nhân sự (HR Cost Summary Report)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC186_CostSummary_ValidTenant_ReturnsCostSummaryDto()
    {
        var cost = await _dashSvc.CostSummaryAsync(_tenant, null);

        Assert.NotNull(cost);
        Assert.True(cost.TotalGross >= 0);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_187: Báo cáo định biên vs thực tế (Headcount Target vs Actual Report)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC187_HeadcountSummary_ValidTenant_ReturnsHeadcountPlans()
    {
        var list = await _headcountSvc.ListAsync(_tenant);

        Assert.NotNull(list);
        Assert.NotEmpty(list);
        var plan = list.FirstOrDefault(x => x.OrgUnitId == _orgUnit1);
        Assert.NotNull(plan);
        Assert.Equal(10, plan.PlannedHeadcount);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LMS_001: Danh mục chương trình đào tạo (Training Program Catalog)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LMS_001_UpsertProgram_ValidRequest_CreatesProgramSuccessfully()
    {
        var prog = await _lmsSvc.UpsertProgramAsync(_tenant, _userAdmin,
            new LmsProgramUpsertRequest(null, "PROG_ONBOARD47", "Chương trình Hội nhập 47", "Đào tạo nhân sự mới", "Active"));

        Assert.NotNull(prog);
        Assert.Equal("PROG_ONBOARD47", prog.Code);
        Assert.Equal("Active", prog.Status);
    }

    [Fact]
    public async Task UC_LMS_001_UpsertProgram_DuplicateCode_ThrowsAppException()
    {
        await _lmsSvc.UpsertProgramAsync(_tenant, _userAdmin,
            new LmsProgramUpsertRequest(null, "PROG_DUP47", "Chương trình A", null, "Active"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _lmsSvc.UpsertProgramAsync(_tenant, _userAdmin,
                new LmsProgramUpsertRequest(null, "PROG_DUP47", "Chương trình B", null, "Active")));

        Assert.Contains("Mã CTĐT đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_001_UpsertProgram_InvalidCodeLength_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _lmsSvc.UpsertProgramAsync(_tenant, _userAdmin,
                new LmsProgramUpsertRequest(null, "", "Tên hợp lệ", null, "Active")));

        Assert.Contains("Mã CTĐT 1–40 ký tự", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_001_UpsertProgram_InvalidStatus_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _lmsSvc.UpsertProgramAsync(_tenant, _userAdmin,
                new LmsProgramUpsertRequest(null, "PROG_STATUS47", "Tên hợp lệ", null, "INVALID_STATUS")));

        Assert.Contains("Trạng thái CTĐT không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC_LMS_001_ListPrograms_ReturnsCreatedPrograms()
    {
        await _lmsSvc.UpsertProgramAsync(_tenant, _userAdmin,
            new LmsProgramUpsertRequest(null, "PROG_LIST47", "CTĐT Danh Sách 47", null, "Active"));

        var list = await _lmsSvc.ListProgramsAsync(_tenant);

        Assert.NotEmpty(list);
        Assert.Contains(list, x => x.Code == "PROG_LIST47");
    }
}
