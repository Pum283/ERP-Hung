namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_093: Ghi nhận mục đích – kết quả visit
// ────────────────────────────────────────────────────────────────────────────

public record CrmRecordVisitOutcomeRequest(
    Guid VisitPlanId,
    string Purpose,
    string OutcomeStatus, // Successful | Partial | FollowUpRequired | Unsuccessful
    string SummaryNotes,
    string ActionItems
);

public record CrmVisitOutcomeDto(
    Guid Id,
    Guid VisitPlanId,
    string Purpose,
    string OutcomeStatus,
    string SummaryNotes,
    string ActionItems,
    DateTimeOffset RecordedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_094: Ghi nhận nhu cầu khách hàng
// ────────────────────────────────────────────────────────────────────────────

public record CrmRecordCustomerDemandRequest(
    Guid VisitPlanId,
    Guid CustomerId,
    string ProductInterestCategory,
    int EstimatedQuantity,
    string Urgency, // Low | Medium | High
    string CompetitorInfo,
    string CustomerFeedback
);

public record CrmVisitDemandDto(
    Guid Id,
    Guid VisitPlanId,
    Guid CustomerId,
    string ProductInterestCategory,
    int EstimatedQuantity,
    string Urgency,
    string CompetitorInfo,
    string CustomerFeedback,
    DateTimeOffset LoggedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_095: Đặt hàng tại điểm thăm
// ────────────────────────────────────────────────────────────────────────────

public record CrmOnSiteOrderItemRequest(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);

public record CrmCreateOnSiteOrderRequest(
    Guid VisitPlanId,
    Guid CustomerId,
    List<CrmOnSiteOrderItemRequest> Items,
    string Note
);

public record CrmOnSiteOrderDto(
    Guid OrderId,
    string OrderCode,
    Guid CustomerId,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTimeOffset CreatedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_096: Xem lịch sử visit
// ────────────────────────────────────────────────────────────────────────────

public record CrmVisitHistoryLogDto(
    Guid VisitPlanId,
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
    string? OutcomeStatus,
    string? SummaryNotes
);
