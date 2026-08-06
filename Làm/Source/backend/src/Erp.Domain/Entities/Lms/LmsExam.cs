using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Đề thi cố định + cấu hình điểm/lần thi (UC_LMS_012, 014).</summary>
public class LmsExam : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>ChapterQuiz · Final</summary>
    public string ExamType { get; set; } = "Final";
    public Guid? CourseId { get; set; }
    public Guid? ChapterId { get; set; }
    public decimal PassScore { get; set; } = 70;
    public int MaxAttempts { get; set; } = 3;
    public int? TimeLimitMin { get; set; }
    /// <summary>Draft · Published · Archived</summary>
    public string Status { get; set; } = "Draft";
}
