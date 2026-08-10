using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class ImportExportJob : TenantEntity
{
    public string JobType { get; set; } = "";       // Import | Export
    public string EntityType { get; set; } = "";     // Users | Customers | Products …
    public string? Format { get; set; }              // Csv | Excel | Pdf
    public string Status { get; set; } = "Pending";  // Pending | Running | Completed | Failed
    public int RowCount { get; set; }
    public int ErrorCount { get; set; }
    public string? ErrorDetails { get; set; }
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid? ActorId { get; set; }
}
