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

            // —— admin ——
            if (!usersByName.TryGetValue("admin", out var admin))
            {
                var adminEmpId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001");
                admin = new AppUser
                {
                    Id = AdminId, TenantId = TenantId, Username = "admin",
                    DisplayName = "Nguyễn Văn Admin", Email = "admin@demo.local", Phone = "0901000001",
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
                        Id = adminEmpId, TenantId = TenantId, EmployeeCode = "NV1000",
                        UserId = AdminId, FullName = "Nguyễn Văn Admin",
                        Dob = new DateOnly(1985, 1, 1), Gender = "Male",
                        Email = "admin@demo.local", Phone = "0901000001",
                        OrgUnitId = OrgHq, DepartmentId = DeptBod, JobLevelId = Level("DIRECTOR"),
                        JobTitleId = (titles.GetValueOrDefault("CEO") ?? titles.Values.First()).Id,
                        EmployeeTypeId = types["FT"].Id, Status = "Active", HireDate = new DateOnly(2018, 1, 15)
                    };
                    db.Employees.Add(emp);
                    empsByUser[AdminId] = emp;
                }
            }
            else
            {
                admin.PasswordHash = pwd;
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
            var userIdByUn = new Dictionary<int, Guid>();

            foreach (var p in roster)
            {
                var orgId = (p.Dept == DeptOps || p.Dept == DeptWh || p.Dept == DeptSalesHcm) ? OrgHcm : OrgHq;
                if (!usersByName.TryGetValue(p.User, out var user))
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
                    usersByName[p.User] = user;
                }
                else
                {
                    user.PasswordHash = pwd;
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

            // Managers
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

                if (!empsByUser.ContainsKey(user.Id))
                {
                    var empId = user.EmployeeId ?? E(p.En);
                    Guid? managerEmp = null;
                    if (user.ManagerUserId is Guid mid && empsByUser.TryGetValue(mid, out var me))
                        managerEmp = me.Id;

                    var title = titles.GetValueOrDefault(p.Title) ?? titles.Values.First();
                    var typeCode = p.Type is "INTERN" or "PROBATION" or "CONTRACT" or "FT" ? p.Type : "FT";
                    if (typeCode == "INTERN" && !types.ContainsKey("INTERN")) typeCode = "FT";
                    var et = types.GetValueOrDefault(typeCode) ?? types["FT"];
                    var orgId = (p.Dept == DeptOps || p.Dept == DeptWh || p.Dept == DeptSalesHcm) ? OrgHcm : OrgHq;

                    var emp = new Employee
                    {
                        Id = empId, TenantId = TenantId, EmployeeCode = $"NV{p.En:D4}",
                        UserId = user.Id, FullName = p.Name,
                        Dob = p.Hire.AddYears(-28 - (p.En % 7)), Gender = p.Gender,
                        Email = p.Email, Phone = p.Phone,
                        OrgUnitId = orgId, DepartmentId = p.Dept, JobLevelId = Level(p.Jl),
                        JobTitleId = title.Id, EmployeeTypeId = et.Id,
                        ManagerEmployeeId = managerEmp,
                        Status = p.Type == "PROBATION" ? "Probation" : "Active",
                        HireDate = p.Hire
                    };
                    db.Employees.Add(emp);
                    empsByUser[user.Id] = emp;
                    db.Contracts.Add(new Contract
                    {
                        TenantId = TenantId, EmployeeId = empId,
                        ContractNo = $"HDLD-{p.En:D4}",
                        ContractType = p.Type is "FT" or "PROBATION" ? "Indefinite" : "FixedTerm",
                        StartDate = p.Hire,
                        EndDate = p.Type is "CONTRACT" or "INTERN" ? p.Hire.AddYears(1) : null,
                        Status = "Active"
                    });
                }
            }

            // Second pass: fix manager employee links now that all emps exist
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
            // Load depts into context
            await db.Departments.Where(x => x.TenantId == TenantId).LoadAsync(ct);
            SetDeptMgr(DeptHr, 101);
            SetDeptMgr(DeptIt, 201);
            SetDeptMgr(DeptSales, 301);
            SetDeptMgr(DeptFinance, 401);
            SetDeptMgr(DeptMkt, 501);
            SetDeptMgr(DeptLegal, 601);
            SetDeptMgr(DeptOps, 701);
            SetDeptMgr(DeptWh, 801);
            SetDeptMgr(DeptSalesHcm, 901);
            SetDeptMgr(DeptBod, 1);

            await db.SaveChangesAsync(ct);
        }
}
