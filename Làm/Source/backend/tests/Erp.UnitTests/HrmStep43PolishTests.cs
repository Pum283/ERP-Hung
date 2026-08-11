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
/// Unit tests cho Bước 43:
///   UC_HRM_163 — Tạo kỳ lương (Create Payroll Period & Key)
///   UC_HRM_164 — Tổng hợp công vào kỳ lương (Aggregate Attendance into Payroll Period)
///   UC_HRM_165 — Tính lương tự động theo rule (Automatic Payroll Calculation Engine)
///   UC_HRM_166 — Nhập thưởng / phụ cấp phát sinh (Bonus & Allowance Adjustments)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep43PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmPayrollService _svc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;

    public HrmStep43PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step43-" + Guid.NewGuid())
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
            Code = "ORG_S43_1", Name = "Phòng Kỳ Lương 43", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_PERIOD43", Name = "Chuyên Viên 43"
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin43", DisplayName = "Admin Kỳ Lương 43" });

        var emp1 = new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP_S43_1", FullName = "Trần Văn Tính Lương 43",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2))
        };
        _db.Employees.Add(emp1);

        _db.EmployeeSalaries.Add(new EmployeeSalary
        {
            TenantId = _tenant, EmployeeId = emp1.Id, BaseSalary = 15000000m,
            HourlyRate = 72115m, DailyRate = 576923m, EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        });

        _db.PayrollPolicies.Add(new PayrollPolicy
        {
            TenantId = _tenant, StandardWorkDays = 26, OtMultiplier = 1.5m,
            SocialInsuranceEmpRate = 0.08m, HealthInsuranceEmpRate = 0.015m, UnemploymentEmpRate = 0.01m,
            PersonalDeduction = 11000000m, FlatTaxRate = 0.05m
        });

        _db.SaveChanges();

        _empId1 = emp1.Id;

        _svc = new HrmPayrollService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_163: Tạo kỳ lương (Create Payroll Period)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC163_CreatePeriod_ValidKey_CreatesPeriodSuccessfully()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin,
            new PayrollPeriodCreateRequest("2026-09", "Kỳ lương tháng 9/2026"));

        Assert.NotNull(period);
        Assert.Equal("2026-09", period.PeriodKey);
        Assert.Equal("Draft", period.Status);
    }

    [Fact]
    public async Task UC163_CreatePeriod_InvalidKeyFormat_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.CreatePeriodAsync(_tenant, _userAdmin,
                new PayrollPeriodCreateRequest("INVALID_KEY", "Note")));

        Assert.Contains("PeriodKey phải dạng yyyy-MM", ex.Message);
    }

    [Fact]
    public async Task UC163_CreatePeriod_DuplicateKey_ThrowsAppException()
    {
        await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-10", "First"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-10", "Duplicate")));

        Assert.Contains("Kỳ lương đã tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_164 & 165: Tổng hợp công & Tính lương tự động theo rule
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC164_UC165_CalculatePeriod_AggregatesAttendanceAndCalculatesSalary()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-11", "Kỳ T11"));
        _db.AttendanceRecords.Add(new AttendanceRecord
        {
            TenantId = _tenant, EmployeeId = _empId1, WorkDate = new DateOnly(2026, 11, 10),
            WorkUnit = 26m, OtMinutes = 120, Status = "Closed"
        });
        await _db.SaveChangesAsync();

        var calculated = await _svc.CalculateAsync(_tenant, _userAdmin, period.Id);

        Assert.Equal("Calculated", calculated.Status);
        var line = await _db.PayrollLines.FirstAsync(x => x.PayrollPeriodId == period.Id);
        Assert.Equal(26m, line.WorkUnits);
        Assert.Equal(120, line.OtMinutes);
        Assert.True(line.GrossPay > 0);
    }

    [Fact]
    public async Task UC165_CalculatePeriod_LockedPeriod_ThrowsAppException()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-12", "Kỳ T12"));
        await _svc.CalculateAsync(_tenant, _userAdmin, period.Id);
        await _svc.ConfirmAsync(_tenant, _userAdmin, period.Id);
        await _svc.LockAsync(_tenant, _userAdmin, period.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.CalculateAsync(_tenant, _userAdmin, period.Id));

        Assert.Contains("Kỳ đã khóa — không tính lại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_166: Nhập thưởng / phụ cấp phát sinh (Bonus & Allowance Adjustments)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC166_AddAdjustment_Bonus_CreatesAdjustmentSuccessfully()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2027-01", "Kỳ T1/2027"));

        var adj = await _svc.AddAdjustmentAsync(_tenant, _userAdmin,
            new PayrollAdjustmentCreateRequest(period.Id, _empId1, "Bonus", "Thưởng Tết Kỷ Dậu", 5000000m, "Thưởng đạt KPI xuất sắc"));

        Assert.NotNull(adj);
        Assert.Equal("Bonus", adj.Kind);
        Assert.Equal(5000000m, adj.Amount);
    }

    [Fact]
    public async Task UC166_AddAdjustment_InvalidKind_ThrowsAppException()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2027-02", "Kỳ T2/2027"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.AddAdjustmentAsync(_tenant, _userAdmin,
                new PayrollAdjustmentCreateRequest(period.Id, _empId1, "UNKNOWN", "Tiêu đề", 1000000m, null)));

        Assert.Contains("Kind: Bonus | Allowance | Deduction | Advance", ex.Message);
    }

    [Fact]
    public async Task UC166_AddAdjustment_EmptyTitle_ThrowsAppException()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2027-03", "Kỳ T3/2027"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.AddAdjustmentAsync(_tenant, _userAdmin,
                new PayrollAdjustmentCreateRequest(period.Id, _empId1, "Allowance", "", 1000000m, null)));

        Assert.Contains("Tiêu đề bắt buộc", ex.Message);
    }

    [Fact]
    public async Task UC166_AddAdjustment_LockedPeriod_ThrowsAppException()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2027-04", "Kỳ T4/2027"));
        await _svc.CalculateAsync(_tenant, _userAdmin, period.Id);
        await _svc.ConfirmAsync(_tenant, _userAdmin, period.Id);
        await _svc.LockAsync(_tenant, _userAdmin, period.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.AddAdjustmentAsync(_tenant, _userAdmin,
                new PayrollAdjustmentCreateRequest(period.Id, _empId1, "Bonus", "Thưởng", 1000000m, null)));

        Assert.Contains("Kỳ đã khóa", ex.Message);
    }
}
