using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Chương trong khóa (UC_LMS_004).</summary>
public class LmsChapter : TenantEntity
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = "";
    public int SortOrder { get; set; }
}
