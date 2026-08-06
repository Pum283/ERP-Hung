using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Loại dự án (UC_PJM_001).</summary>
public class PjmProjectType : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
