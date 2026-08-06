using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class UserDepartment : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid DepartmentId { get; set; }
    /// <summary>Cấp bậc (job level) của user tại phòng ban này — mỗi membership một level.</summary>
    public Guid? JobLevelId { get; set; }
    /// <summary>Đúng một membership primary; các phòng còn lại ngang hàng.</summary>
    public bool IsPrimary { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
}
