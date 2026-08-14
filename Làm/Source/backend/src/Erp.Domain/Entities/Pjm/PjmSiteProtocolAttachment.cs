using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Ghi nhận ảnh hiện trường & biên bản nghiệm thu dự án (UC_PJM_028).</summary>
public class PjmSiteProtocolAttachment : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; } = "PRJ-2026-088";
    public string AttachmentTitle { get; set; } = "Biên bản nghiệm thu đóng điện trạm biến áp có chữ ký";
    public string AttachmentType { get; set; } = "ProtocolPdf"; // PhotoJpg | ProtocolPdf | DrawingCad
    public string FileUrl { get; set; } = "/uploads/pjm/protocols/prj-088-handover-signed.pdf";
    public long FileSizeBytes { get; set; } = 2450000;
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}
