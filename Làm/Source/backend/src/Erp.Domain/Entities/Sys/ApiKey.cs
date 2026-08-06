using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class ApiKey : TenantEntity
{
    public string Name { get; set; } = "";
    public string KeyPrefix { get; set; } = "";
    public string KeyHash { get; set; } = "";
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastUsedAt { get; set; }
}
