using Erp.Domain.Base;

namespace Erp.Domain.Entities.Wf;

/// <summary>Task/ticket vận hành (khác WfTask phê duyệt).</summary>
public class WorkItem : TenantEntity
{
    public string Kind { get; set; } = "Task";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? AssigneeUserId { get; set; }
    public Guid? ReporterUserId { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Normal";
}
