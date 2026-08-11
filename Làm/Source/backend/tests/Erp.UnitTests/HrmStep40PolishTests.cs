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
/// Unit tests cho Bước 40:
///   UC_HRM_151 — Báo cáo nghỉ việc / lý do (Offboarding & Turnover Reason Report)
///   UC_HRM_152 — Tạo thang bậc lương (Salary Grade Structure)
///   UC_HRM_153 — Gán bậc lương theo nhân sự (Assign Salary Grade per Employee)
///   UC_HRM_154 — Cấu hình phụ cấp theo bậc lương (Configure Allowance Types)
/// 11 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep40PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmOffboardingService _offboardingSvc;
    private readonly HrmPayrollService _payrollSvc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userEmp1   = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;

    public HrmStep40PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step40-" + Guid.NewGuid())
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
            Code = "ORG_S40_1", Name = "Phòng Lương Thưởng 40", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_PAY40", Name = "Chuyên Viên Lương 40"
        });

        _db.Users.Add(new AppUser { Id = _userEmp1, TenantId = _tenant, Username = "emp40_1", DisplayName = "Phạm Văn Lương 40" });
        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin40", DisplayName = "Admin Lương Thưởng 40" });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp1, EmployeeCode = "EMP_S40_1", FullName = "Phạm Văn Lương 40",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2))
        };
        _db.Employees.Add(emp1);
        _db.SaveChanges();

        _empId1 = emp1.Id;

        _offboardingSvc = new HrmOffboardingService(_db);
        _payrollSvc = new HrmPayrollService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_151: Báo cáo nghỉ việc / lý do (Offboarding & Turnover Report)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC151_ListOffboardingCases_ReturnsCasesForTurnoverReport()
    {
        var reqDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var lastDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(35));

        await _offboardingSvc.CreateAsync(_tenant, _userEmp1,
            new OffboardingCreateRequest(_empId1, reqDate, lastDay, "Salary", "Lương thưởng không cạnh tranh"));

        var cases = await _offboardingSvc.ListAsync(_tenant);

        Assert.NotEmpty(cases);
        Assert.Equal("Salary", cases[0].ReasonCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_152: Tạo thang bậc lương (Salary Grade Structure)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC152_UpsertGrade_ValidParameters_CreatesGradeSuccessfully()
    {
        var grade = await _payrollSvc.UpsertGradeAsync(_tenant, _userAdmin,
            new SalaryGradeUpsertRequest(null, "GR_SENIOR", "Bậc Chuyên Viên Cao Cấp", 3, 18000000m, true, "Ghi chú"));

        Assert.NotNull(grade);
        Assert.Equal("GR_SENIOR", grade.Code);
        Assert.Equal(18000000m, grade.BaseAmount);
        Assert.Equal(3, grade.Level);
    }

    [Fact]
    public async Task UC152_UpsertGrade_DuplicateCode_ThrowsAppException()
    {
        await _payrollSvc.UpsertGradeAsync(_tenant, _userAdmin,
            new SalaryGradeUpsertRequest(null, "GR_JUNIOR", "Bậc Junior", 1, 10000000m, true, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _payrollSvc.UpsertGradeAsync(_tenant, _userAdmin,
                new SalaryGradeUpsertRequest(null, "GR_JUNIOR", "Bậc Junior 2", 1, 12000000m, true, null)));

        Assert.Contains("Mã bậc đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC152_UpsertGrade_NegativeBaseAmount_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _payrollSvc.UpsertGradeAsync(_tenant, _userAdmin,
                new SalaryGradeUpsertRequest(null, "GR_ERR", "Bậc Lỗi", 1, -5000000m, true, null)));

        Assert.Contains("Mức lương cơ bản không hợp lệ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_153: Gán bậc lương theo nhân sự (Assign Salary Grade)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC153_UpsertEmployeeSalary_ValidGradeAndAmount_AssignsSalarySuccessfully()
    {
        var grade = await _payrollSvc.UpsertGradeAsync(_tenant, _userAdmin,
            new SalaryGradeUpsertRequest(null, "GR_MID", "Bậc Mid-Level", 2, 14000000m, true, null));

        var empSalary = await _payrollSvc.UpsertEmployeeSalaryAsync(_tenant, _userAdmin,
            new EmployeeSalaryUpsertRequest(null, _empId1, grade.Id, 14000000m, null, null, "Active",
                DateOnly.FromDateTime(DateTime.UtcNow), null, true, "Gán bậc lương chính thức"));

        Assert.NotNull(empSalary);
        Assert.Equal(_empId1, empSalary.EmployeeId);
        Assert.Equal(grade.Id, empSalary.SalaryGradeId);
        Assert.Equal(14000000m, empSalary.BaseSalary);
    }

    [Fact]
    public async Task UC153_UpsertEmployeeSalary_NegativeBaseSalary_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _payrollSvc.UpsertEmployeeSalaryAsync(_tenant, _userAdmin,
                new EmployeeSalaryUpsertRequest(null, _empId1, null, -1000000m, null, null, null,
                    DateOnly.FromDateTime(DateTime.UtcNow), null, true, null)));

        Assert.Contains("Lương cơ bản không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC153_UpsertEmployeeSalary_NonExistentEmployee_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _payrollSvc.UpsertEmployeeSalaryAsync(_tenant, _userAdmin,
                new EmployeeSalaryUpsertRequest(null, Guid.NewGuid(), null, 10000000m, null, null, null,
                    DateOnly.FromDateTime(DateTime.UtcNow), null, true, null)));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Nhân viên không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_154: Cấu hình phụ cấp theo bậc lương (Configure Allowances)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC154_UpsertAllowanceType_ValidParameters_CreatesAllowanceTypeSuccessfully()
    {
        var allow = await _payrollSvc.UpsertAllowanceTypeAsync(_tenant, _userAdmin,
            new AllowanceTypeUpsertRequest(null, "ALLOW_LUNCH", "Phụ cấp ăn trưa", 1000000m, false, true));

        Assert.NotNull(allow);
        Assert.Equal("ALLOW_LUNCH", allow.Code);
        Assert.Equal(1000000m, allow.DefaultAmount);
        Assert.False(allow.IsTaxable);
    }

    [Fact]
    public async Task UC154_UpsertAllowanceType_DuplicateCode_ThrowsAppException()
    {
        await _payrollSvc.UpsertAllowanceTypeAsync(_tenant, _userAdmin,
            new AllowanceTypeUpsertRequest(null, "ALLOW_PHONE", "Phụ cấp điện thoại", 500000m, true, true));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _payrollSvc.UpsertAllowanceTypeAsync(_tenant, _userAdmin,
                new AllowanceTypeUpsertRequest(null, "ALLOW_PHONE", "Phụ cấp điện thoại 2", 600000m, true, true)));

        Assert.Contains("Mã phụ cấp đã tồn tại", ex.Message);
    }
}
