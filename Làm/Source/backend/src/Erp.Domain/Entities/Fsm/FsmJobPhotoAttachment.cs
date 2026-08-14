using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Chụp ảnh chứng minh trước/sau khi sửa chữa (UC_FSM_023).</summary>
public class FsmJobPhotoAttachment : TenantEntity
{
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = "";
    public string PhotoType { get; set; } = "Before"; // Before | After | SparePartEvidence
    public string PhotoUrl { get; set; } = "/uploads/fsm/photos/ticket-before-01.jpg";
    public string Caption { get; set; } = "Hiện trạng bo mạch bị cháy tụ điện";
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}
