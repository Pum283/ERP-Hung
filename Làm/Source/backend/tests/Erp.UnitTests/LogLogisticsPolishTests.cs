using System;
using System.Linq;
using System.Threading.Tasks;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Log;
using Erp.Domain.Entities.Log;
using Erp.Infrastructure.Implementations.Services.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class LogLogisticsPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LogLogisticsService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();

    public LogLogisticsPolishTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"LogTestDb_{Guid.NewGuid()}")
            .Options;
        _db = new AppDbContext(options);
        _svc = new LogLogisticsService(_db);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public async Task UpdateStatus_DeliveredOrder_ToInTransit_ThrowsAppException()
    {
        var order = new LogDeliveryOrder
        {
            TenantId = _tenant, Code = "DG-001", SourceOrderCode = "SO-100", CustomerName = "Công ty ABC",
            Status = "Delivered", DeliveredAt = DateTimeOffset.UtcNow, CreatedBy = _user
        };
        _db.LogDeliveryOrders.Add(order);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(
            () => _svc.UpdateStatusAsync(_tenant, _user, order.Id, new LogStatusRequest("InTransit", "Thử quay lại InTransit")));
    }

    [Fact]
    public async Task UpdateStatus_ValidStatusTransition_UpdatesDeliveredAt()
    {
        var order = new LogDeliveryOrder
        {
            TenantId = _tenant, Code = "DG-002", SourceOrderCode = "SO-101", CustomerName = "Khách Hàng X",
            Status = "InTransit", CreatedBy = _user
        };
        _db.LogDeliveryOrders.Add(order);
        await _db.SaveChangesAsync();

        var updated = await _svc.UpdateStatusAsync(_tenant, _user, order.Id, new LogStatusRequest("Delivered", "Giao thành công cho KH"));

        Assert.Equal("Delivered", updated.Status);
        Assert.NotNull(updated.DeliveredAt);
    }

    [Fact]
    public async Task AssignCarrier_ActiveCarrier_UpdatesCarrierId()
    {
        var carrier = new LogCarrier { TenantId = _tenant, Code = "GHN", Name = "Giao Hàng Nhanh", Status = "Active", CreatedBy = _user };
        _db.LogCarriers.Add(carrier);
        var order = new LogDeliveryOrder
        {
            TenantId = _tenant, Code = "DG-003", SourceOrderCode = "SO-102", CustomerName = "Khách Hàng Y",
            Status = "Draft", CreatedBy = _user
        };
        _db.LogDeliveryOrders.Add(order);
        await _db.SaveChangesAsync();

        var updated = await _svc.AssignAsync(_tenant, _user, order.Id, new LogAssignRequest(carrier.Id, null, "Tài xế Nguyễn Văn A"));

        Assert.Equal(carrier.Id, updated.CarrierId);
        Assert.Equal("Tài xế Nguyễn Văn A", updated.DriverName);
    }

    [Fact]
    public async Task PrintWaybill_GeneratesWaybillNoFormat()
    {
        var order = new LogDeliveryOrder
        {
            TenantId = _tenant, Code = "DG-004", SourceOrderCode = "SO-103", CustomerName = "Khách Hàng Z",
            Status = "Ready", CreatedBy = _user
        };
        _db.LogDeliveryOrders.Add(order);
        await _db.SaveChangesAsync();

        var updated = await _svc.PrintWaybillAsync(_tenant, _user, order.Id);

        Assert.NotNull(updated.WaybillNo);
        Assert.StartsWith($"VD-{order.Code}-", updated.WaybillNo);
    }

    [Fact]
    public async Task FailDelivery_ValidReason_SetsStatusFailedAndReason()
    {
        var order = new LogDeliveryOrder
        {
            TenantId = _tenant, Code = "DG-005", SourceOrderCode = "SO-104", CustomerName = "Khách Hàng M",
            Status = "InTransit", CreatedBy = _user
        };
        _db.LogDeliveryOrders.Add(order);
        await _db.SaveChangesAsync();

        var updated = await _svc.FailAsync(_tenant, _user, order.Id, new LogFailRequest("Khách không nghe máy sau 3 lần gọi"));

        Assert.Equal("Failed", updated.Status);
        Assert.Equal("Khách không nghe máy sau 3 lần gọi", updated.FailureReason);
    }
}
