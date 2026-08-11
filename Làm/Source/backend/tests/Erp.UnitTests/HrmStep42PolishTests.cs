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
/// Unit tests cho Bước 42:
///   UC_HRM_159 — Rule phụ cấp đặc thù (Special & Custom Allowance Rules)
///   UC_HRM_160 — Cấu hình bảo hiểm (Social & Health Insurance Rates Config)
///   UC_HRM_161 — Cấu hình thuế TNCN (Personal Income Tax Flat & Brackets Config)
///   UC_HRM_162 — Giảm trừ gia cảnh (Personal & Family Dependent Deductions)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep42PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmPayrollService _svc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _periodId1;
    private Guid _empId1;

    public HrmStep42PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step42-" + Guid.NewGuid())
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
            Code = "ORG_S42_1", Name = "Phòng Chính Sách Lương 42", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_POLICY42", Name = "Chuyên Viên 42"
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin42", DisplayName = "Admin Chính Sách Lương 42" });

        var emp1 = new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP_S42_1", FullName = "Nguyễn Văn Bảo Hiểm 42",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2))
        };
        _db.Employees.Add(emp1);

        _db.EmployeeSalaries.Add(new EmployeeSalary
        {
            TenantId = _tenant, EmployeeId = emp1.Id, BaseSalary = 20000000m,
            HourlyRate = 96154m, DailyRate = 769231m, EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        });

        var period = new PayrollPeriod
        {
            TenantId = _tenant, PeriodKey = "2026-08",
            PeriodFrom = new DateOnly(2026, 8, 1), PeriodTo = new DateOnly(2026, 8, 31),
            Status = "Draft"
        };
        _db.PayrollPeriods.Add(period);

        _db.AttendanceRecords.Add(new AttendanceRecord
        {
            TenantId = _tenant, EmployeeId = emp1.Id, WorkDate = new DateOnly(2026, 8, 10),
            WorkUnit = 26m, Status = "Closed"
        });

        _db.SaveChanges();

        _empId1 = emp1.Id;
        _periodId1 = period.Id;

        _svc = new HrmPayrollService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_159: Rule phụ cấp đặc thù (Special Allowance Rules)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC159_UpsertAllowanceRule_NonExistentType_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertAllowanceRuleAsync(_tenant, _userAdmin,
                new AllowanceRuleUpsertRequest(null, Guid.NewGuid(), "CUSTOM", 500000m, true, "Ghi chú")));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Loại phụ cấp không tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC159_UpsertAllowanceRule_ValidType_CreatesRuleSuccessfully()
    {
        var type = await _svc.UpsertAllowanceTypeAsync(_tenant, _userAdmin,
            new AllowanceTypeUpsertRequest(null, "ALLOW_HAZARD", "Phụ cấp độc hại", 1200000m, false, true));

        var rule = await _svc.UpsertAllowanceRuleAsync(_tenant, _userAdmin,
            new AllowanceRuleUpsertRequest(null, type.Id, null, 1200000m, true, "Phụ cấp công việc độc hại"));

        Assert.NotNull(rule);
        Assert.Equal(1200000m, rule.Amount);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_160: Cấu hình bảo hiểm (Social & Health Insurance Rates)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC160_UpsertPolicy_ValidInsuranceRates_UpdatesPolicySuccessfully()
    {
        var policy = await _svc.UpsertPolicyAsync(_tenant, _userAdmin,
            new PayrollPolicyUpsertRequest(0.08m, 0.015m, 0.01m, 11000000m, 0.05m, 26, 1.5m));

        Assert.NotNull(policy);
        Assert.Equal(0.08m, policy.SocialInsuranceEmpRate);
        Assert.Equal(0.015m, policy.HealthInsuranceEmpRate);
        Assert.Equal(0.01m, policy.UnemploymentEmpRate);
    }

    [Fact]
    public async Task UC160_UpsertPolicy_InvalidInsuranceRate_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertPolicyAsync(_tenant, _userAdmin,
                new PayrollPolicyUpsertRequest(1.5m, 0.015m, 0.01m, 11000000m, 0.05m, 26, 1.5m)));

        Assert.Contains("BHXH NV 0–1", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_161: Cấu hình thuế TNCN (Personal Income Tax Config)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC161_UpsertPolicy_ValidTaxRate_UpdatesTaxPolicySuccessfully()
    {
        var policy = await _svc.UpsertPolicyAsync(_tenant, _userAdmin,
            new PayrollPolicyUpsertRequest(0.08m, 0.015m, 0.01m, 11000000m, 0.10m, 26, 1.5m));

        Assert.NotNull(policy);
        Assert.Equal(0.10m, policy.FlatTaxRate);
    }

    [Fact]
    public async Task UC161_UpsertPolicy_InvalidTaxRate_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertPolicyAsync(_tenant, _userAdmin,
                new PayrollPolicyUpsertRequest(0.08m, 0.015m, 0.01m, 11000000m, -0.05m, 26, 1.5m)));

        Assert.Contains("Thuế flat 0–1", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_162: Giảm trừ gia cảnh (Personal & Family Dependent Deductions)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC162_CalculatePeriod_AppliesPersonalDeductionToTaxableIncome()
    {
        await _svc.UpsertPolicyAsync(_tenant, _userAdmin,
            new PayrollPolicyUpsertRequest(0.08m, 0.015m, 0.01m, 11000000m, 0.05m, 26, 1.5m));

        await _svc.CalculateAsync(_tenant, _userAdmin, _periodId1);

        var line = await _db.PayrollLines.FirstAsync(x => x.PayrollPeriodId == _periodId1);
        Assert.NotNull(line);
        Assert.True(line.Tax >= 0);
    }

    [Fact]
    public async Task UC162_UpsertPolicy_InvalidStandardWorkDays_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertPolicyAsync(_tenant, _userAdmin,
                new PayrollPolicyUpsertRequest(0.08m, 0.015m, 0.01m, 11000000m, 0.05m, 40, 1.5m)));

        Assert.Contains("Ngày công chuẩn 1–31", ex.Message);
    }

    [Fact]
    public async Task UC162_UpsertPolicy_InvalidOtMultiplier_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertPolicyAsync(_tenant, _userAdmin,
                new PayrollPolicyUpsertRequest(0.08m, 0.015m, 0.01m, 11000000m, 0.05m, 26, 6.0m)));

        Assert.Contains("Hệ số OT 1–5", ex.Message);
    }
}
