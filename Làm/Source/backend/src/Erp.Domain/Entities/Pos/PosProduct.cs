using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Sản phẩm bán (UC_POS_010, 014, 015).</summary>
public class PosProduct : TenantEntity
{
    public Guid? CategoryId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Unit { get; set; }
    /// <summary>Active · Suspended</summary>
    public string Status { get; set; } = "Active";
    public int SortOrder { get; set; }
    public DateTimeOffset? SyncedAt { get; set; }
}
