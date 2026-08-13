namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_004: Cấu hình máy in bếp/khu vực
// ────────────────────────────────────────────────────────────────────────────

public record PosSaveKitchenPrinterConfigRequest(
    string PrinterName,
    string Area, // Kitchen | Bar | Bakery | Cashier
    string ConnectionType, // LAN_IP | USB | Serial | Bluetooth
    string IpAddressOrPort,
    int PaperWidthMm,
    bool AutoCutPaper
);

public record PosKitchenPrinterConfigDto(
    Guid Id,
    string PrinterName,
    string Area,
    string ConnectionType,
    string IpAddressOrPort,
    int PaperWidthMm,
    bool AutoCutPaper,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_005: Cấu hình ngăn kéo tiền
// ────────────────────────────────────────────────────────────────────────────

public record PosSaveCashDrawerConfigRequest(
    string DrawerName,
    string TriggerMode, // PrinterKickout | DirectUSB | SerialRelay
    string OpenPulseCommandHex,
    bool AutoOpenOnCashPayment
);

public record PosCashDrawerConfigDto(
    Guid Id,
    string DrawerName,
    string TriggerMode,
    string OpenPulseCommandHex,
    bool AutoOpenOnCashPayment,
    bool IsActive
);
