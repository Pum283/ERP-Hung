using System.Security.Cryptography;
using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Auth;
using Erp.Application.Interfaces.Services.Sys;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Persistence;
using Erp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Sys;

public sealed class SysMasterService : ISysMasterService
{
    private readonly AppDbContext _db;
    private readonly IDataScopeService _scope;
    private readonly IAuthorizationService _authz;
    private readonly ISysPlatformService _platform;

    public SysMasterService(AppDbContext db, IDataScopeService scope, IAuthorizationService authz, ISysPlatformService platform)
    {
        _db = db;
        _scope = scope;
        _authz = authz;
        _platform = platform;
    }

    public async Task<IReadOnlyList<OrgUnitDto>> ListOrgUnitsAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _db.OrgUnits.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
            .Select(x => new OrgUnitDto(x.Id, x.Code, x.Name, x.ParentId, x.UnitType, x.IsActive))
            .ToListAsync(ct);
    }

    public async Task<OrgUnitDto> UpsertOrgUnitAsync(Guid tenantId, Guid? userId, OrgUnitUpsertRequest req, CancellationToken ct = default)
    {
        OrgUnit entity;
        if (req.Id is Guid id)
        {
            entity = await _db.OrgUnits.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("OrgUnit không tồn tại.", 404);
        }
        else
        {
            await _platform.EnsureMaxOrgUnitsAsync(tenantId, ct);
            entity = new OrgUnit { TenantId = tenantId, CreatedBy = userId, Path = "/" };
            _db.OrgUnits.Add(entity);
        }

        entity.Code = req.Code.Trim();
        entity.Name = req.Name.Trim();
        entity.ParentId = req.ParentId;
        entity.UnitType = req.UnitType;
        entity.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);

        var parentPath = req.ParentId is Guid pid
            ? await _db.OrgUnits.Where(x => x.Id == pid).Select(x => x.Path).FirstAsync(ct)
            : "/";
        entity.Path = parentPath == "/" ? $"/{entity.Id:N}/" : parentPath + $"{entity.Id:N}/";
        await _db.SaveChangesAsync(ct);

        return new OrgUnitDto(entity.Id, entity.Code, entity.Name, entity.ParentId, entity.UnitType, entity.IsActive);
    }

    public async Task<IReadOnlyList<DepartmentDto>> ListDepartmentsAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _db.Departments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
            .Select(x => new DepartmentDto(x.Id, x.Code, x.Name, x.ParentId, x.OrgUnitId, x.ManagerUserId, x.IsActive))
            .ToListAsync(ct);
    }

    public async Task<DepartmentDto> UpsertDepartmentAsync(Guid tenantId, Guid? userId, DepartmentUpsertRequest req, CancellationToken ct = default)
    {
        Department entity;
        if (req.Id is Guid id)
        {
            entity = await _db.Departments.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("Department không tồn tại.", 404);
        }
        else
        {
            entity = new Department { TenantId = tenantId, CreatedBy = userId };
            _db.Departments.Add(entity);
        }

        entity.Code = req.Code.Trim();
        entity.Name = req.Name.Trim();
        entity.ParentId = req.ParentId;
        entity.OrgUnitId = req.OrgUnitId;
        entity.ManagerUserId = req.ManagerUserId;
        entity.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);

        entity.Path = req.ParentId is Guid pid
            ? (await _db.Departments.Where(x => x.Id == pid).Select(x => x.Path).FirstAsync(ct)) + $"{entity.Id:N}/"
            : $"/{entity.Id:N}/";
        await _db.SaveChangesAsync(ct);

        return new DepartmentDto(entity.Id, entity.Code, entity.Name, entity.ParentId, entity.OrgUnitId, entity.ManagerUserId, entity.IsActive);
    }

    public async Task<IReadOnlyList<JobLevelDto>> ListJobLevelsAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _db.JobLevels.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.LevelOrder)
            .Select(x => new JobLevelDto(x.Id, x.Code, x.Name, x.LevelOrder, x.DefaultScopeType, x.IsActive))
            .ToListAsync(ct);
    }

    public async Task<JobLevelDto> UpsertJobLevelAsync(Guid tenantId, Guid? userId, JobLevelUpsertRequest req, CancellationToken ct = default)
    {
        JobLevel entity;
        if (req.Id is Guid id)
        {
            entity = await _db.JobLevels.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("JobLevel không tồn tại.", 404);
        }
        else
        {
            entity = new JobLevel { TenantId = tenantId, CreatedBy = userId };
            _db.JobLevels.Add(entity);
        }

        entity.Code = req.Code.Trim();
        entity.Name = req.Name.Trim();
        entity.LevelOrder = req.LevelOrder;
        entity.DefaultScopeType = req.DefaultScopeType;
        entity.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return new JobLevelDto(entity.Id, entity.Code, entity.Name, entity.LevelOrder, entity.DefaultScopeType, entity.IsActive);
    }

    public async Task<IReadOnlyList<RoleDto>> ListRolesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var roles = await _db.Roles.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
            .ToListAsync(ct);
        var roleIds = roles.Select(r => r.Id).ToList();
        var perms = await _db.RolePermissions.AsNoTracking()
            .Where(x => roleIds.Contains(x.RoleId) && !x.IsDeleted)
            .GroupBy(x => x.RoleId)
            .ToDictionaryAsync(g => g.Key, g => (IReadOnlyList<Guid>)g.Select(x => x.PermissionId).ToList(), ct);

        return roles.Select(r => new RoleDto(
            r.Id, r.Code, r.Name, r.BypassDataScope, r.IsSystem, r.IsActive,
            perms.GetValueOrDefault(r.Id) ?? Array.Empty<Guid>())).ToList();
    }

    public async Task<RoleDto> UpsertRoleAsync(Guid tenantId, Guid? userId, RoleUpsertRequest req, CancellationToken ct = default)
    {
        Role entity;
        if (req.Id is Guid id)
        {
            entity = await _db.Roles.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("Role không tồn tại.", 404);
            if (entity.IsSystem && !string.Equals(entity.Code, req.Code, StringComparison.OrdinalIgnoreCase))
                throw new AppException("Không đổi mã role hệ thống.");
        }
        else
        {
            entity = new Role { TenantId = tenantId, CreatedBy = userId };
            _db.Roles.Add(entity);
        }

        entity.Code = req.Code.Trim();
        entity.Name = req.Name.Trim();
        entity.Description = req.Description;
        entity.BypassDataScope = req.BypassDataScope;
        entity.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);

        var permIds = await _db.RolePermissions.AsNoTracking()
            .Where(x => x.RoleId == entity.Id && !x.IsDeleted)
            .Select(x => x.PermissionId).ToListAsync(ct);
        return new RoleDto(entity.Id, entity.Code, entity.Name, entity.BypassDataScope, entity.IsSystem, entity.IsActive, permIds);
    }

    public async Task SetRolePermissionsAsync(Guid tenantId, Guid roleId, IReadOnlyList<Guid> permissionIds, CancellationToken ct = default)
    {
        _ = await _db.Roles.FirstOrDefaultAsync(x => x.Id == roleId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Role không tồn tại.", 404);

        var existing = await _db.RolePermissions.Where(x => x.RoleId == roleId && !x.IsDeleted).ToListAsync(ct);
        _db.RolePermissions.RemoveRange(existing);

        foreach (var pid in permissionIds.Distinct())
        {
            _db.RolePermissions.Add(new RolePermission
            {
                TenantId = tenantId,
                RoleId = roleId,
                PermissionId = pid
            });
        }

        _db.PermissionChangeLogs.Add(new PermissionChangeLog
        {
            TenantId = tenantId,
            ChangeType = "SetRolePermissions",
            RoleId = roleId,
            DetailJson = $"{{\"count\":{permissionIds.Count}}}"
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<PermissionDto>> ListPermissionsAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var q = _db.Permissions.AsNoTracking().Where(x => !x.IsDeleted);
        if (!includeInactive) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.ModuleCode).ThenBy(x => x.Code)
            .Select(x => new PermissionDto(x.Id, x.ModuleCode, x.Code, x.Name, x.Resource, x.Action, x.Description, x.IsActive))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<UserDto>> ListUsersAsync(Guid tenantId, Guid currentUserId, CancellationToken ct = default)
    {
        var scope = await _scope.GetUserScopeContextAsync(currentUserId, ct);
        var query = _db.Users.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);

        query = scope.Scope switch
        {
            ScopeType.Own => query.Where(u => u.Id == currentUserId),
            ScopeType.Team => query.Where(u => u.Id == currentUserId || u.ManagerUserId == currentUserId),
            ScopeType.Department => query.Where(u =>
                (u.DepartmentId != null && scope.AccessibleDepartmentIds.Contains(u.DepartmentId.Value))
                || _db.UserDepartments.Any(ud =>
                    ud.UserId == u.Id && !ud.IsDeleted
                    && scope.AccessibleDepartmentIds.Contains(ud.DepartmentId))),
            _ => query
        };

        var users = await query.OrderBy(x => x.Username).ToListAsync(ct);
        return await MapUsersAsync(tenantId, users, ct);
    }

    public async Task<UserDto> UpsertUserAsync(Guid tenantId, Guid? actorId, UserUpsertRequest req, CancellationToken ct = default)
    {
        AppUser entity;
        if (req.Id is Guid id)
        {
            entity = await _db.Users.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("User không tồn tại.", 404);
        }
        else
        {
            if (await _db.Users.AnyAsync(x => x.TenantId == tenantId && x.Username == req.Username && !x.IsDeleted, ct))
                throw new AppException("Username đã tồn tại.");
            await _platform.EnsureMaxUsersAsync(tenantId, ct);
            entity = new AppUser { TenantId = tenantId, CreatedBy = actorId };
            _db.Users.Add(entity);
        }

        if (req.PrimaryOrgUnitId is Guid orgId &&
            !await _db.OrgUnits.AnyAsync(x => x.Id == orgId && x.TenantId == tenantId && !x.IsDeleted, ct))
            throw new AppException("Chi nhánh không hợp lệ.");

        entity.Username = req.Username.Trim();
        entity.DisplayName = req.DisplayName;
        entity.Email = req.Email;
        entity.Phone = req.Phone;
        entity.Status = req.Status;
        entity.PrimaryOrgUnitId = req.PrimaryOrgUnitId;
        entity.ManagerUserId = req.ManagerUserId;
        if (!string.IsNullOrWhiteSpace(req.Password))
        {
            await _platform.ValidatePasswordAsync(tenantId, req.Password, ct);
            entity.PasswordHash = PasswordHasher.Hash(req.Password);
        }

        await _db.SaveChangesAsync(ct);
        await SyncUserDepartmentsAsync(tenantId, entity, req, ct);
        await _db.SaveChangesAsync(ct);

        return (await MapUsersAsync(tenantId, [entity], ct))[0];
    }

    private async Task SyncUserDepartmentsAsync(
        Guid tenantId, AppUser entity, UserUpsertRequest req, CancellationToken ct)
    {
        IReadOnlyList<UserDepartmentAssignRequest> assigns;
        if (req.Departments is { Count: > 0 })
            assigns = req.Departments;
        else if (req.DepartmentId is Guid singleDept)
            assigns = [new UserDepartmentAssignRequest(singleDept, req.JobLevelId, true)];
        else
            assigns = Array.Empty<UserDepartmentAssignRequest>();

        if (assigns.Count == 0)
        {
            var old = await _db.UserDepartments.Where(x => x.UserId == entity.Id && !x.IsDeleted).ToListAsync(ct);
            foreach (var o in old)
            {
                o.IsDeleted = true;
                o.DeletedAt = DateTimeOffset.UtcNow;
            }
            entity.DepartmentId = null;
            entity.JobLevelId = null;
            return;
        }

        var primaryCount = assigns.Count(x => x.IsPrimary);
        List<UserDepartmentAssignRequest> assignList;
        if (primaryCount == 0)
            assignList = assigns.Select((x, i) => x with { IsPrimary = i == 0 }).ToList();
        else if (primaryCount > 1)
            throw new AppException("Chỉ được chọn đúng một phòng ban chính.");
        else
            assignList = assigns.ToList();
        assigns = assignList;

        var deptIds = assigns.Select(x => x.DepartmentId).Distinct().ToList();
        if (deptIds.Count != assigns.Count)
            throw new AppException("Không gán trùng phòng ban.");

        var validDepts = await _db.Departments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && deptIds.Contains(x.Id))
            .Select(x => x.Id).ToListAsync(ct);
        if (validDepts.Count != deptIds.Count)
            throw new AppException("Phòng ban không hợp lệ.");

        var jlIds = assigns.Where(x => x.JobLevelId is not null).Select(x => x.JobLevelId!.Value).Distinct().ToList();
        if (jlIds.Count > 0)
        {
            var validJl = await _db.JobLevels.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && jlIds.Contains(x.Id))
                .Select(x => x.Id).ToListAsync(ct);
            if (validJl.Count != jlIds.Count)
                throw new AppException("Job level không hợp lệ.");
        }

        var existing = await _db.UserDepartments.Where(x => x.UserId == entity.Id && !x.IsDeleted).ToListAsync(ct);
        var keep = new HashSet<Guid>();
        foreach (var a in assigns)
        {
            var row = existing.FirstOrDefault(x => x.DepartmentId == a.DepartmentId);
            if (row is null)
            {
                row = new UserDepartment
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UserId = entity.Id,
                    DepartmentId = a.DepartmentId,
                    CreatedBy = entity.UpdatedBy ?? entity.CreatedBy
                };
                _db.UserDepartments.Add(row);
            }
            row.JobLevelId = a.JobLevelId;
            row.IsPrimary = a.IsPrimary;
            row.ValidFrom ??= DateOnly.FromDateTime(DateTime.UtcNow);
            keep.Add(row.DepartmentId);
        }

        foreach (var o in existing.Where(x => !keep.Contains(x.DepartmentId)))
        {
            o.IsDeleted = true;
            o.DeletedAt = DateTimeOffset.UtcNow;
        }

        var primary = assigns.First(x => x.IsPrimary);
        entity.DepartmentId = primary.DepartmentId;
        entity.JobLevelId = primary.JobLevelId;
    }

    private async Task<IReadOnlyList<UserDto>> MapUsersAsync(
        Guid tenantId, List<AppUser> users, CancellationToken ct)
    {
        var ids = users.Select(u => u.Id).ToList();
        var now = DateTimeOffset.UtcNow;
        var roles = await _db.UserRoles.AsNoTracking()
            .Where(x => ids.Contains(x.UserId) && x.IsActive && !x.IsDeleted && x.RevokedAt == null
                        && (x.ValidFrom == null || x.ValidFrom <= now)
                        && (x.ValidTo == null || x.ValidTo >= now))
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(g => g.Key, g => (IReadOnlyList<Guid>)g.Select(x => x.RoleId).ToList(), ct);

        var udRows = await (
            from ud in _db.UserDepartments.AsNoTracking()
            join d in _db.Departments.AsNoTracking() on ud.DepartmentId equals d.Id
            join jl in _db.JobLevels.AsNoTracking() on ud.JobLevelId equals jl.Id into jlj
            from jl in jlj.DefaultIfEmpty()
            where ids.Contains(ud.UserId) && !ud.IsDeleted && ud.TenantId == tenantId
            select new
            {
                ud.UserId,
                Dto = new UserDepartmentDto(
                    ud.DepartmentId, d.Name, ud.JobLevelId, jl != null ? jl.Name : null, ud.IsPrimary)
            }).ToListAsync(ct);

        var byUser = udRows.GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<UserDepartmentDto>)g
                .OrderByDescending(x => x.Dto.IsPrimary)
                .ThenBy(x => x.Dto.DepartmentName)
                .Select(x => x.Dto).ToList());

        return users.Select(u => new UserDto(
            u.Id, u.Username, u.DisplayName, u.Email, u.Status,
            u.PrimaryOrgUnitId, u.DepartmentId, u.JobLevelId, u.ManagerUserId,
            roles.GetValueOrDefault(u.Id) ?? Array.Empty<Guid>(),
            byUser.GetValueOrDefault(u.Id) ?? Array.Empty<UserDepartmentDto>())).ToList();
    }

    public async Task SetUserRolesAsync(Guid tenantId, Guid userId, IReadOnlyList<Guid> roleIds, Guid? actorId, CancellationToken ct = default)
    {
        _ = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("User không tồn tại.", 404);

        var existing = await _db.UserRoles.Where(x => x.UserId == userId && !x.IsDeleted).ToListAsync(ct);
        foreach (var ur in existing)
        {
            ur.IsActive = false;
            ur.RevokedAt = DateTimeOffset.UtcNow;
        }

        foreach (var rid in roleIds.Distinct())
        {
            _db.UserRoles.Add(new UserRole
            {
                TenantId = tenantId,
                UserId = userId,
                RoleId = rid,
                IsActive = true,
                ValidFrom = DateTimeOffset.UtcNow,
                AssignedBy = actorId
            });
        }

        _db.PermissionChangeLogs.Add(new PermissionChangeLog
        {
            TenantId = tenantId,
            ActorUserId = actorId,
            ChangeType = "SetUserRoles",
            TargetUserId = userId,
            DetailJson = $"{{\"roles\":{roleIds.Count}}}"
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteUserAsync(Guid tenantId, Guid userId, Guid actorId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && !x.IsDeleted, ct)
                   ?? throw new AppException("User không tồn tại.", 404);
        if (user.Id == actorId) throw new AppException("Không tự xóa chính mình.");

        var isSuper = await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == userId && ur.IsActive && !ur.IsDeleted && r.Code == "SUPER_ADMIN" && !r.IsDeleted
            select r.Id).AnyAsync(ct);
        if (isSuper)
        {
            var otherSupers = await (
                from ur in _db.UserRoles.AsNoTracking()
                join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
                join u in _db.Users.AsNoTracking() on ur.UserId equals u.Id
                where r.Code == "SUPER_ADMIN" && ur.IsActive && !ur.IsDeleted && !u.IsDeleted && u.Id != userId && u.TenantId == tenantId
                select u.Id).AnyAsync(ct);
            if (!otherSupers) throw new AppException("Không xóa SUPER_ADMIN cuối cùng.");
        }

        user.IsDeleted = true;
        user.DeletedAt = DateTimeOffset.UtcNow;
        user.Status = UserStatus.Disabled;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<ResetPasswordResultDto> AdminResetPasswordAsync(Guid tenantId, Guid userId, Guid actorId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && !x.IsDeleted, ct)
                   ?? throw new AppException("User không tồn tại.", 404);
        var temp = "!Tmp" + Guid.NewGuid().ToString("N")[..8];
        user.PasswordHash = PasswordHasher.Hash(temp);
        user.MustChangePassword = true;
        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        if (user.Status == UserStatus.Locked) user.Status = UserStatus.Active;
        user.UpdatedBy = actorId;
        await _db.SaveChangesAsync(ct);
        return new ResetPasswordResultDto(temp);
    }

    public async Task<InviteUserResultDto> InviteUserAsync(
        Guid tenantId, Guid actorId, InviteUserRequest req, CancellationToken ct = default)
    {
        var username = (req.Username ?? "").Trim();
        if (string.IsNullOrEmpty(username)) throw new AppException("Username bắt buộc.");
        var email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim();
        var phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();
        if (email is null && phone is null)
            throw new AppException("Cần Email hoặc SĐT để gửi lời mời.");

        await _platform.EnsureMaxUsersAsync(tenantId, ct);

        var user = await _db.Users.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Username == username && !x.IsDeleted, ct);
        if (user is null)
        {
            var temp = "!Inv" + Guid.NewGuid().ToString("N")[..8];
            user = new AppUser
            {
                TenantId = tenantId,
                Username = username,
                DisplayName = string.IsNullOrWhiteSpace(req.DisplayName) ? username : req.DisplayName.Trim(),
                Email = email,
                Phone = phone,
                PasswordHash = PasswordHasher.Hash(temp),
                MustChangePassword = true,
                Status = UserStatus.Active,
                PrimaryOrgUnitId = req.PrimaryOrgUnitId,
                DepartmentId = req.DepartmentId,
                JobLevelId = req.JobLevelId,
                CreatedBy = actorId,
            };
            _db.Users.Add(user);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(req.DisplayName)) user.DisplayName = req.DisplayName.Trim();
            if (email is not null) user.Email = email;
            if (phone is not null) user.Phone = phone;
            user.MustChangePassword = true;
            user.Status = UserStatus.Active;
            user.UpdatedBy = actorId;
            if (req.PrimaryOrgUnitId is Guid org) user.PrimaryOrgUnitId = org;
            if (req.DepartmentId is Guid dept) user.DepartmentId = dept;
            if (req.JobLevelId is Guid lvl) user.JobLevelId = lvl;
        }

        await _db.SaveChangesAsync(ct);

        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            TenantId = tenantId,
            UserId = user.Id,
            OtpCode = otp,
            TokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(otp))),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
            CreatedBy = actorId,
        });
        await _db.SaveChangesAsync(ct);

        var vars = new Dictionary<string, string>
        {
            ["otp"] = otp,
            ["username"] = user.Username,
            ["displayName"] = user.DisplayName ?? user.Username,
            ["expiresMinutes"] = "15",
        };
        ChannelSendResultDto sent;
        string channel;
        string target;
        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            channel = "Email";
            target = user.Email!;
            sent = await _platform.SendChannelMessageAsync(tenantId, actorId, new ChannelSendRequest(
                "Email", "USER_INVITE", target, vars, "sys.auth.invite"), ct);
        }
        else
        {
            channel = "SMS";
            target = user.Phone!;
            sent = await _platform.SendChannelMessageAsync(tenantId, actorId, new ChannelSendRequest(
                "SMS", "USER_INVITE", target, vars, "sys.auth.invite"), ct);
        }

        return new InviteUserResultDto(
            user.Id, user.Username, channel, target, sent.LogId,
            $"Đã gửi lời mời qua {channel} tới {target}.");
    }

    public async Task<RoleDto> CopyRoleAsync(Guid tenantId, Guid roleId, Guid? actorId, string newCode, string newName, CancellationToken ct = default)
    {
        var src = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == roleId && x.TenantId == tenantId && !x.IsDeleted, ct)
                  ?? throw new AppException("Role nguồn không tồn tại.", 404);
        if (await _db.Roles.AnyAsync(x => x.TenantId == tenantId && x.Code == newCode && !x.IsDeleted, ct))
            throw new AppException("Mã role mới đã tồn tại.");

        var copy = new Role
        {
            TenantId = tenantId, Code = newCode.Trim(), Name = newName.Trim(),
            BypassDataScope = src.BypassDataScope, IsSystem = false, IsActive = true, CreatedBy = actorId
        };
        _db.Roles.Add(copy);
        await _db.SaveChangesAsync(ct);

        var perms = await _db.RolePermissions.AsNoTracking()
            .Where(x => x.RoleId == roleId && !x.IsDeleted).Select(x => x.PermissionId).ToListAsync(ct);
        foreach (var pid in perms)
            _db.RolePermissions.Add(new RolePermission { TenantId = tenantId, RoleId = copy.Id, PermissionId = pid });
        _db.PermissionChangeLogs.Add(new PermissionChangeLog
        {
            TenantId = tenantId, ActorUserId = actorId, ChangeType = "CopyRole", RoleId = copy.Id,
            DetailJson = $"{{\"from\":\"{src.Code}\"}}"
        });
        await _db.SaveChangesAsync(ct);
        return new RoleDto(copy.Id, copy.Code, copy.Name, copy.BypassDataScope, copy.IsSystem, copy.IsActive, perms);
    }

    public async Task<IReadOnlyList<MenuItemDto>> GetMyMenuAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var modules = await (
            from l in _db.Licenses.AsNoTracking()
            join lm in _db.LicenseModules.AsNoTracking() on l.Id equals lm.LicenseId
            where l.TenantId == tenantId && l.Status == "Active" && lm.IsEnabled && !lm.IsDeleted
            select lm.ModuleCode
        ).Distinct().ToListAsync(ct);

        var menus = await _db.MenuItems.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive && !x.IsDeleted && modules.Contains(x.ModuleCode))
            .OrderBy(x => x.SortOrder)
            .ToListAsync(ct);

        var result = new List<MenuItemDto>();
        foreach (var m in menus)
        {
            if (!string.IsNullOrWhiteSpace(m.PermissionCode)
                && !await _authz.HasPermissionAsync(userId, m.PermissionCode, ct))
                continue;

            result.Add(new MenuItemDto(m.Id, m.Code, m.ParentId, m.ModuleCode, m.Title, m.RoutePath, m.PermissionCode, m.Icon, m.SortOrder));
        }

        return result;
    }
}
