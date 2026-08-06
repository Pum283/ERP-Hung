using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Thành viên dự án (UC_PJM_008).</summary>
public class PjmProjectMember : TenantEntity
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>PM · Member · Viewer</summary>
    public string Role { get; set; } = "Member";
    public bool IsActive { get; set; } = true;
    /// <summary>% phân bổ nguồn lực (UC_PJM_019).</summary>
    public decimal AllocationPct { get; set; } = 100;
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }
}
