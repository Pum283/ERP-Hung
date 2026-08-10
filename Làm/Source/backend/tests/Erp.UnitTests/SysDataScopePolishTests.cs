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

public sealed class SysDataScopePolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly DataScopeService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();

    public SysDataScopePolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("sys-datascope-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new DataScopeService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetUserScopeContext_BypassUser_ReturnsAllSalesPoints()
    {
        var role = new Role { TenantId = _tenant, Code = "ADMIN", Name = "Admin", BypassDataScope = true };
        var appUser = new AppUser { Id = _user, TenantId = _tenant, Username = "admin-user", PasswordHash = "hash" };
        var userRole = new UserRole { TenantId = _tenant, UserId = _user, RoleId = role.Id, IsActive = true };
        var sp1 = new SalesPoint { TenantId = _tenant, Code = "CH-01", Name = "Chi nhánh 1" };
        var sp2 = new SalesPoint { TenantId = _tenant, Code = "CH-02", Name = "Chi nhánh 2" };

        _db.Roles.Add(role);
        _db.Users.Add(appUser);
        _db.UserRoles.Add(userRole);
        _db.SalesPoints.AddRange(sp1, sp2);
        await _db.SaveChangesAsync();

        var ctx = await _svc.GetUserScopeContextAsync(_user);
        Assert.True(ctx.BypassDataScope);
        Assert.Equal(ScopeType.All, ctx.Scope);
        Assert.NotNull(ctx.AccessibleSalesPointIds);
        Assert.Equal(2, ctx.AccessibleSalesPointIds.Count);
    }

    [Fact]
    public async Task GetUserScopeContext_SalesPointScopedUser_ReturnsAssignedSalesPoints()
    {
        var appUser = new AppUser { Id = _user, TenantId = _tenant, Username = "sp-user", PasswordHash = "hash" };
        var sp1 = new SalesPoint { TenantId = _tenant, Code = "CH-01", Name = "Chi nhánh 1" };
        var dataScope = new UserDataScope { TenantId = _tenant, UserId = _user, Dimension = "SalesPoint", ScopeId = sp1.Id };

        _db.Users.Add(appUser);
        _db.SalesPoints.Add(sp1);
        _db.UserDataScopes.Add(dataScope);
        await _db.SaveChangesAsync();

        var ctx = await _svc.GetUserScopeContextAsync(_user);
        Assert.False(ctx.BypassDataScope);
        Assert.NotNull(ctx.AccessibleSalesPointIds);
        Assert.Single(ctx.AccessibleSalesPointIds);
        Assert.Equal(sp1.Id, ctx.AccessibleSalesPointIds[0]);
    }

    [Fact]
    public async Task UC_SYS_028_GetUserScopeContext_OrgUnitScopeUser_ReturnsAssignedOrgUnits()
    {
        var appUser = new AppUser { Id = _user, TenantId = _tenant, Username = "org-user", PasswordHash = "hash" };
        var org1 = new OrgUnit { TenantId = _tenant, Code = "CN_DN", Name = "Chi nhánh Đà Nẵng", Path = "/1/" };
        var dataScope = new UserDataScope { TenantId = _tenant, UserId = _user, Dimension = "OrgUnit", ScopeId = org1.Id };

        _db.Users.Add(appUser);
        _db.OrgUnits.Add(org1);
        _db.UserDataScopes.Add(dataScope);
        await _db.SaveChangesAsync();

        var ctx = await _svc.GetUserScopeContextAsync(_user);
        Assert.False(ctx.BypassDataScope);
        Assert.NotNull(ctx.AccessibleSalesPointIds);
        Assert.Contains(org1.Id, ctx.AccessibleSalesPointIds);
    }

    [Fact]
    public async Task UC_SYS_030_GetUserScopeContext_DepartmentScopeUser_ReturnsSubDepartmentTree()
    {
        var jl = new JobLevel { TenantId = _tenant, Code = "JL_MGR", Name = "Manager", DefaultScopeType = ScopeType.Department };
        var rootDept = new Department { TenantId = _tenant, Code = "D_KT", Name = "Phòng Kế Toán", Path = "/dept_root/" };
        var subDept = new Department { TenantId = _tenant, Code = "D_KT_HD", Name = "Bộ phận Hóa Đơn", ParentId = rootDept.Id, Path = "/dept_root/dept_sub/", IsActive = true };

        var appUser = new AppUser { Id = _user, TenantId = _tenant, Username = "dept-user", PasswordHash = "hash", DepartmentId = rootDept.Id, JobLevelId = jl.Id };
        var userDept = new UserDepartment { TenantId = _tenant, UserId = _user, DepartmentId = rootDept.Id, JobLevelId = jl.Id, IsPrimary = true };

        _db.JobLevels.Add(jl);
        _db.Departments.AddRange(rootDept, subDept);
        _db.Users.Add(appUser);
        _db.UserDepartments.Add(userDept);
        await _db.SaveChangesAsync();

        var ctx = await _svc.GetUserScopeContextAsync(_user);
        Assert.Equal(ScopeType.Department, ctx.Scope);
        Assert.Equal(rootDept.Id, ctx.DepartmentId);
        Assert.Contains(rootDept.Id, ctx.AccessibleDepartmentIds);
        Assert.Contains(subDept.Id, ctx.AccessibleDepartmentIds);
    }

    [Fact]
    public async Task UC_SYS_036_UpsertOrgUnit_CreatesAndUpdatesOrgUnitHierarchy()
    {
        var dummyScope = new DummyScopeService();
        var platform = new SysPlatformService(_db, new OutboxWriter(_db));
        var authz = new AuthorizationService(_db);
        var sysSvc = new SysMasterService(_db, dummyScope, authz, platform);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant,
            PlanCode = "ENTERPRISE",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)),
            MaxUsers = 10,
            MaxOrgUnits = 10,
            Status = "Active"
        });
        await _db.SaveChangesAsync();

        // Create parent branch
        var parentReq = new OrgUnitUpsertRequest(null, "HO", "Hội Sở Chính", null, "Company", true);
        var parent = await sysSvc.UpsertOrgUnitAsync(_tenant, _user, parentReq);
        Assert.Equal("HO", parent.Code);

        // Create child branch under parent
        var childReq = new OrgUnitUpsertRequest(null, "CN_HN", "Chi Nhánh Hà Nội", parent.Id, "Branch", true);
        var child = await sysSvc.UpsertOrgUnitAsync(_tenant, _user, childReq);
        Assert.Equal("CN_HN", child.Code);
        Assert.Equal(parent.Id, child.ParentId);

        var childInDb = await _db.OrgUnits.FirstAsync(o => o.Id == child.Id);
        Assert.Contains(parent.Id.ToString("N"), childInDb.Path);
    }

    [Fact]
    public async Task UC_SYS_028_GetUserScopeContext_MultiOrgUnitScopes_CombinesAllAssigned()
    {
        var appUser = new AppUser { Id = Guid.NewGuid(), TenantId = _tenant, Username = "multi-org-user", PasswordHash = "hash" };
        var org1 = new OrgUnit { TenantId = _tenant, Code = "CN_HCM", Name = "Chi nhánh TP.HCM", Path = "/1/" };
        var org2 = new OrgUnit { TenantId = _tenant, Code = "CN_CT", Name = "Chi nhánh Cần Thơ", Path = "/2/" };
        var scope1 = new UserDataScope { TenantId = _tenant, UserId = appUser.Id, Dimension = "OrgUnit", ScopeId = org1.Id };
        var scope2 = new UserDataScope { TenantId = _tenant, UserId = appUser.Id, Dimension = "OrgUnit", ScopeId = org2.Id };

        _db.Users.Add(appUser);
        _db.OrgUnits.AddRange(org1, org2);
        _db.UserDataScopes.AddRange(scope1, scope2);
        await _db.SaveChangesAsync();

        var ctx = await _svc.GetUserScopeContextAsync(appUser.Id);
        Assert.NotNull(ctx.AccessibleSalesPointIds);
        Assert.Equal(2, ctx.AccessibleSalesPointIds.Count);
        Assert.Contains(org1.Id, ctx.AccessibleSalesPointIds);
        Assert.Contains(org2.Id, ctx.AccessibleSalesPointIds);
    }

    [Fact]
    public async Task UC_SYS_028_GetUserScopeContext_DeletedOrgUnitScope_FilteredOut()
    {
        var appUser = new AppUser { Id = Guid.NewGuid(), TenantId = _tenant, Username = "deleted-scope-user", PasswordHash = "hash" };
        var orgActive = new OrgUnit { TenantId = _tenant, Code = "CN_HP", Name = "Chi nhánh Hải Phòng", Path = "/1/" };
        var scopeActive = new UserDataScope { TenantId = _tenant, UserId = appUser.Id, Dimension = "OrgUnit", ScopeId = orgActive.Id, IsDeleted = false };
        var scopeDeleted = new UserDataScope { TenantId = _tenant, UserId = appUser.Id, Dimension = "OrgUnit", ScopeId = Guid.NewGuid(), IsDeleted = true };

        _db.Users.Add(appUser);
        _db.OrgUnits.Add(orgActive);
        _db.UserDataScopes.AddRange(scopeActive, scopeDeleted);
        await _db.SaveChangesAsync();

        var ctx = await _svc.GetUserScopeContextAsync(appUser.Id);
        Assert.NotNull(ctx.AccessibleSalesPointIds);
        Assert.Single(ctx.AccessibleSalesPointIds);
        Assert.Equal(orgActive.Id, ctx.AccessibleSalesPointIds[0]);
    }

    [Fact]
    public async Task UC_SYS_029_GetUserScopeContext_SalesPointAndOrgUnitScopes_UnionOfScopes()
    {
        var appUser = new AppUser { Id = Guid.NewGuid(), TenantId = _tenant, Username = "union-user", PasswordHash = "hash" };
        var sp = new SalesPoint { TenantId = _tenant, Code = "SP_01", Name = "Điểm bán 01" };
        var org = new OrgUnit { TenantId = _tenant, Code = "CN_VT", Name = "Chi nhánh Vũng Tàu", Path = "/vt/" };
        var dsSp = new UserDataScope { TenantId = _tenant, UserId = appUser.Id, Dimension = "SalesPoint", ScopeId = sp.Id };
        var dsOrg = new UserDataScope { TenantId = _tenant, UserId = appUser.Id, Dimension = "OrgUnit", ScopeId = org.Id };

        _db.Users.Add(appUser);
        _db.SalesPoints.Add(sp);
        _db.OrgUnits.Add(org);
        _db.UserDataScopes.AddRange(dsSp, dsOrg);
        await _db.SaveChangesAsync();

        var ctx = await _svc.GetUserScopeContextAsync(appUser.Id);
        Assert.NotNull(ctx.AccessibleSalesPointIds);
        Assert.Equal(2, ctx.AccessibleSalesPointIds.Count);
        Assert.Contains(sp.Id, ctx.AccessibleSalesPointIds);
        Assert.Contains(org.Id, ctx.AccessibleSalesPointIds);
    }

    [Fact]
    public async Task UC_SYS_029_GetUserScopeContext_NonExistentUser_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.GetUserScopeContextAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UC_SYS_030_GetUserScopeContext_Deep4LevelHierarchy_TraversesAllLevels()
    {
        var jl = new JobLevel { TenantId = _tenant, Code = "JL_DIR", Name = "Director", DefaultScopeType = ScopeType.Department };
        var l1 = new Department { TenantId = _tenant, Code = "D_LV1", Name = "Khối Công Nghệ", Path = "/l1/" };
        var l2 = new Department { TenantId = _tenant, Code = "D_LV2", Name = "Trung Tâm Phần Mềm", ParentId = l1.Id, Path = "/l1/l2/", IsActive = true };
        var l3 = new Department { TenantId = _tenant, Code = "D_LV3", Name = "Phòng Core Banking", ParentId = l2.Id, Path = "/l1/l2/l3/", IsActive = true };
        var l4 = new Department { TenantId = _tenant, Code = "D_LV4", Name = "Nhóm Payment", ParentId = l3.Id, Path = "/l1/l2/l3/l4/", IsActive = true };

        var user = new AppUser { Id = Guid.NewGuid(), TenantId = _tenant, Username = "dir-user", PasswordHash = "hash", DepartmentId = l1.Id, JobLevelId = jl.Id };
        var uDept = new UserDepartment { TenantId = _tenant, UserId = user.Id, DepartmentId = l1.Id, JobLevelId = jl.Id, IsPrimary = true };

        _db.JobLevels.Add(jl);
        _db.Departments.AddRange(l1, l2, l3, l4);
        _db.Users.Add(user);
        _db.UserDepartments.Add(uDept);
        await _db.SaveChangesAsync();

        var ctx = await _svc.GetUserScopeContextAsync(user.Id);
        Assert.Equal(4, ctx.AccessibleDepartmentIds.Count);
        Assert.Contains(l1.Id, ctx.AccessibleDepartmentIds);
        Assert.Contains(l2.Id, ctx.AccessibleDepartmentIds);
        Assert.Contains(l3.Id, ctx.AccessibleDepartmentIds);
        Assert.Contains(l4.Id, ctx.AccessibleDepartmentIds);
    }

    [Fact]
    public async Task UC_SYS_030_GetUserScopeContext_OwnScopeType_ReturnsEmptySubDepartments()
    {
        var jl = new JobLevel { TenantId = _tenant, Code = "JL_STAFF", Name = "Staff", DefaultScopeType = ScopeType.Own };
        var dept = new Department { TenantId = _tenant, Code = "D_HR", Name = "Phòng Nhân Sự", Path = "/hr/" };
        var subDept = new Department { TenantId = _tenant, Code = "D_HR_REC", Name = "Bộ phận Tuyển Dụng", ParentId = dept.Id, Path = "/hr/rec/", IsActive = true };

        var user = new AppUser { Id = Guid.NewGuid(), TenantId = _tenant, Username = "staff-user", PasswordHash = "hash", DepartmentId = dept.Id, JobLevelId = jl.Id };
        var uDept = new UserDepartment { TenantId = _tenant, UserId = user.Id, DepartmentId = dept.Id, JobLevelId = jl.Id, IsPrimary = true };

        _db.JobLevels.Add(jl);
        _db.Departments.AddRange(dept, subDept);
        _db.Users.Add(user);
        _db.UserDepartments.Add(uDept);
        await _db.SaveChangesAsync();

        var ctx = await _svc.GetUserScopeContextAsync(user.Id);
        Assert.Equal(ScopeType.Own, ctx.Scope);
        Assert.Empty(ctx.AccessibleDepartmentIds);
    }

    [Fact]
    public async Task UC_SYS_036_UpsertOrgUnit_DuplicateCode_ThrowsAppException()
    {
        var dummyScope = new DummyScopeService();
        var platform = new SysPlatformService(_db, new OutboxWriter(_db));
        var authz = new AuthorizationService(_db);
        var sysSvc = new SysMasterService(_db, dummyScope, authz, platform);

        _db.Licenses.Add(new License { TenantId = _tenant, PlanCode = "ENTERPRISE", MaxOrgUnits = 10, Status = "Active" });
        _db.OrgUnits.Add(new OrgUnit { TenantId = _tenant, Code = "CN_DUPLICATE", Name = "Chi nhánh A", Path = "/a/" });
        await _db.SaveChangesAsync();

        var req = new OrgUnitUpsertRequest(null, "CN_DUPLICATE", "Chi nhánh B", null, "Branch", true);
        var ex = await Assert.ThrowsAsync<AppException>(() => sysSvc.UpsertOrgUnitAsync(_tenant, _user, req));
        Assert.Contains("đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_036_UpsertOrgUnit_SelfParent_ThrowsAppException()
    {
        var dummyScope = new DummyScopeService();
        var platform = new SysPlatformService(_db, new OutboxWriter(_db));
        var authz = new AuthorizationService(_db);
        var sysSvc = new SysMasterService(_db, dummyScope, authz, platform);

        _db.Licenses.Add(new License { TenantId = _tenant, PlanCode = "ENTERPRISE", MaxOrgUnits = 10, Status = "Active" });
        var org = new OrgUnit { TenantId = _tenant, Code = "CN_SELF", Name = "Chi nhánh Self", Path = "/self/" };
        _db.OrgUnits.Add(org);
        await _db.SaveChangesAsync();

        var req = new OrgUnitUpsertRequest(org.Id, "CN_SELF", "Chi nhánh Self Updated", org.Id, "Branch", true);
        var ex = await Assert.ThrowsAsync<AppException>(() => sysSvc.UpsertOrgUnitAsync(_tenant, _user, req));
        Assert.Contains("không thể làm đơn vị cấp trên của chính nó", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_036_UpsertOrgUnit_CircularParent_ThrowsAppException()
    {
        var dummyScope = new DummyScopeService();
        var platform = new SysPlatformService(_db, new OutboxWriter(_db));
        var authz = new AuthorizationService(_db);
        var sysSvc = new SysMasterService(_db, dummyScope, authz, platform);

        _db.Licenses.Add(new License { TenantId = _tenant, PlanCode = "ENTERPRISE", MaxOrgUnits = 10, Status = "Active" });
        var parent = new OrgUnit { TenantId = _tenant, Code = "ORG_P", Name = "Cha", Path = "/p/" };
        _db.OrgUnits.Add(parent);
        await _db.SaveChangesAsync();

        parent.Path = $"/{parent.Id:N}/";
        var child = new OrgUnit { TenantId = _tenant, Code = "ORG_C", Name = "Con", ParentId = parent.Id, Path = $"/{parent.Id:N}/{Guid.NewGuid():N}/" };
        _db.OrgUnits.Add(child);
        await _db.SaveChangesAsync();

        // Attempting to set child as parent of parent
        var req = new OrgUnitUpsertRequest(parent.Id, "ORG_P", "Cha Updated", child.Id, "Branch", true);
        var ex = await Assert.ThrowsAsync<AppException>(() => sysSvc.UpsertOrgUnitAsync(_tenant, _user, req));
        Assert.Contains("vòng lặp phân cấp", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_036_UpsertOrgUnit_ExceedMaxOrgUnitsLicense_ThrowsAppException()
    {
        var dummyScope = new DummyScopeService();
        var platform = new SysPlatformService(_db, new OutboxWriter(_db));
        var authz = new AuthorizationService(_db);
        var sysSvc = new SysMasterService(_db, dummyScope, authz, platform);

        // License allows only 1 OrgUnit
        _db.Licenses.Add(new License { TenantId = _tenant, PlanCode = "BASIC", MaxOrgUnits = 1, Status = "Active" });
        _db.OrgUnits.Add(new OrgUnit { TenantId = _tenant, Code = "ORG_LIMIT_1", Name = "Org 1", Path = "/1/" });
        await _db.SaveChangesAsync();

        var req = new OrgUnitUpsertRequest(null, "ORG_LIMIT_2", "Org 2 Exceed", null, "Branch", true);
        var ex = await Assert.ThrowsAsync<AppException>(() => sysSvc.UpsertOrgUnitAsync(_tenant, _user, req));
        Assert.Contains("đạt giới hạn", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_036_UpsertOrgUnit_UpdateParent_RecursivelyUpdatesChildrenPath()
    {
        var dummyScope = new DummyScopeService();
        var platform = new SysPlatformService(_db, new OutboxWriter(_db));
        var authz = new AuthorizationService(_db);
        var sysSvc = new SysMasterService(_db, dummyScope, authz, platform);

        _db.Licenses.Add(new License { TenantId = _tenant, PlanCode = "ENTERPRISE", MaxOrgUnits = 20, Status = "Active" });
        await _db.SaveChangesAsync();

        // 1. Create Head Office (HO1)
        var ho1 = await sysSvc.UpsertOrgUnitAsync(_tenant, _user, new OrgUnitUpsertRequest(null, "HO_1", "Hội sở 1", null, "Company", true));
        // 2. Create Branch 1 under HO1
        var b1 = await sysSvc.UpsertOrgUnitAsync(_tenant, _user, new OrgUnitUpsertRequest(null, "B_1", "Chi nhánh 1", ho1.Id, "Branch", true));
        // 3. Create Sub-branch 1.1 under Branch 1
        var sub1 = await sysSvc.UpsertOrgUnitAsync(_tenant, _user, new OrgUnitUpsertRequest(null, "SUB_1", "Điểm 1.1", b1.Id, "Branch", true));

        // Create Head Office 2 (HO2)
        var ho2 = await sysSvc.UpsertOrgUnitAsync(_tenant, _user, new OrgUnitUpsertRequest(null, "HO_2", "Hội sở 2", null, "Company", true));

        // Move Branch 1 to be under Head Office 2 (HO2)
        await sysSvc.UpsertOrgUnitAsync(_tenant, _user, new OrgUnitUpsertRequest(b1.Id, "B_1", "Chi nhánh 1 Moved", ho2.Id, "Branch", true));

        // Verify Sub-branch 1.1 path has been recursively updated to start with HO2's path
        var sub1Db = await _db.OrgUnits.FirstAsync(x => x.Id == sub1.Id);
        Assert.Contains(ho2.Id.ToString("N"), sub1Db.Path);
    }

    private sealed class DummyScopeService : IDataScopeService
    {
        public Task<UserScopeContext> GetUserScopeContextAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(new UserScopeContext(ScopeType.All, true, userId, null, Array.Empty<Guid>(), null));
    }
}

