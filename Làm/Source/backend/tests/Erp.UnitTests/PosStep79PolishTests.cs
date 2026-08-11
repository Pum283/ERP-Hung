using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pos;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 79:
///   UC_POS_022 — Nhập mã voucher (UpsertVoucherAsync & ApplyVoucherAsync)
///   UC_POS_024 — Giảm giá tay có quyền (RequestManualDiscountAsync & DecideManualDiscountAsync)
///   UC_POS_026 — Mở đơn / chọn khu vực (OpenSaleAsync & GetSaleDetailAsync)
///   UC_POS_027 — Thêm / sửa / xóa sản phẩm (UpsertSaleLineAsync & CancelSaleLineAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PosStep79PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosConfigService _configSvc;
    private readonly PosPromoService _promoSvc;
    private readonly PosSalesService _salesSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public PosStep79PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-step79-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin79", DisplayName = "Admin 79" });
        _db.InvWarehouses.Add(new InvWarehouse { TenantId = _tenant, Code = "WH-79", Name = "Kho 79", Status = "Active" });
        _db.SaveChanges();

        _configSvc = new PosConfigService(_db);
        _salesSvc = new PosSalesService(_db, null!, null!);
        _promoSvc = new PosPromoService(_db, _salesSvc);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(PosStoreDto store, PosShiftDto shift)> CreateOpenShiftAsync()
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-79", "CH POS 79", null, "Active", null, null));
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 1000000m, "Mở ca test 79"));
        return (store, shift);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_026: Mở đơn / chọn khu vực
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_026_OpenSale_ValidShift_CreatesSaleSuccessfully()
    {
        var (store, shift) = await CreateOpenShiftAsync();
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, "Bàn 05", "Tạo đơn test"));

        Assert.NotNull(sale);
        Assert.Equal(shift.Id, sale.ShiftId);
        Assert.Equal("Bàn 05", sale.AreaName);
        Assert.Equal("Open", sale.Status);
    }

    [Fact]
    public async Task UC_POS_026_OpenSale_ClosedShift_ThrowsAppException()
    {
        var (store, shift) = await CreateOpenShiftAsync();

        // Đóng ca
        await _salesSvc.CloseShiftAsync(_tenant, _userAdmin, shift.Id, new PosShiftCloseRequest(1000000m, "Đóng ca test"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, "Bàn 01", null)));

        Assert.Contains("Ca đã đóng", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_027: Thêm / sửa / xóa sản phẩm
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_027_UpsertSaleLine_ValidProduct_AddsLineAndRecalculatesTotals()
    {
        var (store, shift) = await CreateOpenShiftAsync();
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, null, null));
        var prod = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-79-1", "Trà Đào", "Ly", "Active", 1));

        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale.Id, new PosSaleLineUpsertRequest(null, prod.Id, prod.Code, prod.Name, 2, 35000m, 0m));

        var updatedSale = (await _salesSvc.GetSaleDetailAsync(_tenant, sale.Id)).Sale;
        Assert.NotNull(updatedSale);
        Assert.Equal(70000m, updatedSale.TotalAmount);
    }

    [Fact]
    public async Task UC_POS_027_CancelSaleLine_ExistingLine_RemovesLineAndRecalculates()
    {
        var (store, shift) = await CreateOpenShiftAsync();
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, null, null));
        var prod = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-79-2", "Cà Phê", "Ly", "Active", 1));
        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale.Id, new PosSaleLineUpsertRequest(null, prod.Id, prod.Code, prod.Name, 1, 30000m, 0m));

        var detail = await _salesSvc.GetSaleDetailAsync(_tenant, sale.Id);
        var lineId = detail.Lines[0].Id;

        await _salesSvc.CancelSaleLineAsync(_tenant, _userAdmin, sale.Id, lineId);

        var afterDelete = (await _salesSvc.GetSaleDetailAsync(_tenant, sale.Id)).Sale;
        Assert.Equal(0m, afterDelete.TotalAmount);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_024: Giảm giá tay có quyền
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_024_RequestManualDiscount_ValidPercent_UpdatesDiscountAmount()
    {
        var (store, shift) = await CreateOpenShiftAsync();
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, null, null));
        var prod = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-79-3", "Pizza Hải Sản", "Cái", "Active", 1));
        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale.Id, new PosSaleLineUpsertRequest(null, prod.Id, prod.Code, prod.Name, 1, 200000m, 0m));

        await _promoSvc.RequestManualDiscountAsync(_tenant, _userAdmin, sale.Id, new PosManualDiscountRequest("Percent", 10m, "Giảm 10% khách VIP"));
        var approved = await _promoSvc.DecideManualDiscountAsync(_tenant, _userAdmin, sale.Id, new PosDecideDiscountRequest(true, "Duyệt"));

        Assert.NotNull(approved);
        Assert.Equal(20000m, approved.DiscountAmount);
        Assert.Equal(180000m, approved.TotalAmount);
    }

    [Fact]
    public async Task UC_POS_024_RequestManualDiscount_InvalidDiscountType_ThrowsAppException()
    {
        var (store, shift) = await CreateOpenShiftAsync();
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _promoSvc.RequestManualDiscountAsync(_tenant, _userAdmin, sale.Id, new PosManualDiscountRequest("UnknownType", 10m, null)));

        Assert.Contains("Loại: Percent | Amount", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_022: Nhập mã voucher
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_022_ApplyVoucher_ValidVoucher_AppliesVoucherToSaleSuccessfully()
    {
        var (store, shift) = await CreateOpenShiftAsync();
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, null, null));
        var prod = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-79-VOU", "Combo Gà Rán", "Bộ", "Active", 1));
        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale.Id, new PosSaleLineUpsertRequest(null, prod.Id, prod.Code, prod.Name, 2, 100000m, 0m));

        var promo = await _promoSvc.UpsertPromotionAsync(_tenant, _userAdmin, new PosPromotionUpsertRequest(null, "PROMO-VOU", "KM Voucher 50k", "Amount", 50000m, 100000m, null, null, "Active", null));
        await _promoSvc.UpsertVoucherAsync(_tenant, _userAdmin, new PosVoucherUpsertRequest(null, "VOUCHER-50K", promo.Id, 10, "Active", null));

        var updatedSale = await _promoSvc.ApplyVoucherAsync(_tenant, _userAdmin, sale.Id, new PosApplyVoucherRequest("VOUCHER-50K"));

        Assert.NotNull(updatedSale);
        Assert.Equal("VOUCHER-50K", updatedSale.AppliedVoucherCode);
        Assert.Equal(50000m, updatedSale.DiscountAmount);
    }

    [Fact]
    public async Task UC_POS_022_ApplyVoucher_OrderBelowMinAmount_ThrowsAppException()
    {
        var (store, shift) = await CreateOpenShiftAsync();
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, null, null));
        var prod = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-79-MIN", "Nước Suối", "Chai", "Active", 1));
        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale.Id, new PosSaleLineUpsertRequest(null, prod.Id, prod.Code, prod.Name, 1, 10000m, 0m));

        var promo = await _promoSvc.UpsertPromotionAsync(_tenant, _userAdmin, new PosPromotionUpsertRequest(null, "PROMO-MIN", "KM Min 200k", "Amount", 30000m, 200000m, null, null, "Active", null));
        await _promoSvc.UpsertVoucherAsync(_tenant, _userAdmin, new PosVoucherUpsertRequest(null, "VOUCHER-MIN200", promo.Id, 10, "Active", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _promoSvc.ApplyVoucherAsync(_tenant, _userAdmin, sale.Id, new PosApplyVoucherRequest("VOUCHER-MIN200")));

        Assert.Contains("Đơn tối thiểu", ex.Message);
    }

    [Fact]
    public async Task UC_POS_022_ApplyVoucher_NonExistentVoucher_ThrowsAppException()
    {
        var (store, shift) = await CreateOpenShiftAsync();
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _promoSvc.ApplyVoucherAsync(_tenant, _userAdmin, sale.Id, new PosApplyVoucherRequest("INVALID-VOUCHER-CODE")));

        Assert.Contains("Mã voucher không hợp lệ", ex.Message);
    }
}
