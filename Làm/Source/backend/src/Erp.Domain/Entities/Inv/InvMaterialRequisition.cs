using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Đề nghị cấp hàng & quy trình duyệt, chuyển thành phiếu xuất (UC_INV_057, UC_INV_058, UC_INV_059).</summary>
public class InvMaterialRequisition : TenantEntity
{
    public string RequisitionNumber { get; set; } = "";
    public string RequesterName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public decimal RequestedQuantity { get; set; }
    public string Status { get; set; } = "Submitted"; // Submitted | Approved | Rejected | ConvertedToIssue
    public string ApproverName { get; set; } = "";
    public string ConvertedIssueNumber { get; set; } = "";
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ApprovedAt { get; set; }
}
