using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Đề nghị xuất nội bộ (UC_INV_056).</summary>
public class InvInternalIssueRequest : TenantEntity
{
    public string RequestNumber { get; set; } = "";
    public string RequestingDepartment { get; set; } = "";
    public string Purpose { get; set; } = "";
    public Guid WarehouseId { get; set; }
    public decimal EstimatedTotalCostVnd { get; set; }
    public string Status { get; set; } = "Submitted"; // Submitted | Approved | Dispatched | Rejected
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
}
