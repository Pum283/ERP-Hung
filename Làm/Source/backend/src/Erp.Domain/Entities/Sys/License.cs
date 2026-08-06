using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class License : TenantEntity
{
    public string PlanCode { get; set; } = string.Empty;
    public DateOnly ValidFrom { get; set; }
    public DateOnly ValidTo { get; set; }
    public int MaxUsers { get; set; } = 10;
    public int MaxOrgUnits { get; set; } = 50;
    public string Status { get; set; } = "Active";
}

public class LicenseModule : TenantEntity
{
    public Guid LicenseId { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string? QuotaJson { get; set; }
}
