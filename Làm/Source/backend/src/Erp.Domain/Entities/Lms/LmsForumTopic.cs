using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Thực thể diễn đàn khóa học (UC_LMS_039).</summary>
public class LmsForumTopic : TenantEntity
{
    public Guid CourseId { get; set; }
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int ReplyCount { get; set; } = 0;
    public bool IsPinned { get; set; } = false;
}
