using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Trung tâm chi phí (UC_FIN_006).</summary>
public class FinCostCenter : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
