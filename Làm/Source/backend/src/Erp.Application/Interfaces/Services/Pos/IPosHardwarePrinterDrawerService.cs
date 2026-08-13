using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPosHardwarePrinterDrawerService
{
    // UC_POS_004: Cấu hình máy in bếp/khu vực
    Task<PosKitchenPrinterConfigDto> SaveKitchenPrinterConfigAsync(Guid tenantId, PosSaveKitchenPrinterConfigRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PosKitchenPrinterConfigDto>> GetKitchenPrinterConfigsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_POS_005: Cấu hình ngăn kéo tiền
    Task<PosCashDrawerConfigDto> SaveCashDrawerConfigAsync(Guid tenantId, PosSaveCashDrawerConfigRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PosCashDrawerConfigDto>> GetCashDrawerConfigsAsync(Guid tenantId, CancellationToken ct = default);
}
