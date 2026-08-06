using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Cấu hình thời hạn onboarding / thử việc theo tenant (UC_HRM_066–067).</summary>
public class OnboardingSetting : TenantEntity
{
    public int OnboardingDays { get; set; } = 30;
    public int TrialDays { get; set; } = 60;
}
