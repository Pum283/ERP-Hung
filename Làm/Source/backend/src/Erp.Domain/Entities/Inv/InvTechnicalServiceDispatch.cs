using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Xuất cho dịch vụ kỹ thuật / sửa chữa (UC_INV_027).</summary>
public class InvTechnicalServiceDispatch : TenantEntity
{
    public string DispatchNumber { get; set; } = "";
    public Guid ServiceTicketId { get; set; }
    public string TechnicianName { get; set; } = "";
    public Guid WarehouseId { get; set; }
    public decimal TotalPartsValueVnd { get; set; }
    public string PurposeComments { get; set; } = "";
    public DateTimeOffset DispatchedAt { get; set; } = DateTimeOffset.UtcNow;
}
