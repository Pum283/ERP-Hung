using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

/// <summary>UC_SYS_009 — Nhà cung cấp SSO/OAuth (IdP).</summary>
public class SysSsoProvider : TenantEntity
{
    public string Code { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string? ClientSecret { get; set; }
    public string? AuthorityUrl { get; set; }
    public string RedirectUri { get; set; } = "";
    public string Scopes { get; set; } = "openid profile email";
    public bool JitProvisioning { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}

/// <summary>UC_SYS_009 — Liên kết tài khoản ngoài với user nội bộ.</summary>
public class SysExternalLogin : TenantEntity
{
    public Guid UserId { get; set; }
    public string ProviderCode { get; set; } = "";
    public string ProviderSubject { get; set; } = "";
    public string? Email { get; set; }
    public DateTimeOffset LinkedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>UC_SYS_031 — Danh mục trường nhạy cảm.</summary>
public class SysSensitiveField : TenantEntity
{
    public string ModuleCode { get; set; } = "SYS";
    public string EntityName { get; set; } = "";
    public string FieldKey { get; set; } = "";
    public string DisplayName { get; set; } = "";
    /// <summary>Hide | Mask | ReadOnly</summary>
    public string DefaultMask { get; set; } = "Mask";
    public bool IsActive { get; set; } = true;
}

/// <summary>UC_SYS_031 — Quyền trường theo vai trò. Access: None|Masked|Read|Write</summary>
public class SysRoleFieldPermission : TenantEntity
{
    public Guid RoleId { get; set; }
    public Guid SensitiveFieldId { get; set; }
    public string Access { get; set; } = "None";
}

/// <summary>UC_SYS_062 — Device token nhận push.</summary>
public class SysPushDevice : TenantEntity
{
    public Guid UserId { get; set; }
    /// <summary>Fcm | Apns | Web</summary>
    public string Platform { get; set; } = "Fcm";
    public string DeviceToken { get; set; } = "";
    public string? AppVersion { get; set; }
    public bool IsValid { get; set; } = true;
    public DateTimeOffset LastSeenAt { get; set; } = DateTimeOffset.UtcNow;
}
