using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Danh mục khoản mục thu/chi dòng tiền Cash Flow (UC_FIN_007).</summary>
public class FinCashFlowCategory : TenantEntity
{
    public string CategoryCode { get; set; } = "CASH-IN-PRJ";
    public string CategoryName { get; set; } = "Thu tiền theo tiến độ hợp đồng dự án";
    public string CashFlowType { get; set; } = "Inflow"; // Inflow | Outflow
    public string SectionCode { get; set; } = "Operating"; // Operating | Investing | Financing
    public bool IsActive { get; set; } = true;
}
