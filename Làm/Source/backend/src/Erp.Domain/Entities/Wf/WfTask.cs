using Erp.Domain.Base;

namespace Erp.Domain.Entities.Wf;

public class WfTask : TenantEntity
{
    public Guid InstanceId { get; set; }
    public Guid NodeId { get; set; }
    public Guid? AssigneeUserId { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTimeOffset? DueAt { get; set; }
}
