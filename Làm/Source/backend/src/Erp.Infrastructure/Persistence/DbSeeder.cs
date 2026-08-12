using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Entities.Wf;
using Erp.Domain.Enums.Sys;
using Erp.Domain.Modules;
using Erp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Erp.Infrastructure.Persistence;

public static partial class DbSeeder
{
    public const string DefaultPassword = "!Abc123";

    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AdminId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid RoleSuperAdmin = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid RoleHrManager = Guid.Parse("33333333-3333-3333-3333-333333333301");
    private static readonly Guid RoleStaff = Guid.Parse("33333333-3333-3333-3333-333333333302");
    private static readonly Guid RoleLmsInstructor = Guid.Parse("33333333-3333-3333-3333-333333333303");
    private static readonly Guid OrgHq = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid OrgHcm = Guid.Parse("44444444-4444-4444-4444-444444444401");
    private static readonly Guid DeptIt = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid DeptHr = Guid.Parse("55555555-5555-5555-5555-555555555501");
    private static readonly Guid DeptSales = Guid.Parse("55555555-5555-5555-5555-555555555502");
    private static readonly Guid DeptFinance = Guid.Parse("55555555-5555-5555-5555-555555555503");
    private static readonly Guid DeptOps = Guid.Parse("55555555-5555-5555-5555-555555555504");
    private static readonly Guid JlDirector = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid JlManager = Guid.Parse("66666666-6666-6666-6666-666666666601");
    private static readonly Guid JlStaff = Guid.Parse("66666666-6666-6666-6666-666666666602");
    private static readonly Guid LicenseId = Guid.Parse("77777777-7777-7777-7777-777777777777");

    public static async Task SeedAsync(IServiceProvider sp, CancellationToken ct = default)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await db.Database.MigrateAsync(ct);

        if (!await db.Tenants.AnyAsync(ct))
        {
            await SeedFreshAsync(db, log, ct);
            return;
        }

        await SyncDemoAsync(db, log, ct);
    }

    /// <summary>DB đã có: cập nhật mật khẩu demo + bổ sung HRM nếu thiếu.</summary>
    private static async Task SyncDemoAsync(AppDbContext db, ILogger log, CancellationToken ct)
    {
        var admin = await db.Users.FirstOrDefaultAsync(x => x.Id == AdminId || x.Username == "admin", ct);
        if (admin is not null)
        {
            admin.PasswordHash = PasswordHasher.Hash(DefaultPassword);
            if (string.IsNullOrWhiteSpace(admin.Email))
                admin.Email = "admin@local.test";
            await db.SaveChangesAsync(ct);
            log.LogInformation("Demo password synced — admin / {Password}", DefaultPassword);
        }

        await EnsureOrgCatalogAsync(db, ct);
        await EnsureRolesAndPermsAsync(db, ct);
        await EnsureAllLicenseModulesAsync(db, ct);
        await SeedMenusAndWfAsync(db, ct);

        var lic = await db.Licenses.FirstOrDefaultAsync(x => x.Id == LicenseId || x.TenantId == TenantId, ct);
        if (lic is not null)
        {
            if (lic.MaxUsers < 200) lic.MaxUsers = 200;
            if (lic.MaxOrgUnits < 50) lic.MaxOrgUnits = 50;
        }

        // Idempotent — bổ sung user/role/org còn thiếu cho full công ty demo
        await SeedPersonnelAsync(db, ct);
        await SeedLeaveBalancesAsync(db, ct);
        await db.SaveChangesAsync(ct);
        log.LogInformation("Seed sync OK (full company roster · perms/menus/leave).");
    }

    private static async Task EnsureAllLicenseModulesAsync(AppDbContext db, CancellationToken ct)
    {
        var lic = await db.Licenses.FirstOrDefaultAsync(x => x.TenantId == TenantId && !x.IsDeleted, ct);
        if (lic is null) return;
        if (lic.MaxOrgUnits <= 0) lic.MaxOrgUnits = 50;

        var mods = ModuleCatalog.SellableCodes;
        var have = await db.LicenseModules.Where(x => x.TenantId == TenantId && !x.IsDeleted).Select(x => x.ModuleCode).ToListAsync(ct);
        foreach (var mod in mods)
        {
            if (have.Any(h => string.Equals(h, mod, StringComparison.OrdinalIgnoreCase))) continue;
            db.LicenseModules.Add(new LicenseModule
            {
                TenantId = TenantId, LicenseId = lic.Id, ModuleCode = mod, IsEnabled = true
            });
        }
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedFreshAsync(AppDbContext db, ILogger log, CancellationToken ct)
    {
        db.Tenants.Add(new Tenant
        {
            Id = TenantId,
            Code = "DEMO",
            Name = "Công ty Demo Pum's ERP",
            Status = "Active"
        });

        await EnsureOrgCatalogAsync(db, ct);
        await EnsureRolesAndPermsAsync(db, ct);

        db.Licenses.Add(new License
        {
            Id = LicenseId,
            TenantId = TenantId,
            PlanCode = "PLAN_ENTERPRISE",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(1)),
            MaxUsers = 200,
            Status = "Active"
        });

        foreach (var mod in ModuleCatalog.SellableCodes)
        {
            db.LicenseModules.Add(new LicenseModule
            {
                TenantId = TenantId,
                LicenseId = LicenseId,
                ModuleCode = mod,
                IsEnabled = true
            });
        }

        await SeedPersonnelAsync(db, ct);
        await SeedLeaveBalancesAsync(db, ct);
        await SeedMenusAndWfAsync(db, ct);

        await db.SaveChangesAsync(ct);
        log.LogInformation("Seed OK — admin / {Password} (tenant DEMO, HRM đầy đủ)", DefaultPassword);
    }

    private static async Task EnsureOrgCatalogAsync(AppDbContext db, CancellationToken ct)
    {
        if (!await db.OrgUnits.AnyAsync(x => x.Id == OrgHq, ct))
        {
            db.OrgUnits.Add(new OrgUnit
            {
                Id = OrgHq, TenantId = TenantId, Code = "HQ", Name = "Trụ sở Hà Nội",
                UnitType = "Company", Path = $"/{OrgHq:N}/", IsActive = true, SortOrder = 1
            });
        }

        if (!await db.OrgUnits.AnyAsync(x => x.Id == OrgHcm, ct))
        {
            db.OrgUnits.Add(new OrgUnit
            {
                Id = OrgHcm, TenantId = TenantId, Code = "HCM", Name = "Chi nhánh TP.HCM",
                UnitType = "Branch", ParentId = OrgHq, Path = $"/{OrgHq:N}/{OrgHcm:N}/", IsActive = true, SortOrder = 2
            });
        }

        var depts = new (Guid Id, string Code, string Name, Guid Org, int Sort)[]
        {
            (DeptIt, "IT", "Công nghệ thông tin", OrgHq, 1),
            (DeptHr, "HR", "Nhân sự", OrgHq, 2),
            (DeptSales, "SALES", "Kinh doanh", OrgHq, 3),
            (DeptFinance, "FIN", "Tài chính – Kế toán", OrgHq, 4),
            (DeptOps, "OPS", "Vận hành", OrgHcm, 5),
        };

        foreach (var d in depts)
        {
            if (await db.Departments.AnyAsync(x => x.Id == d.Id, ct)) continue;
            db.Departments.Add(new Department
            {
                Id = d.Id, TenantId = TenantId, Code = d.Code, Name = d.Name,
                OrgUnitId = d.Org, Path = $"/{d.Id:N}/", IsActive = true, SortOrder = d.Sort
            });
        }

        var levels = new (Guid PreferredId, string Code, string Name, int Order, ScopeType Scope)[]
        {
            (JlDirector, "DIRECTOR", "Giám đốc", 1, ScopeType.All),
            (JlManager, "MANAGER", "Quản lý", 2, ScopeType.Department),
            (JlStaff, "STAFF", "Nhân viên", 3, ScopeType.Own),
        };
        foreach (var jl in levels)
        {
            if (await db.JobLevels.AnyAsync(x => x.TenantId == TenantId && x.Code == jl.Code, ct)) continue;
            db.JobLevels.Add(new JobLevel
            {
                Id = jl.PreferredId, TenantId = TenantId, Code = jl.Code, Name = jl.Name,
                LevelOrder = jl.Order, DefaultScopeType = jl.Scope, IsActive = true
            });
        }

        await db.SaveChangesAsync(ct);

        if (!await db.EmployeeTypes.AnyAsync(ct))
        {
            db.EmployeeTypes.AddRange(
                new EmployeeType { TenantId = TenantId, Code = "FT", Name = "Chính thức" },
                new EmployeeType { TenantId = TenantId, Code = "PROBATION", Name = "Thử việc" },
                new EmployeeType { TenantId = TenantId, Code = "CONTRACT", Name = "Hợp đồng thời vụ" },
                new EmployeeType { TenantId = TenantId, Code = "INTERN", Name = "Thực tập" }
            );
        }

        if (!await db.JobTitles.AnyAsync(ct))
        {
            var jlMap = await db.JobLevels.Where(x => x.TenantId == TenantId).ToDictionaryAsync(x => x.Code, x => x.Id, ct);
            db.JobTitles.AddRange(
                new JobTitle { TenantId = TenantId, Code = "CEO", Name = "Tổng giám đốc", DefaultJobLevelId = jlMap["DIRECTOR"], SortOrder = 1 },
                new JobTitle { TenantId = TenantId, Code = "HR_MGR", Name = "Trưởng phòng Nhân sự", DefaultJobLevelId = jlMap["MANAGER"], SortOrder = 2 },
                new JobTitle { TenantId = TenantId, Code = "IT_MGR", Name = "Trưởng phòng CNTT", DefaultJobLevelId = jlMap["MANAGER"], SortOrder = 3 },
                new JobTitle { TenantId = TenantId, Code = "SALES_MGR", Name = "Trưởng phòng Kinh doanh", DefaultJobLevelId = jlMap["MANAGER"], SortOrder = 4 },
                new JobTitle { TenantId = TenantId, Code = "ACC", Name = "Kế toán viên", DefaultJobLevelId = jlMap["STAFF"], SortOrder = 5 },
                new JobTitle { TenantId = TenantId, Code = "DEV", Name = "Lập trình viên", DefaultJobLevelId = jlMap["STAFF"], SortOrder = 6 },
                new JobTitle { TenantId = TenantId, Code = "HR_SPEC", Name = "Chuyên viên Nhân sự", DefaultJobLevelId = jlMap["STAFF"], SortOrder = 7 },
                new JobTitle { TenantId = TenantId, Code = "SALES", Name = "Nhân viên Kinh doanh", DefaultJobLevelId = jlMap["STAFF"], SortOrder = 8 },
                new JobTitle { TenantId = TenantId, Code = "OPS", Name = "Nhân viên Vận hành", DefaultJobLevelId = jlMap["STAFF"], SortOrder = 9 }
            );
        }

        if (!await db.LeaveTypes.AnyAsync(ct))
        {
            db.LeaveTypes.AddRange(
                new LeaveType { TenantId = TenantId, Code = "AL", Name = "Phép năm", IsPaid = true, DefaultDaysPerYear = 12 },
                new LeaveType { TenantId = TenantId, Code = "SL", Name = "Ốm đau", IsPaid = true, DefaultDaysPerYear = 30 },
                new LeaveType { TenantId = TenantId, Code = "UL", Name = "Không lương", IsPaid = false, DefaultDaysPerYear = 0 },
                new LeaveType { TenantId = TenantId, Code = "ML", Name = "Thai sản", IsPaid = true, DefaultDaysPerYear = 180 },
                new LeaveType { TenantId = TenantId, Code = "WH", Name = "Công tác", IsPaid = true, DefaultDaysPerYear = 0 }
            );
        }

        await db.SaveChangesAsync(ct);

        await EnsureExpandedCatalogAsync(db, ct);
    }

    private static async Task EnsureRolesAndPermsAsync(AppDbContext db, CancellationToken ct)
    {
        // Digi-style catalog: {module}.{resource}.{action} · lowercase
        var permDefs = new[]
        {
            ("SYS", "sys.user.read", "user", "Read", "Xem người dùng"),
            ("SYS", "sys.user.manage", "user", "Manage", "Quản trị người dùng"),
            ("SYS", "sys.role.read", "role", "Read", "Xem vai trò"),
            ("SYS", "sys.role.update", "role", "Update", "Tạo / sửa vai trò"),
            ("SYS", "sys.role.assign", "role", "Assign", "Gán quyền vào vai trò"),
            ("SYS", "sys.role.manage", "role", "Manage", "Quản trị vai trò (tương thích)"),
            ("SYS", "sys.permission.read", "permission", "Read", "Xem danh mục quyền (catalog seed)"),
            // Quyền không tạo/sửa qua UI/API — chỉ seed khi làm chức năng. Giữ code cũ inactive nếu đã có.
            ("SYS", "sys.license.manage", "license", "Manage", "Quản trị license"),
            ("SYS", "sys.org.manage", "org", "Manage", "Quản trị tổ chức"),
            ("SYS", "sys.msg.read", "msg", "Read", "Xem tin nhắn"),
            ("SYS", "sys.msg.send", "msg", "Send", "Gửi tin nhắn"),
            ("SYS", "sys.sso.read", "sso", "Read", "Xem cấu hình SSO / OAuth"),
            ("SYS", "sys.sso.manage", "sso", "Manage", "Quản trị IdP SSO / OAuth"),
            ("SYS", "sys.fieldperm.read", "fieldperm", "Read", "Xem quyền trường nhạy cảm"),
            ("SYS", "sys.fieldperm.manage", "fieldperm", "Manage", "Gán quyền trường nhạy cảm"),
            ("SYS", "sys.config.version.read", "config", "Read", "Xem phiên bản cấu hình"),
            ("SYS", "sys.config.version.rollback", "config", "Rollback", "Rollback phiên bản cấu hình"),
            ("SYS", "sys.push.device.self", "push", "Self", "Đăng ký / thu hồi device push của mình"),
            ("SYS", "sys.push.manage", "push", "Manage", "Gửi thử push notification"),
            ("SYS", "sys.file.scan", "file", "Scan", "Quét virus / xem trạng thái bảo mật file"),
            ("SYS", "sys.export.bulk", "export", "Bulk", "Xuất dữ liệu hàng loạt"),
            ("SYS", "sys.export.job.read", "export", "Read", "Xem / tải job xuất dữ liệu"),
            ("SYS", "sys.ip.read", "ip", "Read", "Xem quy tắc IP allow/deny"),
            ("SYS", "sys.ip.manage", "ip", "Manage", "Quản trị quy tắc IP allow/deny"),
            ("SYS", "sys.brand.read", "brand", "Read", "Xem theme / branding"),
            ("SYS", "sys.brand.manage", "brand", "Manage", "Quản trị theme / màu / favicon"),
            ("SYS", "sys.ui.home.manage", "ui", "Manage", "Cấu hình trang chủ theo vai trò"),
            ("WF", "wf.task.read", "task", "Read", "Xem tác vụ phê duyệt"),
            ("WF", "wf.task.act", "task", "Act", "Xử lý phê duyệt"),
            ("HRM", "hrm.employee.read", "employee", "Read", "Xem hồ sơ nhân sự"),
            ("HRM", "hrm.employee.manage", "employee", "Manage", "Quản trị hồ sơ nhân sự"),
            ("HRM", "hrm.leave.read", "leave", "Read", "Xem nghỉ phép"),
            ("HRM", "hrm.leave.manage", "leave", "Manage", "Quản trị nghỉ phép"),
            ("HRM", "hrm.contract.read", "contract", "Read", "Xem hợp đồng LĐ"),
            ("HRM", "hrm.contract.manage", "contract", "Manage", "Quản trị hợp đồng LĐ"),
            ("HRM", "hrm.recruit.read", "recruit", "Read", "Xem nhu cầu tuyển dụng"),
            ("HRM", "hrm.recruit.manage", "recruit", "Manage", "Quản trị nhu cầu tuyển dụng"),
            ("HRM", "hrm.payroll.read", "payroll", "Read", "Xem lương kỳ"),
            ("HRM", "hrm.payroll.manage", "payroll", "Manage", "Quản trị lương kỳ"),
            ("LMS", "lms.class.read", "class", "Read", "Xem lớp đào tạo offline"),
            ("LMS", "lms.class.manage", "class", "Manage", "Quản trị lớp đào tạo offline"),
            ("LMS", "lms.course.read", "course", "Read", "Xem CTĐT / khóa học"),
            ("LMS", "lms.course.manage", "course", "Manage", "Quản trị CTĐT / khóa học"),
            ("LMS", "lms.learn.read", "learn", "Read", "Xem catalog & học online"),
            ("LMS", "lms.learn.enroll", "learn", "Enroll", "Ghi danh / mua khóa online"),
            ("LMS", "lms.exam.read", "exam", "Read", "Xem NHCH / đề thi"),
            ("LMS", "lms.exam.manage", "exam", "Manage", "Quản trị NHCH / đề thi"),
            ("LMS", "lms.instructor.read", "instructor", "Read", "Xem hồ sơ giảng viên"),
            ("LMS", "lms.instructor.manage", "instructor", "Manage", "Quản trị giảng viên / phân quyền"),
            ("LMS", "lms.report.read", "report", "Read", "Xem / xuất báo cáo đào tạo"),
            ("CRM", "crm.customer.read", "customer", "Read", "Xem khách hàng CRM"),
            ("CRM", "crm.customer.manage", "customer", "Manage", "Quản trị khách hàng CRM"),
            ("CRM", "crm.lead.read", "lead", "Read", "Xem lead / nguồn / báo cáo chuyển đổi"),
            ("CRM", "crm.lead.manage", "lead", "Manage", "Quản trị lead / task / convert"),
            ("CRM", "crm.opportunity.read", "opportunity", "Read", "Xem cơ hội / báo giá stub"),
            ("CRM", "crm.opportunity.manage", "opportunity", "Manage", "Quản trị cơ hội / stage / quote"),
            ("CRM", "crm.quote.read", "quote", "Read", "Xem báo giá / bảng giá CRM"),
            ("CRM", "crm.quote.manage", "quote", "Manage", "Quản trị báo giá / chiết khấu / gửi"),
            ("CRM", "crm.order.read", "order", "Read", "Xem đơn hàng bán CRM"),
            ("CRM", "crm.order.manage", "order", "Manage", "Quản trị đơn / thanh toán / giữ tồn"),
            ("CRM", "crm.campaign.read", "campaign", "Read", "Xem campaign / chi phí / dashboard marketing"),
            ("CRM", "crm.campaign.manage", "campaign", "Manage", "Quản trị campaign / web-lead / đóng chiến dịch"),
            ("CRM", "crm.promotion.read", "promotion", "Read", "Xem CTKM / voucher"),
            ("CRM", "crm.promotion.manage", "promotion", "Manage", "Quản trị CTKM / sinh voucher / áp báo giá"),
            ("CRM", "crm.chat.read", "chat", "Read", "Xem lịch sử chat omnichannel"),
            ("CRM", "crm.chat.manage", "chat", "Manage", "Ghi lịch sử chat omnichannel"),
            ("POS", "pos.store.read", "store", "Read", "Xem điểm bán POS"),
            ("POS", "pos.store.manage", "store", "Manage", "Quản trị điểm bán POS"),
            ("POS", "pos.shift.read", "shift", "Read", "Xem ca thu ngân POS"),
            ("POS", "pos.shift.manage", "shift", "Manage", "Mở/đóng ca · báo cáo ca"),
            ("POS", "pos.sale.read", "sale", "Read", "Xem đơn bán / trả hàng POS"),
            ("POS", "pos.sale.manage", "sale", "Manage", "Bán hàng · thanh toán · trả hàng"),
            ("POS", "pos.catalog.read", "catalog", "Read", "Xem catalog / giá / thuế POS"),
            ("POS", "pos.catalog.manage", "catalog", "Manage", "Quản trị catalog / giá / thuế POS"),
            ("POS", "pos.promo.read", "promo", "Read", "Xem CTKM / voucher POS"),
            ("POS", "pos.promo.manage", "promo", "Manage", "CTKM · voucher · duyệt giảm tay"),
            ("PUR", "pur.vendor.read", "vendor", "Read", "Xem nhà cung cấp"),
            ("PUR", "pur.vendor.manage", "vendor", "Manage", "Quản trị nhà cung cấp"),
            ("PUR", "pur.pr.read", "pr", "Read", "Xem yêu cầu mua hàng"),
            ("PUR", "pur.pr.manage", "pr", "Manage", "Tạo / gửi PR"),
            ("PUR", "pur.pr.approve", "pr", "Approve", "Duyệt / từ chối / trả PR"),
            ("PUR", "pur.po.read", "po", "Read", "Xem đơn mua hàng"),
            ("PUR", "pur.po.manage", "po", "Manage", "Tạo / gửi PO"),
            ("PUR", "pur.po.approve", "po", "Approve", "Duyệt PO vượt hạn mức"),
            ("PUR", "pur.grn.read", "grn", "Read", "Xem phiếu nhận hàng"),
            ("PUR", "pur.grn.manage", "grn", "Manage", "Tạo / post GRN · đẩy INV"),
            ("PUR", "pur.invoice.read", "invoice", "Read", "Xem hóa đơn NCC"),
            ("PUR", "pur.invoice.manage", "invoice", "Manage", "Nhập HĐ · 3-way · đẩy AP"),
            ("INV", "inv.item.read", "item", "Read", "Xem SKU / nhóm / ĐVT"),
            ("INV", "inv.item.manage", "item", "Manage", "Quản trị SKU / nhóm / ĐVT"),
            ("INV", "inv.warehouse.read", "warehouse", "Read", "Xem kho / loại kho"),
            ("INV", "inv.warehouse.manage", "warehouse", "Manage", "Quản trị kho / thủ kho"),
            ("INV", "inv.stock.read", "stock", "Read", "Xem tồn / phiếu nhập xuất / chuyển"),
            ("INV", "inv.stock.manage", "stock", "Manage", "Nhập xuất · chuyển kho · post tồn"),
            ("INV", "inv.stocktake.read", "stocktake", "Read", "Xem phiếu kiểm kê"),
            ("INV", "inv.stocktake.manage", "stocktake", "Manage", "Kiểm kê · duyệt điều chỉnh"),
            ("LOG", "log.carrier.read", "carrier", "Read", "Xem đơn vị vận chuyển"),
            ("LOG", "log.carrier.manage", "carrier", "Manage", "Quản trị đơn vị vận chuyển"),
            ("LOG", "log.delivery.read", "delivery", "Read", "Xem lệnh giao / vận đơn"),
            ("LOG", "log.delivery.manage", "delivery", "Manage", "Quản trị lệnh giao / vận đơn"),
            ("LOG", "log.cod.read", "cod", "Read", "Xem COD / bàn giao / báo cáo"),
            ("LOG", "log.cod.manage", "cod", "Manage", "Thu · bàn giao · đối soát COD"),
            ("LOG", "log.return.read", "return", "Read", "Xem phiếu hoàn hàng LOG"),
            ("LOG", "log.return.manage", "return", "Manage", "Tạo · đếm · nhập kho hoàn"),
            ("MFG", "mfg.master.read", "master", "Read", "Xem danh mục SX / BOM"),
            ("MFG", "mfg.master.manage", "master", "Manage", "Quản trị danh mục SX / BOM"),
            ("MFG", "mfg.plan.read", "plan", "Read", "Xem kế hoạch SX"),
            ("MFG", "mfg.plan.manage", "plan", "Manage", "Quản trị kế hoạch SX"),
            ("MFG", "mfg.wo.read", "wo", "Read", "Xem lệnh sản xuất"),
            ("MFG", "mfg.wo.manage", "wo", "Manage", "Quản trị lệnh sản xuất"),
            ("FSM", "fsm.master.read", "master", "Read", "Xem danh mục FSM / SLA"),
            ("FSM", "fsm.master.manage", "master", "Manage", "Quản trị danh mục FSM / SLA"),
            ("FSM", "fsm.asset.read", "asset", "Read", "Xem thiết bị install base"),
            ("FSM", "fsm.asset.manage", "asset", "Manage", "Quản trị thiết bị install base"),
            ("FSM", "fsm.ticket.read", "ticket", "Read", "Xem ticket FSM"),
            ("FSM", "fsm.ticket.manage", "ticket", "Manage", "Quản trị ticket FSM"),
            ("PJM", "pjm.master.read", "master", "Read", "Xem loại DA / WBS mẫu / TT"),
            ("PJM", "pjm.master.manage", "master", "Manage", "Quản trị loại DA / WBS mẫu / TT"),
            ("PJM", "pjm.project.read", "project", "Read", "Xem dự án / WBS"),
            ("PJM", "pjm.project.manage", "project", "Manage", "Quản trị dự án / WBS"),
            ("FIN", "fin.master.read", "master", "Read", "Xem COA / kỳ / TTCP / thuế"),
            ("FIN", "fin.master.manage", "master", "Manage", "Quản trị COA / kỳ / TTCP / thuế"),
            ("FIN", "fin.journal.read", "journal", "Read", "Xem bút toán / sổ cái"),
            ("FIN", "fin.journal.manage", "journal", "Manage", "Ghi / đảo bút toán"),
            ("FIN", "fin.cash.read", "cash", "Read", "Xem quỹ / phiếu / sổ quỹ"),
            ("FIN", "fin.cash.manage", "cash", "Manage", "Quỹ · phiếu thu/chi · ghi sổ"),
            ("FIN", "fin.bank.read", "bank", "Read", "Xem TKNH / giấy báo / sao kê / đề nghị CK"),
            ("FIN", "fin.bank.manage", "bank", "Manage", "TKNH · giấy báo · đối soát · chuyển khoản"),
            ("FIN", "fin.ap.read", "ap", "Read", "Xem AP / đề nghị TT / tuổi nợ"),
            ("FIN", "fin.ap.manage", "ap", "Manage", "HĐ AP · đề nghị · duyệt · thanh toán"),
            ("FIN", "fin.ar.read", "ar", "Read", "Xem AR / thu tiền / hạn mức / tuổi nợ"),
            ("FIN", "fin.ar.manage", "ar", "Manage", "HĐ AR · thu tiền · hạn mức tín dụng"),
            ("FIN", "fin.tax.read", "tax", "Read", "Xem thuế suất / bảng kê GTGT"),
            ("FIN", "fin.tax.manage", "tax", "Manage", "Cấu hình thuế suất · ghi nhận GTGT"),
            ("FIN", "fin.revenue.read", "revenue", "Read", "Xem ghi nhận doanh thu / giá vốn"),
            ("FIN", "fin.revenue.manage", "revenue", "Manage", "Ghi nhận doanh thu POS/đơn/AR · COGS"),
            ("AST", "ast.master.read", "master", "Read", "Xem nhóm TS / vị trí / PP KH"),
            ("AST", "ast.master.manage", "master", "Manage", "Quản trị nhóm TS / vị trí / PP KH"),
            ("AST", "ast.asset.read", "asset", "Read", "Xem thẻ TS / sổ KH"),
            ("AST", "ast.asset.manage", "asset", "Manage", "Quản trị thẻ TS / tính KH"),
            ("BI", "bi.catalog.read", "catalog", "Read", "Xem dataset / dashboard / quyền BC"),
            ("BI", "bi.catalog.manage", "catalog", "Manage", "Quản trị dataset / dashboard / quyền BC"),
            ("BI", "bi.report.read", "report", "Read", "Xem danh mục / lịch sử chạy BC"),
            ("BI", "bi.report.run", "report", "Run", "Chạy / xuất báo cáo"),
            ("PRT", "prt.account.read", "account", "Read", "Xem tài khoản portal"),
            ("PRT", "prt.account.manage", "account", "Manage", "Quản trị / đăng ký / login stub portal"),
            ("PRT", "prt.portal.read", "portal", "Read", "Xem đơn / công nợ / ticket portal"),
            ("PRT", "prt.portal.manage", "portal", "Manage", "Quản trị đơn / công nợ / ticket portal"),
        };

        var existingCodes = await db.Permissions.AsNoTracking().Select(x => x.Code).ToListAsync(ct);
        var newPerms = new List<Permission>();
        foreach (var p in permDefs)
        {
            var code = p.Item2.ToLowerInvariant();
            if (existingCodes.Any(c => string.Equals(c, code, StringComparison.OrdinalIgnoreCase))) continue;
            newPerms.Add(new Permission
            {
                Id = Guid.NewGuid(),
                ModuleCode = p.Item1,
                Code = code,
                Name = p.Item5,
                Resource = p.Item3.ToLowerInvariant(),
                Action = p.Item4,
                IsActive = true
            });
        }

        if (newPerms.Count > 0)
            db.Permissions.AddRange(newPerms);

        // Chuẩn hóa code lowercase (Digi)
        foreach (var p in await db.Permissions.Where(x => !x.IsDeleted).ToListAsync(ct))
        {
            var lower = p.Code.ToLowerInvariant();
            if (p.Code != lower) p.Code = lower;
            var res = p.Resource.ToLowerInvariant();
            if (p.Resource != res) p.Resource = res;
            // Catalog quyền chỉ xem — tắt API-create/edit legacy codes
            if (p.Code is "sys.permission.update" or "sys.permission.delete")
            {
                p.IsActive = false;
                p.Name = p.Code.EndsWith("update")
                    ? "(Ngưng) Tạo/sửa quyền — chỉ seed"
                    : "(Ngưng) Xóa quyền — chỉ seed";
            }
        }

        await db.SaveChangesAsync(ct);

        var allPerms = await db.Permissions.Where(x => !x.IsDeleted).ToListAsync(ct);

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
                await db.SaveChangesAsync(ct);
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

        await EnsureRole(RoleSuperAdmin, "SUPER_ADMIN", "Super Admin", true, allPerms.Select(p => p.Code));
        await EnsureRole(RoleHrManager, "HR_MANAGER", "Quản lý Nhân sự", false,
            new[] {
                "sys.user.read", "sys.user.manage", "sys.role.read", "sys.permission.read",
                "sys.msg.read", "sys.msg.send",
                "hrm.employee.read", "hrm.employee.manage", "hrm.leave.read", "hrm.leave.manage",
                "hrm.contract.read", "hrm.contract.manage",
                "hrm.recruit.read", "hrm.recruit.manage",
                "hrm.payroll.read", "hrm.payroll.manage",
                "lms.class.read", "lms.class.manage",
                "lms.course.read", "lms.course.manage",
                "lms.learn.read", "lms.learn.enroll",
                "lms.exam.read", "lms.exam.manage",
                "lms.instructor.read", "lms.instructor.manage",
                "lms.report.read",
                "crm.customer.read", "crm.customer.manage",
                "crm.lead.read", "crm.lead.manage",
                "crm.opportunity.read", "crm.opportunity.manage",
                "crm.quote.read", "crm.quote.manage",
                "crm.order.read", "crm.order.manage",
                "crm.campaign.read", "crm.campaign.manage",
                "crm.promotion.read", "crm.promotion.manage",
                "crm.chat.read", "crm.chat.manage",
                "pos.store.read", "pos.store.manage",
                "pos.shift.read", "pos.shift.manage",
                "pos.sale.read", "pos.sale.manage",
                "pos.catalog.read", "pos.catalog.manage",
                "pos.promo.read", "pos.promo.manage",
                "pur.vendor.read", "pur.vendor.manage",
                "pur.pr.read", "pur.pr.manage", "pur.pr.approve",
                "pur.po.read", "pur.po.manage", "pur.po.approve",
                "pur.grn.read", "pur.grn.manage",
                "pur.invoice.read", "pur.invoice.manage",
                "inv.item.read", "inv.item.manage",
                "inv.warehouse.read", "inv.warehouse.manage",
                "inv.stock.read", "inv.stock.manage",
                "inv.stocktake.read", "inv.stocktake.manage",
                "log.carrier.read", "log.carrier.manage",
                "log.delivery.read", "log.delivery.manage",
                "log.cod.read", "log.cod.manage",
                "log.return.read", "log.return.manage",
                "mfg.master.read", "mfg.master.manage",
                "mfg.plan.read", "mfg.plan.manage",
                "mfg.wo.read", "mfg.wo.manage",
                "fsm.master.read", "fsm.master.manage",
                "fsm.asset.read", "fsm.asset.manage",
                "fsm.ticket.read", "fsm.ticket.manage",
                "pjm.master.read", "pjm.master.manage",
                "pjm.project.read", "pjm.project.manage",
                "fin.master.read", "fin.master.manage",
                "fin.journal.read", "fin.journal.manage",
                "fin.cash.read", "fin.cash.manage",
                "fin.bank.read", "fin.bank.manage",
                "fin.ap.read", "fin.ap.manage",
                "fin.ar.read", "fin.ar.manage",
                "fin.tax.read", "fin.tax.manage",
                "fin.revenue.read", "fin.revenue.manage",
                "ast.master.read", "ast.master.manage",
                "ast.asset.read", "ast.asset.manage",
                "bi.catalog.read", "bi.catalog.manage",
                "bi.report.read", "bi.report.run",
                "prt.account.read", "prt.account.manage",
                "prt.portal.read", "prt.portal.manage",
                "wf.task.read", "wf.task.act"
            });
        await EnsureRole(RoleStaff, "STAFF", "Nhân viên", false,
            new[] {
                "sys.msg.read", "sys.msg.send", "hrm.employee.read",
                "hrm.leave.read", "hrm.leave.manage",
                "hrm.recruit.read", "hrm.recruit.manage",
                "hrm.payroll.read",
                "lms.class.read",
                "lms.learn.read", "lms.learn.enroll",
                "crm.customer.read",
                "crm.lead.read", "crm.lead.manage",
                "crm.opportunity.read", "crm.opportunity.manage",
                "crm.quote.read", "crm.quote.manage",
                "crm.order.read", "crm.order.manage",
                "crm.campaign.read", "crm.campaign.manage",
                "crm.promotion.read", "crm.promotion.manage",
                "crm.chat.read", "crm.chat.manage",
                "pos.store.read", "pos.catalog.read",
                "pos.shift.read", "pos.shift.manage",
                "pos.sale.read", "pos.sale.manage",
                "pos.promo.read",
                "pur.vendor.read", "pur.pr.read", "pur.pr.manage",
                "pur.po.read",
                "pur.grn.read", "pur.grn.manage",
                "pur.invoice.read", "pur.invoice.manage",
                "inv.item.read", "inv.warehouse.read",
                "inv.stock.read", "inv.stock.manage",
                "inv.stocktake.read", "inv.stocktake.manage",
                "log.carrier.read", "log.delivery.read", "log.delivery.manage",
                "log.cod.read", "log.cod.manage",
                "log.return.read", "log.return.manage",
                "mfg.master.read", "mfg.plan.read", "mfg.wo.read", "mfg.wo.manage",
                "fsm.master.read", "fsm.asset.read", "fsm.ticket.read", "fsm.ticket.manage",
                "pjm.master.read", "pjm.project.read", "pjm.project.manage",
                "fin.master.read", "fin.journal.read", "fin.journal.manage",
                "fin.cash.read", "fin.cash.manage",
                "fin.bank.read", "fin.bank.manage",
                "fin.ap.read", "fin.ap.manage",
                "fin.ar.read", "fin.ar.manage",
                "fin.tax.read", "fin.tax.manage",
                "fin.revenue.read", "fin.revenue.manage",
                "ast.master.read", "ast.asset.read", "ast.asset.manage",
                "bi.catalog.read", "bi.report.read", "bi.report.run",
                "prt.account.read", "prt.portal.read", "prt.portal.manage",
                "wf.task.read", "wf.task.act"
            });

        await EnsureRole(RoleLmsInstructor, "LMS_INSTRUCTOR", "Giảng viên LMS", false,
            new[] {
                "lms.instructor.read",
                "lms.class.read", "lms.class.manage",
                "lms.course.read",
                "lms.learn.read",
                "lms.exam.read",
                "lms.report.read",
                "hrm.employee.read"
            });

        await EnsureExpandedRolesAsync(db, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedLeaveBalancesAsync(AppDbContext db, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var leaveTypes = await db.LeaveTypes.AsNoTracking()
            .Where(x => x.TenantId == TenantId && !x.IsDeleted && x.IsActive)
            .ToListAsync(ct);
        if (leaveTypes.Count == 0) return;

        var employees = await db.Employees.AsNoTracking()
            .Where(x => x.TenantId == TenantId && !x.IsDeleted)
            .Select(x => x.Id)
            .ToListAsync(ct);

        var existing = await db.LeaveBalances.AsNoTracking()
            .Where(x => x.TenantId == TenantId && x.Year == year && !x.IsDeleted)
            .Select(x => new { x.EmployeeId, x.LeaveTypeId })
            .ToListAsync(ct);
        var have = existing.Select(x => (x.EmployeeId, x.LeaveTypeId)).ToHashSet();

        foreach (var empId in employees)
        {
            foreach (var lt in leaveTypes.Where(t => t.DefaultDaysPerYear > 0))
            {
                if (have.Contains((empId, lt.Id))) continue;
                db.LeaveBalances.Add(new LeaveBalance
                {
                    TenantId = TenantId,
                    EmployeeId = empId,
                    LeaveTypeId = lt.Id,
                    Year = year,
                    Entitled = lt.DefaultDaysPerYear,
                    Used = 0,
                    Remaining = lt.DefaultDaysPerYear
                });
            }
        }
    }

    private static async Task SeedMenusAndWfAsync(AppDbContext db, CancellationToken ct)
    {
        if (!await db.MenuItems.AnyAsync(x => x.Code == "HRM_EMP", ct))
        {
            db.MenuItems.AddRange(
                new MenuItem
                {
                    TenantId = TenantId, Code = "HRM_EMP", ModuleCode = "HRM", Title = "Hồ sơ nhân sự",
                    RoutePath = "/app/hrm/employees", PermissionCode = "hrm.employee.read", Icon = "users", SortOrder = 30
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "HRM_LEAVE", ModuleCode = "HRM", Title = "Nghỉ phép",
                    RoutePath = "/app/hrm/leaves", PermissionCode = "hrm.leave.read", Icon = "calendar-days", SortOrder = 31
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "HRM_CONTRACT", ModuleCode = "HRM", Title = "Hợp đồng LĐ",
                    RoutePath = "/app/hrm/contracts", PermissionCode = "hrm.contract.read", Icon = "file-signature", SortOrder = 32
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "HRM_RECRUIT", ModuleCode = "HRM", Title = "Nhu cầu tuyển",
                    RoutePath = "/app/hrm/recruit", PermissionCode = "hrm.recruit.read", Icon = "briefcase", SortOrder = 33
                }
            );
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "HRM_RECRUIT", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "HRM_RECRUIT", ModuleCode = "HRM", Title = "Nhu cầu tuyển",
                RoutePath = "/app/hrm/recruit", PermissionCode = "hrm.recruit.read", Icon = "briefcase", SortOrder = 33
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "HRM_CANDIDATES", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "HRM_CANDIDATES", ModuleCode = "HRM", Title = "Tin & ứng viên",
                RoutePath = "/app/hrm/candidates", PermissionCode = "hrm.recruit.read", Icon = "contact", SortOrder = 34
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "HRM_ONBOARD", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "HRM_ONBOARD", ModuleCode = "HRM", Title = "Onboarding",
                RoutePath = "/app/hrm/onboarding", PermissionCode = "hrm.employee.read", Icon = "user-check", SortOrder = 35
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "HRM_HEADCOUNT", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "HRM_HEADCOUNT", ModuleCode = "HRM", Title = "Định biên",
                RoutePath = "/app/hrm/headcount", PermissionCode = "hrm.employee.read", Icon = "users-round", SortOrder = 36
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "HRM_SHIFTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "HRM_SHIFTS", ModuleCode = "HRM", Title = "Ca làm việc",
                RoutePath = "/app/hrm/shifts", PermissionCode = "hrm.employee.read", Icon = "clock", SortOrder = 37
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "HRM_TRANSFERS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "HRM_TRANSFERS", ModuleCode = "HRM", Title = "Điều động",
                RoutePath = "/app/hrm/transfers", PermissionCode = "hrm.employee.read", Icon = "arrow-left-right", SortOrder = 38
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "HRM_ATTENDANCE", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "HRM_ATTENDANCE", ModuleCode = "HRM", Title = "Chấm công",
                RoutePath = "/app/hrm/attendance", PermissionCode = "hrm.employee.read", Icon = "fingerprint", SortOrder = 39
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "HRM_PAYROLL", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "HRM_PAYROLL", ModuleCode = "HRM", Title = "Lương kỳ",
                RoutePath = "/app/hrm/payroll", PermissionCode = "hrm.payroll.read", Icon = "banknote", SortOrder = 40
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "HRM_REWARDS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "HRM_REWARDS", ModuleCode = "HRM", Title = "Khen thưởng / Kỷ luật",
                RoutePath = "/app/hrm/rewards", PermissionCode = "hrm.employee.read", Icon = "award", SortOrder = 41
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "HRM_OFFBOARD", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "HRM_OFFBOARD", ModuleCode = "HRM", Title = "Nghỉ việc",
                RoutePath = "/app/hrm/offboarding", PermissionCode = "hrm.employee.read", Icon = "door-open", SortOrder = 42
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "HRM_DASHBOARD", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "HRM_DASHBOARD", ModuleCode = "HRM", Title = "Dashboard HRM",
                RoutePath = "/app/hrm/dashboard", PermissionCode = "hrm.employee.read", Icon = "layout-dashboard", SortOrder = 29
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "LMS_CLASSES", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "LMS_CLASSES", ModuleCode = "LMS", Title = "Lớp đào tạo offline",
                RoutePath = "/app/lms/classes", PermissionCode = "lms.class.read", Icon = "graduation-cap", SortOrder = 40
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "LMS_COURSES", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "LMS_COURSES", ModuleCode = "LMS", Title = "Khóa học (catalog)",
                RoutePath = "/app/lms/courses", PermissionCode = "lms.course.read", Icon = "book-open", SortOrder = 38
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "LMS_CATALOG", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "LMS_CATALOG", ModuleCode = "LMS", Title = "Học online",
                RoutePath = "/app/lms/catalog", PermissionCode = "lms.learn.read", Icon = "play-circle", SortOrder = 39
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "LMS_EXAMS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "LMS_EXAMS", ModuleCode = "LMS", Title = "Đề thi & NHCH",
                RoutePath = "/app/lms/exams", PermissionCode = "lms.exam.read", Icon = "clipboard-check", SortOrder = 37
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "LMS_CERTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "LMS_CERTS", ModuleCode = "LMS", Title = "Chứng chỉ của tôi",
                RoutePath = "/app/lms/certificates", PermissionCode = "lms.learn.read", Icon = "award", SortOrder = 41
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "LMS_INSTRUCTORS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "LMS_INSTRUCTORS", ModuleCode = "LMS", Title = "Giảng viên",
                RoutePath = "/app/lms/instructors", PermissionCode = "lms.instructor.read", Icon = "user-check", SortOrder = 42
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "LMS_REPORTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "LMS_REPORTS", ModuleCode = "LMS", Title = "Báo cáo đào tạo",
                RoutePath = "/app/lms/reports", PermissionCode = "lms.report.read", Icon = "file-text", SortOrder = 43
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "CRM_CUSTOMERS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "CRM_CUSTOMERS", ModuleCode = "CRM", Title = "Khách hàng",
                RoutePath = "/app/crm/customers", PermissionCode = "crm.customer.read", Icon = "contact", SortOrder = 50
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "CRM_LEADS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "CRM_LEADS", ModuleCode = "CRM", Title = "Lead",
                RoutePath = "/app/crm/leads", PermissionCode = "crm.lead.read", Icon = "user-plus", SortOrder = 51
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "CRM_OPPORTUNITIES", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "CRM_OPPORTUNITIES", ModuleCode = "CRM", Title = "Cơ hội",
                RoutePath = "/app/crm/opportunities", PermissionCode = "crm.opportunity.read", Icon = "briefcase", SortOrder = 52
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "CRM_QUOTES", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "CRM_QUOTES", ModuleCode = "CRM", Title = "Báo giá",
                RoutePath = "/app/crm/quotes", PermissionCode = "crm.quote.read", Icon = "file-text", SortOrder = 53
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "CRM_ORDERS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "CRM_ORDERS", ModuleCode = "CRM", Title = "Đơn hàng",
                RoutePath = "/app/crm/orders", PermissionCode = "crm.order.read", Icon = "shopping-cart", SortOrder = 54
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "CRM_CAMPAIGNS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "CRM_CAMPAIGNS", ModuleCode = "CRM", Title = "Campaign",
                RoutePath = "/app/crm/campaigns", PermissionCode = "crm.campaign.read", Icon = "megaphone", SortOrder = 55
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "CRM_PROMOTIONS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "CRM_PROMOTIONS", ModuleCode = "CRM", Title = "Khuyến mại",
                RoutePath = "/app/crm/promotions", PermissionCode = "crm.promotion.read", Icon = "ticket", SortOrder = 56
            });
        }

        // Cập nhật CRM_HOME cũ trỏ ModKit → trang KH thật
        var crmHome = await db.MenuItems.FirstOrDefaultAsync(x => x.Code == "CRM_HOME", ct);
        if (crmHome is not null)
        {
            crmHome.RoutePath = "/app/crm/leads";
            crmHome.PermissionCode = "crm.lead.read";
            crmHome.Icon = "user-plus";
            crmHome.Title = "CRM";
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "POS_STORES", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "POS_STORES", ModuleCode = "POS", Title = "Điểm bán POS",
                RoutePath = "/app/pos/stores", PermissionCode = "pos.store.read", Icon = "store", SortOrder = 60
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "POS_CATALOG", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "POS_CATALOG", ModuleCode = "POS", Title = "Catalog & giá",
                RoutePath = "/app/pos/catalog", PermissionCode = "pos.catalog.read", Icon = "package", SortOrder = 61
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "POS_SHIFTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "POS_SHIFTS", ModuleCode = "POS", Title = "Ca thu ngân",
                RoutePath = "/app/pos/shifts", PermissionCode = "pos.shift.read", Icon = "banknote", SortOrder = 62
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "POS_SELL", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "POS_SELL", ModuleCode = "POS", Title = "Bán hàng",
                RoutePath = "/app/pos/sell", PermissionCode = "pos.sale.read", Icon = "shopping-cart", SortOrder = 63
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "POS_PROMOS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "POS_PROMOS", ModuleCode = "POS", Title = "Khuyến mại",
                RoutePath = "/app/pos/promos", PermissionCode = "pos.promo.read", Icon = "tag", SortOrder = 64
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "POS_REPORTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "POS_REPORTS", ModuleCode = "POS", Title = "Báo cáo POS",
                RoutePath = "/app/pos/reports", PermissionCode = "pos.sale.read", Icon = "file-text", SortOrder = 65
            });
        }

        var posHome = await db.MenuItems.FirstOrDefaultAsync(x => x.Code == "POS_HOME", ct);
        if (posHome is not null)
        {
            posHome.RoutePath = "/app/pos/sell";
            posHome.PermissionCode = "pos.sale.read";
            posHome.Icon = "shopping-cart";
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "PUR_VENDORS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "PUR_VENDORS", ModuleCode = "PUR", Title = "Nhà cung cấp",
                RoutePath = "/app/pur/vendors", PermissionCode = "pur.vendor.read", Icon = "truck", SortOrder = 70
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "PUR_ORDERS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "PUR_ORDERS", ModuleCode = "PUR", Title = "PR / PO",
                RoutePath = "/app/pur/orders", PermissionCode = "pur.pr.read", Icon = "shopping-cart", SortOrder = 71
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "PUR_RECEIPTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "PUR_RECEIPTS", ModuleCode = "PUR", Title = "Nhận hàng",
                RoutePath = "/app/pur/receipts", PermissionCode = "pur.grn.read", Icon = "clipboard-list", SortOrder = 72
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "PUR_INVOICES", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "PUR_INVOICES", ModuleCode = "PUR", Title = "Hóa đơn NCC",
                RoutePath = "/app/pur/invoices", PermissionCode = "pur.invoice.read", Icon = "file-text", SortOrder = 73
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "PUR_REPORTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "PUR_REPORTS", ModuleCode = "PUR", Title = "Báo cáo mua",
                RoutePath = "/app/pur/reports", PermissionCode = "pur.grn.read", Icon = "file-text", SortOrder = 74
            });
        }

        var purHome = await db.MenuItems.FirstOrDefaultAsync(x => x.Code == "PUR_HOME", ct);
        if (purHome is not null)
        {
            purHome.RoutePath = "/app/pur/vendors";
            purHome.PermissionCode = "pur.vendor.read";
            purHome.Icon = "truck";
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "INV_ITEMS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "INV_ITEMS", ModuleCode = "INV", Title = "SKU / danh mục",
                RoutePath = "/app/inv/items", PermissionCode = "inv.item.read", Icon = "boxes", SortOrder = 80
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "INV_WAREHOUSES", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "INV_WAREHOUSES", ModuleCode = "INV", Title = "Kho / thủ kho",
                RoutePath = "/app/inv/warehouses", PermissionCode = "inv.warehouse.read", Icon = "warehouse", SortOrder = 81
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "INV_STOCK", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "INV_STOCK", ModuleCode = "INV", Title = "Tồn & phiếu",
                RoutePath = "/app/inv/stock", PermissionCode = "inv.stock.read", Icon = "boxes", SortOrder = 82
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "INV_TRANSFERS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "INV_TRANSFERS", ModuleCode = "INV", Title = "Chuyển kho",
                RoutePath = "/app/inv/transfers", PermissionCode = "inv.stock.read", Icon = "arrow-left-right", SortOrder = 83
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "INV_STOCKTAKES", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "INV_STOCKTAKES", ModuleCode = "INV", Title = "Kiểm kê",
                RoutePath = "/app/inv/stocktakes", PermissionCode = "inv.stocktake.read", Icon = "clipboard-check", SortOrder = 84
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "INV_REPORTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "INV_REPORTS", ModuleCode = "INV", Title = "Báo cáo kho",
                RoutePath = "/app/inv/reports", PermissionCode = "inv.stock.read", Icon = "file-text", SortOrder = 85
            });
        }

        var invHome = await db.MenuItems.FirstOrDefaultAsync(x => x.Code == "INV_HOME", ct);
        if (invHome is not null)
        {
            invHome.RoutePath = "/app/inv/items";
            invHome.PermissionCode = "inv.item.read";
            invHome.Icon = "boxes";
            invHome.Title = "Kho";
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "LOG_CARRIERS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "LOG_CARRIERS", ModuleCode = "LOG", Title = "Đơn vị vận chuyển",
                RoutePath = "/app/log/carriers", PermissionCode = "log.carrier.read", Icon = "truck", SortOrder = 90
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "LOG_DELIVERIES", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "LOG_DELIVERIES", ModuleCode = "LOG", Title = "Lệnh giao hàng",
                RoutePath = "/app/log/deliveries", PermissionCode = "log.delivery.read", Icon = "map", SortOrder = 91
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "LOG_COD", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "LOG_COD", ModuleCode = "LOG", Title = "COD",
                RoutePath = "/app/log/cod", PermissionCode = "log.cod.read", Icon = "banknote", SortOrder = 92
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "LOG_RETURNS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "LOG_RETURNS", ModuleCode = "LOG", Title = "Hoàn hàng",
                RoutePath = "/app/log/returns", PermissionCode = "log.return.read", Icon = "package", SortOrder = 93
            });
        }

        var logHome = await db.MenuItems.FirstOrDefaultAsync(x => x.Code == "LOG_HOME", ct);
        if (logHome is not null)
        {
            logHome.RoutePath = "/app/log/deliveries";
            logHome.PermissionCode = "log.delivery.read";
            logHome.Icon = "truck";
            logHome.Title = "Logistics";
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "MFG_CATALOG", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "MFG_CATALOG", ModuleCode = "MFG", Title = "Danh mục / BOM",
                RoutePath = "/app/mfg/catalog", PermissionCode = "mfg.master.read", Icon = "factory", SortOrder = 100
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "MFG_ORDERS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "MFG_ORDERS", ModuleCode = "MFG", Title = "KH / lệnh SX",
                RoutePath = "/app/mfg/orders", PermissionCode = "mfg.wo.read", Icon = "clipboard-list", SortOrder = 101
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "MFG_REPORTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "MFG_REPORTS", ModuleCode = "MFG", Title = "Báo cáo SX",
                RoutePath = "/app/mfg/reports", PermissionCode = "mfg.wo.read", Icon = "file-text", SortOrder = 102
            });
        }

        var mfgHome = await db.MenuItems.FirstOrDefaultAsync(x => x.Code == "MFG_HOME", ct);
        if (mfgHome is not null)
        {
            mfgHome.RoutePath = "/app/mfg/catalog";
            mfgHome.PermissionCode = "mfg.master.read";
            mfgHome.Icon = "factory";
            mfgHome.Title = "Sản xuất";
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "FSM_CATALOG", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "FSM_CATALOG", ModuleCode = "FSM", Title = "Danh mục FSM",
                RoutePath = "/app/fsm/catalog", PermissionCode = "fsm.master.read", Icon = "wrench", SortOrder = 110
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "FSM_TICKETS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "FSM_TICKETS", ModuleCode = "FSM", Title = "Ticket / thiết bị",
                RoutePath = "/app/fsm/tickets", PermissionCode = "fsm.ticket.read", Icon = "headset", SortOrder = 111
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "FSM_PARTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "FSM_PARTS", ModuleCode = "FSM", Title = "Kho linh kiện KT",
                RoutePath = "/app/fsm/parts", PermissionCode = "fsm.master.read", Icon = "package", SortOrder = 112
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "FSM_REPORTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "FSM_REPORTS", ModuleCode = "FSM", Title = "Báo cáo FSM",
                RoutePath = "/app/fsm/reports", PermissionCode = "fsm.ticket.read", Icon = "file-text", SortOrder = 113
            });
        }

        var fsmHome = await db.MenuItems.FirstOrDefaultAsync(x => x.Code == "FSM_HOME", ct);
        if (fsmHome is not null)
        {
            fsmHome.RoutePath = "/app/fsm/tickets";
            fsmHome.PermissionCode = "fsm.ticket.read";
            fsmHome.Icon = "headset";
            fsmHome.Title = "Dịch vụ hiện trường";
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "PJM_CATALOG", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "PJM_CATALOG", ModuleCode = "PJM", Title = "Danh mục dự án",
                RoutePath = "/app/pjm/catalog", PermissionCode = "pjm.master.read", Icon = "folder-kanban", SortOrder = 120
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "PJM_PROJECTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "PJM_PROJECTS", ModuleCode = "PJM", Title = "Dự án",
                RoutePath = "/app/pjm/projects", PermissionCode = "pjm.project.read", Icon = "briefcase", SortOrder = 121
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "PJM_REPORTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "PJM_REPORTS", ModuleCode = "PJM", Title = "Báo cáo dự án",
                RoutePath = "/app/pjm/reports", PermissionCode = "pjm.project.read", Icon = "file-text", SortOrder = 122
            });
        }

        var pjmHome = await db.MenuItems.FirstOrDefaultAsync(x => x.Code == "PJM_HOME", ct);
        if (pjmHome is not null)
        {
            pjmHome.RoutePath = "/app/pjm/projects";
            pjmHome.PermissionCode = "pjm.project.read";
            pjmHome.Icon = "briefcase";
            pjmHome.Title = "Dự án";
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "FIN_CATALOG", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "FIN_CATALOG", ModuleCode = "FIN", Title = "Danh mục kế toán",
                RoutePath = "/app/fin/catalog", PermissionCode = "fin.master.read", Icon = "book-open", SortOrder = 130
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "FIN_JOURNALS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "FIN_JOURNALS", ModuleCode = "FIN", Title = "Bút toán / sổ",
                RoutePath = "/app/fin/journals", PermissionCode = "fin.journal.read", Icon = "banknote", SortOrder = 131
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "FIN_CASH", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "FIN_CASH", ModuleCode = "FIN", Title = "Quỹ tiền mặt",
                RoutePath = "/app/fin/cash", PermissionCode = "fin.cash.read", Icon = "wallet", SortOrder = 132
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "FIN_BANK", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "FIN_BANK", ModuleCode = "FIN", Title = "Ngân hàng",
                RoutePath = "/app/fin/bank", PermissionCode = "fin.bank.read", Icon = "landmark", SortOrder = 133
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "FIN_AP", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "FIN_AP", ModuleCode = "FIN", Title = "Công nợ phải trả",
                RoutePath = "/app/fin/ap", PermissionCode = "fin.ap.read", Icon = "receipt", SortOrder = 134
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "FIN_AR", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "FIN_AR", ModuleCode = "FIN", Title = "Công nợ phải thu",
                RoutePath = "/app/fin/ar", PermissionCode = "fin.ar.read", Icon = "hand-coins", SortOrder = 135
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "FIN_TAX", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "FIN_TAX", ModuleCode = "FIN", Title = "Thuế GTGT",
                RoutePath = "/app/fin/tax", PermissionCode = "fin.tax.read", Icon = "percent", SortOrder = 136
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "FIN_REVENUE", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "FIN_REVENUE", ModuleCode = "FIN", Title = "Doanh thu & giá vốn",
                RoutePath = "/app/fin/revenue", PermissionCode = "fin.revenue.read", Icon = "trending-up", SortOrder = 137
            });
        }

        var finHome = await db.MenuItems.FirstOrDefaultAsync(x => x.Code == "FIN_HOME", ct);
        if (finHome is not null)
        {
            finHome.RoutePath = "/app/fin/journals";
            finHome.PermissionCode = "fin.journal.read";
            finHome.Icon = "banknote";
            finHome.Title = "Tài chính";
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "AST_CATALOG", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "AST_CATALOG", ModuleCode = "AST", Title = "Danh mục TSCĐ",
                RoutePath = "/app/ast/catalog", PermissionCode = "ast.master.read", Icon = "layers", SortOrder = 140
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "AST_ASSETS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "AST_ASSETS", ModuleCode = "AST", Title = "Thẻ TS / khấu hao",
                RoutePath = "/app/ast/assets", PermissionCode = "ast.asset.read", Icon = "building-2", SortOrder = 141
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "AST_MOVEMENTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "AST_MOVEMENTS", ModuleCode = "AST", Title = "Điều chuyển / thanh lý",
                RoutePath = "/app/ast/movements", PermissionCode = "ast.asset.read", Icon = "arrow-left-right", SortOrder = 142
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "AST_STOCKTAKES", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "AST_STOCKTAKES", ModuleCode = "AST", Title = "Kiểm kê TSCĐ",
                RoutePath = "/app/ast/stocktakes", PermissionCode = "ast.asset.read", Icon = "clipboard-check", SortOrder = 143
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "AST_REPORTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "AST_REPORTS", ModuleCode = "AST", Title = "Báo cáo TSCĐ",
                RoutePath = "/app/ast/reports", PermissionCode = "ast.asset.read", Icon = "file-text", SortOrder = 144
            });
        }

        var astHome = await db.MenuItems.FirstOrDefaultAsync(x => x.Code == "AST_HOME", ct);
        if (astHome is not null)
        {
            astHome.RoutePath = "/app/ast/assets";
            astHome.PermissionCode = "ast.asset.read";
            astHome.Icon = "building-2";
            astHome.Title = "Tài sản";
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "BI_CATALOG", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "BI_CATALOG", ModuleCode = "BI", Title = "Dataset / Dashboard",
                RoutePath = "/app/bi/catalog", PermissionCode = "bi.catalog.read", Icon = "layout-dashboard", SortOrder = 150
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "BI_REPORTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "BI_REPORTS", ModuleCode = "BI", Title = "Thư viện báo cáo",
                RoutePath = "/app/bi/reports", PermissionCode = "bi.report.read", Icon = "file-text", SortOrder = 151
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "BI_KPI", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "BI_KPI", ModuleCode = "BI", Title = "KPI & cảnh báo",
                RoutePath = "/app/bi/kpi", PermissionCode = "bi.catalog.read", Icon = "chart", SortOrder = 152
            });
        }

        var biHome = await db.MenuItems.FirstOrDefaultAsync(x => x.Code == "BI_HOME", ct);
        if (biHome is not null)
        {
            biHome.RoutePath = "/app/bi/reports";
            biHome.PermissionCode = "bi.report.read";
            biHome.Icon = "file-text";
            biHome.Title = "BI / Báo cáo";
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "PRT_ACCOUNTS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "PRT_ACCOUNTS", ModuleCode = "PRT", Title = "Tài khoản portal",
                RoutePath = "/app/prt/accounts", PermissionCode = "prt.account.read", Icon = "user-check", SortOrder = 160
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "PRT_PORTAL", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "PRT_PORTAL", ModuleCode = "PRT", Title = "Đơn / công nợ / ticket",
                RoutePath = "/app/prt/portal", PermissionCode = "prt.portal.read", Icon = "store", SortOrder = 161
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "PRT_PACKAGE", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "PRT_PACKAGE", ModuleCode = "PRT", Title = "Gói module portal",
                RoutePath = "/app/prt/package", PermissionCode = "prt.portal.read", Icon = "layers", SortOrder = 162
            });
        }

        var prtHome = await db.MenuItems.FirstOrDefaultAsync(x => x.Code == "PRT_HOME", ct);
        if (prtHome is not null)
        {
            prtHome.RoutePath = "/app/prt/portal";
            prtHome.PermissionCode = "prt.portal.read";
            prtHome.Icon = "store";
            prtHome.Title = "Portal KH";
        }

        // Bổ sung menu SYS nếu DB cũ thiếu
        if (!await db.MenuItems.AnyAsync(x => x.Code == "SYS_USERS", ct))
        {
            db.MenuItems.AddRange(
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_USERS", ModuleCode = "SYS", Title = "Người dùng",
                    RoutePath = "/app/sys/users", PermissionCode = "sys.user.read", Icon = "users", SortOrder = 11
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_ROLES", ModuleCode = "SYS", Title = "Vai trò",
                    RoutePath = "/app/sys/roles", PermissionCode = "sys.role.read", Icon = "shield", SortOrder = 12
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_ORG", ModuleCode = "SYS", Title = "Tổ chức",
                    RoutePath = "/app/sys/org", PermissionCode = "sys.user.read", Icon = "building", SortOrder = 14
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "WF_TASKS", ModuleCode = "WF", Title = "Phê duyệt của tôi",
                    RoutePath = "/app/wf/tasks", PermissionCode = "wf.task.read", Icon = "inbox", SortOrder = 20
                }
            );
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "SYS_MSG", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "SYS_MSG", ModuleCode = "SYS", Title = "Tin nhắn",
                RoutePath = "/app/sys/messages", PermissionCode = "sys.msg.read", Icon = "message", SortOrder = 14
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "SYS_TENANT", ct))
        {
            db.MenuItems.AddRange(
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_TENANT", ModuleCode = "SYS", Title = "Công ty / Tenant",
                    RoutePath = "/app/sys/tenant", PermissionCode = "sys.license.manage", Icon = "building", SortOrder = 15
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_LOOKUPS", ModuleCode = "SYS", Title = "Danh mục dùng chung",
                    RoutePath = "/app/sys/lookups", PermissionCode = "sys.license.manage", Icon = "layers", SortOrder = 16
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_AUDIT_LOGIN", ModuleCode = "SYS", Title = "Nhật ký đăng nhập",
                    RoutePath = "/app/sys/login-audits", PermissionCode = "sys.license.manage", Icon = "shield", SortOrder = 17
                }
            );
        }

        // Bước 153 — SSO / Field ACL / Config versions / Push
        if (!await db.MenuItems.AnyAsync(x => x.Code == "SYS_SSO", ct))
        {
            db.MenuItems.AddRange(
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_SSO", ModuleCode = "SYS", Title = "SSO / OAuth",
                    RoutePath = "/app/sys/sso", PermissionCode = "sys.sso.read", Icon = "key", SortOrder = 18
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_FIELD_PERM", ModuleCode = "SYS", Title = "Quyền trường nhạy cảm",
                    RoutePath = "/app/sys/field-permissions", PermissionCode = "sys.fieldperm.read", Icon = "shield", SortOrder = 19
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_CFG_VER", ModuleCode = "SYS", Title = "Phiên bản cấu hình",
                    RoutePath = "/app/sys/config-versions", PermissionCode = "sys.config.version.read", Icon = "layers", SortOrder = 20
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_PUSH", ModuleCode = "SYS", Title = "Push devices",
                    RoutePath = "/app/sys/push-devices", PermissionCode = "sys.push.device.self", Icon = "message", SortOrder = 21
                }
            );
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "SYS_NOTIF_PREF", ct))
        {
            db.MenuItems.AddRange(
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_NOTIF_PREF", ModuleCode = "SYS", Title = "Tùy chọn thông báo",
                    RoutePath = "/app/sys/notification-preferences", PermissionCode = "sys.user.read", Icon = "bell", SortOrder = 22
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_FILE_SEC", ModuleCode = "SYS", Title = "Bảo mật file",
                    RoutePath = "/app/sys/file-security", PermissionCode = "sys.file.scan", Icon = "shield", SortOrder = 23
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_EXPORT_JOBS", ModuleCode = "SYS", Title = "Xuất hàng loạt",
                    RoutePath = "/app/sys/export-jobs", PermissionCode = "sys.export.job.read", Icon = "download", SortOrder = 24
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_IP_RULES", ModuleCode = "SYS", Title = "IP allow/deny",
                    RoutePath = "/app/sys/ip-rules", PermissionCode = "sys.ip.read", Icon = "shield", SortOrder = 25
                }
            );
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "SYS_BRANDING", ct))
        {
            db.MenuItems.AddRange(
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_BRANDING", ModuleCode = "SYS", Title = "Theme / Branding",
                    RoutePath = "/app/sys/branding", PermissionCode = "sys.brand.read", Icon = "layers", SortOrder = 26
                },
                new MenuItem
                {
                    TenantId = TenantId, Code = "SYS_ROLE_HOME", ModuleCode = "SYS", Title = "Trang chủ theo vai trò",
                    RoutePath = "/app/sys/role-homes", PermissionCode = "sys.ui.home.manage", Icon = "home", SortOrder = 27
                }
            );
        }

        if (!await db.SysSensitiveFields.AnyAsync(x => x.TenantId == TenantId && !x.IsDeleted, ct))
        {
            db.SysSensitiveFields.AddRange(
                new SysSensitiveField
                {
                    TenantId = TenantId, ModuleCode = "HRM", EntityName = "Employee", FieldKey = "salary",
                    DisplayName = "Lương cơ bản", DefaultMask = "Mask", IsActive = true
                },
                new SysSensitiveField
                {
                    TenantId = TenantId, ModuleCode = "HRM", EntityName = "Employee", FieldKey = "bankAccount",
                    DisplayName = "Số tài khoản ngân hàng", DefaultMask = "Mask", IsActive = true
                },
                new SysSensitiveField
                {
                    TenantId = TenantId, ModuleCode = "SYS", EntityName = "AppUser", FieldKey = "phone",
                    DisplayName = "Số điện thoại", DefaultMask = "Mask", IsActive = true
                }
            );
        }

        if (!await db.SysSsoProviders.AnyAsync(x => x.TenantId == TenantId && !x.IsDeleted, ct))
        {
            db.SysSsoProviders.Add(new SysSsoProvider
            {
                TenantId = TenantId,
                Code = "GOOGLE_DEV",
                DisplayName = "Google (Day-1 stub)",
                ClientId = "erp-dev-client",
                AuthorityUrl = "https://accounts.google.com/o/oauth2/v2",
                RedirectUri = "http://localhost:3000/login?sso=callback",
                Scopes = "openid profile email",
                JitProvisioning = true,
                IsActive = true,
                Note = "Dùng callback code=dev:email|subject để giả lập token exchange."
            });
        }

        // Password policy + notification rules mặc định
        if (!await db.SystemSettings.AnyAsync(x => x.TenantId == TenantId && x.Key == "password.policy", ct))
        {
            db.SystemSettings.Add(new SystemSetting
            {
                TenantId = TenantId, Key = "password.policy",
                ValueJson = """{"MinLength":8,"RequireDigit":true,"RequireUpper":true,"RequireLower":true,"MaxFailedLogins":5,"LockMinutes":15,"SessionMinutes":120}"""
            });
        }

        if (!await db.NotificationRules.AnyAsync(x => x.TenantId == TenantId && x.EventType == "wf.task.assigned", ct))
        {
            db.NotificationRules.Add(new NotificationRule
            {
                TenantId = TenantId, EventType = "wf.task.assigned", IsEnabled = true,
                TitleTemplate = "Có phiếu cần duyệt", BodyTemplate = "Bạn có task WF mới: {title}"
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "WF_WORK", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "WF_WORK", ModuleCode = "WF", Title = "Công việc / ticket",
                RoutePath = "/app/wf/work", PermissionCode = "wf.task.read", Icon = "inbox", SortOrder = 21
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "WF_DELEGATION", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "WF_DELEGATION", ModuleCode = "WF", Title = "Ủy quyền duyệt",
                RoutePath = "/app/wf/delegation", PermissionCode = "wf.task.read", Icon = "users", SortOrder = 22
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "WF_DASHBOARD", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "WF_DASHBOARD", ModuleCode = "WF", Title = "Dashboard WF",
                RoutePath = "/app/wf/dashboard", PermissionCode = "wf.task.read", Icon = "layers", SortOrder = 23
            });
        }

        if (!await db.MenuItems.AnyAsync(x => x.Code == "SYS_PERMISSIONS", ct))
        {
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = "SYS_PERMISSIONS", ModuleCode = "SYS", Title = "Danh mục quyền",
                RoutePath = "/app/sys/permissions", PermissionCode = "sys.permission.read", Icon = "key", SortOrder = 13
            });
        }

        var rolesMenu = await db.MenuItems.FirstOrDefaultAsync(x => x.Code == "SYS_ROLES", ct);
        if (rolesMenu is not null && rolesMenu.PermissionCode == "sys.role.manage")
            rolesMenu.PermissionCode = "sys.role.read";

        // Đồng bộ icon menu (DB cũ hay dùng users/calendar/file trùng nhau)
        var menuIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["HRM_DASHBOARD"] = "layout-dashboard",
            ["HRM_EMP"] = "users",
            ["HRM_LEAVE"] = "calendar-days",
            ["HRM_CONTRACT"] = "file-signature",
            ["HRM_RECRUIT"] = "briefcase",
            ["HRM_CANDIDATES"] = "contact",
            ["HRM_ONBOARD"] = "user-check",
            ["HRM_HEADCOUNT"] = "users-round",
            ["HRM_SHIFTS"] = "clock",
            ["HRM_TRANSFERS"] = "arrow-left-right",
            ["HRM_ATTENDANCE"] = "fingerprint",
            ["HRM_PAYROLL"] = "banknote",
            ["HRM_REWARDS"] = "award",
            ["HRM_OFFBOARD"] = "door-open",
            ["LMS_CLASSES"] = "graduation-cap",
            ["SYS_PERMISSIONS"] = "key",
            ["WF_WORK"] = "clipboard",
            ["WF_DELEGATION"] = "user-plus",
            ["WF_DASHBOARD"] = "layout-dashboard",
        };
        var menuCodes = menuIcons.Keys.ToList();
        var menusToFix = await db.MenuItems.Where(x => menuCodes.Contains(x.Code)).ToListAsync(ct);
        foreach (var item in menusToFix)
        {
            if (menuIcons.TryGetValue(item.Code, out var icon) &&
                !string.Equals(item.Icon, icon, StringComparison.OrdinalIgnoreCase))
                item.Icon = icon;
        }

        // Menu Day-1 cấp 1 cho các module còn lại
        var modMenus = new (string Code, string Mod, string Title, string Path, int Sort)[]
        {
            ("LMS_HOME", "LMS", "Đào tạo", "/app/lms", 40),
            ("CRM_HOME", "CRM", "CRM", "/app/crm/leads", 41),
            ("POS_HOME", "POS", "POS / điểm bán", "/app/pos/stores", 42),
            ("PUR_HOME", "PUR", "Mua hàng", "/app/pur/vendors", 43),
            ("INV_HOME", "INV", "Kho", "/app/inv/items", 44),
            ("LOG_HOME", "LOG", "Logistics", "/app/log/deliveries", 45),
            ("MFG_HOME", "MFG", "Sản xuất", "/app/mfg/catalog", 46),
            ("FSM_HOME", "FSM", "Dịch vụ hiện trường", "/app/fsm/tickets", 47),
            ("PJM_HOME", "PJM", "Dự án", "/app/pjm/projects", 48),
            ("FIN_HOME", "FIN", "Tài chính", "/app/fin/journals", 49),
            ("AST_HOME", "AST", "Tài sản", "/app/ast/assets", 50),
            ("BI_HOME", "BI", "BI / Báo cáo", "/app/bi/reports", 51),
            ("PRT_HOME", "PRT", "Portal", "/app/prt/portal", 52),
        };
        foreach (var m in modMenus)
        {
            if (await db.MenuItems.AnyAsync(x => x.Code == m.Code, ct)) continue;
            db.MenuItems.Add(new MenuItem
            {
                TenantId = TenantId, Code = m.Code, ModuleCode = m.Mod, Title = m.Title,
                RoutePath = m.Path, PermissionCode = "sys.user.read", Icon = "layers", SortOrder = m.Sort
            });
        }

        if (!await db.NumberSequences.AnyAsync(x => x.TenantId == TenantId && x.DocType == "HRM.EMP", ct))
        {
            db.NumberSequences.Add(new NumberSequence
            {
                TenantId = TenantId, DocType = "HRM.EMP", Pattern = "NV-{yyyy}-{seq:5}", NextValue = 1,
                ResetYear = DateTime.UtcNow.Year
            });
        }

        if (!await db.WfDefinitions.AnyAsync(x => x.Code == "LEAVE_APPROVE", ct))
        {
            var defId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0001");
            var verId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002");
            var nodeId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0003");
            db.WfDefinitions.Add(new WfDefinition
            {
                Id = defId, TenantId = TenantId, Code = "LEAVE_APPROVE", Name = "Phê duyệt nghỉ phép",
                ModuleCode = "HRM", DocType = "leave_request", IsActive = true
            });
            db.WfDefinitionVersions.Add(new WfDefinitionVersion
            {
                Id = verId, TenantId = TenantId, DefinitionId = defId, VersionNo = 1, IsPublished = true
            });
            db.WfNodes.Add(new WfNode
            {
                Id = nodeId, TenantId = TenantId, DefinitionVersionId = verId,
                Code = "MGR", Name = "Quản lý duyệt", NodeType = "Approve", SortOrder = 1
            });
        }

        if (!await db.WfDefinitions.AnyAsync(x => x.Code == "RECRUIT_APPROVE", ct))
        {
            var defId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0011");
            var verId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0012");
            var nodeId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0013");
            db.WfDefinitions.Add(new WfDefinition
            {
                Id = defId, TenantId = TenantId, Code = "RECRUIT_APPROVE", Name = "Phê duyệt nhu cầu tuyển",
                ModuleCode = "HRM", DocType = "recruitment_request", IsActive = true
            });
            db.WfDefinitionVersions.Add(new WfDefinitionVersion
            {
                Id = verId, TenantId = TenantId, DefinitionId = defId, VersionNo = 1, IsPublished = true
            });
            db.WfNodes.Add(new WfNode
            {
                Id = nodeId, TenantId = TenantId, DefinitionVersionId = verId,
                Code = "MGR", Name = "Quản lý duyệt", NodeType = "Approve", SortOrder = 1
            });
        }

        if (!await db.NumberSequences.AnyAsync(x => x.TenantId == TenantId && x.DocType == "HRM.RECRUIT", ct))
        {
            db.NumberSequences.Add(new NumberSequence
            {
                TenantId = TenantId, DocType = "HRM.RECRUIT", Pattern = "TD-{yyyy}-{seq:4}", NextValue = 1,
                ResetYear = DateTime.UtcNow.Year
            });
        }

        if (!await db.NumberSequences.AnyAsync(x => x.TenantId == TenantId && x.DocType == "HRM.TRANSFER", ct))
        {
            db.NumberSequences.Add(new NumberSequence
            {
                TenantId = TenantId, DocType = "HRM.TRANSFER", Pattern = "DD-{yyyy}-{seq:4}", NextValue = 1,
                ResetYear = DateTime.UtcNow.Year
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
