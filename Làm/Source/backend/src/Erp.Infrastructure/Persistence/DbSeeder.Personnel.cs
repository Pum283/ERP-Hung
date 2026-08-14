using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Persistence;

public static partial class DbSeeder
{
    private static async Task SeedPersonnelAsync(AppDbContext db, CancellationToken ct)
    {
        var types = await db.EmployeeTypes.Where(x => x.TenantId == TenantId).ToDictionaryAsync(x => x.Code, ct);
        var titles = await db.JobTitles.Where(x => x.TenantId == TenantId).ToDictionaryAsync(x => x.Code, ct);
        var levels = await db.JobLevels.Where(x => x.TenantId == TenantId).ToDictionaryAsync(x => x.Code, ct);
        var pwd = PasswordHasher.Hash(DefaultPassword);
        Guid Level(string code) => levels[code].Id;

        var usersByName = await db.Users.Where(x => x.TenantId == TenantId && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Username, StringComparer.OrdinalIgnoreCase, ct);
        var empsByUser = await db.Employees.Where(x => x.TenantId == TenantId && !x.IsDeleted && x.UserId != null)
            .ToDictionaryAsync(x => x.UserId!.Value, ct);
        var activeRoles = await db.UserRoles.Where(x => x.TenantId == TenantId && x.IsActive && !x.IsDeleted)
            .Select(x => new { x.UserId, x.RoleId }).ToListAsync(ct);
        var roleSet = activeRoles.Select(x => (x.UserId, x.RoleId)).ToHashSet();
        var udKeys = (await db.UserDepartments.Where(x => x.TenantId == TenantId && !x.IsDeleted)
            .Select(x => new { x.UserId, x.DepartmentId }).ToListAsync(ct))
            .Select(x => (x.UserId, x.DepartmentId)).ToHashSet();

        // —— admin (tài khoản hệ thống) ——
        if (!usersByName.TryGetValue("admin", out var admin))
        {
            var adminEmpId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001");
            admin = new AppUser
            {
                Id = AdminId, TenantId = TenantId, Username = "admin",
                DisplayName = "System Admin", Email = "admin@demo.local", Phone = "0901000001",
                PasswordHash = pwd, Status = UserStatus.Active,
                PrimaryOrgUnitId = OrgHq, DepartmentId = DeptBod, JobLevelId = Level("DIRECTOR"),
                EmployeeId = adminEmpId
            };
            db.Users.Add(admin);
            usersByName["admin"] = admin;
            if (!empsByUser.ContainsKey(AdminId))
            {
                var emp = new Employee
                {
                    Id = adminEmpId, TenantId = TenantId, EmployeeCode = "ADMIN",
                    UserId = AdminId, FullName = "System Admin",
                    Dob = new DateOnly(1985, 1, 1), Gender = "Male",
                    Email = "admin@demo.local", Phone = "0901000001",
                    OrgUnitId = OrgHq, DepartmentId = DeptBod, JobLevelId = Level("DIRECTOR"),
                    JobTitleId = (titles.GetValueOrDefault("DIR") ?? titles.GetValueOrDefault("CEO") ?? titles.Values.First()).Id,
                    EmployeeTypeId = types["FT"].Id, Status = "Active", HireDate = new DateOnly(2018, 1, 15)
                };
                db.Employees.Add(emp);
                empsByUser[AdminId] = emp;
            }
        }
        else
        {
            admin.PasswordHash = pwd;
            admin.Status = UserStatus.Active;
            admin.DepartmentId = DeptBod;
            admin.JobLevelId = Level("DIRECTOR");
            admin.PrimaryOrgUnitId = OrgHq;
        }

        if (!roleSet.Contains((admin.Id, RoleSuperAdmin)))
        {
            db.UserRoles.Add(new UserRole
            {
                TenantId = TenantId, UserId = admin.Id, RoleId = RoleSuperAdmin,
                IsActive = true, ValidFrom = DateTimeOffset.UtcNow
            });
            roleSet.Add((admin.Id, RoleSuperAdmin));
        }

        var roster = BuildCompanyRoster();
        var keepUsernames = roster.Select(p => p.User).Append("admin").ToHashSet(StringComparer.OrdinalIgnoreCase);
        var userIdByUn = new Dictionary<int, Guid>();

        foreach (var p in roster)
        {
            var orgId = (p.Dept == DeptOps || p.Dept == DeptWh || p.Dept == DeptSalesHcm) ? OrgHcm : OrgHq;
            if (!usersByName.TryGetValue(p.User, out var user))
            {
                // Tái sử dụng slot GUID cũ (tránh PK trùng với roster demo trước)
                user = await db.Users.FirstOrDefaultAsync(x => x.Id == U(p.Un), ct);
                if (user is null)
                {
                    var empId = E(p.En);
                    user = new AppUser
                    {
                        Id = U(p.Un), TenantId = TenantId, Username = p.User,
                        DisplayName = p.Name, Email = p.Email, Phone = p.Phone,
                        PasswordHash = pwd, Status = UserStatus.Active,
                        PrimaryOrgUnitId = orgId, DepartmentId = p.Dept,
                        JobLevelId = Level(p.Jl), EmployeeId = empId
                    };
                    db.Users.Add(user);
                }
                else
                {
                    // Đổi username từ demo cũ (vd. ceo → SangTQ)
                    usersByName.Remove(user.Username);
                    user.Username = p.User;
                    user.PasswordHash = pwd;
                    user.Status = UserStatus.Active;
                    user.DisplayName = p.Name;
                    user.Email = p.Email;
                    user.Phone = p.Phone;
                    user.DepartmentId = p.Dept;
                    user.JobLevelId = Level(p.Jl);
                    user.PrimaryOrgUnitId = orgId;
                    user.EmployeeId ??= E(p.En);
                }
                usersByName[p.User] = user;
            }
            else
            {
                user.PasswordHash = pwd;
                user.Status = UserStatus.Active;
                user.DisplayName = p.Name;
                user.Email = p.Email;
                user.Phone = p.Phone;
                user.DepartmentId = p.Dept;
                user.JobLevelId = Level(p.Jl);
                user.PrimaryOrgUnitId = orgId;
                user.EmployeeId ??= E(p.En);
            }
            userIdByUn[p.Un] = user.Id;
        }

        foreach (var p in roster)
        {
            var user = usersByName[p.User];
            user.ManagerUserId = p.MgrUn is int m && userIdByUn.TryGetValue(m, out var mu) ? mu : null;

            void EnsureRole(Guid roleId)
            {
                if (roleSet.Contains((user.Id, roleId))) return;
                db.UserRoles.Add(new UserRole
                {
                    TenantId = TenantId, UserId = user.Id, RoleId = roleId,
                    IsActive = true, ValidFrom = DateTimeOffset.UtcNow
                });
                roleSet.Add((user.Id, roleId));
            }
            EnsureRole(p.Role);
            foreach (var er in p.ExtraRoles) EnsureRole(er);

            if (!udKeys.Contains((user.Id, p.Dept)))
            {
                db.UserDepartments.Add(new UserDepartment
                {
                    TenantId = TenantId, UserId = user.Id, DepartmentId = p.Dept,
                    JobLevelId = Level(p.Jl), IsPrimary = true,
                    ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow)
                });
                udKeys.Add((user.Id, p.Dept));
            }

            var title = titles.GetValueOrDefault(p.Title)
                        ?? titles.GetValueOrDefault("DEV")
                        ?? titles.Values.First();
            var typeCode = p.Type is "INTERN" or "PROBATION" or "CONTRACT" or "FT" ? p.Type : "FT";
            if (typeCode == "INTERN" && !types.ContainsKey("INTERN")) typeCode = "FT";
            var et = types.GetValueOrDefault(typeCode) ?? types["FT"];
            var orgId = (p.Dept == DeptOps || p.Dept == DeptWh || p.Dept == DeptSalesHcm) ? OrgHcm : OrgHq;

            if (!empsByUser.ContainsKey(user.Id))
            {
                var empId = user.EmployeeId ?? E(p.En);
                // Slot emp GUID cũ (có thể đã gắn user demo trước)
                var existingEmp = await db.Employees.FirstOrDefaultAsync(x => x.Id == empId, ct);
                Guid? managerEmp = null;
                if (user.ManagerUserId is Guid mid && empsByUser.TryGetValue(mid, out var me))
                    managerEmp = me.Id;

                if (existingEmp is not null)
                {
                    existingEmp.UserId = user.Id;
                    existingEmp.EmployeeCode = p.User;
                    existingEmp.FullName = p.Name;
                    existingEmp.Email = p.Email;
                    existingEmp.Phone = p.Phone;
                    existingEmp.OrgUnitId = orgId;
                    existingEmp.DepartmentId = p.Dept;
                    existingEmp.JobLevelId = Level(p.Jl);
                    existingEmp.JobTitleId = title.Id;
                    existingEmp.EmployeeTypeId = et.Id;
                    existingEmp.ManagerEmployeeId = managerEmp;
                    existingEmp.Status = p.Type == "PROBATION" ? "Probation" : "Active";
                    existingEmp.HireDate = p.Hire;
                    existingEmp.TerminateDate = null;
                    existingEmp.Gender = p.Gender;
                    empsByUser[user.Id] = existingEmp;
                }
                else
                {
                    var emp = new Employee
                    {
                        Id = empId, TenantId = TenantId, EmployeeCode = p.User,
                        UserId = user.Id, FullName = p.Name,
                        Dob = p.Hire.AddYears(-28 - (p.En % 7)), Gender = p.Gender,
                        Email = p.Email, Phone = p.Phone,
                        OrgUnitId = orgId, DepartmentId = p.Dept, JobLevelId = Level(p.Jl),
                        JobTitleId = title.Id, EmployeeTypeId = et.Id,
                        ManagerEmployeeId = managerEmp,
                        Status = p.Type == "PROBATION" ? "Probation" : "Active",
                        HireDate = p.Hire, TerminateDate = null
                    };
                    db.Employees.Add(emp);
                    empsByUser[user.Id] = emp;
                    db.Contracts.Add(new Contract
                    {
                        TenantId = TenantId, EmployeeId = empId,
                        ContractNo = $"HDLD-{p.User}",
                        ContractType = p.Type is "FT" or "PROBATION" ? "Indefinite" : "FixedTerm",
                        StartDate = p.Hire,
                        EndDate = p.Type is "CONTRACT" or "INTERN" ? p.Hire.AddYears(1) : null,
                        Status = "Active"
                    });
                }
            }
            else
            {
                var emp = empsByUser[user.Id];
                emp.EmployeeCode = p.User;
                emp.FullName = p.Name;
                emp.Email = p.Email;
                emp.Phone = p.Phone;
                emp.OrgUnitId = orgId;
                emp.DepartmentId = p.Dept;
                emp.JobLevelId = Level(p.Jl);
                emp.JobTitleId = title.Id;
                emp.EmployeeTypeId = et.Id;
                emp.Status = p.Type == "PROBATION" ? "Probation" : "Active";
                emp.HireDate = p.Hire;
                emp.TerminateDate = null;
                emp.Gender = p.Gender;
            }
        }

        foreach (var p in roster)
        {
            var user = usersByName[p.User];
            if (!empsByUser.TryGetValue(user.Id, out var emp)) continue;
            if (user.ManagerUserId is Guid mid && empsByUser.TryGetValue(mid, out var me))
                emp.ManagerEmployeeId = me.Id;
        }

        void SetDeptMgr(Guid deptId, int un)
        {
            var dept = db.Departments.Local.FirstOrDefault(x => x.Id == deptId)
                       ?? db.Departments.FirstOrDefault(x => x.Id == deptId);
            if (dept is not null && userIdByUn.TryGetValue(un, out var uid))
                dept.ManagerUserId = uid;
        }

        await db.Departments.Where(x => x.TenantId == TenantId).LoadAsync(ct);
        SetDeptMgr(DeptBod, 1);      // Trần Quang Sang
        SetDeptMgr(DeptFinance, 2);  // Huỳnh Thị Kim Phương
        SetDeptMgr(DeptIt, 3);       // Trần Nguyễn Đức Lương
        SetDeptMgr(DeptMkt, 8);      // Nguyễn Thị Thùy Trang
        SetDeptMgr(DeptEcom, 10);    // Hoàng Thị Ánh Tuyết

        await db.SaveChangesAsync(ct);

        // Xóa hẳn user/NV không thuộc roster (giữ admin + 14 người)
        await PurgeUsersOutsideRosterAsync(db, keepUsernames, ct);
    }

    /// <summary>Hard-delete user + hồ sơ HRM không nằm trong danh sách công ty.</summary>
    private static async Task PurgeUsersOutsideRosterAsync(
        AppDbContext db, HashSet<string> keepUsernames, CancellationToken ct)
    {
        var keepCodes = new HashSet<string>(keepUsernames, StringComparer.OrdinalIgnoreCase) { "ADMIN" };

        var allUsers = await db.Users.Where(x => x.TenantId == TenantId).ToListAsync(ct);
        var removeUsers = allUsers.Where(u => !keepUsernames.Contains(u.Username)).ToList();
        var removeUserIds = removeUsers.Select(u => u.Id).ToHashSet();

        var allEmps = await db.Employees.Where(x => x.TenantId == TenantId).ToListAsync(ct);
        var removeEmps = allEmps.Where(e =>
            (e.UserId is Guid uid && removeUserIds.Contains(uid))
            || !keepCodes.Contains(e.EmployeeCode)).ToList();
        var removeEmpIds = removeEmps.Select(e => e.Id).ToHashSet();

        if (removeUsers.Count == 0 && removeEmps.Count == 0) return;

        // Gỡ FK trỏ vào người sắp xóa
        foreach (var u in allUsers)
        {
            if (u.ManagerUserId is Guid mid && removeUserIds.Contains(mid))
                u.ManagerUserId = null;
        }
        foreach (var e in allEmps)
        {
            if (e.ManagerEmployeeId is Guid mid && removeEmpIds.Contains(mid))
                e.ManagerEmployeeId = null;
        }
        foreach (var d in await db.Departments.Where(x => x.TenantId == TenantId).ToListAsync(ct))
        {
            if (d.ManagerUserId is Guid mid && removeUserIds.Contains(mid))
                d.ManagerUserId = null;
        }

        if (removeUserIds.Count > 0)
        {
            await db.UserRoles.Where(x => removeUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
            await db.UserDepartments.Where(x => removeUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
            await db.UserDataScopes.Where(x => removeUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
            await db.UserSessions.Where(x => removeUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
            await db.TrustedDevices.Where(x => removeUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
            await db.SysPushDevices.Where(x => removeUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
            await db.SysUserNotificationPreferences.Where(x => removeUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
            await db.SysExternalLogins.Where(x => removeUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
            await db.PasswordResetTokens.Where(x => removeUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
            await db.LoginAudits.Where(x => x.UserId != null && removeUserIds.Contains(x.UserId.Value)).ExecuteDeleteAsync(ct);
            await db.ConversationMembers.Where(x => removeUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
            await db.AppNotifications.Where(x => removeUserIds.Contains(x.UserId)).ExecuteDeleteAsync(ct);
        }

        if (removeEmpIds.Count > 0)
        {
            await db.Contracts.Where(x => removeEmpIds.Contains(x.EmployeeId)).ExecuteDeleteAsync(ct);
            await db.LeaveBalances.Where(x => removeEmpIds.Contains(x.EmployeeId)).ExecuteDeleteAsync(ct);
            await db.LeaveRequests.Where(x => removeEmpIds.Contains(x.EmployeeId)).ExecuteDeleteAsync(ct);
            await db.EmployeeDocuments.Where(x => removeEmpIds.Contains(x.EmployeeId)).ExecuteDeleteAsync(ct);
            await db.EmployeeSalaries.Where(x => removeEmpIds.Contains(x.EmployeeId)).ExecuteDeleteAsync(ct);
            await db.EmployeeRelatives.Where(x => removeEmpIds.Contains(x.EmployeeId)).ExecuteDeleteAsync(ct);
            await db.HrmEmployeeSkills.Where(x => removeEmpIds.Contains(x.EmployeeId)).ExecuteDeleteAsync(ct);
            await db.ShiftAssignments.Where(x => removeEmpIds.Contains(x.EmployeeId)).ExecuteDeleteAsync(ct);
            await db.AttendanceRecords.Where(x => removeEmpIds.Contains(x.EmployeeId)).ExecuteDeleteAsync(ct);
            await db.EmploymentStatusHistories.Where(x => removeEmpIds.Contains(x.EmployeeId)).ExecuteDeleteAsync(ct);
            await db.EmploymentStatusChanges.Where(x => removeEmpIds.Contains(x.EmployeeId)).ExecuteDeleteAsync(ct);
            await db.StaffTransfers.Where(x => x.EmployeeId != null && removeEmpIds.Contains(x.EmployeeId.Value)).ExecuteDeleteAsync(ct);
            await db.PayrollPenalties.Where(x => removeEmpIds.Contains(x.EmployeeId)).ExecuteDeleteAsync(ct);
        }

        if (removeEmps.Count > 0)
            db.Employees.RemoveRange(removeEmps);
        if (removeUsers.Count > 0)
            db.Users.RemoveRange(removeUsers);

        await db.SaveChangesAsync(ct);
    }
}
