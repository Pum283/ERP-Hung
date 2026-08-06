using Erp.Domain.Base;

namespace Erp.Domain.Entities.Bi;

/// <summary>Lần chạy báo cáo + xuất (UC_BI_014, 016).</summary>
public class BiReportRun : TenantEntity
{
    public Guid ReportId { get; set; }
    public DateTimeOffset RunAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid RunByUserId { get; set; }
    /// <summary>JSON bộ lọc đã áp dụng</summary>
    public string? FilterJson { get; set; }
    /// <summary>Succeeded · Failed</summary>
    public string Status { get; set; } = "Succeeded";
    public int RowCount { get; set; }
    /// <summary>None · Excel · Pdf</summary>
    public string ExportFormat { get; set; } = "None";
    public string? ExportFileName { get; set; }
    public string? ResultPreviewJson { get; set; }
    public string? Note { get; set; }
}
