using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

/// <summary>Quản lý phiên bản cấu hình hệ thống (UC_SYS_058).</summary>
public class SysConfigVersion : TenantEntity
{
    public string ConfigKey { get; set; } = "";
    public string ConfigValue { get; set; } = "";
    public int VersionNumber { get; set; } = 1;
    public string? CommitNote { get; set; }
    public bool IsCurrent { get; set; }
    public Guid? CreatedByUserId { get; set; }
}
