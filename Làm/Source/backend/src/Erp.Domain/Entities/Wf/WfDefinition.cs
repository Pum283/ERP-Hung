using Erp.Domain.Base;

namespace Erp.Domain.Entities.Wf;

public class WfDefinition : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public string DocType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
