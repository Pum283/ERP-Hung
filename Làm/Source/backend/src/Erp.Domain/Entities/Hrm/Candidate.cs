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
    /// <summary>Ghi chú sơ loại / lý do từ chối sơ loại (UC_HRM_059).</summary>
    public string? ScreeningNote { get; set; }
    /// <summary>Kết quả đề xuất đánh giá: Pass | Fail | Hold (UC_HRM_061).</summary>
    public string? EvalResult { get; set; }
    /// <summary>Ghi chú thư mời làm việc / lý do từ chối tuyển dụng (UC_HRM_062).</summary>
    public string? DecisionNote { get; set; }
}
