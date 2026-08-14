using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Ghi nhận chi phí sửa chữa chi tiết của ticket (UC_FSM_026).</summary>
public class FsmRepairCostRecord : TenantEntity
{
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = "";
    public decimal LaborCostVnd { get; set; } = 500000;
    public decimal PartsCostVnd { get; set; } = 350000;
    public decimal TravelFeeVnd { get; set; } = 150000;
    public decimal TotalBillableAmountVnd { get; set; } = 1000000;
    public bool IsCoveredByWarranty { get; set; } = false;
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
}
