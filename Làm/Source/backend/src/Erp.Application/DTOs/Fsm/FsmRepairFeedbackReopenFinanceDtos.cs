namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_026: Ghi nhận phí sửa chữa
// ────────────────────────────────────────────────────────────────────────────

public record FsmRecordRepairCostRequest(
    Guid TicketId,
    string TicketNumber,
    decimal LaborCostVnd,
    decimal PartsCostVnd,
    decimal TravelFeeVnd,
    bool IsCoveredByWarranty
);

public record FsmRepairCostRecordDto(
    Guid Id,
    Guid TicketId,
    string TicketNumber,
    decimal LaborCostVnd,
    decimal PartsCostVnd,
    decimal TravelFeeVnd,
    decimal TotalBillableAmountVnd,
    bool IsCoveredByWarranty,
    DateTimeOffset RecordedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_029: Đánh giá dịch vụ
// ────────────────────────────────────────────────────────────────────────────

public record FsmSubmitFeedbackRequest(
    Guid TicketId,
    string TicketNumber,
    int StarRating,
    string FeedbackComment,
    string CustomerSignerName
);

public record FsmCustomerServiceFeedbackDto(
    Guid Id,
    Guid TicketId,
    string TicketNumber,
    int StarRating,
    string FeedbackComment,
    string CustomerSignerName,
    DateTimeOffset SubmittedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_031: Tái mở ticket
// ────────────────────────────────────────────────────────────────────────────

public record FsmReopenTicketRequest(
    Guid TicketId,
    string TicketNumber,
    string ReopenReason,
    string ReopenedBy,
    string RootCauseClassification
);

public record FsmReopenedTicketLogDto(
    Guid Id,
    Guid TicketId,
    string TicketNumber,
    string ReopenReason,
    string ReopenedBy,
    string RootCauseClassification,
    DateTimeOffset ReopenedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_032: Chuyển chi phí sang FIN
// ────────────────────────────────────────────────────────────────────────────

public record FsmTransferCostToFinanceRequest(
    Guid TicketId,
    string TicketNumber,
    decimal TransferredAmountVnd,
    string DebitAccount,
    string CreditAccount
);

public record FsmFinanceCostTransferDto(
    Guid Id,
    string TransferVoucherNumber,
    Guid TicketId,
    string TicketNumber,
    decimal TransferredAmountVnd,
    string DebitAccount,
    string CreditAccount,
    string JournalEntryStatus,
    DateTimeOffset TransferredAt
);
