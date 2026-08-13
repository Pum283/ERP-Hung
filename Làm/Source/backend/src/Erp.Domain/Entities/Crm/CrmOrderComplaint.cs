using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Xử lý khiếu nại đơn hàng (UC_CRM_103).</summary>
public class CrmOrderComplaint : TenantEntity
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string ComplaintReason { get; set; } = "";
    /// <summary>Low | Medium | High | Critical</summary>
    public string Severity { get; set; } = "Medium";
    /// <summary>Open | InInvestigation | Resolved | Rejected</summary>
    public string Status { get; set; } = "Open";
    public string ResolutionNotes { get; set; } = "";
    public Guid? AssignedUserId { get; set; }
    public DateTimeOffset LoggedAt { get; set; } = DateTimeOffset.UtcNow;
}
