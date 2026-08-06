using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Cấu hình BH / thuế / khấu trừ mặc định (UC_HRM_160–162).</summary>
public class PayrollPolicy : TenantEntity
{
    public decimal SocialInsuranceEmpRate { get; set; } = 0.08m;
    public decimal HealthInsuranceEmpRate { get; set; } = 0.015m;
    public decimal UnemploymentEmpRate { get; set; } = 0.01m;
    public decimal PersonalDeduction { get; set; } = 11_000_000m;
    /// <summary>Thuế TNCN flat Day-1 (%).</summary>
    public decimal FlatTaxRate { get; set; } = 0.10m;
    public int StandardWorkDays { get; set; } = 26;
    public decimal OtMultiplier { get; set; } = 1.5m;
}
