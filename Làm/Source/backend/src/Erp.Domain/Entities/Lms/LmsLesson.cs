using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Bài học — video / PDF / text (UC_LMS_004–006, 034).</summary>
public class LmsLesson : TenantEntity
{
    public Guid ChapterId { get; set; }
    public string Title { get; set; } = "";
    /// <summary>Video · Document · Text</summary>
    public string LessonType { get; set; } = "Text";
    public string? ContentUrl { get; set; }
    public string? Body { get; set; }
    public int SortOrder { get; set; }
    public int? DurationSec { get; set; }
}
