using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class SystemSetting : TenantEntity
{
    public string Key { get; set; } = "";
    public string ValueJson { get; set; } = "{}";
}
