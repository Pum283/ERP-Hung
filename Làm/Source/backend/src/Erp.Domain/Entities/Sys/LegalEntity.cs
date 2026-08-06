using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class LegalEntity : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? TaxCode { get; set; }
    public bool IsActive { get; set; } = true;
}
