using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Danh mục nguồn lead (UC_CRM_024, 050).</summary>
public class CrmLeadSource : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Manual · Website · Social · Other</summary>
    public string ChannelType { get; set; } = "Manual";
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
