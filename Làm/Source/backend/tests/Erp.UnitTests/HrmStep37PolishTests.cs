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
/// Unit tests cho Bước 37:
///   UC_HRM_139 — Ghi nhận quyết định khen thưởng (Record Reward Decision)
///   UC_HRM_140 — Ghi nhận quyết định kỷ luật (Record Discipline Decision)
///   UC_HRM_141 — Theo dõi chấp hành & Áp dụng lương (Monitor Discipline & Payroll Impact)
///   UC_HRM_143 — Lịch sử khen thưởng / kỷ luật (Reward & Discipline History per Employee)
/// 11 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmStep37PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmRewardDisciplineService _svc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userEmp1   = Guid.NewGuid();
    private readonly Guid _orgUnit1    = Guid.NewGuid();
    private readonly Guid _jobTitleId = Guid.NewGuid();

    private Guid _empId1;
    private Guid _payrollPeriodId1;

    public HrmStep37PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step37-" + Guid.NewGuid())
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
            Code = "ORG_S37_1", Name = "Phòng Nhân Sự 37", UnitType = "Department", Path = "/1"
        });
        _db.JobTitles.Add(new JobTitle
        {
            Id = _jobTitleId, TenantId = _tenant, Code = "JT_REWARD37", Name = "Chuyên Viên 37"
        });

        _db.Users.Add(new AppUser { Id = _userEmp1, TenantId = _tenant, Username = "emp37_1", DisplayName = "Trần Văn Khen Thưởng 37" });

        var emp1 = new Employee
        {
            TenantId = _tenant, UserId = _userEmp1, EmployeeCode = "EMP_S37_1", FullName = "Trần Văn Khen Thưởng 37",
            OrgUnitId = _orgUnit1, JobTitleId = _jobTitleId, Status = "Active",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
        };
        _db.Employees.Add(emp1);

        var period = new PayrollPeriod
        {
            TenantId = _tenant, PeriodKey = "2026-08",
            PeriodFrom = new DateOnly(2026, 8, 1), PeriodTo = new DateOnly(2026, 8, 31),
            Status = "Draft"
        };
        _db.PayrollPeriods.Add(period);

        _db.SaveChanges();

        _empId1 = emp1.Id;
        _payrollPeriodId1 = period.Id;

        _svc = new HrmRewardDisciplineService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_139: Ghi nhận quyết định khen thưởng (Record Reward Decision)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC139_CreateReward_ValidParameters_CreatesDecisionSuccessfully()
    {
        var req = new RewardDisciplineCreateRequest(
            _empId1, "Reward", "Khen thưởng sáng kiến xuất sắc", DateOnly.FromDateTime(DateTime.UtcNow),
            "Có nhiều đóng góp cải tiến quy trình", 1000000m, "Bonus", null, "Ghi chú khen thưởng");

        var decision = await _svc.CreateAsync(_tenant, _userEmp1, req);

        Assert.NotNull(decision);
        Assert.Equal("Reward", decision.Kind);
        Assert.Equal("Khen thưởng sáng kiến xuất sắc", decision.Title);
        Assert.Equal("Issued", decision.Status);
        Assert.Equal(1000000m, decision.PayrollImpactAmount);
    }

    [Fact]
    public async Task UC139_CreateReward_EmptyTitle_ThrowsAppException()
    {
        var req = new RewardDisciplineCreateRequest(
            _empId1, "Reward", "", DateOnly.FromDateTime(DateTime.UtcNow),
            "Lý do", 500000m, "Bonus", null, null);

        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateAsync(_tenant, _userEmp1, req));

        Assert.Contains("Tiêu đề bắt buộc", ex.Message);
    }

    [Fact]
    public async Task UC139_CreateReward_NonExistentEmployee_ThrowsAppException()
    {
        var req = new RewardDisciplineCreateRequest(
            Guid.NewGuid(), "Reward", "Khen thưởng không tồn tại", DateOnly.FromDateTime(DateTime.UtcNow),
            "Lý do", 500000m, "Bonus", null, null);

        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateAsync(_tenant, _userEmp1, req));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Nhân viên không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_140: Ghi nhận quyết định kỷ luật (Record Discipline Decision)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC140_CreateDiscipline_ValidParameters_CreatesDecisionSuccessfully()
    {
        var req = new RewardDisciplineCreateRequest(
            _empId1, "Discipline", "Kỷ luật vi phạm nội quy", DateOnly.FromDateTime(DateTime.UtcNow),
            "Đi trễ nhiều lần không lý do", 200000m, "Deduction", null, "Khiển trách bằng văn bản");

        var decision = await _svc.CreateAsync(_tenant, _userEmp1, req);

        Assert.NotNull(decision);
        Assert.Equal("Discipline", decision.Kind);
        Assert.Equal("Issued", decision.Status);
        Assert.Equal(200000m, decision.PayrollImpactAmount);
    }

    [Fact]
    public async Task UC140_CreateDiscipline_InvalidKind_ThrowsAppException()
    {
        var req = new RewardDisciplineCreateRequest(
            _empId1, "InvalidKind", "Tiêu đề", DateOnly.FromDateTime(DateTime.UtcNow),
            "Lý do", 0m, "None", null, null);

        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateAsync(_tenant, _userEmp1, req));

        Assert.Contains("Kind: Reward | Discipline", ex.Message);
    }

    [Fact]
    public async Task UC140_CreateDiscipline_InvalidImpactKind_ThrowsAppException()
    {
        var req = new RewardDisciplineCreateRequest(
            _empId1, "Discipline", "Tiêu đề", DateOnly.FromDateTime(DateTime.UtcNow),
            "Lý do", 100000m, "InvalidImpact", null, null);

        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateAsync(_tenant, _userEmp1, req));

        Assert.Contains("PayrollImpactKind", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_141: Theo dõi chấp hành & Áp dụng lương (Payroll Impact)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC141_ApplyToPayroll_ValidDecision_CreatesPayrollAdjustmentAndUpdatesStatus()
    {
        var req = new RewardDisciplineCreateRequest(
            _empId1, "Reward", "Thưởng năng suất", DateOnly.FromDateTime(DateTime.UtcNow),
            "Vượt KPI", 500000m, "Bonus", null, null);

        var decision = await _svc.CreateAsync(_tenant, _userEmp1, req);
        var applied = await _svc.ApplyToPayrollAsync(_tenant, _userEmp1, decision.Id, _payrollPeriodId1);

        Assert.NotNull(applied);
        Assert.Equal("Applied", applied.Status);

        var adj = await _db.PayrollAdjustments.FirstOrDefaultAsync(x => x.EmployeeId == _empId1);
        Assert.NotNull(adj);
        Assert.Equal(500000m, adj.Amount);
    }

    [Fact]
    public async Task UC141_ApplyToPayroll_AlreadyApplied_ThrowsAppException()
    {
        var req = new RewardDisciplineCreateRequest(
            _empId1, "Reward", "Thưởng lặp", DateOnly.FromDateTime(DateTime.UtcNow),
            "KPI", 300000m, "Bonus", null, null);

        var decision = await _svc.CreateAsync(_tenant, _userEmp1, req);
        await _svc.ApplyToPayrollAsync(_tenant, _userEmp1, decision.Id, _payrollPeriodId1);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.ApplyToPayrollAsync(_tenant, _userEmp1, decision.Id, _payrollPeriodId1));

        Assert.Contains("Đã áp dụng vào lương", ex.Message);
    }

    [Fact]
    public async Task UC141_ApplyToPayroll_NoPayrollImpact_ThrowsAppException()
    {
        var req = new RewardDisciplineCreateRequest(
            _empId1, "Reward", "Khen thưởng tinh thần", DateOnly.FromDateTime(DateTime.UtcNow),
            "Không có tiền thưởng", 0m, "None", null, null);

        var decision = await _svc.CreateAsync(_tenant, _userEmp1, req);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.ApplyToPayrollAsync(_tenant, _userEmp1, decision.Id, _payrollPeriodId1));

        Assert.Contains("không có ảnh hưởng lương", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_HRM_143: Lịch sử khen thưởng / kỷ luật (Reward & Discipline History)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC143_ListHistory_FilteredByKind_ReturnsMatchingDecisions()
    {
        await _svc.CreateAsync(_tenant, _userEmp1, new RewardDisciplineCreateRequest(
            _empId1, "Reward", "Khen thưởng A", DateOnly.FromDateTime(DateTime.UtcNow), "Reason", 0m, "None", null, null));
        await _svc.CreateAsync(_tenant, _userEmp1, new RewardDisciplineCreateRequest(
            _empId1, "Discipline", "Kỷ luật B", DateOnly.FromDateTime(DateTime.UtcNow), "Reason", 0m, "None", null, null));

        var rewards = await _svc.ListAsync(_tenant, "Reward");
        var disciplines = await _svc.ListAsync(_tenant, "Discipline");

        Assert.True(rewards.All(x => x.Kind == "Reward"));
        Assert.True(disciplines.All(x => x.Kind == "Discipline"));
    }

    [Fact]
    public async Task UC143_Attach_ValidStorageKey_UpdatesDecisionStorageKey()
    {
        var decision = await _svc.CreateAsync(_tenant, _userEmp1, new RewardDisciplineCreateRequest(
            _empId1, "Reward", "Khen thưởng đính kèm", DateOnly.FromDateTime(DateTime.UtcNow), "Reason", 0m, "None", null, null));

        var updated = await _svc.AttachAsync(_tenant, _userEmp1, decision.Id,
            new RewardDisciplineAttachRequest("docs/rewards/2026/decision_123.pdf"));

        Assert.NotNull(updated);
        Assert.Equal("docs/rewards/2026/decision_123.pdf", updated.DecisionStorageKey);
    }
}
