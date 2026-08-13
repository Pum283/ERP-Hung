using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Cấu hình chống tải & bảo vệ video (UC_LMS_055).</summary>
public class LmsVideoProtection : TenantEntity
{
    public Guid LessonId { get; set; }
    public bool IsDownloadBlocked { get; set; } = true;
    public bool WatermarkEnabled { get; set; } = true;
    public string WatermarkText { get; set; } = "";
    public int SignedUrlExpiryMinutes { get; set; } = 120;
    public string AllowedRoles { get; set; } = "Instructor,Admin";
}
