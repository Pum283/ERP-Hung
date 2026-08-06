using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

public class MfgPlanLine : TenantEntity
{
    public Guid PlanId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Qty { get; set; }
    public Guid? WorkshopId { get; set; }
    public string? Note { get; set; }
}
