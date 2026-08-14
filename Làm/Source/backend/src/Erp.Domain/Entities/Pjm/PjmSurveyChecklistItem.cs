using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Checklist khảo sát hiện trường trước thi công dự án (UC_PJM_025).</summary>
public class PjmSurveyChecklistItem : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; } = "PRJ-2026-088";
    public string SurveyItemTitle { get; set; } = "1. Kiểm tra tải trọng mặt sàn đặt máy biến áp";
    public string TechnicalStandard { get; set; } = "Chịu tải tối thiểu 1.500 kg/m2";
    public bool IsSatisfied { get; set; } = true;
    public string InspectorNotes { get; set; } = "Sàn bê tông cốt thép đạt yêu cầu bản vẽ thiết kế";
    public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;
}
