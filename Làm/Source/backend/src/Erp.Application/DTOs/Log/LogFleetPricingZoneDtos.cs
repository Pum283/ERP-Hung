namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_LOG_002: Danh mục tài xế / xe
// ────────────────────────────────────────────────────────────────────────────

public record LogCreateDriverVehicleRequest(
    string DriverName,
    string PhoneNumber,
    string DriverLicenseNumber,
    string VehiclePlateNumber,
    string VehicleType,
    decimal MaxPayloadKg
);

public record LogDriverVehicleDto(
    Guid Id,
    string DriverName,
    string PhoneNumber,
    string DriverLicenseNumber,
    string VehiclePlateNumber,
    string VehicleType,
    decimal MaxPayloadKg,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LOG_003: Bảng giá cước vận chuyển
// ────────────────────────────────────────────────────────────────────────────

public record LogCreateFreightPricingRateRequest(
    string RateCode,
    string VehicleType,
    decimal BasePriceVnd,
    decimal PricePerKmFirst10Km,
    decimal PricePerKmAfter10Km,
    decimal LoadingUnloadingFeeVnd
);

public record LogFreightPricingRateDto(
    Guid Id,
    string RateCode,
    string VehicleType,
    decimal BasePriceVnd,
    decimal PricePerKmFirst10Km,
    decimal PricePerKmAfter10Km,
    decimal LoadingUnloadingFeeVnd,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LOG_004: Cấu hình khu vực giao
// ────────────────────────────────────────────────────────────────────────────

public record LogCreateDeliveryZoneConfigRequest(
    string ZoneCode,
    string ZoneName,
    string CityProvince,
    IReadOnlyList<string> DistrictCoverageList,
    int EstimatedTransitHours
);

public record LogDeliveryZoneConfigDto(
    Guid Id,
    string ZoneCode,
    string ZoneName,
    string CityProvince,
    IReadOnlyList<string> DistrictCoverageList,
    int EstimatedTransitHours,
    bool IsActive
);
