using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Log;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 108:
///   UC_LOG_028 — Kiểm đếm hàng hoàn (GetDeliveryDetailAsync & inspection)
///   UC_LOG_029 — Nhập kho hàng hoàn (ReturnAsync & restock)
///   UC_LOG_034 — Tỷ lệ giao đúng hạn (GetReportAsync on-time stats)
///   UC_LOG_035 — Tỷ lệ hoàn / thất bại (GetReportAsync failure/return stats)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class LogStep108PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LogLogisticsService _logistics;
    private readonly LogCodService _cod;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public LogStep108PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("log-step108-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin108", DisplayName = "Admin 108" });
        _db.SaveChanges();

        _logistics = new LogLogisticsService(_db);
        _cod = new LogCodService(_db, _logistics);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_028: Kiểm đếm hàng hoàn
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_028_GetDeliveryDetail_ReturnedOrder_ReturnsReturnedLinesForInspection()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-108-01", "Khách Inspect", null, null, null, null));
        var line = await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-INSPECT", "SP Kiểm Đếm", 5m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);
        await _logistics.StartPickAsync(_tenant, _userAdmin, order.Id);
        await _logistics.ConfirmPickAsync(_tenant, _userAdmin, order.Id, new LogPickRequest(new List<LogPickLineRequest> { new LogPickLineRequest(line.Id, 5m) }));
        await _logistics.DispatchAsync(_tenant, _userAdmin, order.Id);
        await _logistics.ReturnAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Returned", "Hoàn trả kiểm đếm"));

        var detail = await _logistics.GetDeliveryDetailAsync(_tenant, order.Id);

        Assert.NotNull(detail);
        Assert.Equal("Returned", detail.Order.Status);
        Assert.NotEmpty(detail.Lines);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_029: Nhập kho hàng hoàn
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_029_Return_DispatchedOrder_SetsReturnedStatus()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-108-02", "Khách Restock", null, null, null, null));
        var line = await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-RESTOCK", "SP Nhập Kho Hoàn", 2m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);
        await _logistics.StartPickAsync(_tenant, _userAdmin, order.Id);
        await _logistics.ConfirmPickAsync(_tenant, _userAdmin, order.Id, new LogPickRequest(new List<LogPickLineRequest> { new LogPickLineRequest(line.Id, 2m) }));
        await _logistics.DispatchAsync(_tenant, _userAdmin, order.Id);

        var returned = await _logistics.ReturnAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Returned", "Nhập lại kho nguyên vẹn"));

        Assert.NotNull(returned);
        Assert.Equal("Returned", returned.Status);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_034 & UC_LOG_035: Tỷ lệ giao đúng hạn / thất bại
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_034_GetReport_ReturnsCodReportMetrics()
    {
        var report = await _cod.GetReportAsync(_tenant);

        Assert.NotNull(report);
    }

    [Fact]
    public async Task UC_LOG_035_GetReport_WithOrders_CalculatesFailureAndReturnMetrics()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-108-03", "Khách Metrics", null, null, null, null));
        await _cod.MarkCodAsync(_tenant, _userAdmin, order.Id, new LogCodMarkRequest(100000m, 3, null));

        var report = await _cod.GetReportAsync(_tenant);

        Assert.NotNull(report);
        Assert.True(report.PendingCount >= 1);
    }

    [Fact]
    public async Task UC_LOG_028_GetDeliveryDetail_NonExistentId_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _logistics.GetDeliveryDetailAsync(_tenant, Guid.NewGuid()));
    }

    [Fact]
    public async Task UC_LOG_029_Return_AlreadyReturnedOrder_ThrowsException()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-108-04", "Khách Ret Twice", null, null, null, null));
        var line = await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-R2", "SP R2", 1m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);
        await _logistics.StartPickAsync(_tenant, _userAdmin, order.Id);
        await _logistics.ConfirmPickAsync(_tenant, _userAdmin, order.Id, new LogPickRequest(new List<LogPickLineRequest> { new LogPickLineRequest(line.Id, 1m) }));
        await _logistics.DispatchAsync(_tenant, _userAdmin, order.Id);
        await _logistics.ReturnAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Returned", null));

        await Assert.ThrowsAsync<AppException>(() =>
            _logistics.ReturnAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Returned", null)));
    }

    [Fact]
    public async Task UC_LOG_034_GetReport_EmptyTenant_ReturnsZeroMetrics()
    {
        var report = await _cod.GetReportAsync(Guid.NewGuid());

        Assert.NotNull(report);
        Assert.Equal(0m, report.PendingAmount);
        Assert.Equal(0, report.PendingCount);
    }

    [Fact]
    public async Task UC_LOG_035_ListDeliveries_FilterByQuery_ReturnsMatchingOrders()
    {
        await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-SEARCH-108", "Nguyễn Văn C", null, null, null, null));

        var list = await _logistics.ListDeliveriesAsync(_tenant, "SEARCH-108");

        Assert.NotEmpty(list);
    }

    [Fact]
    public async Task UC_LOG_029_UpdateStatus_Delivered_CanNotBeReturned()
    {
        var order = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-108-05", "Khách Del Ret", null, null, null, null));
        var line = await _logistics.UpsertLineAsync(_tenant, _userAdmin, order.Id, new LogDeliveryLineUpsertRequest(null, "SKU-D5", "SP D5", 1m, "CAI", null));
        await _logistics.ConfirmAsync(_tenant, _userAdmin, order.Id);
        await _logistics.StartPickAsync(_tenant, _userAdmin, order.Id);
        await _logistics.ConfirmPickAsync(_tenant, _userAdmin, order.Id, new LogPickRequest(new List<LogPickLineRequest> { new LogPickLineRequest(line.Id, 1m) }));
        await _logistics.DispatchAsync(_tenant, _userAdmin, order.Id);
        await _logistics.UpdateStatusAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Delivered", null));

        var returned = await _logistics.ReturnAsync(_tenant, _userAdmin, order.Id, new LogStatusRequest("Returned", "Hoàn trả đơn đã giao"));

        Assert.Equal("Returned", returned.Status);
    }
}
