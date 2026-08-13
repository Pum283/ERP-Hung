using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Chương trình loyalty & tích điểm (UC_CRM_116).</summary>
public class CrmLoyaltyProgram : TenantEntity
{
    public string ProgramCode { get; set; } = "";
    public string ProgramName { get; set; } = "";
    public decimal PointsPerVnd { get; set; } = 0.001m; // 1000 VNĐ = 1 điểm
    public int MinPointsToRedeem { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public string Description { get; set; } = "";
}
