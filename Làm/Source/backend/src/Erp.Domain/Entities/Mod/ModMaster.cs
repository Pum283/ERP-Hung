using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mod;

/// <summary>Master dùng chung Day-1 cho các module (type phân biệt danh mục).</summary>
public class ModMaster : TenantEntity
{
    public string ModuleCode { get; set; } = "";
    public string RecordType { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "Active";
    public string? PayloadJson { get; set; }
}
