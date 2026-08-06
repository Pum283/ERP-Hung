using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mod;

/// <summary>Chứng từ/vận hành gốc Day-1 theo module.</summary>
public class ModDocument : TenantEntity
{
    public string ModuleCode { get; set; } = "";
    public string DocType { get; set; } = "";
    public string DocNo { get; set; } = "";
    public string Title { get; set; } = "";
    public string Status { get; set; } = "Draft";
    public Guid? OwnerUserId { get; set; }
    public Guid? RefMasterId { get; set; }
    public string? PayloadJson { get; set; }
}
