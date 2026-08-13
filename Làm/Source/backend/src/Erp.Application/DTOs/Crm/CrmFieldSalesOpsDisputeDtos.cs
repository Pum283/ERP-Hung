namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_097: AI gợi ý việc ưu tiên
// ────────────────────────────────────────────────────────────────────────────

public record CrmAiPriorityActionDto(
    Guid CustomerId,
    string CustomerName,
    string PriorityLevel, // High | Medium | Low
    string ActionTitle,
    string RecommendationReason,
    decimal ExpectedRevenuePotential
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_098: Dashboard doanh số field
// ────────────────────────────────────────────────────────────────────────────

public record CrmFieldSalesRevenueMetricsDto(
    decimal TotalFieldRevenue,
    int TotalStoreVisitsPlanned,
    int TotalStoreVisitsCompleted,
    double VisitCompletionRatePercent,
    int NewOrdersCreatedOnSite,
    decimal AverageOrderValue
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_102: Đối soát chứng từ đơn
// ────────────────────────────────────────────────────────────────────────────

public record CrmReconcileOrderDocumentRequest(
    Guid OrderId,
    string DocumentCode,
    string DocumentType, // VATInvoice | DeliveryNote | PaymentReceipt
    string ReconciliationStatus, // Pending | Matched | Discrepancy
    string Notes
);

public record CrmOrderDocumentReconciliationDto(
    Guid Id,
    Guid OrderId,
    string DocumentCode,
    string DocumentType,
    string ReconciliationStatus,
    string Notes,
    DateTimeOffset ReconciledAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_103: Xử lý khiếu nại đơn hàng
// ────────────────────────────────────────────────────────────────────────────

public record CrmCreateOrderComplaintRequest(
    Guid OrderId,
    Guid CustomerId,
    string ComplaintReason,
    string Severity // Low | Medium | High | Critical
);

public record CrmResolveComplaintRequest(
    Guid ComplaintId,
    string Status, // Resolved | Rejected
    string ResolutionNotes
);

public record CrmOrderComplaintDto(
    Guid Id,
    Guid OrderId,
    Guid CustomerId,
    string CustomerName,
    string ComplaintReason,
    string Severity,
    string Status,
    string ResolutionNotes,
    Guid? AssignedUserId,
    DateTimeOffset LoggedAt
);
