using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Sao chép BOM & nhân bản phiên bản công thức (UC_MFG_011).</summary>
public class MfgBomCopyLog : TenantEntity
{
    public Guid SourceBomId { get; set; }
    public string SourceBomCode { get; set; } = "";
    public string SourceVersion { get; set; } = "v1.0";
    public Guid NewBomId { get; set; }
    public string NewBomCode { get; set; } = "";
    public string NewVersion { get; set; } = "v1.1-COPY";
    public int CopiedLinesCount { get; set; }
    public string CopiedBy { get; set; } = "";
    public DateTimeOffset CopiedAt { get; set; } = DateTimeOffset.UtcNow;
}
