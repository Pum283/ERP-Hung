using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 98:
///   UC_INV_033 — Xuất bên gửi / nhập bên nhận (ShipTransferAsync & ReceiveTransferAsync)
///   UC_INV_035 — Theo dõi hàng đang chuyển (ListTransfersAsync with status InTransit)
///   UC_INV_036 — Chuyển từ kho trung tâm (CreateTransferAsync from Central WH)
///   UC_INV_037 — Giữ hàng theo đơn đã duyệt (CreateReservationAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class InvStep98PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvMasterService _invMaster;
    private readonly InvStockService _invStock;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public InvStep98PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-step98-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin98", DisplayName = "Admin 98" });
        _db.SaveChanges();

        var finAcc = new Erp.Infrastructure.Implementations.Services.Fin.FinAccountingService(_db);
        var finRev = new Erp.Infrastructure.Implementations.Services.Fin.FinRevenueService(_db, finAcc);
        _invMaster = new InvMasterService(_db);
        _invStock = new InvStockService(_db, finRev);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_033: Xuất bên gửi / nhập bên nhận
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_033_ShipTransfer_DraftStatus_TransitionsToInTransit()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-S98", "SP Ship 98", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var w1 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-S1", "Kho S1", null, null, "Active", null, true));
        var w2 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-S2", "Kho S2", null, null, "Active", null, true));
        var tr = await _invStock.CreateTransferAsync(_tenant, _userAdmin, new InvTransferCreateRequest(w1.Id, w2.Id, "Xuất kho đi"));
        await _invStock.UpsertTransferLineAsync(_tenant, _userAdmin, tr.Id, new InvTransferLineRequest(null, sku.Id, 10m, null, null));

        var shipped = await _invStock.ShipTransferAsync(_tenant, _userAdmin, tr.Id);

        Assert.NotNull(shipped);
        Assert.Equal("InTransit", shipped.Status);
    }

    [Fact]
    public async Task UC_INV_033_ReceiveTransfer_InTransitStatus_TransitionsToCompleted()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-R98", "SP Receive 98", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var w1 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-R1", "Kho R1", null, null, "Active", null, true));
        var w2 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-R2", "Kho R2", null, null, "Active", null, true));
        var tr = await _invStock.CreateTransferAsync(_tenant, _userAdmin, new InvTransferCreateRequest(w1.Id, w2.Id, null));
        await _invStock.UpsertTransferLineAsync(_tenant, _userAdmin, tr.Id, new InvTransferLineRequest(null, sku.Id, 10m, null, null));
        var shipped = await _invStock.ShipTransferAsync(_tenant, _userAdmin, tr.Id);

        var received = await _invStock.ReceiveTransferAsync(_tenant, _userAdmin, shipped.Id);

        Assert.NotNull(received);
        Assert.Equal("Completed", received.Status);
    }

    [Fact]
    public async Task UC_INV_033_ReceiveTransfer_DraftStatus_ThrowsException()
    {
        var w1 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-E1", "Kho E1", null, null, "Active", null, true));
        var w2 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-E2", "Kho E2", null, null, "Active", null, true));
        var tr = await _invStock.CreateTransferAsync(_tenant, _userAdmin, new InvTransferCreateRequest(w1.Id, w2.Id, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.ReceiveTransferAsync(_tenant, _userAdmin, tr.Id));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_035: Theo dõi hàng đang chuyển
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_035_ListTransfers_FilterInTransit_ReturnsOnlyInTransit()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-T98", "SP Transit 98", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var w1 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-T1", "Kho T1", null, null, "Active", null, true));
        var w2 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-T2", "Kho T2", null, null, "Active", null, true));
        var tr = await _invStock.CreateTransferAsync(_tenant, _userAdmin, new InvTransferCreateRequest(w1.Id, w2.Id, null));
        await _invStock.UpsertTransferLineAsync(_tenant, _userAdmin, tr.Id, new InvTransferLineRequest(null, sku.Id, 10m, null, null));
        await _invStock.ShipTransferAsync(_tenant, _userAdmin, tr.Id);

        var list = await _invStock.ListTransfersAsync(_tenant, "InTransit");

        Assert.NotEmpty(list);
        Assert.All(list, x => Assert.Equal("InTransit", x.Status));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_036: Chuyển từ kho trung tâm
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_036_CreateTransfer_FromCentralWarehouse_CreatesTransfer()
    {
        var wCentral = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-TONG", "Kho Tổng Trung Tâm", null, null, "Active", null, true));
        var wBranch = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-CN01", "Kho Chi Nhánh 1", null, null, "Active", null, true));

        var tr = await _invStock.CreateTransferAsync(_tenant, _userAdmin, new InvTransferCreateRequest(wCentral.Id, wBranch.Id, "Phân phối hàng từ Kho Tổng"));

        Assert.NotNull(tr);
        Assert.Equal(wCentral.Id, tr.FromWarehouseId);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_037: Giữ hàng theo đơn đã duyệt
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_037_CreateReservation_ValidRequest_CreatesActiveReservation()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-RESV", "SP Giữ Hàng", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-RESV", "Kho Giữ Hàng", null, null, "Active", null, true));

        var req = new InvReservationCreateRequest(wh.Id, "SO", Guid.NewGuid(), "SO-98-01", "Giữ hàng cho đơn SO", true, new List<InvReservationLineRequest>
        {
            new InvReservationLineRequest(sku.Id, 15m, null, null)
        });

        var detail = await _invStock.CreateReservationAsync(_tenant, _userAdmin, req);

        Assert.NotNull(detail);
        Assert.NotNull(detail.Header);
        Assert.Equal("Active", detail.Header.Status);
        Assert.Single(detail.Lines);
    }

    [Fact]
    public async Task UC_INV_037_GetReservationDetail_ReturnsHeaderAndLines()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-RESV2", "SP Giữ Hàng 2", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-RESV2", "Kho Giữ Hàng 2", null, null, "Active", null, true));

        var req = new InvReservationCreateRequest(wh.Id, "SO", Guid.NewGuid(), "SO-98-02", null, true, new List<InvReservationLineRequest>
        {
            new InvReservationLineRequest(sku.Id, 5m, null, null)
        });
        var created = await _invStock.CreateReservationAsync(_tenant, _userAdmin, req);

        var detail = await _invStock.GetReservationDetailAsync(_tenant, created.Header.Id);

        Assert.NotNull(detail);
        Assert.Equal("SO-98-02", detail.Header.RefCode);
    }

    [Fact]
    public async Task UC_INV_037_ListReservations_ReturnsReservationsList()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-RESV3", "SP Giữ Hàng 3", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-RESV3", "Kho Giữ Hàng 3", null, null, "Active", null, true));
        var req = new InvReservationCreateRequest(wh.Id, null, null, null, null, false, new List<InvReservationLineRequest>
        {
            new InvReservationLineRequest(sku.Id, 1m, null, null)
        });
        await _invStock.CreateReservationAsync(_tenant, _userAdmin, req);

        var list = await _invStock.ListReservationsAsync(_tenant);

        Assert.NotEmpty(list);
    }

    [Fact]
    public async Task UC_INV_033_ShipTransfer_AlreadyShipped_ThrowsException()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-ERR98", "SP Err 98", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var w1 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ERR1", "Kho Err 1", null, null, "Active", null, true));
        var w2 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ERR2", "Kho Err 2", null, null, "Active", null, true));
        var tr = await _invStock.CreateTransferAsync(_tenant, _userAdmin, new InvTransferCreateRequest(w1.Id, w2.Id, null));
        await _invStock.UpsertTransferLineAsync(_tenant, _userAdmin, tr.Id, new InvTransferLineRequest(null, sku.Id, 10m, null, null));
        var shipped = await _invStock.ShipTransferAsync(_tenant, _userAdmin, tr.Id);

        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.ShipTransferAsync(_tenant, _userAdmin, shipped.Id));
    }

    [Fact]
    public async Task UC_INV_037_CreateReservation_NonExistentWarehouse_ThrowsException()
    {
        var req = new InvReservationCreateRequest(Guid.NewGuid(), null, null, null, null, false, new List<InvReservationLineRequest>());

        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.CreateReservationAsync(_tenant, _userAdmin, req));
    }
}
