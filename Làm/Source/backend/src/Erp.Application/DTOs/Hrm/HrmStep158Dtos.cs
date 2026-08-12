namespace Erp.Application.DTOs.Hrm;

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_088: Import lịch ca Excel
// ────────────────────────────────────────────────────────────────────────────

public record HrmShiftImportItem(
    string EmployeeCode,
    string WorkShiftCode,
    DateOnly WorkDate,
    string? Note = null
);

public record HrmShiftImportError(
    int RowIndex,
    string EmployeeCode,
    string ErrorMessage
);

public record HrmShiftImportResult(
    int TotalProcessed,
    int SuccessCount,
    int FailedCount,
    IReadOnlyList<Guid> AssignedShiftIds,
    IReadOnlyList<HrmShiftImportError> Errors
);

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_124: Lập bảng phạt
// ────────────────────────────────────────────────────────────────────────────

public record PayrollPenaltyDto(
    Guid Id,
    Guid EmployeeId,
    string? EmployeeName,
    Guid? PayrollPeriodId,
    string Reason,
    string PenaltyType,
    decimal Amount,
    DateTimeOffset ViolationDate,
    string Status,
    string? ApprovedByNote,
    DateTimeOffset CreatedAt
);

public record PayrollPenaltyUpsertRequest(
    Guid EmployeeId,
    string Reason,
    string PenaltyType = "LateArrival",
    decimal Amount = 0m,
    DateTimeOffset? ViolationDate = null,
    string? ApprovedByNote = null
);

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_125: Áp dụng phạt vào kỳ lương
// ────────────────────────────────────────────────────────────────────────────

public record ApplyPenaltyToPayrollRequest(
    Guid PayrollPeriodId,
    IReadOnlyList<Guid> PenaltyIds
);

public record ApplyPenaltyToPayrollResult(
    Guid PayrollPeriodId,
    int TotalPenaltiesApplied,
    decimal TotalDeductionAmount,
    IReadOnlyList<Guid> UpdatedPenaltyIds
);

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_174: Đồng bộ bút toán lương sang FIN
// ────────────────────────────────────────────────────────────────────────────

public record PayrollFinSyncRequest(
    Guid PayrollPeriodId,
    string? JournalMemo = null
);

public record PayrollFinSyncResult(
    Guid PayrollPeriodId,
    string JournalEntryCode,
    decimal TotalGrossSalaryAmount,
    decimal TotalNetSalaryAmount,
    decimal TotalPenaltyDeductions,
    DateTimeOffset SyncTimestamp,
    bool IsBalanced
);
