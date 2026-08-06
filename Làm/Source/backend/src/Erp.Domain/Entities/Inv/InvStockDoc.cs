using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Phiếu nhập/xuất kho (UC_INV_017–030).</summary>
public class InvStockDoc : TenantEntity
{
    public string Code { get; set; } = "";
    /// <summary>Receipt · Issue</summary>
    public string DocType { get; set; } = "Receipt";
    /// <summary>Purchase · Adjustment · TransferIn · Internal · TransferOut · Sales · Production</summary>
    public string SourceType { get; set; } = "Adjustment";
    public Guid WarehouseId { get; set; }
    /// <summary>Draft · Posted · Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public string? RefModule { get; set; }
    public Guid? RefId { get; set; }
    public string? RefCode { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public string? Note { get; set; }
}
