namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_089: Phân vùng / tuyến bán hàng
// ────────────────────────────────────────────────────────────────────────────

public record CrmCreateTerritoryRequest(
    string TerritoryCode,
    string TerritoryName,
    string Region,
    string VisitFrequency,
    Guid? AssignedSalespersonId
);

public record CrmTerritoryDto(
    Guid Id,
    string TerritoryCode,
    string TerritoryName,
    string Region,
    string VisitFrequency,
    Guid? AssignedSalespersonId,
    string SalespersonName,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_090: Phân loại tần suất visit
// ────────────────────────────────────────────────────────────────────────────

public record CrmClassifyFrequencyRequest(
    Guid TerritoryId,
    string VisitFrequency // Weekly | BiWeekly | Monthly
);

public record CrmVisitFrequencyDto(
    Guid TerritoryId,
    string TerritoryName,
    string VisitFrequency,
    DateTimeOffset UpdatedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_091: Lập kế hoạch visit
// ────────────────────────────────────────────────────────────────────────────

public record CrmCreateVisitPlanRequest(
    Guid TerritoryId,
    Guid CustomerId,
    Guid SalespersonId,
    DateTime PlannedDate,
    string Notes
);

public record CrmVisitPlanDto(
    Guid Id,
    Guid TerritoryId,
    string TerritoryName,
    Guid CustomerId,
    string CustomerName,
    Guid SalespersonId,
    string SalespersonName,
    DateTime PlannedDate,
    string Status,
    string? CheckInGps,
    DateTimeOffset? CheckInTime,
    string? CheckOutGps,
    DateTimeOffset? CheckOutTime,
    string Notes
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_092: Check-in / check-out GPS
// ────────────────────────────────────────────────────────────────────────────

public record CrmGpsCheckInRequest(
    Guid VisitPlanId,
    string GpsCoordinates // "10.7769,106.7009"
);

public record CrmGpsCheckOutRequest(
    Guid VisitPlanId,
    string GpsCoordinates
);

public record CrmGpsCheckResultDto(
    Guid VisitPlanId,
    string Status,
    string? CheckInGps,
    DateTimeOffset? CheckInTime,
    string? CheckOutGps,
    DateTimeOffset? CheckOutTime
);
