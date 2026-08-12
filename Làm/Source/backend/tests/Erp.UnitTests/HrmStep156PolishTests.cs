using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class HrmStep156PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly HrmStep156Service _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _orgUnit = Guid.NewGuid();
    private readonly Guid _employeeId = Guid.NewGuid();

    public HrmStep156PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("hrm-step156-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "T156", Name = "Tenant 156" });
        _db.OrgUnits.Add(new OrgUnit { Id = _orgUnit, TenantId = _tenant, Code = "OU1", Name = "Chi nhánh Hà Nội" });
        _db.Employees.Add(new Employee { Id = _employeeId, TenantId = _tenant, EmployeeCode = "EMP001", FullName = "Nguyễn Văn A", OrgUnitId = _orgUnit });

        _db.SaveChanges();

        _svc = new HrmStep156Service(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_005: Quản lý bộ phận trong đơn vị
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC005_CreateDepartment_Succeeds()
    {
        var req = new HrmDepartmentUpsertRequest("DEPT_HR", "Phòng Nhân Sự", null, _orgUnit, null, 1, true);
        var dto = await _svc.CreateDepartmentAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal("DEPT_HR", dto.Code);
        Assert.Equal("Phòng Nhân Sự", dto.Name);
        Assert.Equal(dto.Id.ToString(), dto.Path);
    }

    [Fact]
    public async Task UC005_CreateDepartment_DuplicateCode_ThrowsAppException()
    {
        var req1 = new HrmDepartmentUpsertRequest("DEPT_IT", "Phòng IT", null, _orgUnit, null, 1, true);
        await _svc.CreateDepartmentAsync(_tenant, req1);

        var req2 = new HrmDepartmentUpsertRequest("dept_it", "Phòng IT 2", null, _orgUnit, null, 2, true);
        await Assert.ThrowsAsync<AppException>(() => _svc.CreateDepartmentAsync(_tenant, req2));
    }

    [Fact]
    public async Task UC005_UpdateDepartment_SelfParent_ThrowsAppException()
    {
        var req = new HrmDepartmentUpsertRequest("DEPT_ACC", "Phòng Kế Toán", null, _orgUnit, null, 1, true);
        var dto = await _svc.CreateDepartmentAsync(_tenant, req);

        var updateReq = new HrmDepartmentUpsertRequest("DEPT_ACC", "Phòng Kế Toán 2", dto.Id, _orgUnit, null, 1, true);
        await Assert.ThrowsAsync<AppException>(() => _svc.UpdateDepartmentAsync(_tenant, dto.Id, updateReq));
    }

    [Fact]
    public async Task UC005_DeleteDepartment_WithChildren_ThrowsAppException()
    {
        var parent = await _svc.CreateDepartmentAsync(_tenant, new HrmDepartmentUpsertRequest("DEPT_P", "Bộ phận Cha", null, _orgUnit, null, 1, true));
        await _svc.CreateDepartmentAsync(_tenant, new HrmDepartmentUpsertRequest("DEPT_C", "Bộ phận Con", parent.Id, _orgUnit, null, 2, true));

        await Assert.ThrowsAsync<AppException>(() => _svc.DeleteDepartmentAsync(_tenant, parent.Id));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_008: Quản lý vị trí công việc
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC008_CreateJobPosition_Succeeds()
    {
        var req = new JobPositionUpsertRequest("POS_DEV", "Lập trình viên Senior", null, 1, true);
        var dto = await _svc.CreateJobPositionAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal("POS_DEV", dto.Code);
        Assert.Equal("Lập trình viên Senior", dto.Name);
    }

    [Fact]
    public async Task UC008_CreateJobPosition_DuplicateCode_ThrowsAppException()
    {
        var req1 = new JobPositionUpsertRequest("POS_TEST", "Tester", null, 1, true);
        await _svc.CreateJobPositionAsync(_tenant, req1);

        var req2 = new JobPositionUpsertRequest("pos_test", "Tester Lead", null, 2, true);
        await Assert.ThrowsAsync<AppException>(() => _svc.CreateJobPositionAsync(_tenant, req2));
    }

    [Fact]
    public async Task UC008_UpdateJobPosition_Succeeds()
    {
        var dto = await _svc.CreateJobPositionAsync(_tenant, new JobPositionUpsertRequest("POS_PM", "Project Manager", null, 1, true));
        var updated = await _svc.UpdateJobPositionAsync(_tenant, dto.Id, new JobPositionUpsertRequest("POS_PM", "Lead Project Manager", null, 2, true));

        Assert.Equal("Lead Project Manager", updated.Name);
        Assert.Equal(2, updated.SortOrder);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_011: Định nghĩa trung tâm chi phí NS
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC011_CreateCostCenter_Succeeds()
    {
        var req = new HrmCostCenterUpsertRequest("CC_HR_01", "Trung tâm Chi phí Nhân sự 1", _orgUnit, 80.5m, true);
        var dto = await _svc.CreateCostCenterAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal("CC_HR_01", dto.Code);
        Assert.Equal(80.5m, dto.AllocationPercentage);
    }

    [Fact]
    public async Task UC011_CreateCostCenter_InvalidAllocation_ThrowsAppException()
    {
        var req = new HrmCostCenterUpsertRequest("CC_ERR", "Trung tâm Lỗi", _orgUnit, 120m, true);
        await Assert.ThrowsAsync<AppException>(() => _svc.CreateCostCenterAsync(_tenant, req));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_023: Quản lý người thân / liên hệ khẩn
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC023_CreateRelative_Succeeds()
    {
        var req = new EmployeeRelativeUpsertRequest(_employeeId, "Nguyễn Thị B", "Spouse", "0912345678", "Hà Nội", true, true, "123456789");
        var dto = await _svc.CreateRelativeAsync(_tenant, req);

        Assert.NotNull(dto);
        Assert.Equal("Nguyễn Thị B", dto.FullName);
        Assert.Equal("Spouse", dto.Relationship);
        Assert.True(dto.IsEmergencyContact);
        Assert.True(dto.IsTaxDependent);
    }

    [Fact]
    public async Task UC023_CreateRelative_EmployeeNotFound_ThrowsAppException()
    {
        var req = new EmployeeRelativeUpsertRequest(Guid.NewGuid(), "Trần Văn C", "Child", "0987654321", null, false, false, null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.CreateRelativeAsync(_tenant, req));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC023_GetRelatives_FiltersByEmployeeId_Succeeds()
    {
        await _svc.CreateRelativeAsync(_tenant, new EmployeeRelativeUpsertRequest(_employeeId, "Mẹ Nguyễn A", "Parent", "0900000000", null, true, false, null));

        var list = await _svc.GetRelativesAsync(_tenant, _employeeId);
        Assert.Single(list);
        Assert.Equal("Mẹ Nguyễn A", list[0].FullName);
    }
}
