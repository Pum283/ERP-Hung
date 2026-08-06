using Erp.Domain.Base;

namespace Erp.Domain.Entities.Bi;

/// <summary>Danh mục báo cáo chuẩn (UC_BI_013–014, 016).</summary>
public class BiReport : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string ModuleCode { get; set; } = "";
    public Guid? DatasetId { get; set; }
    public string? Description { get; set; }
    /// <summary>JSON schema filter mẫu: [{"key":"from","label":"Từ ngày"}]</summary>
    public string? FilterSchemaJson { get; set; }
    public string Status { get; set; } = "Active";
    public bool RequirePermission { get; set; } = true;
}
