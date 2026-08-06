using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>BOM phiên bản (UC_MFG_006–008).</summary>
public class MfgBom : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid ParentItemId { get; set; }
    public string Version { get; set; } = "1.0";
    /// <summary>Draft · Active · Obsolete</summary>
    public string Status { get; set; } = "Draft";
    public string? Note { get; set; }
}
