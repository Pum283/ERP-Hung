using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class InvLotTraceStocktakeLockInternalRequestPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvLotTraceStocktakeLockInternalRequestService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _warehouseId = Guid.NewGuid();

    public InvLotTraceStocktakeLockInternalRequestPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-lottrace-lock-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new InvLotTraceStocktakeLockInternalRequestService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task RecordAndGetLotGenealogy_RecordsBidirectionalLotTrace()
    {
        var req = new InvCreateLotTraceRecordRequest(
            "LOT-MILK-2026",
            Guid.NewGuid(),
            "Forward",
            "PO-NCC-VINAMILK",
            "BATCH-VNM-001",
            "SO-SUPERMARKET-088"
        );

        var res = await _svc.RecordLotTraceAsync(_tenant, req);
        Assert.NotNull(res);
        Assert.Equal("LOT-MILK-2026", res.LotNumber);

        var genealogy = await _svc.GetLotGenealogyAsync(_tenant, "LOT-MILK-2026");
        Assert.NotEmpty(genealogy);
        Assert.Equal("LOT-MILK-2026", genealogy[0].LotNumber);
    }

    [Fact]
    public async Task CreateStocktakeLocationGroup_GeneratesStocktakeCode()
    {
        var req = new InvCreateStocktakeLocationGroupRequest(_warehouseId, "ByLocation", "Khu Vực Kệ A1-A5", 150);
        var res = await _svc.CreateStocktakeLocationGroupAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("STK-", res.StocktakeCode);
        Assert.Equal(150, res.PlannedItemsCount);
    }

    [Fact]
    public async Task SetStocktakeLock_LocksAndUnlocksTransactions()
    {
        var lockReq = new InvSetStocktakeLockRequest(_warehouseId, "FullWarehouse", "All", true, "Thủ Kho Trưởng", "Kiểm kê định kỳ");
        var locked = await _svc.SetStocktakeLockAsync(_tenant, lockReq);

        Assert.NotNull(locked);
        Assert.True(locked.IsLocked);

        var isLocked = await _svc.IsTransactionLockedAsync(_tenant, _warehouseId, "All");
        Assert.True(isLocked);

        var unlockReq = new InvSetStocktakeLockRequest(_warehouseId, "FullWarehouse", "All", false, "Thủ Kho Trưởng", "Hoàn tất kiểm kê");
        var unlocked = await _svc.SetStocktakeLockAsync(_tenant, unlockReq);

        Assert.False(unlocked.IsLocked);
    }

    [Fact]
    public async Task CreateInternalIssueRequest_GeneratesRequestNumber()
    {
        var req = new InvCreateInternalIssueRequest("Phòng Kỹ Thuật Bảo Trì", "Cấp phát dầu nhờn máy móc", _warehouseId, 850000m);
        var res = await _svc.CreateInternalIssueRequestAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("REQ-INT-", res.RequestNumber);
        Assert.Equal(850000m, res.EstimatedTotalCostVnd);
    }
}
