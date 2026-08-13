using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Quản lý hợp đồng bán (UC_CRM_106, UC_CRM_108).</summary>
public class CrmSalesContract : TenantEntity
{
    public string ContractCode { get; set; } = "";
    public string Title { get; set; } = "";
    public Guid CustomerId { get; set; }
    public decimal ContractValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    /// <summary>Draft | Active | ExpiringSoon | Expired | Renewed</summary>
    public string Status { get; set; } = "Active";
    public Guid? SalesAdminUserId { get; set; }
    public string RenewalNotes { get; set; } = "";
}
