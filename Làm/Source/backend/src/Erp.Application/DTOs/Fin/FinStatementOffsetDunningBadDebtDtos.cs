namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_FIN_028: Import sao kê
// ────────────────────────────────────────────────────────────────────────────

public record FinImportBankStatementRequest(
    string BankAccountNumber,
    string BankName,
    string ImportedFileName,
    int TotalTransactionsCount,
    decimal TotalCreditAmountVnd,
    decimal TotalDebitAmountVnd
);

public record FinBankStatementImportRecordDto(
    Guid Id,
    string BankAccountNumber,
    string BankName,
    string ImportedFileName,
    int TotalTransactionsCount,
    decimal TotalCreditAmountVnd,
    decimal TotalDebitAmountVnd,
    string ImportStatus,
    DateTimeOffset ImportedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FIN_033: Bù trừ công nợ
// ────────────────────────────────────────────────────────────────────────────

public record FinCreateArApOffsetRequest(
    string PartnerName,
    decimal ArAmountToOffsetVnd,
    decimal ApAmountToOffsetVnd,
    decimal NetSettlementAmountVnd,
    string OffsetJournalVoucherNo
);

public record FinArApOffsetSettlementDto(
    Guid Id,
    string SettlementNumber,
    string PartnerName,
    decimal ArAmountToOffsetVnd,
    decimal ApAmountToOffsetVnd,
    decimal NetSettlementAmountVnd,
    string OffsetJournalVoucherNo,
    string Status,
    DateTimeOffset SettledAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FIN_034: Nhắc nợ tự động
// ────────────────────────────────────────────────────────────────────────────

public record FinSendDunningNotificationRequest(
    string InvoiceNumber,
    string CustomerName,
    decimal OverdueAmountVnd,
    int OverdueDays,
    string DunningLevel,
    string DeliveryChannel,
    string RecipientContact
);

public record FinDebtDunningNotificationDto(
    Guid Id,
    string InvoiceNumber,
    string CustomerName,
    decimal OverdueAmountVnd,
    int OverdueDays,
    string DunningLevel,
    string DeliveryChannel,
    string RecipientContact,
    DateTimeOffset SentAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FIN_037: Xử lý nợ khó đòi
// ────────────────────────────────────────────────────────────────────────────

public record FinProcessBadDebtRequest(
    string CustomerName,
    decimal OriginalDebtAmountVnd,
    decimal ProvisionAmountVnd,
    double ProvisionRatePct,
    string ActionType,
    string CouncilApprovalDoc
);

public record FinBadDebtProvisionWriteOffDto(
    Guid Id,
    string DebtRecordNumber,
    string CustomerName,
    decimal OriginalDebtAmountVnd,
    decimal ProvisionAmountVnd,
    double ProvisionRatePct,
    string ActionType,
    string CouncilApprovalDoc,
    DateTimeOffset ActionDate
);
