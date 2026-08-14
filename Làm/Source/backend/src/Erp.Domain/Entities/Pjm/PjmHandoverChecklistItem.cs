using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Checklist bàn giao và đóng điện chạy thử dự án (UC_PJM_027).</summary>
public class PjmHandoverChecklistItem : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; } = "PRJ-2026-088";
    public string HandoverCriteriaName { get; set; } = "1. Bàn giao đầy đủ hồ sơ hoàn công và sơ đồ nguyên lý";
    public bool IsSatisfied { get; set; } = true;
    public string CustomerRepresentativeName { get; set; } = "Đại diện Chủ đầu tư FPT";
    public DateTimeOffset SignedAt { get; set; } = DateTimeOffset.UtcNow;
}
