using Erp.Domain.Base;

namespace Erp.Domain.Entities.Wf;

public class WfDefinitionVersion : TenantEntity
{
    public Guid DefinitionId { get; set; }
    public int VersionNo { get; set; }
    public bool IsPublished { get; set; }
}
