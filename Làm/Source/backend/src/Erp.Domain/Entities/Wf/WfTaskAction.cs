using Erp.Domain.Base;

namespace Erp.Domain.Entities.Wf;

public class WfTaskAction : TenantEntity
{
    public Guid TaskId { get; set; }
    public Guid ActorUserId { get; set; }
    public string Action { get; set; } = "Approve";
    public string? Comment { get; set; }
}
