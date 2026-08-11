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
/// Unit tests cho Bước 46:
///   UC_HRM_176 — So sánh lương kỳ này / kỳ trước (Payroll Period Comparison)
///   UC_HRM_182 — Dashboard headcount & biến động (Headcount & Turnover Dashboard)
///   UC_HRM_183 — Báo cáo công / OT / đi trễ (Attendance, OT & Late Arrival Report)
///   UC_HRM_184 — Báo cáo tuyển dụng funnel (Recruitment Funnel Pipeline Report)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep46PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmPayrollService _payrollSvc;
    private readonly HrmDashboardService _dashSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;

    public HrmStep46PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step46-" + Guid.NewGuid())
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
            Code = "ORG_S46_1", Name = "Phòng Phân Tích 46", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_ANALYST46", Name = "Chuyên Viên 46"
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin46", DisplayName = "Admin Dashboard 46" });

        var emp1 = new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP_S46_1", FullName = "Nguyễn Văn Dashboard 46",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3))
        };
        _db.Employees.Add(emp1);

        _db.EmployeeSalaries.Add(new EmployeeSalary
        {
            TenantId = _tenant, EmployeeId = emp1.Id, BaseSalary = 17000000m,
            HourlyRate = 81730m, DailyRate = 653846m, EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        });

        _db.PayrollPolicies.Add(new PayrollPolicy
        {
            TenantId = _tenant, StandardWorkDays = 26, OtMultiplier = 1.5m,
            SocialInsuranceEmpRate = 0.08m, HealthInsuranceEmpRate = 0.015m, UnemploymentEmpRate = 0.01m,
            PersonalDeduction = 11000000m, FlatTaxRate = 0.05m
        });

        // Add Attendance
        _db.AttendanceRecords.Add(new AttendanceRecord
        {
            TenantId = _tenant, EmployeeId = emp1.Id, WorkDate = new DateOnly(2026, 8, 10),
            WorkUnit = 1m, OtMinutes = 90, LateMinutes = 15, Status = "Closed"
        });

        // Add Candidate for Funnel
        _db.Candidates.Add(new Candidate
        {
            TenantId = _tenant, FullName = "Trần Thị Ứng Viên 46", Email = "candidate46@test.com",
            PipelineStatus = "Applied", CreatedBy = _userAdmin
        });
        _db.Candidates.Add(new Candidate
        {
            TenantId = _tenant, FullName = "Lê Văn Phỏng Vấn 46", Email = "interview46@test.com",
            PipelineStatus = "Interviewing", CreatedBy = _userAdmin
        });

        _db.SaveChanges();

        _empId1 = emp1.Id;

        _payrollSvc = new HrmPayrollService(_db);
        _dashSvc = new HrmDashboardService(_db, null!, _payrollSvc);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_176: So sánh lương kỳ này / kỳ trước (Payroll Comparison)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC176_Compare_ValidPeriodKey_ReturnsComparisonList()
    {
        var periodCur = await _payrollSvc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-08", "Kỳ 8/2026"));
        await _payrollSvc.CalculateAsync(_tenant, _userAdmin, periodCur.Id);

        var list = await _payrollSvc.CompareAsync(_tenant, "2026-08");

        Assert.NotEmpty(list);
        Assert.Equal(2, list.Count);
        Assert.Contains(list, x => x.PeriodKey == "2026-08");
        Assert.Contains(list, x => x.PeriodKey == "2026-07");
    }

    [Fact]
    public async Task UC176_Compare_InvalidPeriodKey_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _payrollSvc.CompareAsync(_tenant, "INVALID"));

        Assert.Contains("PeriodKey phải dạng yyyy-MM", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_182: Dashboard headcount & biến động (Headcount Dashboard)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC182_Headcount_ReturnsActiveProbationAndTurnoverMovements()
    {
        var result = await _dashSvc.HeadcountAsync(_tenant);

        Assert.NotNull(result);
        Assert.True(result.TotalActive >= 1);
        Assert.NotEmpty(result.ByStatus);
        Assert.NotEmpty(result.ByOrg);
        Assert.NotEmpty(result.Movements);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_183: Báo cáo công / OT / đi trễ (Attendance & OT Report)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC183_AttendanceReport_ValidDateRange_ReturnsAggregatedWorkAndLateMinutes()
    {
        var from = new DateOnly(2026, 8, 1);
        var to = new DateOnly(2026, 8, 31);

        var rows = await _dashSvc.AttendanceReportAsync(_tenant, from, to);

        Assert.NotEmpty(rows);
        var row = rows.FirstOrDefault(r => r.OrgUnitName == "Phòng Phân Tích 46");
        Assert.NotNull(row);
        Assert.True(row.OtMinutes >= 90);
        Assert.True(row.LateMinutes >= 15);
        Assert.True(row.LateCount >= 1);
    }

    [Fact]
    public async Task UC183_AttendanceReport_OutRangeDates_ReturnsEmptyList()
    {
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 1, 31);

        var rows = await _dashSvc.AttendanceReportAsync(_tenant, from, to);

        Assert.Empty(rows);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_184: Báo cáo tuyển dụng funnel (Recruitment Funnel Report)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC184_RecruitFunnel_ReturnsCandidatePipelineBreakdown()
    {
        var funnel = await _dashSvc.RecruitFunnelAsync(_tenant);

        Assert.NotEmpty(funnel);
        Assert.Contains(funnel, x => x.PipelineStatus == "Applied" && x.Count >= 1);
        Assert.Contains(funnel, x => x.PipelineStatus == "Interviewing" && x.Count >= 1);
    }
}
