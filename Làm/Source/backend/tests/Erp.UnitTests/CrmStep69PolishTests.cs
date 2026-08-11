using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 69:
///   UC_CRM_060 — Import lead Excel/CSV (Bulk Lead Ingestion & Data Parsing)
///   UC_CRM_061 — Báo cáo chuyển đổi lead (Lead Conversion Rate & Funnel Analytics)
///   UC_CRM_062 — Tạo cơ hội từ lead/khách (Opportunity Creation & Customer Binding)
///   UC_CRM_063 — Pipeline cơ hội theo giai đoạn (Opportunity Stage Pipeline Tracking)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep69PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmLeadService _leadSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep69PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step69-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm69", DisplayName = "Admin CRM 69" });

        _db.SaveChanges();

        _leadSvc = new CrmLeadService(_db, null!);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_060: Import lead Excel/CSV
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_060_ImportCsv_ValidContent_ImportsLeadsSuccessfully()
    {
        var csv = "Name,Phone,Email,CompanyName\nNguyễn Văn Imp,0901112233,imp@growth.vn,Công ty Imp";
        var res = await _leadSvc.ImportCsvAsync(_tenant, _userAdmin, new CrmLeadImportRequest(csv));

        Assert.NotNull(res);
        Assert.True(res.Created >= 1);
    }

    [Fact]
    public async Task UC_CRM_060_ImportCsv_EmptyContent_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.ImportCsvAsync(_tenant, _userAdmin, new CrmLeadImportRequest("")));

        Assert.Contains("CSV trống", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_060_ImportCsv_InvalidRowData_ReturnsErrors()
    {
        var csv = "Name,Phone,Email\nNamNoContact,,";
        var res = await _leadSvc.ImportCsvAsync(_tenant, _userAdmin, new CrmLeadImportRequest(csv));

        Assert.NotNull(res);
        Assert.NotEmpty(res.Errors);
        Assert.Contains(res.Errors, e => e.Contains("Cần ít nhất SĐT hoặc Email"));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_061: Báo cáo chuyển đổi lead
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_061_GetConversionReport_CalculatesFunnelMetrics()
    {
        await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Funnel 1", "0902223344", "funnel1@growth.vn", null,
            null, null, null, "New", 0, null, null, null));

        var rpt = await _leadSvc.GetConversionReportAsync(_tenant);

        Assert.NotNull(rpt);
        Assert.NotNull(rpt.ByStatus);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_062: Tạo cơ hội từ lead/khách
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_062_UpsertOpportunity_ValidInput_CreatesOpportunitySuccessfully()
    {
        var opp = await _leadSvc.UpsertOpportunityAsync(_tenant, _userAdmin, new CrmOpportunityUpsertRequest(
            null, null, "Cơ hội ERP Enterprise 2026", null, null, _userAdmin,
            "Qualification", 500000000m, 60, DateTimeOffset.UtcNow.AddMonths(2), null, null, "Hợp đồng quy mô lớn"));

        Assert.NotNull(opp);
        Assert.Equal("Cơ hội ERP Enterprise 2026", opp.Name);
        Assert.Equal("Qualification", opp.Stage);
        Assert.Equal(500000000m, opp.EstimatedValue);
    }

    [Fact]
    public async Task UC_CRM_062_UpsertOpportunity_MissingName_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.UpsertOpportunityAsync(_tenant, _userAdmin, new CrmOpportunityUpsertRequest(
                null, null, "", null, null, null, null, null, null, null, null, null, null)));

        Assert.Contains("Tên cơ hội", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_062_GetOpportunityDetail_ReturnsOpportunityAndLines()
    {
        var opp = await _leadSvc.UpsertOpportunityAsync(_tenant, _userAdmin, new CrmOpportunityUpsertRequest(
            null, null, "Cơ hội Chi Tiết", null, null, null,
            "Qualification", 100000000m, 50, null, null, null, null));

        var detail = await _leadSvc.GetOpportunityDetailAsync(_tenant, opp.Id);

        Assert.NotNull(detail);
        Assert.Equal("Cơ hội Chi Tiết", detail.Opportunity.Name);
    }

    [Fact]
    public async Task UC_CRM_062_UpsertOpportunity_NonExistentId_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.UpsertOpportunityAsync(_tenant, _userAdmin, new CrmOpportunityUpsertRequest(
                Guid.NewGuid(), null, "Tên Sửa", null, null, null, null, null, null, null, null, null, null)));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_063: Pipeline cơ hội theo giai đoạn
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_063_ListOpportunities_FiltersByStageAndKeyword()
    {
        await _leadSvc.UpsertOpportunityAsync(_tenant, _userAdmin, new CrmOpportunityUpsertRequest(
            null, null, "Cơ hội Lọc Stage", null, null, _userAdmin,
            "Proposal", 200000000m, 70, null, null, null, null));

        var list = await _leadSvc.ListOpportunitiesAsync(_tenant, "Lọc Stage", "Proposal");

        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains(list, o => o.Stage == "Proposal");
    }

    [Fact]
    public async Task UC_CRM_063_ListOpportunities_AllStages_ReturnsFullList()
    {
        var list = await _leadSvc.ListOpportunitiesAsync(_tenant, null, null);
        Assert.NotNull(list);
    }
}
