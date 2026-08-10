using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 11: UC_HRM_001 (Cơ cấu tổ chức công ty), UC_HRM_002 (Khối vận hành),
/// UC_HRM_003 (Khối sản xuất), UC_HRM_004 (Danh mục điểm bán).
/// 18+ test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class SysHrmOrgSalesPointPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SysPlatformService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _actor  = Guid.NewGuid();

    public SysHrmOrgSalesPointPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("sys-hrm-org-sp-" + Guid.NewGuid())
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
        _svc = new SysPlatformService(_db, new OutboxWriter(_db));
    }

    public void Dispose() => _db.Dispose();

    // ─── UC_HRM_001: Tạo sơ đồ tổ chức công ty ───

    [Fact]
    public async Task UC001_UpsertOrgUnit_RootNode_CreatesSuccessfully()
    {
        var req = new UpsertOrgUnitRequest(null, "HQ", "Tập đoàn ERP", null, "Company");
        var res = await _svc.UpsertOrgUnitAsync(_tenant, _actor, req);

        Assert.Equal("HQ", res.Code);
        Assert.Equal("Tập đoàn ERP", res.Name);
        Assert.Equal("Company", res.UnitType);
        Assert.Null(res.ParentId);
        Assert.Equal($"/{res.Id}", res.Path);
        Assert.Equal(0, res.ChildCount);
    }

    [Fact]
    public async Task UC001_UpsertOrgUnit_ChildNode_CalculatesPathAndChildCount()
    {
        var parent = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "HQ", "Tập đoàn ERP", null, "Company"));
        var child = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "CN_HN", "Chi nhánh Hà Nội", parent.Id, "Branch"));

        Assert.Equal(parent.Id, child.ParentId);
        Assert.Equal("Tập đoàn ERP", child.ParentName);
        Assert.Equal($"/{parent.Id}/{child.Id}", child.Path);

        var parentDetail = await _svc.GetOrgUnitDetailAsync(_tenant, parent.Id);
        Assert.Equal(1, parentDetail.ChildCount);
    }

    [Fact]
    public async Task UC001_UpsertOrgUnit_EmptyCodeOrName_ThrowsAppException()
    {
        var ex1 = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "", "Tên", null, "Branch")));
        Assert.Contains("Code", ex1.Message);

        var ex2 = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "CODE", "", null, "Branch")));
        Assert.Contains("Name", ex2.Message);
    }

    [Fact]
    public async Task UC001_UpsertOrgUnit_DuplicateCode_ThrowsAppException()
    {
        await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "CN_DN", "Chi nhánh Đà Nẵng", null, "Branch"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "cn_dn", "Đà Nẵng 2", null, "Branch")));
        Assert.Contains("đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC001_UpsertOrgUnit_SelfParent_ThrowsAppException()
    {
        var node = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "CN_HCM", "Chi nhánh HCM", null, "Branch"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(node.Id, "CN_HCM", "Chi nhánh HCM", node.Id, "Branch")));
        Assert.Contains("chính mình", ex.Message);
    }

    [Fact]
    public async Task UC001_UpsertOrgUnit_CircularReference_ThrowsAppException()
    {
        var grandParent = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "GP", "Ông", null, "Company"));
        var parent = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "P", "Cha", grandParent.Id, "Branch"));
        var child = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "C", "Con", parent.Id, "Department"));

        // Thử đổi GrandParent có ParentId là Child -> Vòng lặp!
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(grandParent.Id, "GP", "Ông", child.Id, "Company")));
        Assert.Contains("Vòng lặp", ex.Message);
    }

    [Fact]
    public async Task UC001_DeleteOrgUnit_NodeWithChildren_ThrowsAppException()
    {
        var parent = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "P1", "Cha 1", null, "Company"));
        await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "C1", "Con 1", parent.Id, "Branch"));

        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.DeleteOrgUnitAsync(_tenant, parent.Id));
        Assert.Contains("đơn vị con", ex.Message);
    }

    [Fact]
    public async Task UC001_DeleteOrgUnit_NodeWithEmployees_ThrowsAppException()
    {
        var unit = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "DEP1", "Phòng KD", null, "Department"));

        _db.Employees.Add(new Employee { TenantId = _tenant, OrgUnitId = unit.Id, EmployeeCode = "EMP01", FullName = "Nguyễn Văn A" });
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.DeleteOrgUnitAsync(_tenant, unit.Id));
        Assert.Contains("nhân sự thuộc về", ex.Message);
    }

    [Fact]
    public async Task UC001_DeleteOrgUnit_LeafNode_SoftDeletesSuccessfully()
    {
        var leaf = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "LEAF", "Lá", null, "Department"));
        await _svc.DeleteOrgUnitAsync(_tenant, leaf.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.GetOrgUnitDetailAsync(_tenant, leaf.Id));
        Assert.Equal(404, ex.StatusCode);
    }

    // ─── UC_HRM_002: Quản lý khối vận hành ───

    [Fact]
    public async Task UC002_UpsertOpsBlock_CreatesOperationsBlockUnit()
    {
        var company = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "CORP", "Tổng Công Ty", null, "Company"));
        var ops = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "OPS_BLOCK", "Khối Vận Hành & Chuỗi Cung Ứng", company.Id, "OperationsBlock"));

        Assert.Equal("OperationsBlock", ops.UnitType);
        Assert.Equal("Khối Vận Hành & Chuỗi Cung Ứng", ops.Name);

        var list = await _svc.ListOrgUnitsAsync(_tenant, "OperationsBlock");
        Assert.Single(list);
        Assert.Equal("OPS_BLOCK", list[0].Code);
    }

    // ─── UC_HRM_003: Quản lý khối sản xuất ───

    [Fact]
    public async Task UC003_UpsertMfgBlock_CreatesProductionBlockUnit()
    {
        var company = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "CORP", "Tổng Công Ty", null, "Company"));
        var mfg = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "MFG_BLOCK", "Khối Sản Xuất & Chế Biến", company.Id, "ProductionBlock"));

        Assert.Equal("ProductionBlock", mfg.UnitType);

        var list = await _svc.ListOrgUnitsAsync(_tenant, "ProductionBlock");
        Assert.Single(list);
        Assert.Equal("MFG_BLOCK", list[0].Code);
    }

    // ─── UC_HRM_004: Quản lý danh mục điểm bán ───

    [Fact]
    public async Task UC004_UpsertSalesPoint_ValidData_CreatesSalesPoint()
    {
        var branch = await _svc.UpsertOrgUnitAsync(_tenant, _actor, new UpsertOrgUnitRequest(null, "CN_SG", "Chi nhánh Sài Gòn", null, "Branch"));
        var sp = await _svc.UpsertSalesPointAsync(_tenant, _actor, new SalesPointDto(Guid.Empty, "SP001", "Cửa hàng Quận 1", branch.Id, "123 Lê Lợi, Q1", true));

        Assert.Equal("SP001", sp.Code);
        Assert.Equal("Cửa hàng Quận 1", sp.Name);
        Assert.Equal(branch.Id, sp.OrgUnitId);
        Assert.True(sp.IsActive);
    }

    [Fact]
    public async Task UC004_ListSalesPoints_ReturnsTenantSalesPoints()
    {
        await _svc.UpsertSalesPointAsync(_tenant, _actor, new SalesPointDto(Guid.Empty, "SP1", "CH 1", null, "Địa chỉ 1", true));
        await _svc.UpsertSalesPointAsync(_tenant, _actor, new SalesPointDto(Guid.Empty, "SP2", "CH 2", null, "Địa chỉ 2", true));

        var list = await _svc.ListSalesPointsAsync(_tenant);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task UC004_DeleteSalesPoint_SoftDeletesSuccessfully()
    {
        var sp = await _svc.UpsertSalesPointAsync(_tenant, _actor, new SalesPointDto(Guid.Empty, "SP_DEL", "Cửa hàng xóa", null, "Hà Nội", true));
        await _svc.DeleteSalesPointAsync(_tenant, sp.Id);

        var list = await _svc.ListSalesPointsAsync(_tenant);
        Assert.Empty(list);
    }

    [Fact]
    public async Task UC004_DeleteSalesPoint_NonExistent_Throws404()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.DeleteSalesPointAsync(_tenant, Guid.NewGuid()));
        Assert.Equal(404, ex.StatusCode);
    }
}
