using Erp.Domain.Base;

namespace Erp.Domain.Entities.Wf;

public class WfNode : TenantEntity
{
    public Guid DefinitionVersionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NodeType { get; set; } = "Approve";
    public int SortOrder { get; set; }
}
