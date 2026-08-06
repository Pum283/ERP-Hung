using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Hồ sơ ứng viên gắn tin tuyển (L2 · UC_HRM_056+).</summary>
public class Candidate : TenantEntity
{
    public Guid JobPostingId { get; set; }
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? CvStorageKey { get; set; }
    /// <summary>New | Screening | Evaluating | Accepted | Rejected</summary>
    public string PipelineStatus { get; set; } = "New";
    public Guid? EvalOrgUnitId { get; set; }
    public int? EvalScore { get; set; }
    public string? EvalComment { get; set; }
    public string? CareNotes { get; set; }
    public Guid? ConvertedEmployeeId { get; set; }
}
