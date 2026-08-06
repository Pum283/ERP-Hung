using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Cấu hình báo trước nghỉ việc (UC_HRM_145).</summary>
public class OffboardingSetting : TenantEntity
{
    public int NoticeDays { get; set; } = 30;
    public bool RequireChecklistComplete { get; set; } = true;
    public bool AutoRevokeAccessOnComplete { get; set; } = true;
}
