using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Mã voucher (UC_CRM_034, 035).</summary>
public class CrmVoucher : TenantEntity
{
    public Guid PromotionId { get; set; }
    public string VoucherCode { get; set; } = "";
    /// <summary>Active · Used · Expired · Cancelled</summary>
    public string Status { get; set; } = "Active";
    public DateTimeOffset? ExpiresAt { get; set; }
    public int UsageCount { get; set; }
    public int MaxUsage { get; set; } = 1;
    public Guid? AssignedCustomerId { get; set; }
}
