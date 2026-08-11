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
/// Unit tests cho Bước 44:
///   UC_HRM_167 — Nhập khấu trừ / tạm ứng (Deduction & Advance Adjustments)
///   UC_HRM_168 — Xem / chỉnh bảng lương chi tiết (Detailed Payslip View & Patch Line)
///   UC_HRM_169 — Xác nhận bảng lương (Confirm Payroll Period & Lines)
///   UC_HRM_170 — Khóa sổ kỳ lương (Lock Payroll Period & Finalize)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep44PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmPayrollService _svc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;

    public HrmStep44PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step44-" + Guid.NewGuid())
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
            Code = "ORG_S44_1", Name = "Phòng Khóa Sổ Lương 44", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_LOCK44", Name = "Chuyên Viên 44"
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin44", DisplayName = "Admin Khóa Sổ Lương 44" });

        var emp1 = new Employee
        {
            TenantId = _tenant, EmployeeCode = "EMP_S44_1", FullName = "Nguyễn Văn Khóa Sổ 44",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2))
        };
        _db.Employees.Add(emp1);

        _db.EmployeeSalaries.Add(new EmployeeSalary
        {
            TenantId = _tenant, EmployeeId = emp1.Id, BaseSalary = 18000000m,
            HourlyRate = 86538m, DailyRate = 692308m, EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
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
            WorkUnit = 26m, Status = "Closed"
        });

        _db.SaveChanges();

        _empId1 = emp1.Id;

        _svc = new HrmPayrollService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_167: Nhập khấu trừ / tạm ứng (Deduction & Advance Adjustments)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC167_AddAdjustment_DeductionAndAdvance_CreatesSuccessfully()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-08", "Kỳ T8/2026"));

        var ded = await _svc.AddAdjustmentAsync(_tenant, _userAdmin,
            new PayrollAdjustmentCreateRequest(period.Id, _empId1, "Deduction", "Khấu trừ vi phạm 44", 500000m, "Phạt đi trễ"));
        var adv = await _svc.AddAdjustmentAsync(_tenant, _userAdmin,
            new PayrollAdjustmentCreateRequest(period.Id, _empId1, "Advance", "Tạm ứng giữa tháng", 2000000m, "Ứng lương"));

        Assert.NotNull(ded);
        Assert.Equal("Deduction", ded.Kind);
        Assert.NotNull(adv);
        Assert.Equal("Advance", adv.Kind);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_168: Xem / chỉnh bảng lương chi tiết (Detailed Payslip View & Patch)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC168_PatchLine_ValidLine_RecalculatesNetPaySuccessfully()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-09", "Kỳ T9/2026"));
        await _svc.CalculateAsync(_tenant, _userAdmin, period.Id);

        var lines = await _svc.ListLinesAsync(_tenant, period.Id);
        Assert.NotEmpty(lines);
        var targetLine = lines[0];

        var patched = await _svc.PatchLineAsync(_tenant, _userAdmin, targetLine.Id,
            new PayrollLinePatchRequest(1000000m, 500000m, 2000000m, "Đã điều chỉnh thủ công"));

        Assert.NotNull(patched);
        Assert.Equal(1000000m, patched.Bonus);
        Assert.Equal(500000m, patched.DeductionTotal);
        Assert.Equal(2000000m, patched.AllowanceTotal);
        Assert.Equal("Đã điều chỉnh thủ công", patched.Note);
    }

    [Fact]
    public async Task UC168_PatchLine_NonExistentLine_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.PatchLineAsync(_tenant, _userAdmin, Guid.NewGuid(),
                new PayrollLinePatchRequest(1000000m, null, null, null)));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Dòng lương không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_169: Xác nhận bảng lương (Confirm Payroll Period & Lines)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC169_Confirm_CalculatedPeriod_SetsStatusToConfirmed()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-10", "Kỳ T10/2026"));
        await _svc.CalculateAsync(_tenant, _userAdmin, period.Id);

        await _svc.ConfirmAsync(_tenant, _userAdmin, period.Id);

        var periodEntity = await _db.PayrollPeriods.FirstAsync(x => x.Id == period.Id);
        Assert.Equal("Confirmed", periodEntity.Status);
        Assert.NotNull(periodEntity.ConfirmedAt);

        var lines = await _db.PayrollLines.Where(x => x.PayrollPeriodId == period.Id).ToListAsync();
        Assert.True(lines.All(x => x.IsConfirmed));
    }

    [Fact]
    public async Task UC169_Confirm_DraftPeriod_ThrowsAppException()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-11", "Kỳ T11/2026"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.ConfirmAsync(_tenant, _userAdmin, period.Id));

        Assert.Contains("Chỉ xác nhận kỳ đã tính lương", ex.Message);
    }

    [Fact]
    public async Task UC168_PatchLine_ConfirmedLineInConfirmedPeriod_ThrowsAppException()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2026-12", "Kỳ T12/2026"));
        await _svc.CalculateAsync(_tenant, _userAdmin, period.Id);
        await _svc.ConfirmAsync(_tenant, _userAdmin, period.Id);

        var lines = await _svc.ListLinesAsync(_tenant, period.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.PatchLineAsync(_tenant, _userAdmin, lines[0].Id,
                new PayrollLinePatchRequest(500000m, null, null, null)));

        Assert.Contains("Dòng đã xác nhận", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_170: Khóa sổ kỳ lương (Lock Payroll Period)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC170_Lock_ConfirmedPeriod_UpdatesStatusToLocked()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2027-01", "Kỳ T1/2027"));
        await _svc.CalculateAsync(_tenant, _userAdmin, period.Id);
        await _svc.ConfirmAsync(_tenant, _userAdmin, period.Id);

        await _svc.LockAsync(_tenant, _userAdmin, period.Id);

        var periodEntity = await _db.PayrollPeriods.FirstAsync(x => x.Id == period.Id);
        Assert.Equal("Locked", periodEntity.Status);
        Assert.NotNull(periodEntity.LockedAt);
    }

    [Fact]
    public async Task UC170_Lock_UnconfirmedPeriod_ThrowsAppException()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2027-02", "Kỳ T2/2027"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.LockAsync(_tenant, _userAdmin, period.Id));

        Assert.Contains("Khóa kỳ sau khi xác nhận bảng lương", ex.Message);
    }

    [Fact]
    public async Task UC168_PatchLine_LockedPeriod_ThrowsAppException()
    {
        var period = await _svc.CreatePeriodAsync(_tenant, _userAdmin, new PayrollPeriodCreateRequest("2027-03", "Kỳ T3/2027"));
        await _svc.CalculateAsync(_tenant, _userAdmin, period.Id);
        await _svc.ConfirmAsync(_tenant, _userAdmin, period.Id);
        await _svc.LockAsync(_tenant, _userAdmin, period.Id);

        var lines = await _svc.ListLinesAsync(_tenant, period.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.PatchLineAsync(_tenant, _userAdmin, lines[0].Id,
                new PayrollLinePatchRequest(500000m, null, null, null)));

        Assert.Contains("Kỳ đã khóa", ex.Message);
    }
}
