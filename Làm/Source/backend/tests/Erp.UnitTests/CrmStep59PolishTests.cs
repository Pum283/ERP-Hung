using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 59:
///   UC_CRM_010 — Hồ sơ khách 360° (Customer 360 View)
///   UC_CRM_011 — Danh sách người liên hệ (Contact Roster Management)
///   UC_CRM_012 — Lịch sử thay đổi dữ liệu (Audit & Handover Log Tracking)
///   UC_CRM_013 — Ngưng sử dụng / blacklist (Customer Deactivation & Blacklisting)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep59PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmCustomerService _svc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep59PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step59-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm59", DisplayName = "Admin CRM 59" });

        _db.SaveChanges();

        _svc = new CrmCustomerService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_010: Hồ sơ khách 360° (Customer 360 View)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_010_Get360_ValidCustomer_ReturnsFull360Details()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_360_59", "Person", "Khách 360 Độ 59", null, "0909998877", "c360@erp.vn",
                null, "Customer", _userAdmin, "Address 360", "Note 360", null, "Active"));

        await _svc.UpsertContactAsync(_tenant, _userAdmin, c.Id,
            new CrmContactUpsertRequest(null, "Nguyễn Văn Liên Hệ 1", "Giám Đốc Kỹ Thuật", "0901112233", "ct1@erp.vn", true));

        var c360 = await _svc.Get360Async(_tenant, c.Id);

        Assert.NotNull(c360);
        Assert.Equal("CUST_360_59", c360.Customer.Code);
        Assert.Single(c360.Contacts);
        Assert.True(c360.Contacts[0].IsPrimary);
    }

    [Fact]
    public async Task UC_CRM_010_Get360_NonExistentCustomer_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.Get360Async(_tenant, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Khách hàng không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_011: Danh sách người liên hệ (Contact Roster Management)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_011_UpsertContact_CreatePrimary_SetsIsPrimaryFlag()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_CNT59", "Person", "Khách Có Danh Bạ", null, null, null,
                null, "Prospect", null, null, null, null, "Active"));

        var ct1 = await _svc.UpsertContactAsync(_tenant, _userAdmin, c.Id,
            new CrmContactUpsertRequest(null, "Liên Hệ 1", "Trưởng Phòng Sales", "0901111111", "ct1@erp.vn", true));

        var ct2 = await _svc.UpsertContactAsync(_tenant, _userAdmin, c.Id,
            new CrmContactUpsertRequest(null, "Liên Hệ 2", "Phó Phòng Sales", "0902222222", "ct2@erp.vn", true));

        var list = await _svc.ListContactsAsync(_tenant, c.Id);

        Assert.Equal(2, list.Count);
        Assert.True(list.First(x => x.Id == ct2.Id).IsPrimary);
        Assert.False(list.First(x => x.Id == ct1.Id).IsPrimary); // Tự động reset IsPrimary cũ
    }

    [Fact]
    public async Task UC_CRM_011_UpsertContact_EmptyFullName_ThrowsAppException()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_NOCNT", "Person", "Khách Danh Bạ Rỗng", null, null, null,
                null, "Prospect", null, null, null, null, "Active"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertContactAsync(_tenant, _userAdmin, c.Id,
                new CrmContactUpsertRequest(null, "", "Chức Vụ", null, null, false)));

        Assert.Contains("Tên liên hệ 1–200 ký tự", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_012: Lịch sử thay đổi dữ liệu (Audit & Handover Log Tracking)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_012_HandoverHistory_RecordedIn360View()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_LOG59", "Person", "Khách Lịch Sử", null, null, null,
                null, "Prospect", _userAdmin, null, null, null, "Active"));

        await _svc.HandoverAsync(_tenant, _userAdmin, c.Id, new CrmHandoverRequest(_userAdmin, "Ghi nhật ký bàn giao 1"));

        var c360 = await _svc.Get360Async(_tenant, c.Id);

        Assert.NotEmpty(c360.Handovers);
        Assert.Contains(c360.Handovers, h => h.Note != null && h.Note.Contains("Ghi nhật ký bàn giao 1"));
    }

    [Fact]
    public async Task UC_CRM_012_ListContacts_NonExistentCustomer_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.ListContactsAsync(_tenant, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Khách hàng không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_013: Ngưng sử dụng / blacklist (Customer Deactivation & Blacklisting)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_013_SetStatus_Blacklist_UpdatesStatusAndNote()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_BLK59", "Person", "Khách Cần Blacklist", null, null, null,
                null, "Customer", null, null, null, null, "Active"));

        var updated = await _svc.SetStatusAsync(_tenant, _userAdmin, c.Id,
            new CrmCustomerSetStatusRequest("Blacklisted", "Vi phạm chính sách thanh toán nhiều lần"));

        Assert.NotNull(updated);
        Assert.Equal("Blacklisted", updated.Status);
        Assert.Contains("Vi phạm chính sách thanh toán nhiều lần", updated.Note);
    }

    [Fact]
    public async Task UC_CRM_013_SetStatus_InvalidStatus_ThrowsAppException()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_BADST59", "Person", "Khách Trạng Thái Lỗi", null, null, null,
                null, "Prospect", null, null, null, null, "Active"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.SetStatusAsync(_tenant, _userAdmin, c.Id,
                new CrmCustomerSetStatusRequest("InvalidStatus", "Lý do sai")));

        Assert.Contains("Trạng thái không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_013_SetStatus_MergedCustomer_ThrowsAppException()
    {
        var c1 = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCustomerUpsertRequest(null, "CUST_MGD1", "Person", "KH 1", null, null, null, null, "Prospect", null, null, null, null, "Active"));
        var c2 = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCustomerUpsertRequest(null, "CUST_MGD2", "Person", "KH 2", null, null, null, null, "Prospect", null, null, null, null, "Active"));
        await _svc.MergeAsync(_tenant, _userAdmin, new CrmMergeRequest(c1.Id, c2.Id));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.SetStatusAsync(_tenant, _userAdmin, c1.Id,
                new CrmCustomerSetStatusRequest("Inactive", "Đổi trạng thái đã gộp")));

        Assert.Contains("Khách đã gộp — không đổi trạng thái", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_011_UpsertContact_UpdateExistingContact_UpdatesFieldsSuccessfully()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_CNTUPD", "Person", "Khách Sửa Danh Bạ", null, null, null,
                null, "Prospect", null, null, null, null, "Active"));

        var initial = await _svc.UpsertContactAsync(_tenant, _userAdmin, c.Id,
            new CrmContactUpsertRequest(null, "Liên Hệ Ban Đầu", "Nhân Viên", "0908887766", "old@erp.vn", false));

        var updated = await _svc.UpsertContactAsync(_tenant, _userAdmin, c.Id,
            new CrmContactUpsertRequest(initial.Id, "Liên Hệ Đã Sửa", "Giám Đốc", "0908887766", "new@erp.vn", true));

        Assert.NotNull(updated);
        Assert.Equal(initial.Id, updated.Id);
        Assert.Equal("Liên Hệ Đã Sửa", updated.FullName);
        Assert.Equal("Giám Đốc", updated.Title);
        Assert.True(updated.IsPrimary);
    }
}
