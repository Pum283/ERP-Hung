using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Phiếu đề xuất nhu cầu tuyển dụng (L2 · UC_HRM_047–053).</summary>
public class RecruitmentRequest : TenantEntity
{
    public string DocNo { get; set; } = "";
    public Guid JobTitleId { get; set; }
    public int Headcount { get; set; } = 1;
    public string Reason { get; set; } = "";
    public Guid OrgUnitId { get; set; }
    /// <summary>Draft | Pending | Approved | Rejected | Closed | Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public Guid? WfInstanceId { get; set; }
    public Guid RequestedByUserId { get; set; }
}
