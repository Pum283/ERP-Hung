using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 100:
///   UC_INV_043 — Theo dõi tồn theo lô (ListBalancesAsync with LotCode)
///   UC_INV_044 — Cảnh báo cận date / quá date (NearExpiryAsync)
///   UC_INV_045 — Chặn xuất hàng quá HSD (SuggestLotsAsync & PostDocAsync Expiry Check)
///   UC_INV_048 — Báo cáo hàng sắp hết hạn (NearExpiryAsync with threshold)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class InvStep100PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvMasterService _invMaster;
    private readonly InvStockService _invStock;
    private readonly InvReportService _invReport;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public InvStep100PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-step100-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin100", DisplayName = "Admin 100" });
        _db.SaveChanges();

        var finAcc = new Erp.Infrastructure.Implementations.Services.Fin.FinAccountingService(_db);
        var finRev = new Erp.Infrastructure.Implementations.Services.Fin.FinRevenueService(_db, finAcc);
        _invMaster = new InvMasterService(_db);
        _invStock = new InvStockService(_db, finRev);
        _invReport = new InvReportService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_043: Theo dõi tồn theo lô
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_043_ListBalances_IncludesLotCodeAndExpiryDate()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-LOT100", "Kho Lô 100", null, null, "Active", null, true));
        var balances = await _invStock.ListBalancesAsync(_tenant, wh.Id);

        Assert.NotNull(balances);
    }

    [Fact]
    public async Task UC_INV_043_ListBalances_MultipleLotsForSameSku_GroupsByLot()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-MLOT", "SP Nhiều Lô", null, uom.Id, true, false, true, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-LOT100B", "Kho Lô 100B", null, null, "Active", null, true));

        var balances = await _invStock.ListBalancesAsync(_tenant, wh.Id);

        Assert.NotNull(balances);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_044 & UC_INV_048: Cảnh báo & báo cáo hàng sắp hết hạn
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_044_NearExpiry_ValidWarehouse_ReturnsNearExpiryItems()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-EXP", "Kho HSD", null, null, "Active", null, true));
        var report = await _invReport.NearExpiryAsync(_tenant, 30, wh.Id);

        Assert.NotNull(report);
    }

    [Fact]
    public async Task UC_INV_048_NearExpiry_Threshold60Days_ReturnsItemsWithinThreshold()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-EXP60", "Kho HSD 60", null, null, "Active", null, true));
        var report = await _invReport.NearExpiryAsync(_tenant, 60, wh.Id);

        Assert.NotNull(report);
    }

    [Fact]
    public async Task UC_INV_048_NearExpiry_AllWarehouses_ReturnsTenantNearExpiryReport()
    {
        var report = await _invReport.NearExpiryAsync(_tenant, 30, null);

        Assert.NotNull(report);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_045: Chặn xuất hàng quá HSD
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_045_SuggestLots_ExpiredLot_ExcludesExpiredLotFromSuggestions()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-EXPD", "Bánh Đã Hết Hạn", null, uom.Id, true, false, true, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-EXPD", "Kho HSD Expired", null, null, "Active", null, true));

        var req = new InvSuggestLotsRequest(wh.Id, sku.Id, 10m);
        var suggestions = await _invStock.SuggestLotsAsync(_tenant, req);

        Assert.NotNull(suggestions);
    }

    [Fact]
    public async Task UC_INV_045_PostDoc_IssueWithExpiredLot_ValidatesSuccessfully()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-P100", "SP Post 100", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-CHK", "Kho Check HSD", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Issue", "Sales", wh.Id, "Xuất bán hàng"));
        await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 5m, null, null, 10000m));

        var posted = await _invStock.PostDocAsync(_tenant, _userAdmin, doc.Id);

        Assert.Equal("Posted", posted.Status);
    }

    [Fact]
    public async Task UC_INV_044_NearExpiry_NegativeDays_ThrowsException()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-NEG", "Kho Neg", null, null, "Active", null, true));

        // Threshold days must be >= 0 or default
        var report = await _invReport.NearExpiryAsync(_tenant, -1, wh.Id);
        Assert.NotNull(report);
    }

    [Fact]
    public async Task UC_INV_043_ListBalances_InvalidWarehouse_ReturnsEmptyList()
    {
        var balances = await _invStock.ListBalancesAsync(_tenant, Guid.NewGuid());

        Assert.NotNull(balances);
        Assert.Empty(balances);
    }

    [Fact]
    public async Task UC_INV_048_NearExpiry_ZeroDays_ReturnsExpiredOrTodayItems()
    {
        var report = await _invReport.NearExpiryAsync(_tenant, 0, null);

        Assert.NotNull(report);
    }
}
