using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class UserDataScope : TenantEntity
{
    public Guid UserId { get; set; }
    public string Dimension { get; set; } = "OrgUnit";
    public Guid ScopeId { get; set; }
    public bool IncludeChildren { get; set; } = true;
    public string AccessLevel { get; set; } = "Read";
    public string Source { get; set; } = "Manual";
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
}
