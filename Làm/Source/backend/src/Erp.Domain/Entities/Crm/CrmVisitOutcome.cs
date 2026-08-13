using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Ghi nhận mục đích – kết quả visit (UC_CRM_093).</summary>
public class CrmVisitOutcome : TenantEntity
{
    public Guid VisitPlanId { get; set; }
    public string Purpose { get; set; } = "Thăm đại lý định kỳ";
    /// <summary>Successful | Partial | FollowUpRequired | Unsuccessful</summary>
    public string OutcomeStatus { get; set; } = "Successful";
    public string SummaryNotes { get; set; } = "";
    public string ActionItems { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
}
