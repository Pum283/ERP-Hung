using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Lệnh sản xuất lại / tái chế / xử lý lỗi (UC_MFG_026).</summary>
public class MfgReworkWorkOrder : TenantEntity
{
    public string ReworkWoNumber { get; set; } = "";
    public Guid OriginalWorkOrderId { get; set; }
    public string OriginalWoNumber { get; set; } = "";
    public string DefectReason { get; set; } = "";
    public decimal ReworkQuantity { get; set; }
    public string AssignedWorkshopCode { get; set; } = "";
    public string Status { get; set; } = "Draft"; // Draft | Approved | InProgress | Completed
    public DateTimeOffset CreatedAtDate { get; set; } = DateTimeOffset.UtcNow;
}
