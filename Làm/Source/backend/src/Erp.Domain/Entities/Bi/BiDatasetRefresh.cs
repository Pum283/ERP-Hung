using Erp.Domain.Base;

namespace Erp.Domain.Entities.Bi;

/// <summary>Nhật ký làm mới dataset (UC_BI_002).</summary>
public class BiDatasetRefresh : TenantEntity
{
    public Guid DatasetId { get; set; }
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishedAt { get; set; }
    /// <summary>Running · Succeeded · Failed</summary>
    public string Status { get; set; } = "Running";
    public int RowsAffected { get; set; }
    public string? Note { get; set; }
    public Guid StartedByUserId { get; set; }
}
