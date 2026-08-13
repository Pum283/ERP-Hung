using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Phiếu kiểm kê nhanh kho tại cửa hàng POS (UC_POS_058).</summary>
public class PosStoreQuickAudit : TenantEntity
{
    public string AuditCode { get; set; } = "";
    public string StoreCode { get; set; } = "";
    public string AuditDetailsJson { get; set; } = "[]";
    public int TotalItemsAudited { get; set; }
    public int DiscrepancyCount { get; set; }
    public Guid AuditedByUserId { get; set; }
    public DateTimeOffset AuditedAt { get; set; } = DateTimeOffset.UtcNow;
}
