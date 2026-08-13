using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Kỳ tính hoa hồng & phê duyệt đồng bộ (UC_CRM_121, UC_CRM_122, UC_CRM_123).</summary>
public class CrmCommissionPeriod : TenantEntity
{
    public string PeriodCode { get; set; } = "";
    public string PeriodName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalCommissionAmount { get; set; }
    /// <summary>Draft | Calculated | Approved | SyncedToHrmFin</summary>
    public string Status { get; set; } = "Draft";
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? SyncedAt { get; set; }
}
