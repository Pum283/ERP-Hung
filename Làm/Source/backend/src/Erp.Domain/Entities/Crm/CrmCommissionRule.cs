using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Cấu hình rule hoa hồng (UC_CRM_120).</summary>
public class CrmCommissionRule : TenantEntity
{
    public string RuleCode { get; set; } = "";
    public string RuleName { get; set; } = "";
    public string SalesRole { get; set; } = "FieldSales";
    public decimal MinRevenueThreshold { get; set; }
    public decimal CommissionRatePercent { get; set; }
    public bool IsActive { get; set; } = true;
}
