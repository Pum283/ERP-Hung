namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_FIN_011: Bút toán định kỳ / mẫu
// ────────────────────────────────────────────────────────────────────────────

public record FinCreateRecurringTemplateRequest(
    string TemplateCode,
    string TemplateName,
    string Frequency,
    decimal DefaultAmountVnd,
    string DebitAccountCode,
    string CreditAccountCode,
    bool IsActive
);

public record FinRecurringTemplateVoucherDto(
    Guid Id,
    string TemplateCode,
    string TemplateName,
    string Frequency,
    decimal DefaultAmountVnd,
    string DebitAccountCode,
    string CreditAccountCode,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FIN_017: Đính kèm chứng từ gốc
// ────────────────────────────────────────────────────────────────────────────

public record FinUploadVoucherAttachmentRequest(
    Guid JournalEntryId,
    string VoucherNumber,
    string AttachmentName,
    string FileUrl,
    string MimeType,
    long FileSizeBytes
);

public record FinOriginalVoucherAttachmentDto(
    Guid Id,
    Guid JournalEntryId,
    string VoucherNumber,
    string AttachmentName,
    string FileUrl,
    string MimeType,
    long FileSizeBytes,
    DateTimeOffset UploadedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FIN_021: Đề nghị tạm ứng / hoàn ứng
// ────────────────────────────────────────────────────────────────────────────

public record FinCreateAdvanceSettlementRequest(
    string EmployeeName,
    string Purpose,
    decimal AdvanceAmountVnd,
    decimal SettledAmountVnd,
    decimal RemainingRefundVnd
);

public record FinAdvanceSettlementRequestDto(
    Guid Id,
    string RequestNumber,
    string EmployeeName,
    string Purpose,
    decimal AdvanceAmountVnd,
    decimal SettledAmountVnd,
    decimal RemainingRefundVnd,
    string Status,
    DateTimeOffset CreatedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FIN_022: Kiểm kê quỹ
// ────────────────────────────────────────────────────────────────────────────

public record FinCreateVaultCountAuditRequest(
    string FundCode,
    string FundName,
    decimal BookBalanceVnd,
    decimal PhysicalCountVnd,
    decimal VarianceVnd,
    string AuditorName,
    string AuditConclusion
);

public record FinCashVaultCountAuditDto(
    Guid Id,
    string FundCode,
    string FundName,
    decimal BookBalanceVnd,
    decimal PhysicalCountVnd,
    decimal VarianceVnd,
    string AuditorName,
    string AuditConclusion,
    DateTimeOffset AuditDate
);
