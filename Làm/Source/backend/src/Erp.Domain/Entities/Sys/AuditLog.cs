using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class AuditLog : TenantEntity
{
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? IpAddress { get; set; }
}
