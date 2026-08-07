using Erp.Domain.Enums.Sys;

namespace Erp.Application.DTOs.Sys;

public sealed record PasswordPolicyDto(int MinLength, bool RequireDigit, bool RequireUpper, bool RequireLower, int MaxFailedLogins, int LockMinutes, int SessionMinutes);

public sealed record TenantDto(
    Guid Id, string Code, string Name, string Status, string Timezone, string DefaultLocale, string DefaultCurrency,
    string? LogoUrl);

public sealed record TenantUpdateRequest(string Name, string Status, string Timezone, string DefaultLocale, string DefaultCurrency);

public sealed record ModuleCatalogItemDto(string Code, string Name, bool AlwaysOn);

public sealed record LicenseDto(Guid Id, string PlanCode, DateOnly ValidFrom, DateOnly ValidTo, int MaxUsers, int MaxOrgUnits, string Status);

public sealed record LicenseUpsertRequest(Guid? Id, string PlanCode, DateOnly ValidFrom, DateOnly ValidTo, int MaxUsers, int MaxOrgUnits, string Status);

public sealed record LicenseStatusDto(string PlanCode, DateOnly ValidTo, int DaysLeft, int MaxUsers, int CurrentUsers, int MaxOrgUnits, int CurrentOrgUnits, bool NearExpiry, bool OverUserLimit, bool OverOrgLimit);

public sealed record SystemSettingDto(string Key, string ValueJson);

public sealed record LookupCategoryDto(Guid Id, string Code, string Name, bool IsActive);

public sealed record LookupItemDto(Guid Id, Guid CategoryId, string Code, string Name, int SortOrder, bool IsActive);

public sealed record NumberSequenceDto(Guid Id, string DocType, string Pattern, int NextValue);

public sealed record LoginAuditDto(Guid Id, Guid? UserId, string Username, bool Success, string? IpAddress, string? FailureReason, DateTimeOffset AttemptedAt);

public sealed record AppNotificationDto(Guid Id, string Title, string Body, string? Link, string? EventType, bool IsRead, DateTimeOffset CreatedAt);

public sealed record NotificationRuleDto(Guid Id, string EventType, string TitleTemplate, string BodyTemplate, bool IsEnabled);

public sealed record ResetPasswordResultDto(string TemporaryPassword);

public sealed record UserSessionDto(Guid Id, string SessionKey, string? IpAddress, string? UserAgent, DateTimeOffset LastSeenAt, DateTimeOffset ExpiresAt, bool IsRevoked);

public sealed record LegalEntityDto(Guid Id, string Code, string Name, string? TaxCode, bool IsActive);

public sealed record SalesPointDto(Guid Id, string Code, string Name, Guid? OrgUnitId, string? Address, bool IsActive);

public sealed record ProvinceDto(Guid Id, string Code, string Name, bool IsActive);

public sealed record WorkCalendarDto(Guid Id, string Code, string Name, string WeekMask, string? HolidaysJson, bool IsActive);

public sealed record MessageTemplateDto(Guid Id, string Code, string Channel, string Subject, string Body, bool IsActive);

public sealed record ApiKeyDto(Guid Id, string Name, string KeyPrefix, DateTimeOffset? ExpiresAt, bool IsActive, DateTimeOffset? LastUsedAt);

public sealed record ApiKeyCreatedDto(Guid Id, string Name, string KeyPrefix, string PlainKey);

public sealed record WebhookDto(Guid Id, string Name, string TargetUrl, string EventTypes, bool IsActive);

public sealed record IntegrationLogDto(Guid Id, string Kind, string Target, int StatusCode, string? RequestSummary, DateTimeOffset CalledAt);

public sealed record PermissionChangeLogDto(Guid Id, Guid? ActorUserId, string ChangeType, Guid? RoleId, Guid? TargetUserId, string DetailJson, DateTimeOffset CreatedAt);

public sealed record LocalePackDto(Guid Id, string Code, string Name, bool IsDefault, bool IsActive);

public sealed record FileFolderDto(Guid Id, string Name, Guid? ParentId);

public sealed record FileObjectDto(Guid Id, string StorageKey, string FileName, string? ContentType, long SizeBytes, Guid? FolderId, bool IsDeleted);

public sealed record ExternalIntegrationDto(Guid Id, string Code, string Name, string Kind, string ConfigJson, bool IsActive);

/// <summary>UC_SYS_060/061 — gửi Email/SMS qua template (stub log + outbox, không gọi gateway thật).</summary>
public sealed record ChannelSendRequest(
    string Channel, string TemplateCode, string Target,
    IReadOnlyDictionary<string, string>? Vars = null, string? EventType = null);

public sealed record ChannelSendResultDto(
    Guid LogId, string Channel, string Target, string TemplateCode,
    string Subject, string Body, string Status, Guid? IntegrationId, string? IntegrationCode);

public sealed record InviteUserRequest(
    string Username, string? DisplayName, string? Email, string? Phone,
    Guid? PrimaryOrgUnitId = null, Guid? DepartmentId = null, Guid? JobLevelId = null);

public sealed record InviteUserResultDto(
    Guid UserId, string Username, string Channel, string Target, Guid LogId, string Message);

public sealed record OrgNodeDto(Guid Id, string Code, string Name, Guid? ParentId, string UnitType);

public sealed record RoleMatrixRowDto(Guid RoleId, string RoleCode, string RoleName, IReadOnlyList<string> PermissionCodes);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record ForgotPasswordRequest(string UsernameOrEmail);

public sealed record ResetPasswordWithOtpRequest(string UsernameOrEmail, string Otp, string NewPassword);

public sealed record Enable2FaResponse(string Secret, string OtpAuthUri);

public sealed record Verify2FaRequest(string Code);

public sealed record SetLocaleRequest(string LocaleCode);
