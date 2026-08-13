namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_006: Cấu hình thiết bị quét mã
// ────────────────────────────────────────────────────────────────────────────

public record PosSaveBarcodeScannerConfigRequest(
    string ScannerName,
    string ConnectionType, // USB_HID | USB_COM | Bluetooth | SerialRS232
    string PrefixKey,
    string SuffixKey,
    int ScanTimeoutMs
);

public record PosBarcodeScannerConfigDto(
    Guid Id,
    string ScannerName,
    string ConnectionType,
    string PrefixKey,
    string SuffixKey,
    int ScanTimeoutMs,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_008: Chế độ offline tạm & Đệm đồng bộ
// ────────────────────────────────────────────────────────────────────────────

public record PosOfflineSyncBufferDto(
    Guid BufferId,
    string PosTerminalCode,
    int OfflineOrdersCount,
    decimal OfflineRevenueTotalVnd,
    string SyncStatus, // Pending | Syncing | Synced | SyncError
    DateTimeOffset LastSyncAttemptAt
);

public record PosTriggerOfflineSyncRequest(
    string PosTerminalCode,
    bool ForceReSync
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_011 & UC_POS_013: Thuộc tính sản phẩm & Ảnh/Thứ tự hiển thị
// ────────────────────────────────────────────────────────────────────────────

public record PosSaveProductAttributeRequest(
    Guid ProductId,
    string AttributeName,
    string OptionValue,
    decimal ExtraPriceVnd,
    string ImageUrl,
    int DisplayOrder,
    bool IsDefault
);

public record PosProductAttributeModifierDto(
    Guid Id,
    Guid ProductId,
    string AttributeName,
    string OptionValue,
    decimal ExtraPriceVnd,
    string ImageUrl,
    int DisplayOrder,
    bool IsDefault
);
