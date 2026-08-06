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
            ("LEGAL", "Chuyên viên Pháp chế", "STAFF", 36),
            ("WH", "Nhân viên Kho", "STAFF", 37),
            ("ASSIST", "Trợ lý", "STAFF", 38),
            ("INTERN_DEV", "Thực tập Dev", "INTERN", 40),
            ("INTERN_HR", "Thực tập HR", "INTERN", 41),
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
    /// Người mẫu: (user#, emp#, username, name, email, phone, dept, jl, title, type, gender, hire, role, managerUser#, extraRoles[])
    /// </summary>
    private static List<(int Un, int En, string User, string Name, string Email, string Phone,
        Guid Dept, string Jl, string Title, string Type, string Gender, DateOnly Hire, Guid Role, int? MgrUn, Guid[] ExtraRoles)>
        BuildCompanyRoster()
    {
        return
        [
            // —— Ban giám đốc ——
            (1, 1, "ceo", "Nguyễn Văn Quang", "quang.nguyen@demo.local", "0902000001",
                DeptBod, "DIRECTOR", "CEO", "FT", "Male", new DateOnly(2015, 3, 1), RoleExecutive, null, [RoleApprover]),
            (2, 2, "dceo", "Trần Minh Châu", "chau.tran@demo.local", "0902000002",
                DeptBod, "DEPUTY", "DCEO", "FT", "Female", new DateOnly(2017, 6, 1), RoleExecutive, 1, [RoleApprover]),
            (3, 3, "cfo", "Lê Hoàng Phúc", "phuc.le@demo.local", "0902000003",
                DeptFinance, "DEPUTY", "CFO", "FT", "Male", new DateOnly(2018, 1, 10), RoleFinManager, 1, [RoleApprover, RoleExecutive]),
            (4, 4, "chro", "Phạm Thị Lan Anh", "lananh.pham@demo.local", "0902000004",
                DeptHr, "DEPUTY", "CHRO", "FT", "Female", new DateOnly(2018, 4, 15), RoleHrManager, 1, [RoleApprover, RoleExecutive]),
            (5, 5, "cto", "Hoàng Đức Khoa", "khoa.hoang@demo.local", "0902000005",
                DeptIt, "DEPUTY", "CTO", "FT", "Male", new DateOnly(2018, 9, 1), RoleItManager, 1, [RoleApprover, RoleExecutive]),
            (6, 6, "assist.ceo", "Ngô Thu Hà", "ha.ngo@demo.local", "0902000006",
                DeptBod, "STAFF", "ASSIST", "FT", "Female", new DateOnly(2020, 2, 1), RoleStaff, 1, []),

            // —— HR ——
            (101, 101, "hr.manager", "Trần Thị Hương", "huong.tran@demo.local", "0901000002",
                DeptHr, "MANAGER", "HR_MGR", "FT", "Female", new DateOnly(2019, 3, 1), RoleHrManager, 4, [RoleDeptManager, RoleApprover]),
            (102, 102, "hr.lead", "Võ Thanh Tùng", "tung.vo@demo.local", "0902000102",
                DeptHr, "LEAD", "HR_LEAD", "FT", "Male", new DateOnly(2020, 5, 12), RoleDeptManager, 101, [RoleApprover]),
            (103, 103, "hr.spec1", "Lê Minh Anh", "anh.le@demo.local", "0901000003",
                DeptHr, "STAFF", "HR_SPEC", "FT", "Female", new DateOnly(2021, 6, 10), RoleStaff, 101, []),
            (104, 104, "hr.spec2", "Phạm Quốc Bảo", "bao.pham@demo.local", "0901000004",
                DeptHr, "STAFF", "HR_SPEC", "PROBATION", "Male", new DateOnly(2025, 11, 1), RoleStaff, 101, []),
            (105, 105, "hr.spec3", "Đinh Thị Ngọc", "ngoc.dinh@demo.local", "0902000105",
                DeptHr, "STAFF", "HR_SPEC", "FT", "Female", new DateOnly(2022, 8, 20), RoleStaff, 102, []),
            (106, 106, "hr.intern", "Nguyễn Hà My", "my.nguyen@demo.local", "0902000106",
                DeptHr, "INTERN", "INTERN_HR", "INTERN", "Female", new DateOnly(2026, 2, 1), RoleIntern, 102, []),

            // —— IT ——
            (201, 201, "it.manager", "Hoàng Đức Minh", "minh.hoang@demo.local", "0901000005",
                DeptIt, "MANAGER", "IT_MGR", "FT", "Male", new DateOnly(2019, 8, 20), RoleItManager, 5, [RoleDeptManager, RoleApprover]),
            (202, 202, "dev.lead", "Cao Xuân Trường", "truong.cao@demo.local", "0902000202",
                DeptIt, "LEAD", "DEV_LEAD", "FT", "Male", new DateOnly(2020, 1, 15), RoleDeptManager, 201, [RoleApprover]),
            (203, 203, "dev.lan", "Ngô Thị Lan", "lan.ngo@demo.local", "0901000006",
                DeptIt, "STAFF", "DEV", "FT", "Female", new DateOnly(2022, 2, 14), RoleStaff, 202, []),
            (204, 204, "dev.tuan", "Đỗ Văn Tuấn", "tuan.do@demo.local", "0901000007",
                DeptIt, "STAFF", "DEV", "FT", "Male", new DateOnly(2023, 5, 2), RoleStaff, 202, []),
            (205, 205, "dev.hung", "Vũ Quang Hùng", "hung.vu@demo.local", "0901000008",
                DeptIt, "STAFF", "DEV", "INTERN", "Male", new DateOnly(2026, 1, 6), RoleIntern, 202, []),
            (206, 206, "dev.linh", "Bùi Khánh Linh", "linh.bui@demo.local", "0902000206",
                DeptIt, "STAFF", "DEV", "FT", "Female", new DateOnly(2024, 3, 1), RoleStaff, 202, []),
            (207, 207, "dev.phong", "Trần Nhật Phong", "phong.tran@demo.local", "0902000207",
                DeptIt, "STAFF", "DEV", "FT", "Male", new DateOnly(2021, 11, 8), RoleStaff, 201, []),
            (208, 208, "it.intern", "Lý Minh Đức", "duc.ly@demo.local", "0902000208",
                DeptIt, "INTERN", "INTERN_DEV", "INTERN", "Male", new DateOnly(2026, 3, 1), RoleIntern, 202, []),

            // —— Sales HQ ——
            (301, 301, "sales.manager", "Bùi Thanh Hà", "ha.bui@demo.local", "0901000009",
                DeptSales, "MANAGER", "SALES_MGR", "FT", "Female", new DateOnly(2020, 4, 12), RoleSalesManager, 2, [RoleDeptManager, RoleApprover]),
            (302, 302, "sales.lead", "Nguyễn Khắc Việt", "viet.nguyen@demo.local", "0902000302",
                DeptSales, "LEAD", "SALES_LEAD", "FT", "Male", new DateOnly(2021, 2, 1), RoleDeptManager, 301, [RoleApprover]),
            (303, 303, "sales.nam", "Nguyễn Thành Nam", "nam.nguyen@demo.local", "0901000010",
                DeptSales, "STAFF", "SALES", "FT", "Male", new DateOnly(2022, 9, 1), RoleStaff, 302, []),
            (304, 304, "sales.mai", "Đặng Thu Mai", "mai.dang@demo.local", "0901000011",
                DeptSales, "STAFF", "SALES", "FT", "Female", new DateOnly(2024, 1, 15), RoleStaff, 302, []),
            (305, 305, "sales.hue", "Phan Thị Huệ", "hue.phan@demo.local", "0902000305",
                DeptSales, "STAFF", "SALES", "FT", "Female", new DateOnly(2023, 6, 1), RoleStaff, 301, []),
            (306, 306, "sales.dat", "Lương Quốc Đạt", "dat.luong@demo.local", "0902000306",
                DeptSales, "STAFF", "SALES", "PROBATION", "Male", new DateOnly(2025, 12, 1), RoleStaff, 302, []),

            // —— Finance ——
            (401, 401, "fin.manager", "Đỗ Thị Kim Ngân", "ngan.do@demo.local", "0902000401",
                DeptFinance, "MANAGER", "FIN_MGR", "FT", "Female", new DateOnly(2019, 7, 1), RoleFinManager, 3, [RoleDeptManager, RoleApprover]),
            (402, 402, "fin.acc1", "Lý Thị Kim", "kim.ly@demo.local", "0901000012",
                DeptFinance, "STAFF", "ACC", "FT", "Female", new DateOnly(2021, 1, 4), RoleAccountant, 401, []),
            (403, 403, "fin.acc2", "Trịnh Văn Khoa", "khoa.trinh@demo.local", "0901000013",
                DeptFinance, "STAFF", "ACC", "CONTRACT", "Male", new DateOnly(2024, 7, 1), RoleAccountant, 401, []),
            (404, 404, "fin.acc3", "Hoàng Mỹ Dung", "dung.hoang@demo.local", "0902000404",
                DeptFinance, "STAFF", "ACC", "FT", "Female", new DateOnly(2022, 4, 18), RoleAccountant, 401, []),

            // —— Marketing ——
            (501, 501, "mkt.manager", "Tạ Hoàng Yến", "yen.ta@demo.local", "0902000501",
                DeptMkt, "MANAGER", "MKT_MGR", "FT", "Female", new DateOnly(2020, 9, 1), RoleDeptManager, 2, [RoleApprover]),
            (502, 502, "mkt.nv1", "Chu Văn Sơn", "son.chu@demo.local", "0902000502",
                DeptMkt, "STAFF", "MKT", "FT", "Male", new DateOnly(2022, 5, 1), RoleStaff, 501, []),
            (503, 503, "mkt.nv2", "Đặng Thảo Vy", "vy.dang@demo.local", "0902000503",
                DeptMkt, "STAFF", "MKT", "FT", "Female", new DateOnly(2023, 8, 15), RoleStaff, 501, []),

            // —— Legal ——
            (601, 601, "legal.manager", "Mai Quốc Việt", "viet.mai@demo.local", "0902000601",
                DeptLegal, "MANAGER", "LEGAL_MGR", "FT", "Male", new DateOnly(2019, 12, 1), RoleDeptManager, 2, [RoleApprover]),
            (602, 602, "legal.nv1", "Trương Ánh Nguyệt", "nguyet.truong@demo.local", "0902000602",
                DeptLegal, "STAFF", "LEGAL", "FT", "Female", new DateOnly(2021, 3, 22), RoleStaff, 601, []),

            // —— Ops HCM ——
            (701, 701, "ops.lead", "Phan Hải Đăng", "dang.phan@demo.local", "0901000014",
                DeptOps, "MANAGER", "OPS_MGR", "FT", "Male", new DateOnly(2020, 10, 8), RoleDeptManager, 2, [RoleApprover]),
            (702, 702, "ops.nv1", "Mai Thị Oanh", "oanh.mai@demo.local", "0901000015",
                DeptOps, "STAFF", "OPS", "FT", "Female", new DateOnly(2023, 3, 20), RoleStaff, 701, []),
            (703, 703, "ops.nv2", "Huỳnh Tấn Tài", "tai.huynh@demo.local", "0902000703",
                DeptOps, "STAFF", "OPS", "FT", "Male", new DateOnly(2022, 7, 1), RoleStaff, 701, []),
            (704, 704, "ops.nv3", "Lâm Bảo Châu", "chau.lam@demo.local", "0902000704",
                DeptOps, "STAFF", "OPS", "PROBATION", "Female", new DateOnly(2025, 10, 1), RoleStaff, 701, []),

            // —— Warehouse HCM ——
            (801, 801, "wh.manager", "Nguyễn Văn Kho", "kho.nguyen@demo.local", "0902000801",
                DeptWh, "MANAGER", "WH_MGR", "FT", "Male", new DateOnly(2021, 1, 5), RoleDeptManager, 701, [RoleApprover]),
            (802, 802, "wh.nv1", "Trần Quốc Bảo", "bao.tran@demo.local", "0902000802",
                DeptWh, "STAFF", "WH", "FT", "Male", new DateOnly(2022, 11, 1), RoleStaff, 801, []),
            (803, 803, "wh.nv2", "Lê Thị Hạnh", "hanh.le@demo.local", "0902000803",
                DeptWh, "STAFF", "WH", "FT", "Female", new DateOnly(2024, 2, 12), RoleStaff, 801, []),

            // —— Sales HCM ——
            (901, 901, "sales.hcm.mgr", "Võ Thị Kim Chi", "chi.vo@demo.local", "0902000901",
                DeptSalesHcm, "MANAGER", "SALES_MGR", "FT", "Female", new DateOnly(2021, 4, 1), RoleSalesManager, 301, [RoleDeptManager, RoleApprover]),
            (902, 902, "sales.hcm1", "Phạm Đức Anh", "anh.pham@demo.local", "0902000902",
                DeptSalesHcm, "STAFF", "SALES", "FT", "Male", new DateOnly(2023, 1, 9), RoleStaff, 901, []),
            (903, 903, "sales.hcm2", "Ngô Bảo Trân", "tran.ngo@demo.local", "0902000903",
                DeptSalesHcm, "STAFF", "SALES", "FT", "Female", new DateOnly(2024, 6, 1), RoleStaff, 901, []),
        ];
    }
}
