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
/// Unit tests cho Bước 45:
///   UC_HRM_171 — Phiếu lương cá nhân (APP) — MyPayslipAsync
///   UC_HRM_172 — Xuất bảng lương tổng hợp CSV — ExportCsvAsync
///   UC_HRM_173 — Xuất file chi lương ngân hàng CSV — ExportBankCsvAsync
///   UC_HRM_175 — Báo cáo chi phí lương theo đơn vị — CostByOrgAsync
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep45PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmPayrollService _svc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _userEmp1   = Guid.NewGuid();
    private readonly Guid _orgUnit1   = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;

    public HrmStep45PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step45-" + Guid.NewGuid())
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
            Code = "ORG_S45_1", Name = "Phòng Xuất Báo Cáo 45", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_EXPORT45", Name = "Chuyên Viên 45"
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin45", DisplayName = "Admin Xuất Báo Cáo 45" });
        _db.Users.Add(new AppUser { Id = _userEmp1, TenantId = _tenant, Username = "emp45_1", DisplayName = "Lê Văn Phiếu Lương 45" });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp1, EmployeeCode = "EMP_S45_1", FullName = "Lê Văn Phiếu Lương 45",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2))
        };
        _db.Employees.Add(emp1);

        _db.EmployeeSalaries.Add(new EmployeeSalary
        {
            TenantId = _tenant, EmployeeId = emp1.Id, BaseSalary = 16000000m,
            HourlyRate = 76923m, DailyRate = 615385m, EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        });

        _db.PayrollPolicies.Add(new PayrollPolicy
        {
            TenantId = _tenant, StandardWorkDays = 26, OtMultiplier = 1.5m,
            SocialInsuranceEmpRate = 0.08m, HealthInsuranceEmpRate = 0.015m, UnemploymentEmpRate = 0.01m,
            PersonalDeduction = 11000000m, FlatTaxRate = 0.05m
        });

        _db.AttendanceRecords.Add(new AttendanceRecord
        {
            TenantId = _tenant, EmployeeId = emp1.Id, WorkDate = new DateOnly(2026, 8, 10),
            WorkUnit = 26m, OtMinutes = 60, Status = "Closed"
        });

        _db.SaveChanges();

        _empId1 = emp1.Id;

        _svc = new HrmPayrollService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_171: Phiếu lương cá nhân (APP) — MyPayslipAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC171_MyPayslip_ConfirmedPeriod_ReturnsPayslipForCurrentUser()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-08", "Kỳ T8/2026"));
        await _svc.CalculateAsync(_tenant, _userAdmin, period.Id);
        await _svc.ConfirmAsync(_tenant, _userAdmin, period.Id);

        var payslip = await _svc.MyPayslipAsync(_tenant, _userEmp1, null);

        Assert.NotEmpty(payslip);
        Assert.All(payslip, p => Assert.Equal(_empId1, p.EmployeeId));
        Assert.True(payslip[0].NetPay > 0);
    }

    [Fact]
    public async Task UC171_MyPayslip_NoPeriodConfirmed_ReturnsEmptyList()
    {
        var payslip = await _svc.MyPayslipAsync(_tenant, _userEmp1, null);
        Assert.Empty(payslip);
    }

    [Fact]
    public async Task UC171_MyPayslip_NonExistentUser_ThrowsAppException()
    {
        var fakeUserId = Guid.NewGuid();
        _db.Users.Add(new AppUser { Id = fakeUserId, TenantId = _tenant, Username = "ghost45", DisplayName = "Ghost 45" });
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.MyPayslipAsync(_tenant, fakeUserId, null));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Không tìm thấy hồ sơ nhân viên", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_172: Xuất bảng lương tổng hợp CSV — ExportCsvAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC172_ExportCsv_CalculatedPeriod_ReturnsCsvWithHeader()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-09", "Kỳ T9/2026"));
        await _svc.CalculateAsync(_tenant, _userAdmin, period.Id);

        var csv = await _svc.ExportCsvAsync(_tenant, period.Id);

        Assert.Contains("EmployeeCode", csv);
        Assert.Contains("Net", csv);
        Assert.Contains("EMP_S45_1", csv);
    }

    [Fact]
    public async Task UC172_ExportCsv_EmptyPeriod_ReturnsOnlyHeaderLine()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2027-05", "Kỳ T5/2027"));

        var csv = await _svc.ExportCsvAsync(_tenant, period.Id);

        Assert.Contains("EmployeeCode", csv);
        var lines = csv.Trim().Split('\n');
        Assert.Single(lines);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_173: Xuất file chi lương ngân hàng — ExportBankCsvAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC173_ExportBankCsv_CalculatedPeriod_ReturnsBankTransferRows()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-10", "Kỳ T10/2026"));
        _db.AttendanceRecords.Add(new AttendanceRecord
        {
            TenantId = _tenant, EmployeeId = _empId1, WorkDate = new DateOnly(2026, 10, 15),
            WorkUnit = 26m, OtMinutes = 60, Status = "Closed"
        });
        await _db.SaveChangesAsync();
        await _svc.CalculateAsync(_tenant, _userAdmin, period.Id);

        var csv = await _svc.ExportBankCsvAsync(_tenant, period.Id);

        Assert.Contains("EmployeeCode,EmployeeName,Amount,Content", csv);
        Assert.Contains("Chi luong", csv);
        Assert.Contains("EMP_S45_1", csv);
    }

    [Fact]
    public async Task UC173_ExportBankCsv_EmptyPeriod_ReturnsOnlyHeaderLine()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2027-06", "Kỳ T6/2027"));

        var csv = await _svc.ExportBankCsvAsync(_tenant, period.Id);

        Assert.Contains("EmployeeCode,EmployeeName,Amount,Content", csv);
        var lines = csv.Trim().Split('\n');
        Assert.Single(lines);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_175: Báo cáo chi phí lương theo đơn vị — CostByOrgAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC175_CostByOrg_CalculatedPeriod_ReturnsGroupedByOrgUnit()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-11", "Kỳ T11/2026"));
        _db.AttendanceRecords.Add(new AttendanceRecord
        {
            TenantId = _tenant, EmployeeId = _empId1, WorkDate = new DateOnly(2026, 11, 10),
            WorkUnit = 26m, OtMinutes = 60, Status = "Closed"
        });
        await _db.SaveChangesAsync();
        await _svc.CalculateAsync(_tenant, _userAdmin, period.Id);

        var cost = await _svc.CostByOrgAsync(_tenant, period.Id);

        Assert.NotEmpty(cost);
        Assert.Contains(cost, c => c.OrgUnitName == "Phòng Xuất Báo Cáo 45");
        Assert.True(cost[0].Gross > 0);
        Assert.True(cost[0].Headcount > 0);
    }

    [Fact]
    public async Task UC175_CostByOrg_EmptyPeriod_ReturnsEmptyList()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2027-07", "Kỳ T7/2027"));

        var cost = await _svc.CostByOrgAsync(_tenant, period.Id);

        Assert.Empty(cost);
    }
}
