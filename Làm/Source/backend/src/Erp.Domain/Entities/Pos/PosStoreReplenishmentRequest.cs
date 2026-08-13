using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Đề nghị nhập bổ sung hàng hóa tại điểm bán POS (UC_POS_056).</summary>
public class PosStoreReplenishmentRequest : TenantEntity
{
    public string RequestNumber { get; set; } = "";
    public string StoreCode { get; set; } = "";
    public string ItemsJson { get; set; } = "[]";
    public string Priority { get; set; } = "Normal"; // Normal | Urgent
    public string Status { get; set; } = "Submitted"; // Submitted | Approved | Dispatched | Received
    public Guid RequestedByUserId { get; set; }
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
}
