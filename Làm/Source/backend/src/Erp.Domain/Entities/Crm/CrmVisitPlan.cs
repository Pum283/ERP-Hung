using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Lập kế hoạch visit & Check-in GPS (UC_CRM_091, UC_CRM_092).</summary>
public class CrmVisitPlan : TenantEntity
{
    public Guid TerritoryId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid SalespersonId { get; set; }
    public DateTime PlannedDate { get; set; }
    /// <summary>Planned | InProgress | Completed | Cancelled</summary>
    public string Status { get; set; } = "Planned";
    public string? CheckInGps { get; set; }
    public DateTimeOffset? CheckInTime { get; set; }
    public string? CheckOutGps { get; set; }
    public DateTimeOffset? CheckOutTime { get; set; }
    public string Notes { get; set; } = "";
}
