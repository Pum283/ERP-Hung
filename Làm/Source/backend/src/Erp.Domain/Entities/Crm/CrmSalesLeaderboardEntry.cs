using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Bảng xếp hạng sales (UC_CRM_125).</summary>
public class CrmSalesLeaderboardEntry : TenantEntity
{
    public Guid SalesUserId { get; set; }
    public string SalesUserName { get; set; } = "";
    public int RankPosition { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalNewCustomers { get; set; }
    public decimal TotalCommissionEarned { get; set; }
    public string RankingPeriod { get; set; } = "Monthly";
}
