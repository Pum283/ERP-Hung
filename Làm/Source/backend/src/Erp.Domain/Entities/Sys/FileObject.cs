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
}
