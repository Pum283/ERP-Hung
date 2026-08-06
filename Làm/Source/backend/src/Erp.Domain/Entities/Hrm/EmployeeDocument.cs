using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Giấy tờ tùy thân / hồ sơ đính kèm nhân viên (UC_HRM_017).</summary>
public class EmployeeDocument : TenantEntity
{
    public Guid EmployeeId { get; set; }
    /// <summary>IdCard · Passport · Household · Degree · Other</summary>
    public string DocType { get; set; } = "Other";
    public string Title { get; set; } = "";
    public string StorageKey { get; set; } = "";
    public DateOnly? IssuedOn { get; set; }
    public DateOnly? ExpiresOn { get; set; }
}
