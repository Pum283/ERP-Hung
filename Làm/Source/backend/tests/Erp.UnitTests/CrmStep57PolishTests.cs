using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 57:
///   UC_CRM_001 — Tạo khách hàng cá nhân (Individual Customer Creation)
///   UC_CRM_002 — Tạo khách hàng doanh nghiệp (Corporate Customer Creation)
///   UC_CRM_003 — Cập nhật thông tin khách hàng (Customer Information Update)
///   UC_CRM_004 — Kiểm tra trùng SĐT / MST (Phone Number & Tax Code Deduplication)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep57PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmCustomerService _svc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep57PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step57-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm57", DisplayName = "Admin CRM 57" });

        _db.SaveChanges();

        _svc = new CrmCustomerService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_001: Tạo khách hàng cá nhân (Individual Customer Creation)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_001_CreatePersonCustomer_ValidInput_CreatesSuccessfully()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_PER57", "Person", "Nguyễn Văn A 57", null, "0909123456", "nva57@gmail.com",
                null, "Prospect", null, "123 Lê Lợi, Q1", "Tạo cá nhân", null, "Active"));

        Assert.NotNull(c);
        Assert.Equal("CUST_PER57", c.Code);
        Assert.Equal("Person", c.CustomerType);
        Assert.Equal("0909123456", c.Phone);
    }

    [Fact]
    public async Task UC_CRM_001_CreatePersonCustomer_InvalidType_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertAsync(_tenant, _userAdmin,
                new CrmCustomerUpsertRequest(null, "CUST_BADTYPE", "InvalidType", "Khách Hàng Sai Loại", null, null, null,
                    null, "Prospect", null, null, null, null, "Active")));

        Assert.Contains("Loại KH không hợp lệ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_002: Tạo khách hàng doanh nghiệp (Corporate Customer Creation)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_002_CreateOrgCustomer_ValidInput_CreatesSuccessfully()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_ORG57", "Organization", "Công ty TNHH ERP Hùng 57", "Công ty TNHH ERP Hùng 57",
                "02838123456", "contact@erphung57.vn", "0312345678", "Customer", null, "456 Nguyễn Huệ", "Tạo DN", null, "Active"));

        Assert.NotNull(c);
        Assert.Equal("CUST_ORG57", c.Code);
        Assert.Equal("Organization", c.CustomerType);
        Assert.Equal("0312345678", c.TaxCode);
    }

    [Fact]
    public async Task UC_CRM_002_CreateOrgCustomer_MissingCompanyNameAndDisplayName_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertAsync(_tenant, _userAdmin,
                new CrmCustomerUpsertRequest(null, "CUST_NOORG", "Organization", "", "", null, null,
                    null, "Customer", null, null, null, null, "Active")));

        Assert.Contains("Tên hiển thị 1–200 ký tự", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_003: Cập nhật thông tin khách hàng (Customer Information Update)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_003_UpdateCustomer_ValidInput_UpdatesInformation()
    {
        var initial = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_UPD57", "Person", "Tên Cũ 57", null, "0908888111", null,
                null, "Prospect", null, null, null, null, "Active"));

        var updated = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(initial.Id, "CUST_UPD57", "Person", "Tên Mới Cập Nhật 57", null, "0908888111", "updated57@erp.vn",
                null, "Customer", null, "789 Cách Mạng Tháng 8", "Đã nâng cấp", null, "Active"));

        Assert.NotNull(updated);
        Assert.Equal(initial.Id, updated.Id);
        Assert.Equal("Tên Mới Cập Nhật 57", updated.DisplayName);
        Assert.Equal("Customer", updated.Segment);
        Assert.Equal("updated57@erp.vn", updated.Email);
    }

    [Fact]
    public async Task UC_CRM_003_UpdateCustomer_NonExistentId_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertAsync(_tenant, _userAdmin,
                new CrmCustomerUpsertRequest(Guid.NewGuid(), "CUST_GHOST", "Person", "Khách Ảo", null, null, null,
                    null, "Prospect", null, null, null, null, "Active")));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Khách hàng không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_004: Kiểm tra trùng SĐT / MST (Phone Number & Tax Code Deduplication)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_004_CreateCustomer_DuplicatePhone_ThrowsAppException()
    {
        await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_P1", "Person", "Khách SĐT 1", null, "09011122233", null,
                null, "Prospect", null, null, null, null, "Active"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertAsync(_tenant, _userAdmin,
                new CrmCustomerUpsertRequest(null, "CUST_P2", "Person", "Khách SĐT 2", null, "09011122233", null,
                    null, "Prospect", null, null, null, null, "Active")));

        Assert.Contains("SĐT đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_004_CreateCustomer_DuplicateTaxCode_ThrowsAppException()
    {
        await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_T1", "Organization", "DN 1", "DN 1", null, null,
                "0399999999", "Customer", null, null, null, null, "Active"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertAsync(_tenant, _userAdmin,
                new CrmCustomerUpsertRequest(null, "CUST_T2", "Organization", "DN 2", "DN 2", null, null,
                    "0399999999", "Customer", null, null, null, null, "Active")));

        Assert.Contains("MST đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_004_FindDuplicates_ReturnsMatchedCustomers()
    {
        await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_CHK1", "Person", "Khách Test Dup", null, "0907776655", null,
                "0377766554", "Prospect", null, null, null, null, "Active"));

        var dupList = await _svc.FindDuplicatesAsync(_tenant, "0907776655", "0377766554", null);

        Assert.NotNull(dupList);
        Assert.NotEmpty(dupList);
        Assert.Contains(dupList, c => c.Code == "CUST_CHK1");
    }

    [Fact]
    public async Task UC_CRM_001_Search_ReturnsFilteredCustomers()
    {
        await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_SRCH57", "Person", "Tìm Kiếm 57", null, "0905554433", null,
                null, "Prospect", null, null, null, null, "Active"));

        var list = await _svc.SearchAsync(_tenant, new CrmCustomerSearchRequest("Tìm Kiếm 57", "Person", null, null, null, null, null, false));

        Assert.NotEmpty(list);
        Assert.Contains(list, c => c.Code == "CUST_SRCH57");
    }
}
