using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Buổi học trong lớp (UC_LMS_017).</summary>
public class LmsClassSession : TenantEntity
{
    public Guid ClassId { get; set; }
    public DateOnly SessionDate { get; set; }
    public string Topic { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SortOrder { get; set; }
}
