using Erp.Domain.Base;

namespace Erp.Domain.Entities.Wf;

public class WfInstance : TenantEntity
{
    public Guid DefinitionVersionId { get; set; }
    public string SourceModule { get; set; } = string.Empty;
    public string SourceDocType { get; set; } = string.Empty;
    public Guid SourceDocId { get; set; }
    public string Status { get; set; } = "Running";
    public Guid? CurrentNodeId { get; set; }
}
