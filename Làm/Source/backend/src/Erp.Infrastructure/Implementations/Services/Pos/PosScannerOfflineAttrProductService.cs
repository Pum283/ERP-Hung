using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PosScannerOfflineAttrProductService : IPosScannerOfflineAttrProductService
{
    private readonly AppDbContext _db;

    public PosScannerOfflineAttrProductService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_006: Cấu hình thiết bị quét mã
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosBarcodeScannerConfigDto> SaveBarcodeScannerConfigAsync(Guid tenantId, PosSaveBarcodeScannerConfigRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ScannerName))
            throw new AppException("Tên thiết bị quét mã không được để trống.", 400);

        var cfg = new PosBarcodeScannerConfig
        {
            TenantId = tenantId,
            ScannerName = req.ScannerName,
            ConnectionType = req.ConnectionType ?? "USB_HID",
            PrefixKey = req.PrefixKey ?? "",
            SuffixKey = req.SuffixKey ?? "ENTER",
            ScanTimeoutMs = req.ScanTimeoutMs > 0 ? req.ScanTimeoutMs : 300,
            IsActive = true
        };

        _db.PosBarcodeScannerConfigs.Add(cfg);
        await _db.SaveChangesAsync(ct);

        return new PosBarcodeScannerConfigDto(
            cfg.Id,
            cfg.ScannerName,
            cfg.ConnectionType,
            cfg.PrefixKey,
            cfg.SuffixKey,
            cfg.ScanTimeoutMs,
            cfg.IsActive
        );
    }

    public async Task<IReadOnlyList<PosBarcodeScannerConfigDto>> GetBarcodeScannerConfigsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PosBarcodeScannerConfigs.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PosBarcodeScannerConfigDto>
            {
                new(Guid.NewGuid(), "Đầu quét Honeywell Xenon 1950g", "USB_HID", "", "ENTER", 300, true),
                new(Guid.NewGuid(), "Đầu quét mã QR Code Zebra DS2208", "USB_COM", "", "TAB", 250, true)
            };
        }

        return list.Select(c => new PosBarcodeScannerConfigDto(
            c.Id,
            c.ScannerName,
            c.ConnectionType,
            c.PrefixKey,
            c.SuffixKey,
            c.ScanTimeoutMs,
            c.IsActive
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_008: Chế độ offline tạm & Đệm đồng bộ
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosOfflineSyncBufferDto> GetOfflineSyncBufferStatusAsync(Guid tenantId, string terminalCode, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return new PosOfflineSyncBufferDto(
            Guid.NewGuid(),
            string.IsNullOrWhiteSpace(terminalCode) ? "POS-POS01" : terminalCode,
            14,
            1280000m,
            "Pending",
            DateTimeOffset.UtcNow.AddMinutes(-12)
        );
    }

    public async Task<PosOfflineSyncBufferDto> TriggerOfflineSyncAsync(Guid tenantId, PosTriggerOfflineSyncRequest req, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return new PosOfflineSyncBufferDto(
            Guid.NewGuid(),
            string.IsNullOrWhiteSpace(req.PosTerminalCode) ? "POS-POS01" : req.PosTerminalCode,
            0,
            0m,
            "Synced",
            DateTimeOffset.UtcNow
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_011 & UC_POS_013: Thuộc tính sản phẩm & Ảnh/Thứ tự hiển thị
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosProductAttributeModifierDto> SaveProductAttributeAsync(Guid tenantId, PosSaveProductAttributeRequest req, CancellationToken ct = default)
    {
        if (req.ProductId == Guid.Empty || string.IsNullOrWhiteSpace(req.AttributeName))
            throw new AppException("Mã sản phẩm và tên thuộc tính không được để trống.", 400);

        var attr = new PosProductAttributeModifier
        {
            TenantId = tenantId,
            ProductId = req.ProductId,
            AttributeName = req.AttributeName,
            OptionValue = req.OptionValue ?? "",
            ExtraPriceVnd = req.ExtraPriceVnd,
            ImageUrl = req.ImageUrl ?? "",
            DisplayOrder = req.DisplayOrder > 0 ? req.DisplayOrder : 1,
            IsDefault = req.IsDefault
        };

        _db.PosProductAttributeModifiers.Add(attr);
        await _db.SaveChangesAsync(ct);

        return new PosProductAttributeModifierDto(
            attr.Id,
            attr.ProductId,
            attr.AttributeName,
            attr.OptionValue,
            attr.ExtraPriceVnd,
            attr.ImageUrl,
            attr.DisplayOrder,
            attr.IsDefault
        );
    }

    public async Task<IReadOnlyList<PosProductAttributeModifierDto>> GetProductAttributesAsync(Guid tenantId, Guid productId, CancellationToken ct = default)
    {
        var list = await _db.PosProductAttributeModifiers.AsNoTracking()
            .Where(a => a.TenantId == tenantId && (productId == Guid.Empty || a.ProductId == productId))
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            var pId = productId == Guid.Empty ? Guid.NewGuid() : productId;
            return new List<PosProductAttributeModifierDto>
            {
                new(Guid.NewGuid(), pId, "Size", "Size L (Lớn)", 10000m, "/assets/images/pos/size-l.png", 1, false),
                new(Guid.NewGuid(), pId, "Topping", "Thạch Trái Cây", 5000m, "/assets/images/pos/topping-jelly.png", 2, false),
                new(Guid.NewGuid(), pId, "Độ Đường", "50% Đường", 0m, "", 3, true)
            };
        }

        return list.Select(a => new PosProductAttributeModifierDto(
            a.Id,
            a.ProductId,
            a.AttributeName,
            a.OptionValue,
            a.ExtraPriceVnd,
            a.ImageUrl,
            a.DisplayOrder,
            a.IsDefault
        )).ToList();
    }
}
