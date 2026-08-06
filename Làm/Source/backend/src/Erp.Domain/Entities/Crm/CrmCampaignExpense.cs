using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Chi phí chiến dịch marketing (UC_CRM_019).</summary>
public class CrmCampaignExpense : TenantEntity
{
    public Guid CampaignId { get; set; }
    public string ExpenseType { get; set; } = "";  // Ads · Media · Event · Agency · Other
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset ExpenseDate { get; set; } = DateTimeOffset.UtcNow;
    public string? InvoiceRef { get; set; }
}
