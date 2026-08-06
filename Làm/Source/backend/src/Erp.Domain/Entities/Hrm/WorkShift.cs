using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Mẫu ca làm việc (UC_HRM_081).</summary>
public class WorkShift : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int BreakMinutes { get; set; }
    /// <summary>Ca qua đêm (EndTime &lt; StartTime).</summary>
    public bool IsOvernight { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}
