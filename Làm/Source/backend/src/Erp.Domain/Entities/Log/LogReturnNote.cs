using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Phiếu hoàn hàng về kho (UC_LOG_027–029).</summary>
public class LogReturnNote : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid DeliveryOrderId { get; set; }
    public Guid WarehouseId { get; set; }
    /// <summary>Draft · Counted · Posted · Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public string? Reason { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset? CountedAt { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public Guid? InvStockDocId { get; set; }
    public string? InvStockDocCode { get; set; }
    public Guid CreatedByUserId { get; set; }
}

/// <summary>Dòng kiểm đếm hàng hoàn.</summary>
public class LogReturnLine : TenantEntity
{
    public Guid ReturnNoteId { get; set; }
    public Guid? DeliveryLineId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string Unit { get; set; } = "CAI";
    public decimal QtyExpected { get; set; }
    public decimal QtyCounted { get; set; }
    public decimal QtyAccepted { get; set; }
    public string? Note { get; set; }
}
