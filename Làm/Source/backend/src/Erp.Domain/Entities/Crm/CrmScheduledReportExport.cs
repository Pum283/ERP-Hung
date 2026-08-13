using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Xuất báo cáo định kỳ CRM (UC_CRM_131).</summary>
public class CrmScheduledReportExport : TenantEntity
{
    public string ReportName { get; set; } = "";
    /// <summary>ReceivablesAging | SalesForecast | CommissionSummary</summary>
    public string ReportType { get; set; } = "ReceivablesAging";
    /// <summary>PDF | Excel | CSV</summary>
    public string ExportFormat { get; set; } = "PDF";
    /// <summary>Daily | Weekly | Monthly</summary>
    public string Frequency { get; set; } = "Monthly";
    public string RecipientEmails { get; set; } = "";
    public DateTimeOffset LastExportedAt { get; set; } = DateTimeOffset.UtcNow;
}
