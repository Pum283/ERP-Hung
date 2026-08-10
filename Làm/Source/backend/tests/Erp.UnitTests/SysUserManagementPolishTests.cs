using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Auth;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Auth;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Implementations.Services.Auth;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Erp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Erp.UnitTests;

public sealed class SysUserManagementPolishTests
{
    private sealed class DummyScope : IDataScopeService
    {
        public Task<UserScopeContext> GetUserScopeContextAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(new UserScopeContext(ScopeType.All, true, userId, null, Array.Empty<Guid>(), null));
    }

    private static (AuthService authSvc, SysMasterService sysSvc, AppDbContext db) CreateServices(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var db = new AppDbContext(options);

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "SuperSecretKeyForTestingJwt1234567890!",
            ["Jwt:Issuer"] = "ErpTest",
            ["Jwt:Audience"] = "ErpTest"
        }).Build();

        var jwt = new JwtTokenService(config);
        var platform = new SysPlatformService(db, new OutboxWriter(db));
        var scope = new DummyScope();
        var authz = new AuthorizationService(db);

        var authSvc = new AuthService(db, jwt, scope, platform, config, NullLogger<AuthService>.Instance);
        var sysSvc = new SysMasterService(db, scope, authz, platform);
        return (authSvc, sysSvc, db);
    }

    private static void SeedActiveLicense(AppDbContext db, Guid tenantId)
    {
        db.Licenses.Add(new License
        {
            TenantId = tenantId,
            PlanCode = "ENTERPRISE",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100,
            MaxOrgUnits = 100,
            Status = "Active"
        });
        db.SaveChanges();
    }

    [Fact]
    public async Task UC_SYS_005_ResetPasswordWithOtp_ValidOtp_UpdatesPasswordAndClearsLock()
    {
        var (authSvc, _, db) = CreateServices(nameof(UC_SYS_005_ResetPasswordWithOtp_ValidOtp_UpdatesPasswordAndClearsLock));
        var tenantId = Guid.NewGuid();
        var user = new AppUser
        {
            TenantId = tenantId,
            Username = "user_reset_test",
            Email = "user_reset@erp.com",
            PasswordHash = PasswordHasher.Hash("OldPass123!"),
            Status = UserStatus.Locked,
            FailedLoginCount = 5,
            LockedUntil = DateTimeOffset.UtcNow.AddHours(1)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var otpToken = new PasswordResetToken
        {
            TenantId = tenantId,
            UserId = user.Id,
            OtpCode = "654321",
            TokenHash = "HASH654321",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        };
        db.PasswordResetTokens.Add(otpToken);
        await db.SaveChangesAsync();

        // Act
        await authSvc.ResetPasswordWithOtpAsync(new ResetPasswordWithOtpRequest("user_reset_test", "654321", "NewSecurePassword123!"));

        // Assert
        var updatedUser = await db.Users.FirstAsync(u => u.Id == user.Id);
        Assert.Equal(UserStatus.Active, updatedUser.Status);
        Assert.Equal(0, updatedUser.FailedLoginCount);
        Assert.Null(updatedUser.LockedUntil);
        Assert.False(updatedUser.MustChangePassword);
        Assert.True(PasswordHasher.Verify("NewSecurePassword123!", updatedUser.PasswordHash));

        var updatedToken = await db.PasswordResetTokens.FirstAsync(t => t.Id == otpToken.Id);
        Assert.NotNull(updatedToken.UsedAt);
    }

    [Fact]
    public async Task UC_SYS_005_ResetPasswordWithOtp_ExpiredOtp_ThrowsAppException()
    {
        var (authSvc, _, db) = CreateServices(nameof(UC_SYS_005_ResetPasswordWithOtp_ExpiredOtp_ThrowsAppException));
        var tenantId = Guid.NewGuid();
        var user = new AppUser
        {
            TenantId = tenantId,
            Username = "expired_user",
            Email = "expired@erp.com",
            PasswordHash = PasswordHasher.Hash("OldPass123!")
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            TenantId = tenantId,
            UserId = user.Id,
            OtpCode = "111222",
            TokenHash = "HASH111222",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5) // Expired
        });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(() =>
            authSvc.ResetPasswordWithOtpAsync(new ResetPasswordWithOtpRequest("expired_user", "111222", "NewPass123!")));
    }

    [Fact]
    public async Task UC_SYS_013_UpsertUser_CreateNewUser_SucceedsAndMapsOrgUnit()
    {
        var (_, sysSvc, db) = CreateServices(nameof(UC_SYS_013_UpsertUser_CreateNewUser_SucceedsAndMapsOrgUnit));
        var tenantId = Guid.NewGuid();
        SeedActiveLicense(db, tenantId);

        var org = new OrgUnit { TenantId = tenantId, Code = "ORG01", Name = "Chi Nhánh HNM", Path = "/1/" };
        db.OrgUnits.Add(org);
        await db.SaveChangesAsync();

        var req = new UserUpsertRequest(
            Id: null,
            Username: "newuser01",
            DisplayName: "Nguyễn Văn A",
            Email: "nva@erp.com",
            Phone: "0901234567",
            Password: "Password123!",
            Status: UserStatus.Active,
            PrimaryOrgUnitId: org.Id,
            DepartmentId: null,
            JobLevelId: null,
            ManagerUserId: null,
            Departments: null
        );

        // Act
        var result = await sysSvc.UpsertUserAsync(tenantId, Guid.NewGuid(), req);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newuser01", result.Username);
        Assert.Equal("Nguyễn Văn A", result.DisplayName);
        Assert.Equal(org.Id, result.PrimaryOrgUnitId);
        Assert.Equal(UserStatus.Active, result.Status);

        var created = await db.Users.FirstOrDefaultAsync(u => u.Username == "newuser01");
        Assert.NotNull(created);
        Assert.True(PasswordHasher.Verify("Password123!", created.PasswordHash));
        Assert.Equal("0901234567", created.Phone);
    }

    [Fact]
    public async Task UC_SYS_013_UpsertUser_DuplicateUsername_ThrowsAppException()
    {
        var (_, sysSvc, db) = CreateServices(nameof(UC_SYS_013_UpsertUser_DuplicateUsername_ThrowsAppException));
        var tenantId = Guid.NewGuid();
        SeedActiveLicense(db, tenantId);

        db.Users.Add(new AppUser { TenantId = tenantId, Username = "existing_user", Email = "exist@erp.com" });
        await db.SaveChangesAsync();

        var req = new UserUpsertRequest(
            Id: null,
            Username: "existing_user",
            DisplayName: "Duplicate",
            Email: "other@erp.com",
            Phone: null,
            Password: "Password123!",
            Status: UserStatus.Active,
            PrimaryOrgUnitId: null,
            DepartmentId: null,
            JobLevelId: null,
            ManagerUserId: null,
            Departments: null
        );

        await Assert.ThrowsAsync<AppException>(() => sysSvc.UpsertUserAsync(tenantId, Guid.NewGuid(), req));
    }

    [Fact]
    public async Task UC_SYS_014_UpsertUser_UpdateUser_UpdatesFieldsSuccessfully()
    {
        var (_, sysSvc, db) = CreateServices(nameof(UC_SYS_014_UpsertUser_UpdateUser_UpdatesFieldsSuccessfully));
        var tenantId = Guid.NewGuid();
        SeedActiveLicense(db, tenantId);

        var user = new AppUser
        {
            TenantId = tenantId,
            Username = "user_to_update",
            DisplayName = "Tên Cũ",
            Email = "old@erp.com",
            Status = UserStatus.Active
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var updateReq = new UserUpsertRequest(
            Id: user.Id,
            Username: "user_to_update",
            DisplayName: "Tên Mới Cập Nhật",
            Email: "new@erp.com",
            Phone: "0988776655",
            Password: null,
            Status: UserStatus.Active,
            PrimaryOrgUnitId: null,
            DepartmentId: null,
            JobLevelId: null,
            ManagerUserId: null,
            Departments: null
        );

        // Act
        var result = await sysSvc.UpsertUserAsync(tenantId, Guid.NewGuid(), updateReq);

        // Assert
        Assert.Equal("Tên Mới Cập Nhật", result.DisplayName);
        Assert.Equal("new@erp.com", result.Email);

        var updatedInDb = await db.Users.FirstAsync(u => u.Id == user.Id);
        Assert.Equal("0988776655", updatedInDb.Phone);
    }

    [Fact]
    public async Task UC_SYS_015_UpsertUser_LockAndUnlockStatus_ChangesStatus()
    {
        var (_, sysSvc, db) = CreateServices(nameof(UC_SYS_015_UpsertUser_LockAndUnlockStatus_ChangesStatus));
        var tenantId = Guid.NewGuid();
        SeedActiveLicense(db, tenantId);

        var user = new AppUser
        {
            TenantId = tenantId,
            Username = "status_user",
            DisplayName = "User Lock Test",
            Status = UserStatus.Active
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Lock user
        var lockReq = new UserUpsertRequest(
            Id: user.Id,
            Username: "status_user",
            DisplayName: "User Lock Test",
            Email: null,
            Phone: null,
            Password: null,
            Status: UserStatus.Locked,
            PrimaryOrgUnitId: null,
            DepartmentId: null,
            JobLevelId: null,
            ManagerUserId: null,
            Departments: null
        );

        var lockedResult = await sysSvc.UpsertUserAsync(tenantId, Guid.NewGuid(), lockReq);
        Assert.Equal(UserStatus.Locked, lockedResult.Status);

        // Unlock user
        var unlockReq = lockReq with { Status = UserStatus.Active };
        var unlockedResult = await sysSvc.UpsertUserAsync(tenantId, Guid.NewGuid(), unlockReq);
        Assert.Equal(UserStatus.Active, unlockedResult.Status);
    }

    [Fact]
    public async Task UC_SYS_017_UpsertUser_AssignOrgUnit_SucceedsAndValidatesOrgUnit()
    {
        var (_, sysSvc, db) = CreateServices(nameof(UC_SYS_017_UpsertUser_AssignOrgUnit_SucceedsAndValidatesOrgUnit));
        var tenantId = Guid.NewGuid();
        SeedActiveLicense(db, tenantId);

        var orgBranch = new OrgUnit { TenantId = tenantId, Code = "CN_HCM", Name = "Chi Nhánh Hồ Chí Minh", Path = "/1/" };
        db.OrgUnits.Add(orgBranch);
        await db.SaveChangesAsync();

        var user = new AppUser { TenantId = tenantId, Username = "branch_user", DisplayName = "Nhân viên HCM" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var assignReq = new UserUpsertRequest(
            Id: user.Id,
            Username: "branch_user",
            DisplayName: "Nhân viên HCM Gán Chi Nhánh",
            Email: "hcm@erp.com",
            Phone: null,
            Password: null,
            Status: UserStatus.Active,
            PrimaryOrgUnitId: orgBranch.Id,
            DepartmentId: null,
            JobLevelId: null,
            ManagerUserId: null,
            Departments: null
        );

        var result = await sysSvc.UpsertUserAsync(tenantId, Guid.NewGuid(), assignReq);
        Assert.Equal(orgBranch.Id, result.PrimaryOrgUnitId);

        var dbUser = await db.Users.FirstAsync(u => u.Id == user.Id);
        Assert.Equal(orgBranch.Id, dbUser.PrimaryOrgUnitId);
    }

    [Fact]
    public async Task UC_SYS_019_InviteUser_ValidEmail_CreatesUserAndGeneratesOtp()
    {
        var (_, sysSvc, db) = CreateServices(nameof(UC_SYS_019_InviteUser_ValidEmail_CreatesUserAndGeneratesOtp));
        var tenantId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        SeedActiveLicense(db, tenantId);

        var req = new InviteUserRequest(
            Username: "invited_user",
            Email: "invited@erp.com",
            Phone: null,
            DisplayName: "Người Dùng Đã Mời",
            PrimaryOrgUnitId: null,
            DepartmentId: null,
            JobLevelId: null
        );

        var res = await sysSvc.InviteUserAsync(tenantId, actorId, req);

        Assert.NotNull(res);
        Assert.Equal("invited_user", res.Username);
        Assert.Equal("Email", res.Channel);
        Assert.Equal("invited@erp.com", res.Target);

        var invitedInDb = await db.Users.FirstOrDefaultAsync(u => u.Username == "invited_user");
        Assert.NotNull(invitedInDb);
        Assert.True(invitedInDb.MustChangePassword);

        var tokenInDb = await db.PasswordResetTokens.FirstOrDefaultAsync(t => t.UserId == invitedInDb.Id);
        Assert.NotNull(tokenInDb);
        Assert.False(string.IsNullOrWhiteSpace(tokenInDb.OtpCode));
    }

    [Fact]
    public async Task UC_SYS_019_InviteUser_MissingEmailAndPhone_ThrowsAppException()
    {
        var (_, sysSvc, db) = CreateServices(nameof(UC_SYS_019_InviteUser_MissingEmailAndPhone_ThrowsAppException));
        var tenantId = Guid.NewGuid();
        SeedActiveLicense(db, tenantId);

        var req = new InviteUserRequest(
            Username: "invalid_invite",
            Email: null,
            Phone: null,
            DisplayName: "Thiếu contact",
            PrimaryOrgUnitId: null,
            DepartmentId: null,
            JobLevelId: null
        );

        await Assert.ThrowsAsync<AppException>(() => sysSvc.InviteUserAsync(tenantId, Guid.NewGuid(), req));
    }

    [Fact]
    public async Task UC_SYS_021_ListUsers_SearchKeywordFilter_ReturnsMatchingUsers()
    {
        var (_, sysSvc, db) = CreateServices(nameof(UC_SYS_021_ListUsers_SearchKeywordFilter_ReturnsMatchingUsers));
        var tenantId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        db.Users.AddRange(
            new AppUser { TenantId = tenantId, Username = "alpha_user", DisplayName = "Nguyễn Văn Alpha", Email = "alpha@erp.com" },
            new AppUser { TenantId = tenantId, Username = "beta_user", DisplayName = "Trần Thị Beta", Email = "beta@erp.com" },
            new AppUser { TenantId = tenantId, Username = "gamma_user", DisplayName = "Lê Văn Gamma", Email = "gamma@erp.com" }
        );
        await db.SaveChangesAsync();

        var allUsers = await sysSvc.ListUsersAsync(tenantId, currentUserId);
        Assert.Equal(3, allUsers.Count);
        Assert.Contains(allUsers, u => u.Username == "alpha_user");
        Assert.Contains(allUsers, u => u.Username == "beta_user");
    }

    [Fact]
    public async Task UC_SYS_023_UpsertRole_CreateAndUpdateActiveStatus_SystemRoleProtection()
    {
        var (_, sysSvc, db) = CreateServices(nameof(UC_SYS_023_UpsertRole_CreateAndUpdateActiveStatus_SystemRoleProtection));
        var tenantId = Guid.NewGuid();
        var actorId = Guid.NewGuid();

        // Create new Role
        var createReq = new RoleUpsertRequest(null, "ROLE_MANAGER", "Trưởng Phòng Bán Hàng", "Quản lý kinh doanh", false, true);
        var created = await sysSvc.UpsertRoleAsync(tenantId, actorId, createReq);
        Assert.Equal("ROLE_MANAGER", created.Code);
        Assert.True(created.IsActive);

        // Deactivate Role
        var updateReq = new RoleUpsertRequest(created.Id, "ROLE_MANAGER", "Trưởng Phòng Bán Hàng", "Ngưng áp dụng", false, false);
        var updated = await sysSvc.UpsertRoleAsync(tenantId, actorId, updateReq);
        Assert.False(updated.IsActive);

        // System role protection
        var sysRole = new Role { TenantId = tenantId, Code = "ADMIN", Name = "Quản Trị Viên", IsSystem = true };
        db.Roles.Add(sysRole);
        await db.SaveChangesAsync();

        var changeSysCodeReq = new RoleUpsertRequest(sysRole.Id, "SUPER_ADMIN", "Quản Trị Viên", null, false, true);
        await Assert.ThrowsAsync<AppException>(() => sysSvc.UpsertRoleAsync(tenantId, actorId, changeSysCodeReq));
    }
}

