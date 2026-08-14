using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Checklist kỹ thuật lắp đặt thi công dự án (UC_PJM_026).</summary>
public class PjmInstallationChecklistItem : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; } = "PRJ-2026-088";
    public string InstallationStepTitle { get; set; } = "1. Siết bu lông chân máy biến áp theo lực siết 120 N.m";
    public string EquipmentTag { get; set; } = "TRANS-2000KVA";
    public bool IsCompleted { get; set; } = true;
    public string TechnicianSigner { get; set; } = "KS. Trần Quốc Toản";
    public DateTimeOffset InstalledAt { get; set; } = DateTimeOffset.UtcNow;
}
