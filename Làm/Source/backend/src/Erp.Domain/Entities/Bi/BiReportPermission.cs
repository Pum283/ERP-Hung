using Erp.Domain.Base;

namespace Erp.Domain.Entities.Bi;

/// <summary>Phân quyền xem báo cáo (UC_BI_003).</summary>
public class BiReportPermission : TenantEntity
{
    public Guid ReportId { get; set; }
    /// <summary>Role | User</summary>
    public string PrincipalType { get; set; } = "Role";
    public string PrincipalCode { get; set; } = "";
    /// <summary>View | Run | Export</summary>
    public string AccessLevel { get; set; } = "View";
}
