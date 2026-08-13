using Erp.Application.DTOs;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PurSupplierCategoryQualityPosSyncPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurSupplierCategoryQualityPosSyncService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _supplierId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    public PurSupplierCategoryQualityPosSyncPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-supplier-category-quality-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TPUR185", Name = "Tenant PUR 185" });
        _db.SaveChanges();

        _svc = new PurSupplierCategoryQualityPosSyncService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_060: Đồng bộ đơn sang CRM
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SyncPosOrderToCrm_SyncsOrderToCustomerCrmProfile()
    {
        var req = new PosSyncOrderToCrmRequest(_orderId, _customerId);
        var res = await _svc.SyncPosOrderToCrmAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.True(res.IsSynced);
        Assert.StartsWith("CRM-ACT-", res.CrmActivityRecordCode);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_002: Phân loại nhóm nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveSupplierCategory_SavesCategorySuccessfully()
    {
        var req = new PurSaveSupplierCategoryRequest("CAT-BEVERAGE", "Nhóm Đồ Uống & Nguyên Liệu Pha Chế", "Cà phê, trà, mứt, siro");
        var res = await _svc.SaveSupplierCategoryAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("CAT-BEVERAGE", res.CategoryCode);

        var list = await _svc.GetSupplierCategoriesAsync(_tenant);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_004: Lead time & MOQ
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSupplierLeadTimeMoq_ReturnsLeadTimeAndMoqDetails()
    {
        var res = await _svc.GetSupplierLeadTimeMoqAsync(_tenant, _supplierId);

        Assert.NotNull(res);
        Assert.True(res.DeliveryLeadTimeDays > 0);
        Assert.True(res.MinimumOrderQuantity > 0);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_005: Đánh giá chất lượng nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EvaluateSupplierQuality_CalculatesOverallScoreAndGrade()
    {
        var req = new PurSaveSupplierQualityEvaluationRequest(
            _supplierId,
            "Q3-2026",
            95,
            90,
            88,
            "Giao hàng đúng hẹn"
        );

        var res = await _svc.EvaluateSupplierQualityAsync(_tenant, _userId, req);

        Assert.NotNull(res);
        Assert.Equal(91.0, res.OverallRatingScore);
        Assert.Equal("A", res.RatingGrade);

        var list = await _svc.GetSupplierQualityEvaluationsAsync(_tenant, _supplierId);
        Assert.NotEmpty(list);
    }
}
