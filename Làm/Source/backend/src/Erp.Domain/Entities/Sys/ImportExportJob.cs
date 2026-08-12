using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class ImportExportJob : TenantEntity
{
    public string JobType { get; set; } = "";       // Import | Export | BulkExport
    public string EntityType { get; set; } = "";     // Users | Customers | Products … (bulk: comma-separated)
    public string? Format { get; set; }              // Csv | Excel | Pdf
    public string Status { get; set; } = "Pending";  // Pending | Running | Completed | Failed
    public int RowCount { get; set; }
    public int ErrorCount { get; set; }
    public string? ErrorDetails { get; set; }
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid? ActorId { get; set; }
    /// <summary>UC_SYS_077 — nội dung kết quả (CSV/PDF stub) để tải lại.</summary>
    public string? ResultContent { get; set; }
    public string? ResultFileName { get; set; }
    public string? ResultContentType { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}
