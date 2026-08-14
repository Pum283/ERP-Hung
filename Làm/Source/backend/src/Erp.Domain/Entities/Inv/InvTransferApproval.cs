using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Duyệt yêu cầu điều chuyển kho (UC_INV_032).</summary>
public class InvTransferApproval : TenantEntity
{
    public string TransferRequestNumber { get; set; } = "";
    public Guid SourceWarehouseId { get; set; }
    public Guid DestinationWarehouseId { get; set; }
    public string ApprovalStatus { get; set; } = "PendingApproval"; // PendingApproval | Approved | Rejected
    public string ApproverName { get; set; } = "";
    public string ApprovalComments { get; set; } = "";
    public DateTimeOffset? DecisionAt { get; set; }
}
