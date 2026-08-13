using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Phân vùng / tuyến bán hàng (UC_CRM_089, UC_CRM_090).</summary>
public class CrmSalesTerritory : TenantEntity
{
    public string TerritoryCode { get; set; } = "";
    public string TerritoryName { get; set; } = "";
    public string Region { get; set; } = "Miền Nam";
    /// <summary>Weekly | BiWeekly | Monthly</summary>
    public string VisitFrequency { get; set; } = "Weekly";
    public Guid? AssignedSalespersonId { get; set; }
    public bool IsActive { get; set; } = true;
}
