namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_006: Kỹ năng / chứng chỉ kỹ thuật viên
// ────────────────────────────────────────────────────────────────────────────

public record FsmCreateTechnicianSkillRequest(
    Guid TechnicianUserId,
    string TechnicianName,
    string SkillCode,
    string SkillName,
    string CertificationLevel,
    string CertificateNumber,
    DateTimeOffset IssuedDate,
    DateTimeOffset? ExpiryDate
);

public record FsmTechnicianSkillCertDto(
    Guid Id,
    Guid TechnicianUserId,
    string TechnicianName,
    string SkillCode,
    string SkillName,
    string CertificationLevel,
    string CertificateNumber,
    DateTimeOffset IssuedDate,
    DateTimeOffset? ExpiryDate,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_007: Vùng phụ trách
// ────────────────────────────────────────────────────────────────────────────

public record FsmCreateTerritoryCoverageRequest(
    string TerritoryCode,
    string TerritoryName,
    string ProvinceOrCity,
    string AssignedHubWarehouseCode,
    Guid LeadTechnicianUserId,
    string LeadTechnicianName
);

public record FsmTerritoryCoverageDto(
    Guid Id,
    string TerritoryCode,
    string TerritoryName,
    string ProvinceOrCity,
    string AssignedHubWarehouseCode,
    Guid LeadTechnicianUserId,
    string LeadTechnicianName,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_011: Cảnh báo hết hạn bảo hành
// ────────────────────────────────────────────────────────────────────────────

public record FsmWarrantyExpiryAlertDto(
    Guid Id,
    Guid AssetId,
    string SerialNumber,
    string ModelName,
    string CustomerName,
    DateTimeOffset WarrantyEndDate,
    int DaysRemaining,
    string AlertStatus,
    bool IsNotifiedToCustomer,
    DateTimeOffset AlertGeneratedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_012: Hợp đồng bảo trì định kỳ
// ────────────────────────────────────────────────────────────────────────────

public record FsmCreatePeriodicMaintenanceContractRequest(
    string ContractNumber,
    Guid CustomerId,
    string CustomerName,
    string ServiceLevelAgreement,
    int VisitsPerYear,
    decimal AnnualContractValueVnd,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate
);

public record FsmPeriodicMaintenanceContractDto(
    Guid Id,
    string ContractNumber,
    Guid CustomerId,
    string CustomerName,
    string ServiceLevelAgreement,
    int VisitsPerYear,
    decimal AnnualContractValueVnd,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string Status
);
