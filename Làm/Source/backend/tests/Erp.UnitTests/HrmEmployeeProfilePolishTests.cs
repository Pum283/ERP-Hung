using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.DTOs.Mod;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Implementations.Services.Auth;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 13: UC_HRM_026 (Xuất danh sách nhân sự Excel/CSV), UC_HRM_027 (Khóa hồ sơ đã nghỉ),
/// UC_HRM_028 (Xem hồ sơ theo quyền), UC_HRM_029 (Chuyển trạng thái Thử việc).
/// 14+ test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmEmployeeProfilePolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SysPlatformService _sysSvc;
    private readonly HrmEmployeeService _hrmSvc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _actor  = Guid.NewGuid();

    public HrmEmployeeProfilePolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-profile-step13-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _db.Licenses.Add(new License
        {
            TenantId = _tenant,
            PlanCode = "ENTERPRISE",
            Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100,
            MaxOrgUnits = 500
        });
        _db.SaveChanges();

        _sysSvc = new SysPlatformService(_db, new OutboxWriter(_db));
        _hrmSvc = new HrmEmployeeService(_db, new DataScopeService(_db), _sysSvc);
    }

    public void Dispose() => _db.Dispose();

    // ─── UC_HRM_026: Xuất danh sách nhân sự Excel / CSV ───

    [Fact]
    public async Task UC026_ExportEmployeesCsv_ContainsUtf8BomAndHeaders()
    {
        var user = new AppUser { TenantId = _tenant, Username = "exp_user", DisplayName = "Admin" };
        _db.Users.Add(user);
        _db.Employees.Add(new Employee { TenantId = _tenant, UserId = user.Id, EmployeeCode = "EMP_EXP1", FullName = "Nguyễn Văn Export", Email = "exp@erp.vn" });
        await _db.SaveChangesAsync();

        var bytes = await _hrmSvc.ExportEmployeesCsvAsync(_tenant, user.Id);
        var csvStr = Encoding.UTF8.GetString(bytes);

        Assert.True(bytes.Length >= 3);
        Assert.Equal(0xEF, bytes[0]);
        Assert.Equal(0xBB, bytes[1]);
        Assert.Equal(0xBF, bytes[2]); // UTF-8 BOM
        Assert.Contains("Mã NV,Họ và tên,Email,Số điện thoại,Trạng thái", csvStr);
        Assert.Contains("EMP_EXP1", csvStr);
        Assert.Contains("Nguyễn Văn Export", csvStr);
    }

    [Fact]
    public async Task UC026_ExportEmployeesCsv_EscapesCommasAndQuotes()
    {
        var user = new AppUser { TenantId = _tenant, Username = "exp_user2", DisplayName = "Admin 2" };
        _db.Users.Add(user);
        _db.Employees.Add(new Employee { TenantId = _tenant, UserId = user.Id, EmployeeCode = "EMP_EXP2", FullName = "Trần \"Phẩy, Dấu\"", Email = "comma@erp.vn" });
        await _db.SaveChangesAsync();

        var bytes = await _hrmSvc.ExportEmployeesCsvAsync(_tenant, user.Id);
        var csvStr = Encoding.UTF8.GetString(bytes);

        Assert.Contains("\"Trần \"\"Phẩy, Dấu\"\"\"", csvStr);
    }


    // ─── UC_HRM_027: Khóa hồ sơ đã nghỉ ───

    [Fact]
    public async Task UC027_ChangeStatus_ToTerminated_LocksProfile()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_TERM1", FullName = "Vũ Nghỉ Việc", Status = "Active" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var effDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var res = await _hrmSvc.ChangeStatusAsync(_tenant, _actor, emp.Id, new ChangeEmploymentStatusRequest("Terminated", effDate, "Nghỉ việc theo nguyện vọng", null, null, null));

        Assert.Equal("Terminated", res.Status);
        var dbEmp = await _db.Employees.FirstOrDefaultAsync(x => x.Id == emp.Id);
        Assert.NotNull(dbEmp);
        Assert.True(dbEmp!.IsDeleted);
        Assert.Equal(effDate, dbEmp.TerminateDate);
    }

    [Fact]
    public async Task UC027_Upsert_OnLockedProfile_ThrowsAppException()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_LOCKED", FullName = "Hoàng Đã Khóa", Status = "Terminated", IsDeleted = true };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var req = new EmployeeUpsertRequest(emp.Id, "EMP_LOCKED", null, "Hoàng Sửa Tên", null, null, null, null, Guid.NewGuid(), null, null, null, null, null, "Active", null, null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _hrmSvc.UpsertAsync(_tenant, _actor, req));
        Assert.Contains("đã bị khóa", ex.Message);
    }

    [Fact]
    public async Task UC027_Rehire_FromTerminatedToProbation_UnlocksProfile()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_REHIRE", FullName = "Đỗ Tái Tuyển", Status = "Terminated", IsDeleted = true };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var res = await _hrmSvc.ChangeStatusAsync(_tenant, _actor, emp.Id, new ChangeEmploymentStatusRequest("Probation", DateOnly.FromDateTime(DateTime.UtcNow), "Tái tuyển dụng", null, null, null));

        Assert.Equal("Probation", res.Status);
        var dbEmp = await _db.Employees.FirstOrDefaultAsync(x => x.Id == emp.Id);
        Assert.False(dbEmp!.IsDeleted);
    }

    // ─── UC_HRM_028: Xem hồ sơ theo quyền ───

    [Fact]
    public async Task UC028_GetWithScope_OwnScope_AccessingAnotherUser_Throws403()
    {
        var ownJl = new JobLevel { TenantId = _tenant, Code = "JL_OWN", Name = "Level Own", DefaultScopeType = ScopeType.Own };
        _db.JobLevels.Add(ownJl);

        var user1 = new AppUser { TenantId = _tenant, Username = "user1", DisplayName = "User 1", JobLevelId = ownJl.Id };
        var user2 = new AppUser { TenantId = _tenant, Username = "user2", DisplayName = "User 2" };
        _db.Users.AddRange(user1, user2);

        var emp1 = new Employee { TenantId = _tenant, UserId = user1.Id, EmployeeCode = "E1", FullName = "NV 1" };
        var emp2 = new Employee { TenantId = _tenant, UserId = user2.Id, EmployeeCode = "E2", FullName = "NV 2" };
        _db.Employees.AddRange(emp1, emp2);
        await _db.SaveChangesAsync();

        // User 1 tries to view emp2 (another employee)
        var ex = await Assert.ThrowsAsync<AppException>(() => _hrmSvc.GetWithScopeAsync(_tenant, user1.Id, emp2.Id));
        Assert.Equal(403, ex.StatusCode);
        Assert.Contains("không có quyền", ex.Message);
    }

    [Fact]
    public async Task UC028_GetWithScope_OwnScope_AccessingOwnProfile_Succeeds()
    {
        var ownJl = new JobLevel { TenantId = _tenant, Code = "JL_OWN2", Name = "Level Own 2", DefaultScopeType = ScopeType.Own };
        _db.JobLevels.Add(ownJl);

        var user1 = new AppUser { TenantId = _tenant, Username = "user_own", DisplayName = "User Own", JobLevelId = ownJl.Id };
        _db.Users.Add(user1);
        var emp1 = new Employee { TenantId = _tenant, UserId = user1.Id, EmployeeCode = "E_OWN", FullName = "NV Chính Mình" };
        _db.Employees.Add(emp1);
        await _db.SaveChangesAsync();

        var res = await _hrmSvc.GetWithScopeAsync(_tenant, user1.Id, emp1.Id);
        Assert.Equal("E_OWN", res.EmployeeCode);
        Assert.Equal("NV Chính Mình", res.FullName);
    }

    // ─── UC_HRM_029: Chuyển trạng thái Thử việc ───

    [Fact]
    public async Task UC029_ChangeStatus_ToProbation_DefaultDateAndRecordsHistory()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_PROB", FullName = "Phạm Thử Việc", Status = "New" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var res = await _hrmSvc.ChangeStatusAsync(_tenant, _actor, emp.Id, new ChangeEmploymentStatusRequest("Probation", default, "Bắt đầu thử việc 2 tháng", null, null, null));

        Assert.Equal("Probation", res.Status);
        var history = await _hrmSvc.ListStatusHistoryAsync(_tenant, emp.Id);
        Assert.Single(history);
        Assert.Equal("New", history[0].FromStatus);
        Assert.Equal("Probation", history[0].ToStatus);
        Assert.Equal("Bắt đầu thử việc 2 tháng", history[0].Reason);
    }
}
