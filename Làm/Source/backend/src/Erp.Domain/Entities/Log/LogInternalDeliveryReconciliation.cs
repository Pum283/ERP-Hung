using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Đối soát giao nội bộ (UC_LOG_033).</summary>
public class LogInternalDeliveryReconciliation : TenantEntity
{
    public string ReconciliationNumber { get; set; } = "";
    public Guid InternalTransferDeliveryId { get; set; }
    public string InternalDeliveryNumber { get; set; } = "";
    public decimal DispatchedTotalQty { get; set; }
    public decimal ReceivedTotalQty { get; set; }
    public decimal DiscrepancyQty { get; set; }
    public decimal DiscrepancyCostVnd { get; set; }
    public string RootCause { get; set; } = "Hao hụt tự nhiên trong vận chuyển";
    public string ResolutionStatus { get; set; } = "Reconciled"; // Reconciled | UnderInvestigation
    public DateTimeOffset ReconciledAt { get; set; } = DateTimeOffset.UtcNow;
}
