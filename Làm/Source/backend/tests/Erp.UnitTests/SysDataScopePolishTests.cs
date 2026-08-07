using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Implementations.Services.Auth;
using Erp.Infrastructure.Persistence;
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
}
