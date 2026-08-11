using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 66:
///   UC_CRM_047 — Lưu lịch sử chat (Multi-channel Live Chat & Interaction History Logging)
///   UC_CRM_049 — Tạo lead thủ công (Manual Lead Creation with Contact Validation)
///   UC_CRM_050 — Tiếp nhận lead tự động (Automated Multi-Channel Lead Ingestion Engine)
///   UC_CRM_051 — Phân bổ lead cho sales (Automated Sales Lead Assignment & Workload Distribution)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep66PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmPromotionService _promoSvc;
    private readonly CrmLeadService _leadSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();
    private readonly Guid _userSales = Guid.NewGuid();

    public CrmStep66PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step66-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm66", DisplayName = "Admin CRM 66" });
        _db.Users.Add(new AppUser { Id = _userSales, TenantId = _tenant, Username = "sales_crm66", DisplayName = "Sales Rep 66" });

        _db.SaveChanges();

        _promoSvc = new CrmPromotionService(_db);
        _leadSvc = new CrmLeadService(_db, null!);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_047: Lưu lịch sử chat
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_047_SaveChat_ValidInput_LogsChatMessageSuccessfully()
    {
        var chat = await _promoSvc.SaveChatAsync(_tenant, _userAdmin, new CrmChatHistoryRequest(
            "Facebook", "CONV_FB_1001", null, "Inbound", "Xin chào tư vấn giá ERP Cloud giúp tôi", null));

        Assert.NotNull(chat);
        Assert.Equal("Facebook", chat.Channel);
        Assert.Equal("Inbound", chat.Direction);
        Assert.Equal("Xin chào tư vấn giá ERP Cloud giúp tôi", chat.MessageText);
    }

    [Fact]
    public async Task UC_CRM_047_SaveChat_InvalidChannel_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _promoSvc.SaveChatAsync(_tenant, _userAdmin, new CrmChatHistoryRequest(
                "InvalidChannel", null, null, "Inbound", "Test chat", null)));

        Assert.Contains("Channel: Facebook|Zalo|WebChat|WhatsApp|Line", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_049: Tạo lead thủ công
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_049_UpsertLead_ManualInput_CreatesLeadSuccessfully()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Tăng Văn Lead", "0908887766", "tangvan@growth.vn", "Công ty TNHH Tăng Trưởng",
            null, null, null, "New", 0, null, "Doanh nghiệp quan tâm ERP", "Manual"));

        Assert.NotNull(lead);
        Assert.Equal("Tăng Văn Lead", lead.Name);
        Assert.Equal("0908887766", lead.Phone);
        Assert.Equal("New", lead.PipelineStatus);
    }

    [Fact]
    public async Task UC_CRM_049_UpsertLead_MissingContactInfo_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
                null, null, "Lead Không Có Liên Hệ", null, null, null,
                null, null, null, "New", 0, null, null, null)));

        Assert.Contains("Cần ít nhất SĐT hoặc Email", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_050: Tiếp nhận lead tự động
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_050_ListLeads_FiltersByKeywordAndStatus()
    {
        await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Khách Tự Động 1", "0901234999", "auto1@erp.vn", null,
            null, null, null, "New", 0, null, null, null));

        var list = await _leadSvc.ListLeadsAsync(_tenant, "Tự Động", "New", null);

        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains(list, l => l.Name == "Khách Tự Động 1");
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_051: Phân bổ lead cho sales
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_051_AssignLead_ValidSalesUser_AssignsLeadToSales()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Cần Phân Bổ", "0906665544", "phanbo@erp.vn", null,
            null, null, null, "New", 0, null, null, null));

        var assigned = await _leadSvc.AssignAsync(_tenant, _userAdmin, lead.Id, new CrmLeadAssignRequest(_userSales));

        Assert.NotNull(assigned);
        Assert.Equal(_userSales, assigned.OwnerUserId);
        Assert.Equal("Sales Rep 66", assigned.OwnerName);
    }

    [Fact]
    public async Task UC_CRM_051_AssignLead_NonExistentLead_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.AssignAsync(_tenant, _userAdmin, Guid.NewGuid(), new CrmLeadAssignRequest(_userSales)));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_CRM_047_ListChats_ReturnsChatHistoryList()
    {
        await _promoSvc.SaveChatAsync(_tenant, _userAdmin, new CrmChatHistoryRequest(
            "Zalo", "ZALO_CONV_1", null, "Inbound", "Tư vấn báo giá Zalo", null));

        var list = await _promoSvc.ListChatAsync(_tenant, null, null);

        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains(list, c => c.Channel == "Zalo");
    }

    [Fact]
    public async Task UC_CRM_049_GetLeadDetail_ReturnsLeadDetail()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Xem Chi Tiết", "0903332211", "detail@erp.vn", null,
            null, null, null, "New", 0, null, null, null));

        var dto = await _leadSvc.GetLeadDetailAsync(_tenant, lead.Id);

        Assert.NotNull(dto);
        Assert.Equal("Lead Xem Chi Tiết", dto.Lead.Name);
    }

    [Fact]
    public async Task UC_CRM_051_AssignLead_NonExistentSalesUser_ThrowsAppException()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Gán Lỗi Sales", "0907778899", "badsales@erp.vn", null,
            null, null, null, "New", 0, null, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.AssignAsync(_tenant, _userAdmin, lead.Id, new CrmLeadAssignRequest(Guid.NewGuid())));

        Assert.Equal(400, ex.StatusCode);
    }
}
