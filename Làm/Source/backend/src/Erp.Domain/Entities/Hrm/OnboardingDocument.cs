using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

public class OnboardingDocument : TenantEntity
{
    public Guid OnboardingCaseId { get; set; }
    public string Title { get; set; } = "";
    public string StorageKey { get; set; } = "";
}
