using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Thực thể hàng đợi POS offline & gợi ý bán kèm (UC_POS_008, UC_POS_041).</summary>
public class PosOfflineQueue : TenantEntity
{
    public string StoreCode { get; set; } = string.Empty;
    public string TransactionPayloadJson { get; set; } = string.Empty;
    public string SyncStatus { get; set; } = "Pending"; // Pending | Synced | Failed
    public string SuggestedCrossSellItemsJson { get; set; } = string.Empty;
}
