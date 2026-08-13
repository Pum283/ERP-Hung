namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_130: Báo cáo công nợ bán
// ────────────────────────────────────────────────────────────────────────────

public record CrmCustomerReceivableAgingDto(
    Guid CustomerId,
    string CustomerName,
    decimal CurrentDebtVnd,
    decimal Debt1To30DaysVnd,
    decimal Debt31To60DaysVnd,
    decimal Debt61To90DaysVnd,
    decimal DebtOver90DaysVnd,
    decimal TotalReceivableVnd
);

public record CrmSalesReceivablesAgingSummaryDto(
    decimal TotalReceivablesAmount,
    decimal TotalOverdueAmount,
    double OverdueRatePercent,
    IReadOnlyList<CrmCustomerReceivableAgingDto> CustomerAgingDetails
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_131: Xuất báo cáo định kỳ
// ────────────────────────────────────────────────────────────────────────────

public record CrmScheduleReportExportRequest(
    string ReportName,
    string ReportType, // ReceivablesAging | SalesForecast | CommissionSummary
    string ExportFormat, // PDF | Excel | CSV
    string Frequency, // Daily | Weekly | Monthly
    string RecipientEmails
);

public record CrmScheduledReportExportDto(
    Guid Id,
    string ReportName,
    string ReportType,
    string ExportFormat,
    string Frequency,
    string RecipientEmails,
    DateTimeOffset LastExportedAt
);
