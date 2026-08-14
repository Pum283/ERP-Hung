using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class LogRealtimeGpsInternalTransferPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LogRealtimeGpsInternalTransferService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public LogRealtimeGpsInternalTransferPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("log-realtime-internal-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new LogRealtimeGpsInternalTransferService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task RecordGpsPing_SavesCoordinatesAndSpeed()
    {
        var req = new LogPingGpsLocationRequest(Guid.NewGuid(), "51D-889.99", 10.7769, 106.7009, 52.0, "Cầu Sài Gòn, TP.HCM");
        var res = await _svc.RecordGpsPingAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("51D-889.99", res.VehiclePlateNumber);
        Assert.Equal(52.0, res.CurrentSpeedKmh);

        var list = await _svc.GetLatestFleetLocationsAsync(_tenant);
        Assert.NotEmpty(list);
    }

    [Fact]
    public async Task CreateAndConfirmInternalTransfer_UpdatesReceivedStatus()
    {
        var createReq = new LogCreateInternalTransferDeliveryRequest(Guid.NewGuid(), "Kho Tổng", Guid.NewGuid(), "Kho CN 1", "Trần Văn Lái", "50LD-123.45", 500);
        var created = await _svc.CreateInternalTransferDeliveryAsync(_tenant, createReq);

        Assert.NotNull(created);
        Assert.Equal("InTransit", created.Status);
        Assert.StartsWith("DEL-INT-", created.InternalDeliveryNumber);

        var confirmReq = new LogConfirmInternalReceiptRequest(created.Id, 480, "Thủ Kho Nhận Long");
        var confirmed = await _svc.ConfirmInternalReceiptAsync(_tenant, confirmReq);

        Assert.Equal("DiscrepancyReported", confirmed.Status);
        Assert.Equal(480, confirmed.ReceivedQuantity);
    }

    [Fact]
    public async Task ReconcileInternalDelivery_CalculatesDiscrepancyCost()
    {
        var createReq = new LogCreateInternalTransferDeliveryRequest(Guid.NewGuid(), "Kho A", Guid.NewGuid(), "Kho B", "Lê Văn Vận Tải", "51D-999.88", 200);
        var created = await _svc.CreateInternalTransferDeliveryAsync(_tenant, createReq);

        await _svc.ConfirmInternalReceiptAsync(_tenant, new LogConfirmInternalReceiptRequest(created.Id, 190, "Thủ Kho"));

        var recReq = new LogCreateInternalReconciliationRequest(created.Id, 2500000m, "Hao hụt 10 đơn vị do va đập");
        var reconciled = await _svc.ReconcileInternalDeliveryAsync(_tenant, recReq);

        Assert.NotNull(reconciled);
        Assert.StartsWith("REC-INT-", reconciled.ReconciliationNumber);
        Assert.Equal(10, reconciled.DiscrepancyQty);
        Assert.Equal(2500000m, reconciled.DiscrepancyCostVnd);
    }
}
