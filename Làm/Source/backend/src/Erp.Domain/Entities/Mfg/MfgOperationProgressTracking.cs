using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Ghi nhận tiến độ công đoạn sản xuất WIP (UC_MFG_021).</summary>
public class MfgOperationProgressTracking : TenantEntity
{
    public Guid WorkOrderId { get; set; }
    public string WorkOrderNumber { get; set; } = "";
    public string OperationCode { get; set; } = "";
    public string OperationName { get; set; } = "";
    public decimal CompletedQuantity { get; set; }
    public decimal DefectiveQuantity { get; set; }
    public string OperatorName { get; set; } = "";
    public DateTimeOffset LoggedAt { get; set; } = DateTimeOffset.UtcNow;
}
