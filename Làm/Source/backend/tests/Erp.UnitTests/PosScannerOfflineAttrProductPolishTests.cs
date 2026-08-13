using Erp.Application.DTOs;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PosScannerOfflineAttrProductPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosScannerOfflineAttrProductService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    public PosScannerOfflineAttrProductPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-scanner-offline-attr-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TPOS179", Name = "Tenant POS 179" });
        _db.SaveChanges();

        _svc = new PosScannerOfflineAttrProductService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_006: Cấu hình thiết bị quét mã
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveBarcodeScannerConfig_SavesConfigSuccessfully()
    {
        var req = new PosSaveBarcodeScannerConfigRequest(
            "Đầu Quét Mã Honeywell 1950g",
            "USB_HID",
            "",
            "ENTER",
            300
        );

        var res = await _svc.SaveBarcodeScannerConfigAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Đầu Quét Mã Honeywell 1950g", res.ScannerName);
        Assert.True(res.IsActive);

        var list = await _svc.GetBarcodeScannerConfigsAsync(_tenant);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_008: Chế độ offline tạm & Đệm đồng bộ
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TriggerOfflineSync_SyncsBufferSuccessfully()
    {
        var status = await _svc.GetOfflineSyncBufferStatusAsync(_tenant, "POS-POS01");
        Assert.NotNull(status);
        Assert.Equal("POS-POS01", status.PosTerminalCode);

        var syncReq = new PosTriggerOfflineSyncRequest("POS-POS01", true);
        var syncRes = await _svc.TriggerOfflineSyncAsync(_tenant, syncReq);

        Assert.NotNull(syncRes);
        Assert.Equal("Synced", syncRes.SyncStatus);
        Assert.Equal(0, syncRes.OfflineOrdersCount);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_011 & UC_POS_013: Thuộc tính sản phẩm & Ảnh/Thứ tự hiển thị
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveProductAttribute_SavesAttributeAndImageDisplayOrder()
    {
        var req = new PosSaveProductAttributeRequest(
            _productId,
            "Size",
            "Size L (Lớn)",
            10000m,
            "/images/pos/size-l.png",
            1,
            true
        );

        var res = await _svc.SaveProductAttributeAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Size L (Lớn)", res.OptionValue);
        Assert.Equal(10000m, res.ExtraPriceVnd);

        var list = await _svc.GetProductAttributesAsync(_tenant, _productId);
        Assert.NotEmpty(list);
    }
}
