using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class ExternalIntegration : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "Generic";
    public string ConfigJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
}
