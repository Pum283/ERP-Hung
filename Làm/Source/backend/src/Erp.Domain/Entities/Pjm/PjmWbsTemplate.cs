using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Mẫu hạng mục / WBS (UC_PJM_002).</summary>
public class PjmWbsTemplate : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
