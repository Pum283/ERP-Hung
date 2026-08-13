using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPosScannerOfflineAttrProductService
{
    // UC_POS_006: Cấu hình thiết bị quét mã
    Task<PosBarcodeScannerConfigDto> SaveBarcodeScannerConfigAsync(Guid tenantId, PosSaveBarcodeScannerConfigRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PosBarcodeScannerConfigDto>> GetBarcodeScannerConfigsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_POS_008: Chế độ offline tạm
    Task<PosOfflineSyncBufferDto> GetOfflineSyncBufferStatusAsync(Guid tenantId, string terminalCode, CancellationToken ct = default);
    Task<PosOfflineSyncBufferDto> TriggerOfflineSyncAsync(Guid tenantId, PosTriggerOfflineSyncRequest req, CancellationToken ct = default);

    // UC_POS_011 & UC_POS_013: Thuộc tính sản phẩm & Ảnh/Thứ tự hiển thị
    Task<PosProductAttributeModifierDto> SaveProductAttributeAsync(Guid tenantId, PosSaveProductAttributeRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PosProductAttributeModifierDto>> GetProductAttributesAsync(Guid tenantId, Guid productId, CancellationToken ct = default);
}
