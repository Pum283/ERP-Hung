using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Log;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 105:
///   UC_LOG_009 — Pick list / soạn hàng (StartPickAsync & ConfirmPickAsync)
///   UC_LOG_011 — In vận đơn / phiếu giao (PrintWaybillAsync & GetDeliveryDetailAsync)
///   UC_LOG_012 — Hủy / hoàn lệnh giao (CancelAsync & ReturnAsync)
///   UC_LOG_013 — Phân công tài xế / đơn vị vận chuyển (AssignAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class LogStep105PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LogLogisticsService _logistics;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public LogStep105PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("log-step105-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin105", DisplayName = "Admin 105" });
        _db.SaveChanges();

        _logistics = new LogLogisticsService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_009: Pick list / soạn hàng
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_009_StartPick_ConfirmedOrder_TransitionsToPicking()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-105-01", "Khách Pick", null, null, null, null));
        await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-P1", "SP Pick 1", 10m, "CAI", null));
        var confirmed = await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);

        var picking = await _logistics.StartPickAsync(_tenant, _userAdmin, confirmed.Id);

        Assert.NotNull(picking);
        Assert.Equal("Picking", picking.Status);
    }

    [Fact]
    public async Task UC_LOG_009_ConfirmPick_FullQtyPicked_TransitionsToReady()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-105-02", "Khách Pick Full", null, null, null, null));
        var line = await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-P2", "SP Pick 2", 15m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);
        await _logistics.StartPickAsync(_tenant, _userAdmin, order.Id);

        var pickReq = new LogPickRequest(new List<LogPickLineRequest>
        {
            new LogPickLineRequest(line.Id, 15m)
        });

        var ready = await _logistics.ConfirmPickAsync(_tenant, _userAdmin, order.Id, pickReq);

        Assert.NotNull(ready);
        Assert.Equal("Ready", ready.Status);
        Assert.NotNull(ready.PickedAt);
    }

    [Fact]
    public async Task UC_LOG_009_ConfirmPick_PartialPicked_ThrowsException()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-105-03", "Khách Pick Partial", null, null, null, null));
        var line = await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-P3", "SP Pick 3", 10m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);
        await _logistics.StartPickAsync(_tenant, _userAdmin, order.Id);

        var pickReq = new LogPickRequest(new List<LogPickLineRequest>
        {
            new LogPickLineRequest(line.Id, 5m)
        });

        await Assert.ThrowsAsync<AppException>(() =>
            _logistics.ConfirmPickAsync(_tenant, _userAdmin, order.Id, pickReq));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_011: In vận đơn / phiếu giao
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_011_PrintWaybill_ConfirmedOrder_GeneratesWaybillNo()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-105-04", "Khách In VD", null, null, null, null));
        await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-VD", "SP VD", 5m, "CAI", null));
        var confirmed = await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);

        var printed = await _logistics.PrintWaybillAsync(_tenant, _userAdmin, confirmed.Id);

        Assert.NotNull(printed);
        Assert.False(string.IsNullOrEmpty(printed.WaybillNo));
        Assert.NotNull(printed.WaybillPrintedAt);
    }

    [Fact]
    public async Task UC_LOG_011_PrintWaybill_DraftOrder_ThrowsException()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-105-05", "Khách Draft VD", null, null, null, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _logistics.PrintWaybillAsync(_tenant, _userAdmin, order.Id));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_012: Hủy / hoàn lệnh giao
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_012_Cancel_DraftOrder_TransitionsToCancelled()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-105-06", "Khách Hủy", null, null, null, null));

        var cancelled = await _logistics.CancelAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Cancelled", "Khách hủy đơn"));

        Assert.NotNull(cancelled);
        Assert.Equal("Cancelled", cancelled.Status);
    }

    [Fact]
    public async Task UC_LOG_012_Return_DispatchedOrder_TransitionsToReturned()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-105-07", "Khách Hoàn", null, null, null, null));
        var line = await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-RET", "SP Ret", 2m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);
        await _logistics.StartPickAsync(_tenant, _userAdmin, order.Id);
        await _logistics.ConfirmPickAsync(_tenant, _userAdmin, order.Id, new LogPickRequest(new List<LogPickLineRequest> { new LogPickLineRequest(line.Id, 2m) }));
        await _logistics.DispatchAsync(_tenant, _userAdmin, order.Id);

        var returned = await _logistics.ReturnAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Returned", "Khách không nhận hàng"));

        Assert.NotNull(returned);
        Assert.Equal("Returned", returned.Status);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_013: Phân công tài xế / đơn vị vận chuyển
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_013_Assign_ValidDriverName_UpdatesDriverInfo()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-105-08", "Khách Gán TX", null, null, null, null));

        var assigned = await _logistics.AssignAsync(_tenant, _userAdmin, order.Id, new LogAssignRequest(null, _userAdmin, "Tài xế Hùng"));

        Assert.NotNull(assigned);
        Assert.Equal(_userAdmin, assigned.DriverUserId);
        Assert.Equal("Tài xế Hùng", assigned.DriverName);
    }

    [Fact]
    public async Task UC_LOG_013_Assign_ValidCarrier_UpdatesCarrierId()
    {
        var carrier = await _logistics.UpsertCarrierAsync(_tenant, _userAdmin, new LogCarrierUpsertRequest(null, "LOG-C105", "ĐVVC 105", null, null, null, "Active"));
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-105-09", "Khách Gán ĐVVC", null, null, null, null));

        var assigned = await _logistics.AssignAsync(_tenant, _userAdmin, order.Id, new LogAssignRequest(carrier.Id, null, null));

        Assert.NotNull(assigned);
        Assert.Equal(carrier.Id, assigned.CarrierId);
    }

    [Fact]
    public async Task UC_LOG_012_Cancel_DeliveredOrder_ThrowsException()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-105-10", "Khách Hủy Err", null, null, null, null));
        var line = await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-DEL", "SP Del", 1m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);
        await _logistics.StartPickAsync(_tenant, _userAdmin, order.Id);
        await _logistics.ConfirmPickAsync(_tenant, _userAdmin, order.Id, new LogPickRequest(new List<LogPickLineRequest> { new LogPickLineRequest(line.Id, 1m) }));
        await _logistics.DispatchAsync(_tenant, _userAdmin, order.Id);
        await _logistics.UpdateStatusAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Delivered", null));

        await Assert.ThrowsAsync<AppException>(() =>
            _logistics.CancelAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Cancelled", null)));
    }
}
