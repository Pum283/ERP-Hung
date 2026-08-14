using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class LogShiftTripPodReschedulePolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LogShiftTripPodRescheduleService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();

    public LogShiftTripPodReschedulePolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("log-shift-trip-pod-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new LogShiftTripPodRescheduleService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateDeliveryShift_SavesShiftTiming()
    {
        var req = new LogCreateDeliveryShiftRequest("SHIFT-SPECIAL", "Ca Đêm Tăng Cường", "22:00", "04:00", 20);
        var res = await _svc.CreateDeliveryShiftAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("SHIFT-SPECIAL", res.ShiftCode);
        Assert.Equal("22:00", res.StartTime);
    }

    [Fact]
    public async Task ConsolidateTrip_CreatesConsolidatedTripNumber()
    {
        var orderIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var req = new LogConsolidateTripRequest(Guid.NewGuid(), "Lê Văn Hùng", "59C-123.45", orderIds, 1500, DateTimeOffset.UtcNow.AddHours(2));
        var res = await _svc.ConsolidateTripAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("TRIP-", res.TripNumber);
        Assert.Equal(3, res.TotalOrdersCount);
        Assert.Equal("Planned", res.Status);
    }

    [Fact]
    public async Task SubmitPod_RecordsSignatureAndRecipient()
    {
        var req = new LogSubmitPodRequest(_orderId, "DEL-2026-0814", "Nguyễn Văn Nhận", "0918112233", "http://pod.png", "http://photo.jpg", "Đã nhận đủ");
        var res = await _svc.SubmitPodAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Nguyễn Văn Nhận", res.RecipientName);
        Assert.Equal("DEL-2026-0814", res.DeliveryOrderNumber);
    }

    [Fact]
    public async Task CreateRedeliveryRequest_SetsPendingReassignment()
    {
        var req = new LogCreateRedeliveryRequest(_orderId, "DEL-2026-0814", "Khách hàng đi vắng", DateTimeOffset.UtcNow.AddDays(2), "Afternoon");
        var res = await _svc.CreateRedeliveryRequestAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("REDELIV-", res.RequestNumber);
        Assert.Equal("PendingReassignment", res.Status);
    }
}
