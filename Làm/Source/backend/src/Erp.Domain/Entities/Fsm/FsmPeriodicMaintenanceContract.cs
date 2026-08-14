using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Hợp đồng bảo trì định kỳ SLA (UC_FSM_012).</summary>
public class FsmPeriodicMaintenanceContract : TenantEntity
{
    public string ContractNumber { get; set; } = "";
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public string ServiceLevelAgreement { get; set; } = "Gold 24/7 (SLA 2h)";
    public int VisitsPerYear { get; set; } = 4;
    public decimal AnnualContractValueVnd { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public string Status { get; set; } = "Active"; // Active | Suspended | Terminated
}
