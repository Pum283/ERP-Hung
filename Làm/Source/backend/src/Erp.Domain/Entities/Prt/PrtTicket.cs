using Erp.Domain.Base;

namespace Erp.Domain.Entities.Prt;

/// <summary>Ticket hỗ trợ portal (UC_PRT_019–020).</summary>
public class PrtTicket : TenantEntity
{
    public Guid AccountId { get; set; }
    public string Code { get; set; } = "";
    public string Subject { get; set; } = "";
    public string? Description { get; set; }
    /// <summary>Open · InProgress · Resolved · Closed</summary>
    public string Status { get; set; } = "Open";
    public DateTimeOffset OpenedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ClosedAt { get; set; }
}
