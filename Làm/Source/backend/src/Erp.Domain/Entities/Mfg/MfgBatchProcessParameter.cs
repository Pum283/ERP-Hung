using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Ghi nhận thông số kỹ thuật mẻ sản xuất (UC_MFG_038).</summary>
public class MfgBatchProcessParameter : TenantEntity
{
    public string BatchNumber { get; set; } = "";
    public string ParameterName { get; set; } = "Nhiệt Độ Lò Nung";
    public decimal TargetValue { get; set; } = 180.0m;
    public decimal ActualMeasuredValue { get; set; } = 181.5m;
    public string UnitOfMeasure { get; set; } = "°C";
    public bool IsWithinTolerance { get; set; } = true;
    public string RecordedBy { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
}
