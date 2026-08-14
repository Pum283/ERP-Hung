using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Persistence;

/// <summary>Roster công ty demo đầy đủ — mở rộng DbSeeder.</summary>
public static partial class DbSeeder
{
    // Roles bổ sung
    private static readonly Guid RoleExecutive = Guid.Parse("33333333-3333-3333-3333-333333333303");
    private static readonly Guid RoleDeptManager = Guid.Parse("33333333-3333-3333-3333-333333333304");
    private static readonly Guid RoleFinManager = Guid.Parse("33333333-3333-3333-3333-333333333305");
    private static readonly Guid RoleItManager = Guid.Parse("33333333-3333-3333-3333-333333333306");
    private static readonly Guid RoleSalesManager = Guid.Parse("33333333-3333-3333-3333-333333333307");
    private static readonly Guid RoleAccountant = Guid.Parse("33333333-3333-3333-3333-333333333308");
    private static readonly Guid RoleIntern = Guid.Parse("33333333-3333-3333-3333-333333333309");
    private static readonly Guid RoleApprover = Guid.Parse("33333333-3333-3333-3333-333333333310");

    // Job levels bổ sung
    private static readonly Guid JlDeputy = Guid.Parse("66666666-6666-6666-6666-666666666603");
    private static readonly Guid JlLead = Guid.Parse("66666666-6666-6666-6666-666666666604");
    private static readonly Guid JlIntern = Guid.Parse("66666666-6666-6666-6666-666666666605");

    // Departments bổ sung
    private static readonly Guid DeptBod = Guid.Parse("55555555-5555-5555-5555-555555555510");
    private static readonly Guid DeptMkt = Guid.Parse("55555555-5555-5555-5555-555555555511");
    private static readonly Guid DeptLegal = Guid.Parse("55555555-5555-5555-5555-555555555512");
    private static readonly Guid DeptWh = Guid.Parse("55555555-5555-5555-5555-555555555513");
    private static readonly Guid DeptSalesHcm = Guid.Parse("55555555-5555-5555-5555-555555555514");
    private static readonly Guid DeptEcom = Guid.Parse("55555555-5555-5555-5555-555555555515");

    private static Guid U(int n) => Guid.Parse($"aaaaaaaa-aaaa-aaaa-aaaa-{n:D12}");
    private static Guid E(int n) => Guid.Parse($"bbbbbbbb-bbbb-bbbb-bbbb-{n:D12}");

    private static async Task EnsureExpandedCatalogAsync(AppDbContext db, CancellationToken ct)
    {
        // Org Đà Nẵng
        var orgDn = Guid.Parse("44444444-4444-4444-4444-444444444402");
        if (!await db.OrgUnits.AnyAsync(x => x.Id == orgDn, ct))
        {
            db.OrgUnits.Add(new OrgUnit
            {
                Id = orgDn, TenantId = TenantId, Code = "DN", Name = "Chi nhánh Đà Nẵng",
                UnitType = "Branch", ParentId = OrgHq, Path = $"/{OrgHq:N}/{orgDn:N}/", IsActive = true, SortOrder = 3
            });
        }

        var extraDepts = new (Guid Id, string Code, string Name, Guid Org, int Sort)[]
        {
            (DeptBod, "BOD", "Ban Giám đốc", OrgHq, 0),
            (DeptMkt, "MKT", "Marketing", OrgHq, 6),
            (DeptLegal, "LEGAL", "Pháp chế", OrgHq, 7),
            (DeptWh, "WH", "Kho vận", OrgHcm, 8),
            (DeptSalesHcm, "SALES_HCM", "Kinh doanh HCM", OrgHcm, 9),
            (DeptEcom, "ECOM", "Thương mại điện tử (Ecom)", OrgHq, 10),
        };
        foreach (var d in extraDepts)
        {
            if (await db.Departments.AnyAsync(x => x.Id == d.Id, ct)) continue;
            db.Departments.Add(new Department
            {
                Id = d.Id, TenantId = TenantId, Code = d.Code, Name = d.Name,
                OrgUnitId = d.Org, Path = $"/{d.Id:N}/", IsActive = true, SortOrder = d.Sort
            });
        }

        var extraLevels = new (Guid Id, string Code, string Name, int Order, ScopeType Scope)[]
        {
            (JlDeputy, "DEPUTY", "Phó giám đốc", 2, ScopeType.All),
            (JlLead, "LEAD", "Trưởng nhóm", 4, ScopeType.Team),
            (JlIntern, "INTERN", "Thực tập / CTV", 6, ScopeType.Own),
        };
        // Re-order: DIRECTOR=1, DEPUTY=2, MANAGER=3, LEAD=4, STAFF=5, INTERN=6
        foreach (var jl in extraLevels)
        {
            if (await db.JobLevels.AnyAsync(x => x.TenantId == TenantId && x.Code == jl.Code, ct)) continue;
            db.JobLevels.Add(new JobLevel
            {
                Id = jl.Id, TenantId = TenantId, Code = jl.Code, Name = jl.Name,
                LevelOrder = jl.Order, DefaultScopeType = jl.Scope, IsActive = true
            });
        }

        var mgr = await db.JobLevels.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Code == "MANAGER", ct);
        if (mgr is not null) mgr.LevelOrder = 3;
        var staff = await db.JobLevels.FirstOrDefaultAsync(x => x.TenantId == TenantId && x.Code == "STAFF", ct);
        if (staff is not null) staff.LevelOrder = 5;

        await db.SaveChangesAsync(ct);

        var jlMap = await db.JobLevels.Where(x => x.TenantId == TenantId).ToDictionaryAsync(x => x.Code, x => x.Id, ct);
        var titleDefs = new (string Code, string Name, string Jl, int Sort)[]
        {
            ("CEO", "Tổng giám đốc", "DIRECTOR", 1),
            ("DCEO", "Phó tổng giám đốc", "DEPUTY", 2),
            ("CFO", "Giám đốc Tài chính", "DEPUTY", 3),
            ("CHRO", "Giám đốc Nhân sự", "DEPUTY", 4),
            ("CTO", "Giám đốc Công nghệ", "DEPUTY", 5),
            ("HR_MGR", "Trưởng phòng Nhân sự", "MANAGER", 10),
            ("IT_MGR", "Trưởng phòng CNTT", "MANAGER", 11),
            ("SALES_MGR", "Trưởng phòng Kinh doanh", "MANAGER", 12),
            ("FIN_MGR", "Trưởng phòng Tài chính", "MANAGER", 13),
            ("MKT_MGR", "Trưởng phòng Marketing", "MANAGER", 14),
            ("LEGAL_MGR", "Trưởng phòng Pháp chế", "MANAGER", 15),
            ("OPS_MGR", "Trưởng phòng Vận hành", "MANAGER", 16),
            ("WH_MGR", "Trưởng kho", "MANAGER", 17),
            ("HR_LEAD", "Trưởng nhóm Tuyển dụng", "LEAD", 20),
            ("DEV_LEAD", "Tech Lead", "LEAD", 21),
            ("SALES_LEAD", "Trưởng nhóm Sale", "LEAD", 22),
            ("ACC", "Kế toán viên", "STAFF", 30),
            ("DEV", "Lập trình viên", "STAFF", 31),
            ("HR_SPEC", "Chuyên viên Nhân sự", "STAFF", 32),
            ("SALES", "Nhân viên Kinh doanh", "STAFF", 33),
            ("OPS", "Nhân viên Vận hành", "STAFF", 34),
            ("MKT", "Nhân viên Marketing", "STAFF", 35),
            ("EDITOR", "Editor / Biên tập nội dung", "STAFF", 39),
            ("ECOM_MGR", "Trưởng phòng Ecom", "MANAGER", 18),
            ("ECOM", "Nhân viên Ecom", "STAFF", 40),
            ("LEGAL", "Chuyên viên Pháp chế", "STAFF", 36),
            ("WH", "Nhân viên Kho", "STAFF", 37),
            ("ASSIST", "Trợ lý", "STAFF", 38),
            ("DIR", "Giám đốc", "DIRECTOR", 0),
            ("INTERN_DEV", "Thực tập Dev", "INTERN", 40),
            ("INTERN_HR", "Thực tập HR", "INTERN", 41),
            ("INTERN_IT", "Thực tập sinh CNTT", "INTERN", 42),
        };
        foreach (var t in titleDefs)
        {
            if (await db.JobTitles.AnyAsync(x => x.TenantId == TenantId && x.Code == t.Code, ct)) continue;
            if (!jlMap.TryGetValue(t.Jl, out var jlid)) continue;
            db.JobTitles.Add(new JobTitle
            {
                TenantId = TenantId, Code = t.Code, Name = t.Name,
                DefaultJobLevelId = jlid, SortOrder = t.Sort, IsActive = true
            });
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task EnsureExpandedRolesAsync(AppDbContext db, CancellationToken ct)
    {
        var allPerms = await db.Permissions.Where(x => !x.IsDeleted && x.IsActive).ToListAsync(ct);

        async Task EnsureRole(Guid id, string code, string name, bool bypass, IEnumerable<string> permCodes)
        {
            var role = await db.Roles.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (role is null)
            {
                role = new Role
                {
                    Id = id, TenantId = TenantId, Code = code, Name = name,
                    BypassDataScope = bypass, IsSystem = true, IsActive = true
                };
                db.Roles.Add(role);
            }
            else
            {
                role.Name = name;
                role.BypassDataScope = bypass;
            }

            var have = await db.RolePermissions.Where(x => x.RoleId == id && !x.IsDeleted)
                .Select(x => x.PermissionId).ToListAsync(ct);
            foreach (var perm in allPerms.Where(p => permCodes.Contains(p.Code) && !have.Contains(p.Id)))
            {
                db.RolePermissions.Add(new RolePermission
                {
                    TenantId = TenantId, RoleId = id, PermissionId = perm.Id
                });
            }
        }

        string[] hrmAll =
        [
            "hrm.employee.read", "hrm.employee.manage", "hrm.leave.read", "hrm.leave.manage",
            "hrm.contract.read", "hrm.contract.manage", "hrm.recruit.read", "hrm.recruit.manage",
            "hrm.payroll.read", "hrm.payroll.manage"
        ];
        string[] hrmRead =
        [
            "hrm.employee.read", "hrm.leave.read", "hrm.contract.read", "hrm.recruit.read", "hrm.payroll.read"
        ];
        string[] msg = ["sys.msg.read", "sys.msg.send"];
        string[] wf = ["wf.task.read", "wf.task.act"];
        string[] lmsLearn = ["lms.learn.read", "lms.learn.enroll"];
        string[] lmsCourse = ["lms.course.read", "lms.course.manage"];
        string[] lmsExam = ["lms.exam.read", "lms.exam.manage"];
        string[] crm = ["crm.customer.read", "crm.customer.manage"];
        string[] pos = [
            "pos.store.read", "pos.store.manage",
            "pos.shift.read", "pos.shift.manage",
            "pos.sale.read", "pos.sale.manage",
            "pos.catalog.read", "pos.catalog.manage"
        ];
        string[] pur =
        [
            "pur.vendor.read", "pur.vendor.manage",
            "pur.pr.read", "pur.pr.manage", "pur.pr.approve",
            "pur.po.read", "pur.po.manage", "pur.po.approve",
            "pur.grn.read", "pur.grn.manage",
            "pur.invoice.read", "pur.invoice.manage"
        ];

        await EnsureRole(RoleExecutive, "EXECUTIVE", "Ban điều hành", false,
            msg.Concat(wf).Concat(hrmRead).Concat(lmsLearn).Concat(lmsCourse).Concat(lmsExam).Concat(crm).Concat(pos).Concat(pur)
                .Concat(["sys.user.read", "sys.role.read", "sys.permission.read", "sys.license.manage", "sys.org.manage"]));

        await EnsureRole(RoleDeptManager, "DEPT_MANAGER", "Trưởng phòng", false,
            msg.Concat(wf).Concat(hrmRead).Concat(lmsLearn).Concat(["crm.customer.read"])
                .Concat(["sys.user.read", "hrm.leave.manage", "hrm.employee.read"]));

        await EnsureRole(RoleFinManager, "FIN_MANAGER", "Quản lý Tài chính", false,
            msg.Concat(wf).Concat(lmsLearn).Concat(["crm.customer.read"]).Concat(pur)
                .Concat(["hrm.employee.read", "hrm.payroll.read", "hrm.payroll.manage", "hrm.contract.read", "sys.user.read"]));

        await EnsureRole(RoleItManager, "IT_MANAGER", "Quản lý CNTT", false,
            msg.Concat(wf).Concat(lmsLearn).Concat(lmsCourse).Concat(lmsExam).Concat(crm).Concat(pos).Concat(pur)
                .Concat(["sys.user.read", "sys.user.manage", "sys.role.read", "sys.permission.read", "hrm.employee.read", "hrm.leave.read"]));

        await EnsureRole(RoleSalesManager, "SALES_MANAGER", "Quản lý Kinh doanh", false,
            msg.Concat(wf).Concat(lmsLearn).Concat(crm).Concat(pos)
                .Concat([
                    "pur.vendor.read", "pur.pr.read", "pur.pr.manage", "pur.po.read",
                    "pur.grn.read", "pur.grn.manage", "pur.invoice.read", "pur.invoice.manage"
                ])
                .Concat(["hrm.employee.read", "hrm.leave.read", "hrm.leave.manage", "hrm.recruit.read"]));

        await EnsureRole(RoleAccountant, "ACCOUNTANT", "Kế toán", false,
            msg.Concat(lmsLearn).Concat(["hrm.payroll.read", "hrm.employee.read", "hrm.contract.read", "wf.task.read"]));

        await EnsureRole(RoleIntern, "INTERN", "Thực tập sinh", false,
            msg.Concat(lmsLearn).Concat(["hrm.employee.read", "hrm.leave.read", "wf.task.read"]));

        await EnsureRole(RoleApprover, "APPROVER", "Người phê duyệt", false, wf.Concat(msg).Concat(lmsLearn));

        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Roster công ty: mã NV = tên gọi + viết tắt họ/đệm (vd. Nguyễn Đình Mạnh Hùng → HungNDM).
    /// (user#, emp#, username=mãNV, name, email, phone, dept, jl, title, type, gender, hire, role, managerUser#, extraRoles[])
    /// </summary>
    private static List<(int Un, int En, string User, string Name, string Email, string Phone,
        Guid Dept, string Jl, string Title, string Type, string Gender, DateOnly Hire, Guid Role, int? MgrUn, Guid[] ExtraRoles)>
        BuildCompanyRoster()
    {
        // Username / EmployeeCode dùng cùng mã viết tắt (HungNDM).
        return
        [
            // —— Ban giám đốc / Tài chính ——
            (1, 1, "SangTQ", "Trần Quang Sang", "sangtq@demo.local", "0903000001",
                DeptBod, "DIRECTOR", "DIR", "FT", "Male", new DateOnly(2018, 1, 15), RoleExecutive, null, [RoleApprover]),
            (2, 2, "PhuongHTK", "Huỳnh Thị Kim Phương", "phuonghtk@demo.local", "0903000002",
                DeptFinance, "STAFF", "ACC", "FT", "Female", new DateOnly(2019, 4, 1), RoleAccountant, 1, []),

            // —— IT ——
            (3, 3, "LuongTND", "Trần Nguyễn Đức Lương", "luongtnd@demo.local", "0903000003",
                DeptIt, "MANAGER", "IT_MGR", "FT", "Male", new DateOnly(2019, 8, 1), RoleItManager, 1, [RoleDeptManager, RoleApprover]),
            (4, 4, "HungNDM", "Nguyễn Đình Mạnh Hùng", "hungndm@demo.local", "0903000004",
                DeptIt, "STAFF", "DEV", "FT", "Male", new DateOnly(2021, 3, 1), RoleStaff, 3, []),
            (5, 5, "HungDNB", "Đinh Nguyễn Bảo Hưng", "hungdnb@demo.local", "0903000005",
                DeptIt, "STAFF", "DEV", "FT", "Male", new DateOnly(2022, 5, 10), RoleStaff, 3, []),
            (6, 6, "HuyTQ", "Trần Quang Huy", "huytq@demo.local", "0903000006",
                DeptIt, "STAFF", "DEV", "FT", "Male", new DateOnly(2023, 2, 20), RoleStaff, 3, []),
            (7, 7, "DaiLT", "Lê Tấn Đại", "dailt@demo.local", "0903000007",
                DeptIt, "INTERN", "INTERN_IT", "INTERN", "Male", new DateOnly(2026, 1, 6), RoleIntern, 3, []),

            // —— Marketing ——
            (8, 8, "TrangNTT", "Nguyễn Thị Thùy Trang", "trangntt@demo.local", "0903000008",
                DeptMkt, "MANAGER", "MKT_MGR", "FT", "Female", new DateOnly(2020, 6, 1), RoleDeptManager, 1, [RoleApprover]),
            (9, 9, "HungPT", "Phạm Thành Hưng", "hungpt@demo.local", "0903000009",
                DeptMkt, "STAFF", "EDITOR", "FT", "Male", new DateOnly(2022, 9, 15), RoleStaff, 8, []),

            // —— Ecom ——
            (10, 10, "TuyetHTA", "Hoàng Thị Ánh Tuyết", "tuyethta@demo.local", "0903000010",
                DeptEcom, "MANAGER", "ECOM_MGR", "FT", "Female", new DateOnly(2020, 11, 1), RoleDeptManager, 1, [RoleApprover]),
            (11, 11, "HuongLTT", "Lê Thị Thanh Hương", "huongltt@demo.local", "0903000011",
                DeptEcom, "STAFF", "ECOM", "FT", "Female", new DateOnly(2022, 1, 10), RoleStaff, 10, []),
            (12, 12, "HaNTC", "Nguyễn Thị Cẩm Hà", "hantc@demo.local", "0903000012",
                DeptEcom, "STAFF", "ECOM", "FT", "Female", new DateOnly(2023, 4, 5), RoleStaff, 10, []),
            (13, 13, "TranVTH", "Võ Thị Hà Tràn", "tranvth@demo.local", "0903000013",
                DeptEcom, "STAFF", "ECOM", "FT", "Female", new DateOnly(2023, 8, 18), RoleStaff, 10, []),
            (14, 14, "HanhTTH", "Trần Thị Hồng Hạnh", "hanhtth@demo.local", "0903000014",
                DeptEcom, "STAFF", "ECOM", "FT", "Female", new DateOnly(2024, 2, 1), RoleStaff, 10, []),
        ];
    }
}
