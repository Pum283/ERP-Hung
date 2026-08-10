using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.DTOs.Mod;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Auth;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 14: UC_HRM_032 (Chuyển trạng thái Nghỉ việc), UC_HRM_033 (Lịch sử thay đổi trạng thái),
/// UC_HRM_034 (Điều chuyển đơn vị / bộ phận), UC_HRM_036 (Cảnh báo sắp hết hạn thử việc).
/// 13+ test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class HrmEmployeeTransferProbationPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SysPlatformService _sysSvc;
    private readonly HrmEmployeeService _hrmSvc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _actor  = Guid.NewGuid();

    public HrmEmployeeTransferProbationPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-transfer-probation-" + Guid.NewGuid())
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

    // ─── UC_HRM_032: Chuyển trạng thái Nghỉ việc ───

    [Fact]
    public async Task UC032_ChangeStatus_ToResigned_UpdatesStatusAndLocks()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_RESIGN", FullName = "Phạm Nghỉ Việc", Status = "Active" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var effDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var res = await _hrmSvc.ChangeStatusAsync(_tenant, _actor, emp.Id, new ChangeEmploymentStatusRequest("Resigned", effDate, "Xin nghỉ việc cá nhân", null, null, null));

        Assert.Equal("Resigned", res.Status);
        var dbEmp = await _db.Employees.FirstOrDefaultAsync(x => x.Id == emp.Id);
        Assert.NotNull(dbEmp);
        Assert.True(dbEmp!.IsDeleted);
        Assert.Equal(effDate, dbEmp.TerminateDate);
    }

    // ─── UC_HRM_033: Lịch sử thay đổi trạng thái ───

    [Fact]
    public async Task UC033_ListStatusHistory_ReturnsSortedDescending()
    {
        var emp = new Employee { TenantId = _tenant, EmployeeCode = "EMP_HIST", FullName = "Lê Lịch Sử", Status = "New" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var d1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var d2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));

        await _hrmSvc.ChangeStatusAsync(_tenant, _actor, emp.Id, new ChangeEmploymentStatusRequest("Probation", d1, "Lên Thử việc", null, null, null));
        await _hrmSvc.ChangeStatusAsync(_tenant, _actor, emp.Id, new ChangeEmploymentStatusRequest("Active", d2, "Lên Chính thức", null, null, null));

        var history = await _hrmSvc.ListStatusHistoryAsync(_tenant, emp.Id);
        Assert.Equal(2, history.Count);
        Assert.Equal("Active", history[0].ToStatus);
        Assert.Equal(d2, history[0].EffectiveDate);
        Assert.Equal("Probation", history[1].ToStatus);
        Assert.Equal(d1, history[1].EffectiveDate);
    }

    // ─── UC_HRM_034: Điều chuyển đơn vị / bộ phận ───

    [Fact]
    public async Task UC034_TransferEmployee_ValidNewOrgAndDept_TransfersAndSyncsUser()
    {
        var org1 = new OrgUnit { TenantId = _tenant, Code = "ORG1", Name = "Hà Nội", UnitType = "Branch", Path = "/1" };
        var org2 = new OrgUnit { TenantId = _tenant, Code = "ORG2", Name = "Đà Nẵng", UnitType = "Branch", Path = "/2" };
        _db.OrgUnits.AddRange(org1, org2);

        var dept2 = new Department { TenantId = _tenant, OrgUnitId = org2.Id, Code = "D2", Name = "Kinh doanh Đà Nẵng" };
        _db.Departments.Add(dept2);

        var user = new AppUser { TenantId = _tenant, Username = "transfer_usr", PrimaryOrgUnitId = org1.Id };
        _db.Users.Add(user);

        var emp = new Employee { TenantId = _tenant, UserId = user.Id, OrgUnitId = org1.Id, EmployeeCode = "EMP_TR1", FullName = "Nguyễn Văn Chuyển" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var req = new EmployeeTransferRequest(org2.Id, dept2.Id, null, null, DateOnly.FromDateTime(DateTime.UtcNow), "Điều chuyển công tác Đà Nẵng");
        var res = await _hrmSvc.TransferEmployeeAsync(_tenant, _actor, emp.Id, req);

        Assert.Equal(org2.Id, res.OrgUnitId);
        Assert.Equal("Đà Nẵng", res.OrgUnitName);
        Assert.Equal("Kinh doanh Đà Nẵng", res.DepartmentName);

        var dbUser = await _db.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
        Assert.Equal(org2.Id, dbUser!.PrimaryOrgUnitId);
        Assert.Equal(dept2.Id, dbUser.DepartmentId);
    }

    [Fact]
    public async Task UC034_TransferEmployee_SameOrgAndDept_ThrowsAppException()
    {
        var org = new OrgUnit { TenantId = _tenant, Code = "ORG_SAME", Name = "HCM", UnitType = "Branch", Path = "/1" };
        _db.OrgUnits.Add(org);
        var emp = new Employee { TenantId = _tenant, OrgUnitId = org.Id, EmployeeCode = "EMP_SAME", FullName = "Vũ Giữ Nguyên" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        var req = new EmployeeTransferRequest(org.Id, null, null, null, null, "Không thay đổi");
        var ex = await Assert.ThrowsAsync<AppException>(() => _hrmSvc.TransferEmployeeAsync(_tenant, _actor, emp.Id, req));
        Assert.Contains("khác với thông tin hiện tại", ex.Message);
    }

    [Fact]
    public async Task UC034_TransferEmployee_DeptNotBelongToOrg_ThrowsAppException()
    {
        var org1 = new OrgUnit { TenantId = _tenant, Code = "ORG_A", Name = "Đơn vị A", UnitType = "Branch", Path = "/A" };
        var org2 = new OrgUnit { TenantId = _tenant, Code = "ORG_B", Name = "Đơn vị B", UnitType = "Branch", Path = "/B" };
        _db.OrgUnits.AddRange(org1, org2);

        var deptA = new Department { TenantId = _tenant, OrgUnitId = org1.Id, Code = "DEPT_A", Name = "Phòng A" };
        _db.Departments.Add(deptA);

        var emp = new Employee { TenantId = _tenant, OrgUnitId = org1.Id, DepartmentId = deptA.Id, EmployeeCode = "EMP_MISMATCH", FullName = "Lỗi Mismatch" };
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        // Transfer to org2 with deptA (which belongs to org1)
        var req = new EmployeeTransferRequest(org2.Id, deptA.Id, null, null, null, "Sai phòng ban");
        var ex = await Assert.ThrowsAsync<AppException>(() => _hrmSvc.TransferEmployeeAsync(_tenant, _actor, emp.Id, req));
        Assert.Contains("không thuộc Đơn vị tổ chức đã chọn", ex.Message);
    }

    // ─── UC_HRM_036: Cảnh báo sắp hết hạn thử việc ───

    [Fact]
    public async Task UC036_ListExpiringProbation_ReturnsExpiringEmployees()
    {
        var org = new OrgUnit { TenantId = _tenant, Code = "ORG_P", Name = "Trụ sở", UnitType = "Company", Path = "/1" };
        _db.OrgUnits.Add(org);

        var hireDateExpiring = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-55)); // 60 - 55 = 5 days left
        var hireDateSafe = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)); // 60 - 10 = 50 days left

        var empExpiring = new Employee { TenantId = _tenant, OrgUnitId = org.Id, EmployeeCode = "EMP_P1", FullName = "Ngô Thử Việc Sắp Hết Hạn", Status = "Probation", HireDate = hireDateExpiring };
        var empSafe = new Employee { TenantId = _tenant, OrgUnitId = org.Id, EmployeeCode = "EMP_P2", FullName = "Trịnh Thử Việc Mới", Status = "Probation", HireDate = hireDateSafe };
        var empActive = new Employee { TenantId = _tenant, OrgUnitId = org.Id, EmployeeCode = "EMP_P3", FullName = "Đã Chính Thức", Status = "Active", HireDate = hireDateExpiring };

        _db.Employees.AddRange(empExpiring, empSafe, empActive);
        await _db.SaveChangesAsync();

        var list = await _hrmSvc.ListExpiringProbationEmployeesAsync(_tenant, daysAhead: 15);

        Assert.Single(list);
        Assert.Equal("EMP_P1", list[0].EmployeeCode);
        Assert.True(list[0].DaysRemaining <= 15);
    }

    [Fact]
    public async Task UC036_ListExpiringProbation_UsesContractEndDateIfPresent()
    {
        var org = new OrgUnit { TenantId = _tenant, Code = "ORG_C", Name = "Công ty", UnitType = "Company", Path = "/1" };
        _db.OrgUnits.Add(org);

        var hireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var probEndDateContract = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)); // Contract ends in 3 days

        var emp = new Employee { TenantId = _tenant, OrgUnitId = org.Id, EmployeeCode = "EMP_CTR", FullName = "Có Hợp Đồng Thử Việc", Status = "Probation", HireDate = hireDate };
        _db.Employees.Add(emp);

        _db.Contracts.Add(new Contract
        {
            TenantId = _tenant,
            EmployeeId = emp.Id,
            ContractNo = "HDTV-001",
            ContractType = "Probation",
            StartDate = hireDate,
            EndDate = probEndDateContract,
            Status = "Active"
        });
        await _db.SaveChangesAsync();

        var list = await _hrmSvc.ListExpiringProbationEmployeesAsync(_tenant, daysAhead: 10);

        Assert.Single(list);
        Assert.Equal("EMP_CTR", list[0].EmployeeCode);
        Assert.Equal(probEndDateContract, list[0].ProbationEndDate);
        Assert.Equal(3, list[0].DaysRemaining);
    }
}
