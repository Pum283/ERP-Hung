using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

/// <summary>Thực thể quy tắc IP allow/deny (UC_SYS_082).</summary>
public class SysIpRule : TenantEntity
{
    public string IpAddressOrCidr { get; set; } = string.Empty;
    public string RuleType { get; set; } = "Allow"; // Allow | Deny
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
