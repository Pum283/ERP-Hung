using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Auth;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Implementations.Services.Auth;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Erp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class SysOrgStructPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SysMasterService _sysMasterSvc;
    private readonly SysPlatformService _sysPlatformSvc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();

    public SysOrgStructPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("sys-orgstruct-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        var dummyScope = new DummyScopeService();
        var outbox = new OutboxWriter(_db);
        _sysPlatformSvc = new SysPlatformService(_db, outbox);
        var authz = new AuthorizationService(_db);
        _sysMasterSvc = new SysMasterService(_db, dummyScope, authz, _sysPlatformSvc);
    }

    public void Dispose() => _db.Dispose();

    private async Task<Guid> SeedOrgUnitAsync()
    {
        var org = new OrgUnit { TenantId = _tenant, Code = "ORG_" + Guid.NewGuid().ToString("N")[..6], Name = "Chi nhánh Test", Path = "/" };
        _db.OrgUnits.Add(org);
        await _db.SaveChangesAsync();
        org.Path = $"/{org.Id:N}/";
        await _db.SaveChangesAsync();
        return org.Id;
    }

    [Fact]
    public async Task UC_SYS_038_UpsertDepartment_CreateRootDepartment_Succeeds()
    {
        var orgId = await SeedOrgUnitAsync();
        var req = new DepartmentUpsertRequest(null, "D_HR", "Phòng Nhân Sự", null, orgId, null, true);
        var res = await _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, req);

        Assert.Equal("D_HR", res.Code);
        Assert.Equal("Phòng Nhân Sự", res.Name);

        var deptDb = await _db.Departments.FirstAsync(d => d.Id == res.Id);
        Assert.Equal($"/{res.Id:N}/", deptDb.Path);
    }

    [Fact]
    public async Task UC_SYS_038_UpsertDepartment_CreateChildDepartment_SetsCorrectPath()
    {
        var orgId = await SeedOrgUnitAsync();
        var rootReq = new DepartmentUpsertRequest(null, "D_IT", "Khối IT", null, orgId, null, true);
        var root = await _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, rootReq);

        var childReq = new DepartmentUpsertRequest(null, "D_DEV", "Bộ Phận Dev", root.Id, orgId, null, true);
        var child = await _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, childReq);

        var childDb = await _db.Departments.FirstAsync(d => d.Id == child.Id);
        Assert.Contains(root.Id.ToString("N"), childDb.Path);
    }

    [Fact]
    public async Task UC_SYS_038_UpsertDepartment_EmptyCode_ThrowsAppException()
    {
        var orgId = await SeedOrgUnitAsync();
        var req = new DepartmentUpsertRequest(null, "  ", "Phòng Rống Code", null, orgId, null, true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, req));
        Assert.Contains("không được để trống", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_038_UpsertDepartment_DuplicateCode_ThrowsAppException()
    {
        var orgId = await SeedOrgUnitAsync();
        await _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, new DepartmentUpsertRequest(null, "D_ACC", "Phòng Kế Toán A", null, orgId, null, true));

        var reqDup = new DepartmentUpsertRequest(null, "D_ACC", "Phòng Kế Toán B", null, orgId, null, true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, reqDup));
        Assert.Contains("đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_038_UpsertDepartment_SelfParent_ThrowsAppException()
    {
        var orgId = await SeedOrgUnitAsync();
        var dept = await _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, new DepartmentUpsertRequest(null, "D_SELF", "Self Dept", null, orgId, null, true));

        var reqSelf = new DepartmentUpsertRequest(dept.Id, "D_SELF", "Self Dept Updated", dept.Id, orgId, null, true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, reqSelf));
        Assert.Contains("không thể chọn chính nó", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_038_UpsertDepartment_CircularParent_ThrowsAppException()
    {
        var orgId = await SeedOrgUnitAsync();
        var parent = await _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, new DepartmentUpsertRequest(null, "D_PARENT", "Phòng Cha", null, orgId, null, true));
        var child = await _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, new DepartmentUpsertRequest(null, "D_CHILD", "Phòng Con", parent.Id, orgId, null, true));

        // Attempting to set child as parent of parent
        var reqCirc = new DepartmentUpsertRequest(parent.Id, "D_PARENT", "Phòng Cha Updated", child.Id, orgId, null, true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, reqCirc));
        Assert.Contains("vòng lặp phân cấp", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_038_UpsertDepartment_InvalidOrgUnit_ThrowsAppException()
    {
        var invalidOrgId = Guid.NewGuid();
        var req = new DepartmentUpsertRequest(null, "D_INVALID_ORG", "Phòng Lỗi Org", null, invalidOrgId, null, true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, req));
        Assert.Contains("Chi nhánh gán vào phòng ban không tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_038_UpsertDepartment_UpdateParent_RecursivelyUpdatesChildPaths()
    {
        var orgId = await SeedOrgUnitAsync();
        var root1 = await _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, new DepartmentUpsertRequest(null, "D_ROOT1", "Khối 1", null, orgId, null, true));
        var deptA = await _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, new DepartmentUpsertRequest(null, "D_A", "Phòng A", root1.Id, orgId, null, true));
        var teamA1 = await _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, new DepartmentUpsertRequest(null, "D_A1", "Team A.1", deptA.Id, orgId, null, true));

        var root2 = await _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, new DepartmentUpsertRequest(null, "D_ROOT2", "Khối 2", null, orgId, null, true));

        // Move Dept A under Root 2
        await _sysMasterSvc.UpsertDepartmentAsync(_tenant, _user, new DepartmentUpsertRequest(deptA.Id, "D_A", "Phòng A Moved", root2.Id, orgId, null, true));

        var teamA1Db = await _db.Departments.FirstAsync(d => d.Id == teamA1.Id);
        Assert.Contains(root2.Id.ToString("N"), teamA1Db.Path);
    }

    [Fact]
    public async Task UC_SYS_039_UpsertJobLevel_CreateJobLevel_Succeeds()
    {
        var req = new JobLevelUpsertRequest(null, "JL_SENIOR", "Senior Staff", 3, ScopeType.Department, true);
        var res = await _sysMasterSvc.UpsertJobLevelAsync(_tenant, _user, req);

        Assert.NotNull(res.Id);
        Assert.Equal("JL_SENIOR", res.Code);
        Assert.Equal(3, res.LevelOrder);
        Assert.Equal(ScopeType.Department, res.DefaultScopeType);
    }

    [Fact]
    public async Task UC_SYS_039_UpsertJobLevel_DuplicateCode_ThrowsAppException()
    {
        await _sysMasterSvc.UpsertJobLevelAsync(_tenant, _user, new JobLevelUpsertRequest(null, "JL_DUP", "Job 1", 1, ScopeType.Own, true));

        var reqDup = new JobLevelUpsertRequest(null, "JL_DUP", "Job 2", 2, ScopeType.Own, true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysMasterSvc.UpsertJobLevelAsync(_tenant, _user, reqDup));
        Assert.Contains("đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_039_UpsertJobLevel_NegativeLevelOrder_ThrowsAppException()
    {
        var req = new JobLevelUpsertRequest(null, "JL_NEG", "Negative Order", -1, ScopeType.Own, true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysMasterSvc.UpsertJobLevelAsync(_tenant, _user, req));
        Assert.Contains("Thứ tự cấp bậc phải lớn hơn hoặc bằng 0", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_040_GetOrgChart_ReturnsActiveOrgUnitsInHierarchy()
    {
        _db.Licenses.Add(new License { TenantId = _tenant, PlanCode = "ENTERPRISE", MaxOrgUnits = 10, Status = "Active" });
        await _db.SaveChangesAsync();

        var ho = await _sysMasterSvc.UpsertOrgUnitAsync(_tenant, _user, new OrgUnitUpsertRequest(null, "HO_CHART", "Hội sở Chart", null, "Company", true));
        var branch = await _sysMasterSvc.UpsertOrgUnitAsync(_tenant, _user, new OrgUnitUpsertRequest(null, "CN_CHART", "Chi nhánh Chart", ho.Id, "Branch", true));

        var nodes = await _sysPlatformSvc.GetOrgChartAsync(_tenant);
        Assert.Equal(2, nodes.Count);
        Assert.Contains(nodes, n => n.Code == "HO_CHART");
        Assert.Contains(nodes, n => n.Code == "CN_CHART" && n.ParentId == ho.Id);
    }

    private sealed class DummyScopeService : IDataScopeService
    {
        public Task<UserScopeContext> GetUserScopeContextAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(new UserScopeContext(ScopeType.All, true, userId, null, Array.Empty<Guid>(), null));
    }
}
