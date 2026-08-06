using Erp.Domain.Base;

namespace Erp.Domain.Entities.Prt;

/// <summary>Cấu hình module portal theo gói (UC_PRT_037).</summary>
public class PrtPortalPackage : TenantEntity
{
    /// <summary>STARTER · STANDARD · ENTERPRISE</summary>
    public string PlanCode { get; set; } = "STANDARD";
    public string Name { get; set; } = "";
    /// <summary>JSON feature flags: orders, ar, tickets, vendor, docs…</summary>
    public string FeaturesJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}
