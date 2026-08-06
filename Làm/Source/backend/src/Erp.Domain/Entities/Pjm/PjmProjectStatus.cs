using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Trạng thái dự án chuẩn (UC_PJM_004).</summary>
public class PjmProjectStatus : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsTerminal { get; set; }
    public bool IsActive { get; set; } = true;
}
