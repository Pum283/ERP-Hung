using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Chương trình đào tạo (UC_LMS_001).</summary>
public class LmsProgram : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
}
