using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class WorkCalendar : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string WeekMask { get; set; } = "1111100";
    public string? HolidaysJson { get; set; }
    public bool IsActive { get; set; } = true;
}
