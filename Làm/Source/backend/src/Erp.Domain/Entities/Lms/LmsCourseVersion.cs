using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Phiên bản nội dung khóa học (UC_LMS_008).</summary>
public class LmsCourseVersion : TenantEntity
{
    public Guid CourseId { get; set; }
    public string VersionNumber { get; set; } = "1.0";
    public string Changelog { get; set; } = "";
    public bool IsPublished { get; set; } = true;
    public DateTimeOffset PublishedAt { get; set; } = DateTimeOffset.UtcNow;
}
