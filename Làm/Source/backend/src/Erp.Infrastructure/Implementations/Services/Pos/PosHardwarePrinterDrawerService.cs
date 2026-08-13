using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PosHardwarePrinterDrawerService : IPosHardwarePrinterDrawerService
{
    private readonly AppDbContext _db;

    public PosHardwarePrinterDrawerService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_004: Cấu hình máy in bếp/khu vực
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosKitchenPrinterConfigDto> SaveKitchenPrinterConfigAsync(Guid tenantId, PosSaveKitchenPrinterConfigRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.PrinterName) || string.IsNullOrWhiteSpace(req.IpAddressOrPort))
            throw new AppException("Tên máy in và địa chỉ IP / Cổng kết nối không được để trống.", 400);

        var cfg = new PosKitchenPrinterConfig
        {
            TenantId = tenantId,
            PrinterName = req.PrinterName,
            Area = req.Area ?? "Kitchen",
            ConnectionType = req.ConnectionType ?? "LAN_IP",
            IpAddressOrPort = req.IpAddressOrPort,
            PaperWidthMm = req.PaperWidthMm > 0 ? req.PaperWidthMm : 80,
            AutoCutPaper = req.AutoCutPaper,
            IsActive = true
        };

        _db.PosKitchenPrinterConfigs.Add(cfg);
        await _db.SaveChangesAsync(ct);

        return new PosKitchenPrinterConfigDto(
            cfg.Id,
            cfg.PrinterName,
            cfg.Area,
            cfg.ConnectionType,
            cfg.IpAddressOrPort,
            cfg.PaperWidthMm,
            cfg.AutoCutPaper,
            cfg.IsActive
        );
    }

    public async Task<IReadOnlyList<PosKitchenPrinterConfigDto>> GetKitchenPrinterConfigsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PosKitchenPrinterConfigs.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PosKitchenPrinterConfigDto>
            {
                new(Guid.NewGuid(), "Máy in Bếp Nóng", "Kitchen", "LAN_IP", "192.168.1.201", 80, true, true),
                new(Guid.NewGuid(), "Máy in Quầy Bar / Đồ uổng", "Bar", "LAN_IP", "192.168.1.202", 80, true, true)
            };
        }

        return list.Select(c => new PosKitchenPrinterConfigDto(
            c.Id,
            c.PrinterName,
            c.Area,
            c.ConnectionType,
            c.IpAddressOrPort,
            c.PaperWidthMm,
            c.AutoCutPaper,
            c.IsActive
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_005: Cấu hình ngăn kéo tiền
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosCashDrawerConfigDto> SaveCashDrawerConfigAsync(Guid tenantId, PosSaveCashDrawerConfigRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.DrawerName))
            throw new AppException("Tên thiết bị ngăn kéo tiền không được để trống.", 400);

        var cfg = new PosCashDrawerConfig
        {
            TenantId = tenantId,
            DrawerName = req.DrawerName,
            TriggerMode = req.TriggerMode ?? "PrinterKickout",
            OpenPulseCommandHex = req.OpenPulseCommandHex ?? "1B700019FA",
            AutoOpenOnCashPayment = req.AutoOpenOnCashPayment,
            IsActive = true
        };

        _db.PosCashDrawerConfigs.Add(cfg);
        await _db.SaveChangesAsync(ct);

        return new PosCashDrawerConfigDto(
            cfg.Id,
            cfg.DrawerName,
            cfg.TriggerMode,
            cfg.OpenPulseCommandHex,
            cfg.AutoOpenOnCashPayment,
            cfg.IsActive
        );
    }

    public async Task<IReadOnlyList<PosCashDrawerConfigDto>> GetCashDrawerConfigsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PosCashDrawerConfigs.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PosCashDrawerConfigDto>
            {
                new(Guid.NewGuid(), "Ngăn kéo tiền Quầy Thu Ngân 01", "PrinterKickout", "1B700019FA", true, true)
            };
        }

        return list.Select(c => new PosCashDrawerConfigDto(
            c.Id,
            c.DrawerName,
            c.TriggerMode,
            c.OpenPulseCommandHex,
            c.AutoOpenOnCashPayment,
            c.IsActive
        )).ToList();
    }
}
