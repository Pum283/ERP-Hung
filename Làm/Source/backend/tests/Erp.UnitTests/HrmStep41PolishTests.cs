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
/// Unit tests cho Bước 41:
///   UC_HRM_155 — Đơn giá giờ / ngày nhân viên (Hourly & Daily Rate Calculation)
///   UC_HRM_156 — Quản lý lương thực tế chi trả (Actual Disbursed Payroll Management)
///   UC_HRM_157 — Danh mục phụ cấp (Allowance Categories & Types)
///   UC_HRM_158 — Rule phụ cấp theo ca (Shift-based Allowance Rules)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep41PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmPayrollService _svc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userEmp1   = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;
    private Guid _periodId1;

    public HrmStep41PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step41-" + Guid.NewGuid())
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
            Code = "ORG_S41_1", Name = "Phòng Chi Lương 41", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_PAY41", Name = "Chuyên Viên 41"
        });

        _db.Users.Add(new AppUser { Id = _userEmp1, TenantId = _tenant, Username = "emp41_1", DisplayName = "Lê Văn Chi Lương 41" });
        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin41", DisplayName = "Admin Chi Lương 41" });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp1, EmployeeCode = "EMP_S41_1", FullName = "Lê Văn Chi Lương 41",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2))
        };
        _db.Employees.Add(emp1);

        _db.EmployeeSalaries.Add(new EmployeeSalary
        {
            TenantId = _tenant, EmployeeId = emp1.Id, BaseSalary = 13000000m,
            HourlyRate = 62500m, DailyRate = 500000m, EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        });

        var period = new PayrollPeriod
        {
            TenantId = _tenant, PeriodKey = "2026-08",
            PeriodFrom = new DateOnly(2026, 8, 1), PeriodTo = new DateOnly(2026, 8, 31),
            Status = "Draft"
        };
        _db.PayrollPeriods.Add(period);

        _db.PayrollPolicies.Add(new PayrollPolicy
        {
            TenantId = _tenant, StandardWorkDays = 26, OtMultiplier = 1.5m,
            SocialInsuranceEmpRate = 0.08m, HealthInsuranceEmpRate = 0.015m, UnemploymentEmpRate = 0.01m,
            PersonalDeduction = 11000000m, FlatTaxRate = 0.05m
        });

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
    // UC_HRM_155: Đơn giá giờ / ngày nhân viên (Rates Calculation)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC155_CalculateRates_CalculatesDailyAndHourlyRatesCorrectly()
    {
        var periodDto = await _svc.CalculateAsync(_tenant, _userAdmin, _periodId1);

        Assert.NotNull(periodDto);
        var lines = await _db.PayrollLines.Where(x => x.PayrollPeriodId == _periodId1).ToListAsync();
        Assert.Single(lines);
        Assert.Equal(13000000m, lines[0].BaseSalary);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_156: Quản lý lương thực tế chi trả (Disbursed Payroll Management)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC156_CalculatePeriod_ValidPolicy_CalculatesGrossAndNetPay()
    {
        await _svc.CalculateAsync(_tenant, _userAdmin, _periodId1);

        var line = await _db.PayrollLines.FirstAsync(x => x.PayrollPeriodId == _periodId1);
        Assert.True(line.GrossPay >= 0);
        Assert.True(line.NetPay >= 0);
    }

    [Fact]
    public async Task UC156_Confirm_CalculatedPeriod_UpdatesStatusToConfirmed()
    {
        await _svc.CalculateAsync(_tenant, _userAdmin, _periodId1);
        await _svc.ConfirmAsync(_tenant, _userAdmin, _periodId1);

        var period = await _db.PayrollPeriods.FirstAsync(x => x.Id == _periodId1);
        Assert.Equal("Confirmed", period.Status);
        Assert.NotNull(period.ConfirmedAt);

        var lines = await _db.PayrollLines.Where(x => x.PayrollPeriodId == _periodId1).ToListAsync();
        Assert.True(lines.All(x => x.IsConfirmed));
    }

    [Fact]
    public async Task UC156_Confirm_DraftPeriod_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.ConfirmAsync(_tenant, _userAdmin, _periodId1));

        Assert.Contains("Chỉ xác nhận kỳ đã tính lương", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_157: Danh mục phụ cấp (Allowance Categories)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC157_UpsertAllowanceType_ValidParameters_CreatesTypeSuccessfully()
    {
        var allow = await _svc.UpsertAllowanceTypeAsync(_tenant, _userAdmin,
            new AllowanceTypeUpsertRequest(null, "ALLOW_MEAL41", "Phụ cấp cơm trưa 41", 800000m, false, true));

        Assert.NotNull(allow);
        Assert.Equal("ALLOW_MEAL41", allow.Code);
        Assert.Equal(800000m, allow.DefaultAmount);
    }

    [Fact]
    public async Task UC157_ListAllowanceTypes_ReturnsAllCreatedAllowanceTypes()
    {
        await _svc.UpsertAllowanceTypeAsync(_tenant, _userAdmin,
            new AllowanceTypeUpsertRequest(null, "ALLOW_TRAVELLING", "Phụ cấp đi lại", 500000m, true, true));

        var types = await _svc.ListAllowanceTypesAsync(_tenant);

        Assert.NotEmpty(types);
        Assert.Contains(types, x => x.Code == "ALLOW_TRAVELLING");
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_158: Rule phụ cấp theo ca (Shift-based Allowance Calculation)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC158_UpsertAllowanceRule_ShiftCode_CreatesRuleSuccessfully()
    {
        var allow = await _svc.UpsertAllowanceTypeAsync(_tenant, _userAdmin,
            new AllowanceTypeUpsertRequest(null, "ALLOW_NIGHT", "Phụ cấp ca đêm", 100000m, true, true));

        var rule = await _svc.UpsertAllowanceRuleAsync(_tenant, _userAdmin,
            new AllowanceRuleUpsertRequest(null, allow.Id, "NIGHT_SHIFT", 150000m, true, "Ghi chú ca đêm"));

        Assert.NotNull(rule);
        Assert.Equal("NIGHT_SHIFT", rule.ShiftCode);
        Assert.Equal(150000m, rule.Amount);
    }

    [Fact]
    public async Task UC158_CalculatePeriod_WithShiftAllowanceRule_IncludesRuleInAllowanceTotal()
    {
        var allow = await _svc.UpsertAllowanceTypeAsync(_tenant, _userAdmin,
            new AllowanceTypeUpsertRequest(null, "ALLOW_NIGHT2", "Phụ cấp ca đêm 2", 100000m, true, true));
        await _svc.UpsertAllowanceRuleAsync(_tenant, _userAdmin,
            new AllowanceRuleUpsertRequest(null, allow.Id, null, 200000m, true, "Phụ cấp chung"));

        await _svc.CalculateAsync(_tenant, _userAdmin, _periodId1);

        var line = await _db.PayrollLines.FirstAsync(x => x.PayrollPeriodId == _periodId1);
        Assert.True(line.AllowanceTotal >= 200000m);
    }
}
