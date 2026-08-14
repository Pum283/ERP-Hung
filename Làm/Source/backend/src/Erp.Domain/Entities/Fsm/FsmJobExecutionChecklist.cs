using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Checklist thực hiện công việc hiện trường của KTV (UC_FSM_021).</summary>
public class FsmJobExecutionChecklist : TenantEntity
{
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = "";
    public string StepDescription { get; set; } = "1. Kiểm tra nguồn điện và áp suất gas";
    public bool IsMandatory { get; set; } = true;
    public bool IsCompleted { get; set; } = false;
    public string CompletedByTechnicianName { get; set; } = "";
    public DateTimeOffset? CompletedAt { get; set; }
}
