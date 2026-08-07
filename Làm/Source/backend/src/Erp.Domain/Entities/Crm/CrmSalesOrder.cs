using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Đơn hàng bán (UC_CRM_077, 079–088).</summary>
public class CrmSalesOrder : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid? QuoteId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? OwnerUserId { get; set; }
    public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>Draft · Confirmed · Holding · Released · Cancelled · Delivered</summary>
    public string Status { get; set; } = "Draft";
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    /// <summary>None · Held · Released</summary>
    public string StockHoldStatus { get; set; } = "None";
    /// <summary>None · Pushed · Failed</summary>
    public string WarehousePushStatus { get; set; } = "None";
    public string? CancelReason { get; set; }
    public string? ReturnReason { get; set; }
    public Guid? ContractId { get; set; }
    public string? Note { get; set; }
}
