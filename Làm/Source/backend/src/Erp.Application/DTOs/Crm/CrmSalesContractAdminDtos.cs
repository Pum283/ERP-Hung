namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_105: Báo cáo năng suất Sales Admin
// ────────────────────────────────────────────────────────────────────────────

public record CrmSalesAdminProductivityDto(
    Guid SalesAdminUserId,
    string AdminName,
    int OrdersProcessedCount,
    int ContractsManagedCount,
    double AverageProcessingTimeHours,
    double AccuracyRatePercent
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_106: Quản lý hợp đồng bán
// ────────────────────────────────────────────────────────────────────────────

public record CrmCreateSalesContractRequest(
    string ContractCode,
    string Title,
    Guid CustomerId,
    decimal ContractValue,
    DateTime StartDate,
    DateTime EndDate,
    Guid? SalesAdminUserId
);

public record CrmSalesContractDto(
    Guid Id,
    string ContractCode,
    string Title,
    Guid CustomerId,
    string CustomerName,
    decimal ContractValue,
    DateTime StartDate,
    DateTime EndDate,
    string Status, // Active | ExpiringSoon | Expired | Renewed
    Guid? SalesAdminUserId,
    string RenewalNotes,
    int AttachmentCount
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_107: Đính kèm file hợp đồng
// ────────────────────────────────────────────────────────────────────────────

public record CrmAttachContractFileRequest(
    Guid ContractId,
    string FileName,
    string FilePath,
    long FileSize,
    string FileType
);

public record CrmContractAttachmentDto(
    Guid Id,
    Guid ContractId,
    string FileName,
    string FilePath,
    long FileSize,
    string FileType,
    DateTimeOffset UploadedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_108: Theo dõi hiệu lực / tái tục
// ────────────────────────────────────────────────────────────────────────────

public record CrmRenewContractRequest(
    Guid ContractId,
    DateTime NewEndDate,
    decimal NewContractValue,
    string RenewalNotes
);

public record CrmContractRenewalStatusDto(
    Guid ContractId,
    string ContractCode,
    string Title,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    int DaysRemaining,
    string RenewalNotes,
    DateTimeOffset RenewedAt
);
