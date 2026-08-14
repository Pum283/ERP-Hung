using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Định mức tỷ lệ hao hụt nguyên vật liệu trong BOM (UC_MFG_009).</summary>
public class MfgBomScrapAllowance : TenantEntity
{
    public Guid BomId { get; set; }
    public string BomCode { get; set; } = "";
    public Guid MaterialProductId { get; set; }
    public string MaterialProductCode { get; set; } = "";
    public string MaterialProductName { get; set; } = "";
    public decimal BaseNetQuantity { get; set; }
    public decimal ScrapAllowancePct { get; set; } = 3.5m; // Tỷ lệ hao hụt %
    public decimal GrossPlannedQuantity { get; set; } // Tổng định mức có dự phòng hao hụt
    public string Reason { get; set; } = "Bao bì rách, mạt kim loại phế phẩm khi cắt gọt";
}
