using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Báo giá (UC_CRM_067, 070–077).</summary>
public class CrmQuote : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid? OpportunityId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? PriceListId { get; set; }
    public DateTimeOffset QuoteDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ValidUntil { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    /// <summary>Draft · PendingDiscount · Sent · Accepted · Rejected · Expired · Converted</summary>
    public string Status { get; set; } = "Draft";
    /// <summary>None · Pending · Approved · Rejected</summary>
    public string DiscountApprovalStatus { get; set; } = "None";
    public int Version { get; set; } = 1;
    public DateTimeOffset? SentAt { get; set; }
    /// <summary>None · Email · Pdf</summary>
    public string SentChannel { get; set; } = "None";
    public Guid? OrderId { get; set; }
    public string? Note { get; set; }
}
