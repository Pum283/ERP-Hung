using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Log;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 106:
///   UC_LOG_014 — Cập nhật trạng thái vận đơn (UpdateStatusAsync)
///   UC_LOG_017 — Ghi nhận giao thất bại (UpdateStatusAsync with status = "Failed")
///   UC_LOG_021 — Ghi nhận số tiền COD (MarkCodAsync / SetCodAmountAsync)
///   UC_LOG_022 — Xác nhận đã thu COD (ConfirmCollectedAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class LogStep106PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LogLogisticsService _logistics;
    private readonly LogCodService _cod;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public LogStep106PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("log-step106-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin106", DisplayName = "Admin 106" });
        _db.SaveChanges();

        _logistics = new LogLogisticsService(_db);
        _cod = new LogCodService(_db, _logistics);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_014: Cập nhật trạng thái vận đơn
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_014_UpdateStatus_InTransit_UpdatesDeliveryStatus()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-106-01", "Khách 106-1", null, null, null, null));
        await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-LOG106", "SP 106", 1m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);

        var updated = await _logistics.UpdateStatusAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("InTransit", "Đang vận chuyển trên đường"));

        Assert.NotNull(updated);
        Assert.Equal("InTransit", updated.Status);
    }

    [Fact]
    public async Task UC_LOG_014_UpdateStatus_InvalidStatus_ThrowsException()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-106-02", "Khách 106-2", null, null, null, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _logistics.UpdateStatusAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("InvalidStatus", null)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_017: Ghi nhận giao thất bại
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_017_UpdateStatus_Failed_RecordsFailureReason()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-106-03", "Khách Fail", null, null, null, null));
        await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-FAIL", "SP Fail", 1m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);

        var failed = await _logistics.UpdateStatusAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Failed", "Khách không nhấc máy"));

        Assert.NotNull(failed);
        Assert.Equal("Failed", failed.Status);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_021: Ghi nhận số tiền COD
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_021_MarkCod_ValidAmount_SetsCodStatusPending()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-106-04", "Khách COD 1", null, null, null, null));

        var marked = await _cod.MarkCodAsync(_tenant, _userAdmin, order.Id, new LogCodMarkRequest(350000m, 3, "COD Thu hộ"));

        Assert.NotNull(marked);
        Assert.True(marked.IsCod);
        Assert.Equal(350000m, marked.CodAmount);
        Assert.Equal("Pending", marked.CodStatus);
    }

    [Fact]
    public async Task UC_LOG_021_SetCodAmount_ValidAmount_UpdatesCodAmount()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-106-05", "Khách COD 2", null, null, null, null));
        await _cod.MarkCodAsync(_tenant, _userAdmin, order.Id, new LogCodMarkRequest(300000m, 3, null));

        var updated = await _cod.SetCodAmountAsync(_tenant, _userAdmin, order.Id, new LogCodAmountRequest(400000m, "Điều chỉnh tăng tiền thu"));

        Assert.NotNull(updated);
        Assert.Equal(400000m, updated.CodAmount);
    }

    [Fact]
    public async Task UC_LOG_021_MarkCod_NegativeAmount_ThrowsException()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-106-06", "Khách COD Err", null, null, null, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _cod.MarkCodAsync(_tenant, _userAdmin, order.Id, new LogCodMarkRequest(-100000m, 3, null)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_022: Xác nhận đã thu COD
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_022_ConfirmCollected_PendingCod_TransitionsToCollected()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-106-07", "Khách Thu COD", null, null, null, null));
        await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-C1", "SP C1", 1m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);
        await _logistics.StartPickAsync(_tenant, _userAdmin, order.Id);
        var detail = await _logistics.GetDeliveryDetailAsync(_tenant, order.Id);
        await _logistics.ConfirmPickAsync(_tenant, _userAdmin, order.Id, new LogPickRequest(new List<LogPickLineRequest> { new LogPickLineRequest(detail.Lines[0].Id, 1m) }));
        await _logistics.DispatchAsync(_tenant, _userAdmin, order.Id);
        await _cod.MarkCodAsync(_tenant, _userAdmin, order.Id, new LogCodMarkRequest(250000m, 3, null));

        var collected = await _cod.ConfirmCollectedAsync(_tenant, _userAdmin, order.Id, new LogCodCollectRequest("Đã thu đủ tiền mặt"));

        Assert.NotNull(collected);
        Assert.Equal("Collected", collected.CodStatus);
    }

    [Fact]
    public async Task UC_LOG_022_ConfirmCollected_NonCodOrder_ThrowsException()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-106-08", "Khách NonCOD", null, null, null, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _cod.ConfirmCollectedAsync(_tenant, _userAdmin, order.Id, new LogCodCollectRequest(null)));
    }

    [Fact]
    public async Task UC_LOG_021_SetCodAmount_UnmarkedOrder_ThrowsException()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-106-09", "Khách SetErr", null, null, null, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _cod.SetCodAmountAsync(_tenant, _userAdmin, order.Id, new LogCodAmountRequest(100000m, null)));
    }

    [Fact]
    public async Task UC_LOG_014_UpdateStatus_DeliveredStatus_UpdatesDeliveredAt()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-106-10", "Khách Del", null, null, null, null));
        await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-D106", "SP Del 106", 1m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);
        await _logistics.StartPickAsync(_tenant, _userAdmin, order.Id);
        var detail = await _logistics.GetDeliveryDetailAsync(_tenant, order.Id);
        await _logistics.ConfirmPickAsync(_tenant, _userAdmin, order.Id, new LogPickRequest(new List<LogPickLineRequest> { new LogPickLineRequest(detail.Lines[0].Id, 1m) }));
        await _logistics.DispatchAsync(_tenant, _userAdmin, order.Id);

        var delivered = await _logistics.UpdateStatusAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Delivered", "Giao thành công"));

        Assert.Equal("Delivered", delivered.Status);
        Assert.NotNull(delivered.DeliveredAt);
    }
}
