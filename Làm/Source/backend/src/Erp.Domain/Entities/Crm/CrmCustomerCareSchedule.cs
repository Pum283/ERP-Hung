using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Lịch chăm sóc & nhắc tái mua (UC_CRM_115).</summary>
public class CrmCustomerCareSchedule : TenantEntity
{
    public Guid CustomerId { get; set; }
    /// <summary>RoutineCheck | RepurchaseReminder | PostServiceFollowUp</summary>
    public string CareType { get; set; } = "RoutineCheck";
    public DateTime ScheduledDate { get; set; }
    /// <summary>Pending | Completed | Cancelled</summary>
    public string Status { get; set; } = "Pending";
    public string Notes { get; set; } = "";
    public Guid? AssignedUserId { get; set; }
}
