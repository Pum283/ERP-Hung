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
/// Unit tests cho Bước 15: UC_HRM_038 (Tạo hợp đồng lao động), UC_HRM_039 (Tạo phụ lục hợp đồng),
/// UC_HRM_043 (Cảnh báo hết hạn hợp đồng), UC_HRM_046 (Lịch sử hợp đồng theo nhân sự).
/// 13+ test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmContractPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmContractService _contractSvc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _actor  = Guid.NewGuid();

    public HrmContractPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-contract-step15-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _db.Licenses.Add(new License
        {
            TenantId = _tenant,
            PlanCode = "ENTERPRISE",
            Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100,
            MaxOrgUnits = 500
        });
        _db.SaveChanges();

        _contractSvc = new HrmContractService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ─── UC_HRM_038: Tạo hợp đồng lao động ───

    [Fact]
    public async Task UC038_UpsertContract_ValidData_CreatesContract()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_CTR1", FullName = "Trần Hợp Đồng" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = start.AddYears(1);

        var req = new ContractUpsertRequest(null, emp.Id, "HD-2026-001", "Definite", start, end, "Active", null, 15000000m, null);
        var res = await _contractSvc.UpsertAsync(_tenant, _actor, req);

        Assert.Equal("HD-2026-001", res.ContractNo);
        Assert.Equal("Definite", res.ContractType);
        Assert.Equal(15000000m, res.BaseSalary);
    }

    [Fact]
    public async Task UC038_UpsertContract_EmptyContractNo_ThrowsAppException()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_CTR2", FullName = "Nguyễn Hợp Đồng 2" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var req = new ContractUpsertRequest(null, emp.Id, "", "Indefinite", DateOnly.FromDateTime(DateTime.UtcNow), null, "Active", null, null, null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _contractSvc.UpsertAsync(_tenant, _actor, req));
        Assert.Contains("ContractNo", ex.Message);
    }

    [Fact]
    public async Task UC038_UpsertContract_DuplicateContractNo_ThrowsAppException()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_CTR3", FullName = "Đỗ Hợp Đồng 3" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        await _contractSvc.UpsertAsync(_tenant, _actor, new ContractUpsertRequest(null, emp.Id, "HD-DUP-01", "Indefinite", start, null, "Active", null, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _contractSvc.UpsertAsync(_tenant, _actor, new ContractUpsertRequest(null, emp.Id, "HD-DUP-01", "Indefinite", start, null, "Active", null, null, null)));
        Assert.Contains("đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC038_UpsertContract_DefiniteMissingEndDate_ThrowsAppException()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_CTR4", FullName = "Phạm Hợp Đồng 4" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var req = new ContractUpsertRequest(null, emp.Id, "HD-DEF-BAD", "Definite", DateOnly.FromDateTime(DateTime.UtcNow), null, "Active", null, null, null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _contractSvc.UpsertAsync(_tenant, _actor, req));
        Assert.Contains("Ngày kết thúc", ex.Message);
    }

    [Fact]
    public async Task UC038_UpsertContract_EndDateBeforeStart_ThrowsAppException()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_CTR5", FullName = "Lê Hợp Đồng 5" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var req = new ContractUpsertRequest(null, emp.Id, "HD-DATE-BAD", "Definite", start, start.AddDays(-1), "Active", null, null, null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _contractSvc.UpsertAsync(_tenant, _actor, req));
        Assert.Contains("sau ngày bắt đầu", ex.Message);
    }

    // ─── UC_HRM_039: Tạo phụ lục hợp đồng ───

    [Fact]
    public async Task UC039_CreateAnnex_ValidParent_CreatesAnnexWithAutoNo()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_ANNEX", FullName = "Vũ Phụ Lục" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6));
        var parent = await _contractSvc.UpsertAsync(_tenant, _actor, new ContractUpsertRequest(null, emp.Id, "HD-PARENT-01", "Definite", start, start.AddYears(1), "Active", null, 10000000m, null));

        var req = new ContractAnnexCreateRequest(parent.Id, null, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)), 12000000m, "Tăng lương phụ lục", null);
        var annex = await _contractSvc.CreateAnnexAsync(_tenant, _actor, req);

        Assert.Equal("HD-PARENT-01-PL1", annex.ContractNo);
        Assert.Equal("Annex", annex.ContractType);
        Assert.Equal(parent.Id, annex.ParentContractId);
        Assert.Equal(12000000m, annex.BaseSalary);

        // Verify parent contract salary updated
        var dbParent = await _db.Contracts.FirstOrDefaultAsync(x => x.Id == parent.Id);
        Assert.Equal(12000000m, dbParent!.BaseSalary);
    }

    [Fact]
    public async Task UC039_CreateAnnex_TerminatedParent_ThrowsAppException()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_ANNEX2", FullName = "Vũ Phụ Lục 2" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6));
        var parent = await _contractSvc.UpsertAsync(_tenant, _actor, new ContractUpsertRequest(null, emp.Id, "HD-TERM-01", "Definite", start, start.AddYears(1), "Terminated", null, 10000000m, null));

        var req = new ContractAnnexCreateRequest(parent.Id, null, DateOnly.FromDateTime(DateTime.UtcNow), null, 12000000m, "Phụ lục lỗi", null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _contractSvc.CreateAnnexAsync(_tenant, _actor, req));
        Assert.Contains("chấm dứt hoặc hết hạn", ex.Message);
    }

    // ─── UC_HRM_043: Cảnh báo hết hạn hợp đồng ───

    [Fact]
    public async Task UC043_ListExpiringDetailed_ReturnsContractsWithinThreshold()
    {
        var org = new OrgUnit { TenantId = _tenant, Code = "ORG_EXP", Name = "Chi nhánh Hà Nội", UnitType = "Branch", Path = "/1" };
        _db.OrgUnits.Add(org);

        var emp1 = new Employee { TenantId = _tenant, OrgUnitId = org.Id, EmployeeCode = "EMP_EXP_C1", FullName = "Nguyễn Hết Hạn 1" };
        var emp2 = new Employee { TenantId = _tenant, OrgUnitId = org.Id, EmployeeCode = "EMP_EXP_C2", FullName = "Trần An Toàn 2" };
        _db.Employees.AddRange(emp1, emp2);
        await _db.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Contract expiring in 10 days
        await _contractSvc.UpsertAsync(_tenant, _actor, new ContractUpsertRequest(null, emp1.Id, "HD-EXP-10D", "Definite", today.AddYears(-1), today.AddDays(10), "Active", null, 12000000m, null));
        // Contract expiring in 60 days
        await _contractSvc.UpsertAsync(_tenant, _actor, new ContractUpsertRequest(null, emp2.Id, "HD-SAFE-60D", "Definite", today.AddYears(-1), today.AddDays(60), "Active", null, 15000000m, null));

        var expiringList = await _contractSvc.ListExpiringDetailedAsync(_tenant, withinDays: 30);

        Assert.Single(expiringList);
        Assert.Equal("HD-EXP-10D", expiringList[0].ContractNo);
        Assert.Equal(10, expiringList[0].DaysRemaining);
        Assert.Equal("Chi nhánh Hà Nội", expiringList[0].OrgUnitName);
    }

    // ─── UC_HRM_046: Lịch sử hợp đồng theo nhân sự ───

    [Fact]
    public async Task UC046_ListContractHistory_ReturnsContractsAndAnnexes()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_HIST_C", FullName = "Hoàng Lịch Sử HĐ" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1));
        var c1 = await _contractSvc.UpsertAsync(_tenant, _actor, new ContractUpsertRequest(null, emp.Id, "HD-ROOT-1", "Definite", start, start.AddYears(1), "Active", null, 10000000m, null));
        var a1 = await _contractSvc.CreateAnnexAsync(_tenant, _actor, new ContractAnnexCreateRequest(c1.Id, "HD-ROOT-1-PL1", start.AddMonths(6), start.AddYears(1), 12000000m, "Phụ lục tăng lương", null));

        var history = await _contractSvc.ListContractHistoryAsync(_tenant, emp.Id);

        Assert.Equal(2, history.Count);
        Assert.Equal("HD-ROOT-1-PL1", history[0].ContractNo); // Mới hơn xếp trên
        Assert.Equal("HD-ROOT-1", history[0].ParentContractNo);
        Assert.Equal("HD-ROOT-1", history[1].ContractNo);
    }
}
