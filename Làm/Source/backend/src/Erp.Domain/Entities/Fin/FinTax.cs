using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Danh mục / cấu hình thuế suất (UC_FIN_009, UC_FIN_056).</summary>
public class FinTax : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal RatePercent { get; set; }
    /// <summary>VatOutput · VatInput · Other</summary>
    public string TaxType { get; set; } = "VatOutput";
    public bool IsDefault { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
