using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Ticket dịch vụ hiện trường (UC_FSM_013–015, 017 · Cap-2 018/022/027/028/030).</summary>
public class FsmTicket : TenantEntity
{
    public string Code { get; set; } = "";
    /// <summary>Phone · Email · Portal · WalkIn · Other</summary>
    public string Channel { get; set; } = "Phone";
    public string Subject { get; set; } = "";
    public string? Description { get; set; }
    public string CustomerName { get; set; } = "";
    public string? CustomerPhone { get; set; }
    public Guid? ServiceTypeId { get; set; }
    public Guid? FaultCodeId { get; set; }
    public Guid? AssetId { get; set; }
    public Guid? SlaPolicyId { get; set; }
    /// <summary>Low · Normal · High · Critical</summary>
    public string Priority { get; set; } = "Normal";
    /// <summary>Open · Assigned · InProgress · Escalated · Resolved · Closed · Cancelled</summary>
    public string Status { get; set; } = "Open";
    public Guid? AssignedTechUserId { get; set; }
    public string? AssignedTechName { get; set; }
    public Guid? PreviousTechUserId { get; set; }
    public DateTimeOffset? DueResponseAt { get; set; }
    public DateTimeOffset? DueResolveAt { get; set; }
    public string? EscalateReason { get; set; }
    public Guid CreatedByUserId { get; set; }

    // Cap-2
    public DateTimeOffset? AppointmentAt { get; set; }
    public string? AppointmentNote { get; set; }
    public string? RootCause { get; set; }
    public string? ResolutionNote { get; set; }
    public DateTimeOffset? CheckedOutAt { get; set; }
    public DateTimeOffset? AcceptanceSignedAt { get; set; }
    public string? AcceptanceSignerName { get; set; }
    public string? AcceptanceNote { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public bool? SlaResponseMet { get; set; }
    public bool? SlaResolveMet { get; set; }
}
