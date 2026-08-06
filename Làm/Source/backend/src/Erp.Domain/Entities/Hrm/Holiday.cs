using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Ngày nghỉ lễ / nghỉ công ty (UC_HRM_137).</summary>
public class Holiday : TenantEntity
{
    public DateOnly Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPaid { get; set; } = true;
    public int Year { get; set; }
    public string? Note { get; set; }
}
