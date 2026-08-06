using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Lệnh giao hàng (UC_LOG_006+).</summary>
public class LogDeliveryOrder : TenantEntity
{
    public string Code { get; set; } = "";
    /// <summary>Mã đơn hàng nguồn (SO) — Cap-1 nhập tay.</summary>
    public string SourceOrderCode { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string? ShipAddress { get; set; }
    public string? Phone { get; set; }
    /// <summary>
    /// Draft · Confirmed · Picking · Ready · Dispatched · InTransit · Delivered · Failed · Cancelled · Returned
    /// </summary>
    public string Status { get; set; } = "Draft";
    public Guid? CarrierId { get; set; }
    public Guid? DriverUserId { get; set; }
    public string? DriverName { get; set; }
    /// <summary>Lệnh gốc khi tách đợt (UC_008).</summary>
    public Guid? ParentOrderId { get; set; }
    public int BatchNo { get; set; } = 1;
    public string? Note { get; set; }
    public string? FailureReason { get; set; }
    public string? WaybillNo { get; set; }
    public DateTimeOffset? WaybillPrintedAt { get; set; }
    public DateTimeOffset? PickedAt { get; set; }
    public DateTimeOffset? DispatchedAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    /// <summary>Hẹn giao / SLA giao (UC_LOG_034).</summary>
    public DateTimeOffset? PromisedAt { get; set; }
    public Guid CreatedByUserId { get; set; }

    /// <summary>COD Cap-2 (UC_LOG_020+).</summary>
    public bool IsCod { get; set; }
    public decimal CodAmount { get; set; }
    /// <summary>None · Pending · Collected · Remitted · Reconciled · Variance</summary>
    public string CodStatus { get; set; } = "None";
    public DateTimeOffset? CodDueAt { get; set; }
    public DateTimeOffset? CodCollectedAt { get; set; }
    public Guid? CodCollectedByUserId { get; set; }
    public Guid? CodHandoverId { get; set; }
    public string? CodNote { get; set; }
}
