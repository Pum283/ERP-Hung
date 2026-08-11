using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 58:
///   UC_CRM_005 — Gộp khách hàng trùng (Customer Deduplication Merge)
///   UC_CRM_006 — Phân loại tệp khách hàng (Customer Segment Classification)
///   UC_CRM_008 — Gán người phụ trách (Customer Owner Assignment)
///   UC_CRM_009 — Bàn giao khách hàng (Customer Ownership Handover)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep58PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmCustomerService _svc;

    private readonly Guid _tenant     = Guid.NewGuid();
    private readonly Guid _userAdmin  = Guid.NewGuid();
    private readonly Guid _userStaff1 = Guid.NewGuid();
    private readonly Guid _userStaff2 = Guid.NewGuid();

    public CrmStep58PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step58-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm58", DisplayName = "Admin CRM 58" });
        _db.Users.Add(new AppUser { Id = _userStaff1, TenantId = _tenant, Username = "staff1_crm58", DisplayName = "Nhân Viên Sales 1" });
        _db.Users.Add(new AppUser { Id = _userStaff2, TenantId = _tenant, Username = "staff2_crm58", DisplayName = "Nhân Viên Sales 2" });

        _db.SaveChanges();

        _svc = new CrmCustomerService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_005: Gộp khách hàng trùng (Customer Deduplication Merge)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_005_MergeCustomers_ValidSourceAndTarget_MergesSuccessfully()
    {
        var source = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_SRC58", "Person", "Khách Trùng Nguồn", null, "0901119999", "source@erp.vn",
                null, "Prospect", _userStaff1, null, "Ghi chú nguồn", null, "Active"));

        var target = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_TGT58", "Person", "Khách Trùng Đích", null, null, null,
                "0311122233", "Customer", _userStaff2, "123 Đích", null, null, "Active"));

        var result = await _svc.MergeAsync(_tenant, _userAdmin, new CrmMergeRequest(source.Id, target.Id));

        Assert.NotNull(result);
        Assert.Equal(target.Id, result.Id);
        Assert.Equal("0901119999", result.Phone); // Điền thông tin rỗng từ nguồn
        Assert.Equal("source@erp.vn", result.Email);

        var updatedSource = await _svc.Get360Async(_tenant, source.Id);
        Assert.Equal("Merged", updatedSource.Customer.Status);
        Assert.Equal(target.Id, updatedSource.Customer.MergedIntoId);
    }

    [Fact]
    public async Task UC_CRM_005_MergeCustomers_SameSourceAndTarget_ThrowsAppException()
    {
        var customer = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_SAME58", "Person", "Khách Tự Gộp", null, null, null,
                null, "Prospect", null, null, null, null, "Active"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.MergeAsync(_tenant, _userAdmin, new CrmMergeRequest(customer.Id, customer.Id)));

        Assert.Contains("Không thể gộp khách vào chính nó", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_005_MergeCustomers_AlreadyMergedSource_ThrowsAppException()
    {
        var c1 = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCustomerUpsertRequest(null, "CUST_M1", "Person", "KH 1", null, null, null, null, "Prospect", null, null, null, null, "Active"));
        var c2 = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCustomerUpsertRequest(null, "CUST_M2", "Person", "KH 2", null, null, null, null, "Prospect", null, null, null, null, "Active"));
        var c3 = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCustomerUpsertRequest(null, "CUST_M3", "Person", "KH 3", null, null, null, null, "Prospect", null, null, null, null, "Active"));

        await _svc.MergeAsync(_tenant, _userAdmin, new CrmMergeRequest(c1.Id, c2.Id));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.MergeAsync(_tenant, _userAdmin, new CrmMergeRequest(c1.Id, c3.Id)));

        Assert.Contains("Không gộp khách đã Merged", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_006: Phân loại tệp khách hàng (Customer Segment Classification)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_006_UpdateSegment_ValidSegment_UpdatesSegmentSuccessfully()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_SEG58", "Person", "Khách Tệp Lead", null, null, null,
                null, "Lead", null, null, null, null, "Active"));

        var updated = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(c.Id, "CUST_SEG58", "Person", "Khách Tệp Partner", null, null, null,
                null, "Partner", null, null, null, null, "Active"));

        Assert.NotNull(updated);
        Assert.Equal("Partner", updated.Segment);
    }

    [Fact]
    public async Task UC_CRM_006_UpdateSegment_InvalidSegment_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertAsync(_tenant, _userAdmin,
                new CrmCustomerUpsertRequest(null, "CUST_BADSEG", "Person", "Khách Tệp Lỗi", null, null, null,
                    null, "InvalidSegment", null, null, null, null, "Active")));

        Assert.Contains("Phân loại tệp không hợp lệ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_008: Gán người phụ trách (Customer Owner Assignment)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_008_AssignOwner_ValidOwner_AssignsSuccessfully()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_OWN58", "Person", "Khách Chưa Gán", null, null, null,
                null, "Prospect", null, null, null, null, "Active"));

        var assigned = await _svc.AssignOwnerAsync(_tenant, _userAdmin, c.Id, new CrmAssignOwnerRequest(_userStaff1));

        Assert.NotNull(assigned);
        Assert.Equal(_userStaff1, assigned.OwnerUserId);
        Assert.Equal("Nhân Viên Sales 1", assigned.OwnerName);
    }

    [Fact]
    public async Task UC_CRM_008_AssignOwner_NonExistentOwner_ThrowsAppException()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_NOOWN", "Person", "Khách Gán Lỗi", null, null, null,
                null, "Prospect", null, null, null, null, "Active"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.AssignOwnerAsync(_tenant, _userAdmin, c.Id, new CrmAssignOwnerRequest(Guid.NewGuid())));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Người phụ trách không tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_009: Bàn giao khách hàng (Customer Ownership Handover)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_009_HandoverCustomer_ValidStaff_RecordsHandoverHistory()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_HND58", "Person", "Khách Bàn Giao", null, null, null,
                null, "Prospect", _userStaff1, null, null, null, "Active"));

        var handover = await _svc.HandoverAsync(_tenant, _userAdmin, c.Id,
            new CrmHandoverRequest(_userStaff2, "Bàn giao lại khu vực Quận 1"));

        Assert.NotNull(handover);
        Assert.Equal(_userStaff1, handover.FromUserId);
        Assert.Equal(_userStaff2, handover.ToUserId);
        Assert.Equal("Bàn giao lại khu vực Quận 1", handover.Note);

        var c360 = await _svc.Get360Async(_tenant, c.Id);
        Assert.Equal(_userStaff2, c360.Customer.OwnerUserId);
        Assert.NotEmpty(c360.Handovers);
    }

    [Fact]
    public async Task UC_CRM_009_HandoverCustomer_MergedCustomer_ThrowsAppException()
    {
        var c1 = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCustomerUpsertRequest(null, "CUST_HM1", "Person", "KH 1", null, null, null, null, "Prospect", null, null, null, null, "Active"));
        var c2 = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCustomerUpsertRequest(null, "CUST_HM2", "Person", "KH 2", null, null, null, null, "Prospect", null, null, null, null, "Active"));
        await _svc.MergeAsync(_tenant, _userAdmin, new CrmMergeRequest(c1.Id, c2.Id));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.HandoverAsync(_tenant, _userAdmin, c1.Id, new CrmHandoverRequest(_userStaff2, "Giao khách đã gộp")));

        Assert.Contains("Khách đã gộp", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_009_HandoverCustomer_NonExistentTargetUser_ThrowsAppException()
    {
        var c = await _svc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_HNDERR", "Person", "Khách Lỗi Giao", null, null, null,
                null, "Prospect", _userStaff1, null, null, null, "Active"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.HandoverAsync(_tenant, _userAdmin, c.Id, new CrmHandoverRequest(Guid.NewGuid(), "Lỗi người nhận")));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Người nhận bàn giao không tồn tại", ex.Message);
    }
}
