using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class FileObject : TenantEntity
{
    public string StorageKey { get; set; } = "";
    public string FileName { get; set; } = "";
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    public Guid? FolderId { get; set; }
    public string? LinkedEntityType { get; set; }
    public Guid? LinkedEntityId { get; set; }
    /// <summary>UC_SYS_071 — Pending | Scanning | Clean | Infected | Skipped</summary>
    public string ScanStatus { get; set; } = "Pending";
    public DateTimeOffset? ScannedAt { get; set; }
    public string? ThreatName { get; set; }
}
