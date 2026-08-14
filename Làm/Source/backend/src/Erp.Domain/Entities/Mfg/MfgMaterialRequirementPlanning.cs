using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Tính toán nhu cầu nguyên vật liệu MRP (UC_MFG_014).</summary>
public class MfgMaterialRequirementPlanning : TenantEntity
{
    public string MrpRunNumber { get; set; } = "";
    public Guid MaterialProductId { get; set; }
    public string MaterialProductCode { get; set; } = "";
    public string MaterialProductName { get; set; } = "";
    public decimal GrossRequirementQty { get; set; }
    public decimal CurrentStockOnHandQty { get; set; }
    public decimal ScheduledReceiptsPoQty { get; set; }
    public decimal NetRequirementQty { get; set; } // Nhu cầu thiếu hụt cần mua thêm = Gross - OnHand - Scheduled
    public decimal SuggestedPurchaseOrderQty { get; set; }
    public DateTimeOffset RequiredDate { get; set; } = DateTimeOffset.UtcNow.AddDays(7);
    public DateTimeOffset CalculatedAt { get; set; } = DateTimeOffset.UtcNow;
}
