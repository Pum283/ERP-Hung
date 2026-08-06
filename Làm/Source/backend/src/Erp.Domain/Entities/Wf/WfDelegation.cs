using Erp.Domain.Base;

namespace Erp.Domain.Entities.Wf;

/// <summary>Ủy quyền duyệt tạm thời (UC_WF_032).</summary>
public class WfDelegation : TenantEntity
{
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    /// <summary>null = mọi module.</summary>
    public string? ModuleCode { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}
