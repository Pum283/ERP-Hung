using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Application.DTOs.Log;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Implementations.Services.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 104:
///   UC_INV_070 — Xuất báo cáo kho Excel / CSV (ExportValuationCsvAsync)
///   UC_LOG_001 — Danh mục đơn vị vận chuyển (UpsertCarrierAsync & ListCarriersAsync)
///   UC_LOG_006 — Tạo lệnh giao từ đơn hàng (UpsertDeliveryAsync & UpsertLineAsync)
///   UC_LOG_008 — Tách lệnh giao nhiều đợt (SplitBatchAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class LogStep104PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvReportService _invReport;
    private readonly LogLogisticsService _logistics;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public LogStep104PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("log-step104-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin104", DisplayName = "Admin 104" });
        _db.SaveChanges();

        _invReport = new InvReportService(_db);
        _logistics = new LogLogisticsService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_070: Xuất báo cáo kho CSV
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_070_ExportValuationCsv_ReturnsCsvString()
    {
        var csv = await _invReport.ExportCsvAsync(_tenant, "stock-value", warehouseId: null);

        Assert.NotNull(csv);
        Assert.Contains("SkuCode,SkuName,Warehouse", csv);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_001: Danh mục đơn vị vận chuyển
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_001_UpsertCarrier_ValidCarrier_CreatesCarrier()
    {
        var req = new LogCarrierUpsertRequest(null, "GHN", "Giao Hàng Nhanh", "0901234567", "Nguyễn Văn A", "ĐVVC Nhanh", "Active");

        var carrier = await _logistics.UpsertCarrierAsync(_tenant, _userAdmin, req);

        Assert.NotNull(carrier);
        Assert.Equal("GHN", carrier.Code);
        Assert.Equal("Giao Hàng Nhanh", carrier.Name);
    }

    [Fact]
    public async Task UC_LOG_001_ListCarriers_ReturnsCarriersList()
    {
        await _logistics.UpsertCarrierAsync(_tenant, _userAdmin, new LogCarrierUpsertRequest(null, "GHTK", "Giao Hàng Tiết Kiệm", null, null, null, "Active"));

        var list = await _logistics.ListCarriersAsync(_tenant, null);

        Assert.NotEmpty(list);
    }

    [Fact]
    public async Task UC_LOG_001_UpsertCarrier_DuplicateCode_ThrowsException()
    {
        await _logistics.UpsertCarrierAsync(_tenant, _userAdmin, new LogCarrierUpsertRequest(null, "VTP", "Viettel Post", null, null, null, "Active"));

        await Assert.ThrowsAsync<AppException>(() =>
            _logistics.UpsertCarrierAsync(_tenant, _userAdmin, new LogCarrierUpsertRequest(null, "VTP", "Viettel Post 2", null, null, null, "Active")));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_006: Tạo lệnh giao từ đơn hàng
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_006_UpsertDelivery_ValidRequest_CreatesDraftDeliveryOrder()
    {
        var req = new LogDeliveryUpsertRequest(null, null, "SO-104-01", "Nguyễn Văn B", "123 Lê Lợi, Q1", "0987654321", "Giao hỏa tốc", null);

        var delivery = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, req);

        Assert.NotNull(delivery);
        Assert.Equal("Draft", delivery.Status);
        Assert.Equal("SO-104-01", delivery.SourceOrderCode);
    }

    [Fact]
    public async Task UC_LOG_006_UpsertLine_ValidItem_AddsLineToDeliveryOrder()
    {
        var delivery = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-104-02", "Khách 104", null, null, null, null));
        var lineReq = new LogDeliveryLineUpsertRequest(null, "SKU-LOG104", "SP Giao Vận 104", 10m, "CAI", null);

        var line = await _logistics.UpsertLineAsync(_tenant, _userAdmin, delivery.Id, lineReq);

        Assert.NotNull(line);
        Assert.Equal(10m, line.Qty);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_008: Tách lệnh giao nhiều đợt
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_008_SplitBatch_ValidLines_CreatesChildBatchDeliveryOrder()
    {
        var delivery = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, "DG-104-BASE", "SO-104-03", "Khách Đợt", null, null, null, null));
        var line = await _logistics.UpsertLineAsync(_tenant, _userAdmin, delivery.Id, new LogDeliveryLineUpsertRequest(null, "SKU-SPLIT", "SP Tách", 20m, "CAI", null));

        var splitReq = new LogSplitBatchRequest(new List<LogSplitLineRequest>
        {
            new LogSplitLineRequest(line.Id, 5m)
        }, "Tách giao đợt 2");

        var child = await _logistics.SplitBatchAsync(_tenant, _userAdmin, delivery.Id, splitReq);

        Assert.NotNull(child);
        Assert.Equal(2, child.BatchNo);
        Assert.Equal("Draft", child.Status);
    }

    [Fact]
    public async Task UC_LOG_008_SplitBatch_EmptyLines_ThrowsException()
    {
        var delivery = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-104-04", "Khách Err", null, null, null, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _logistics.SplitBatchAsync(_tenant, _userAdmin, delivery.Id, new LogSplitBatchRequest(new List<LogSplitLineRequest>(), null)));
    }

    [Fact]
    public async Task UC_LOG_006_Confirm_ValidOrderWithLines_ConfirmsDeliveryOrder()
    {
        var delivery = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-104-05", "Khách Confirm", null, null, null, null));
        await _logistics.UpsertLineAsync(_tenant, _userAdmin, delivery.Id, new LogDeliveryLineUpsertRequest(null, "SKU-CFM", "SP CFM", 5m, "CAI", null));

        var confirmed = await _logistics.ConfirmAsync(_tenant, _userAdmin, delivery.Id);

        Assert.Equal("Confirmed", confirmed.Status);
    }

    [Fact]
    public async Task UC_LOG_006_Confirm_OrderWithoutLines_ThrowsException()
    {
        var delivery = await _logistics.UpsertDeliveryAsync(_tenant, _userAdmin, new LogDeliveryUpsertRequest(null, null, "SO-104-06", "Khách Empty", null, null, null, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _logistics.ConfirmAsync(_tenant, _userAdmin, delivery.Id));
    }
}
