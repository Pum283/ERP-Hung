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

public sealed record FileUploadRequest(string FileName, string? ContentType, long SizeBytes, Guid? FolderId = null);

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

/// <summary>UC_SYS_069 — gắn file vào đối tượng nghiệp vụ (LinkedEntity ACL).</summary>
public sealed record LinkFileToEntityRequest(Guid FileId, string EntityType, Guid EntityId);

/// <summary>UC_SYS_069 — danh sách file gắn theo đối tượng.</summary>
public sealed record LinkedFileDto(Guid FileId, string FileName, string? ContentType, long SizeBytes, string EntityType, Guid EntityId, DateTimeOffset LinkedAt);

/// <summary>UC_SYS_074/075 — generic export request hỗ trợ Csv/Excel/Pdf.</summary>
public sealed record GenericExportRequest(string EntityType, string Format, string? FilterJson = null);

/// <summary>UC_SYS_074/075 — kết quả export trả về file name, content type, data bytes.</summary>
public sealed record GenericExportResult(string FileName, string ContentType, byte[] Data, int RowCount);

/// <summary>UC_SYS_076 — lịch sử job import/export.</summary>
public sealed record ImportExportJobDto(Guid Id, string JobType, string EntityType, string? Format, string Status, int RowCount, int ErrorCount, string? ErrorDetails, DateTimeOffset StartedAt, DateTimeOffset? CompletedAt, Guid? ActorId);

// ─── UC_SYS_078 — Nhật ký thao tác người dùng ───

/// <summary>UC_SYS_078 — một dòng audit log.</summary>
public sealed record AuditLogDto(
    Guid Id, string EntityType, Guid? EntityId, string Action,
    string? BeforeJson, string? AfterJson,
    Guid? ActorUserId, string? IpAddress, DateTimeOffset CreatedAt);

/// <summary>UC_SYS_078 — request lọc / phân trang audit log.</summary>
public sealed record AuditLogQueryRequest(
    string? EntityType = null,
    string? Action = null,
    Guid? ActorUserId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int Page = 1,
    int PageSize = 50);

// ─── UC_SYS_080 — Xem chi tiết thay đổi field ───

/// <summary>UC_SYS_080 — diff từng field giữa Before và After JSON.</summary>
public sealed record FieldDiffDto(string FieldName, string? OldValue, string? NewValue, string ChangeKind);

/// <summary>UC_SYS_080 — audit log chi tiết kèm field diff.</summary>
public sealed record AuditLogDetailDto(
    Guid Id, string EntityType, Guid? EntityId, string Action,
    Guid? ActorUserId, string? IpAddress, DateTimeOffset CreatedAt,
    string? BeforeJson, string? AfterJson,
    IReadOnlyList<FieldDiffDto> FieldDiffs);

// ─── UC_SYS_081 — Xuất audit log ───

/// <summary>UC_SYS_081 — export audit log ra CSV, phải chỉ định khoảng thời gian.</summary>
public sealed record AuditLogExportRequest(
    DateTimeOffset From, DateTimeOffset To,
    string? EntityType = null,
    string? Action = null,
    Guid? ActorUserId = null);

// ─── UC_SYS_083 — Chính sách hết hạn phiên ───

/// <summary>UC_SYS_083 — toàn bộ chính sách phiên làm việc (tách riêng khỏi PasswordPolicy).</summary>
public sealed record SessionPolicyDto(
    int SessionMinutes,
    int IdleTimeoutMinutes,
    int MaxConcurrentSessions,
    bool ForceLogoutOnPasswordChange);

/// <summary>UC_SYS_083 — request cập nhật chính sách phiên.</summary>
public sealed record SessionPolicyUpdateRequest(
    int SessionMinutes,
    int IdleTimeoutMinutes,
    int MaxConcurrentSessions,
    bool ForceLogoutOnPasswordChange);

// ─── UC_SYS_087 — Hàng đợi sự kiện liên module (Outbox Queue) ───

public sealed record OutboxMessageDto(
    Guid Id, string EventType, string SourceModule, Guid? CorrelationId,
    string PayloadJson, string Status, int AttemptCount,
    DateTimeOffset? NextAttemptAt, DateTimeOffset? PublishedAt, string? LastError, DateTimeOffset CreatedAt);

public sealed record EnqueueOutboxRequest(string EventType, string SourceModule, string PayloadJson, Guid? CorrelationId = null);

public sealed record OutboxQueryRequest(string? Status = null, string? SourceModule = null, int Page = 1, int PageSize = 50);

public sealed record OutboxBatchProcessResultDto(int ProcessedCount, int SuccessCount, int FailedCount);

// ─── UC_SYS_088 — Kết nối Email Gateway ───

public sealed record EmailGatewayConfigDto(
    string ProviderType, // Smtp | SendGrid | AmazonSES
    string SmtpHost, int SmtpPort, bool UseSsl,
    string SenderEmail, string SenderName,
    string? ApiKey, string? Username);

public sealed record UpsertEmailGatewayRequest(string Code, string Name, EmailGatewayConfigDto Config, bool IsActive = true);

public sealed record TestGatewayResultDto(bool Success, string Message, int LatencyMs, DateTimeOffset TestedAt);

public sealed record SendTestEmailRequest(Guid GatewayId, string TargetEmail, string Subject, string Body);

// ─── UC_SYS_089 — Kết nối SMS Gateway ───

public sealed record SmsGatewayConfigDto(
    string ProviderType, // Twilio | VietGuys | eSMS | SpeedSMS
    string SenderId, string AccountSidOrUser,
    string ApiKeyOrSecret, string? ApiUrl);

public sealed record UpsertSmsGatewayRequest(string Code, string Name, SmsGatewayConfigDto Config, bool IsActive = true);

public sealed record SendTestSmsRequest(Guid GatewayId, string TargetPhone, string Message);

// ─── UC_SYS_101 — Đính kèm file trong tin nhắn ───

public sealed record SendChatMessageRequest(
    Guid ConversationId, string Body, Guid? AttachmentFileId = null, Guid? ParentMessageId = null);

public sealed record ChatMessageAttachmentDto(
    Guid Id, Guid ConversationId, Guid SenderUserId, string Body,
    Guid? AttachmentFileId, string? AttachmentFileName, string? AttachmentStorageKey, long? AttachmentSizeBytes, string? AttachmentContentType,
    Guid? ParentMessageId, DateTimeOffset SentAt, bool IsEdited, DateTimeOffset? RecalledAt);

// ─── UC_HRM_001 / 002 / 003 / 004 — Cơ cấu tổ chức & Điểm bán ───

public sealed record UpsertOrgUnitRequest(
    Guid? Id, string Code, string Name, Guid? ParentId, string UnitType,
    Guid? ManagerUserId = null, int SortOrder = 0, bool IsActive = true);

public sealed record OrgUnitDetailDto(
    Guid Id, string Code, string Name, Guid? ParentId, string? ParentName, string UnitType,
    string Path, Guid? ManagerUserId, int SortOrder, bool IsActive, int ChildCount);

public sealed record SalesPointUpsertRequest(
    Guid? Id, string Code, string Name, Guid? OrgUnitId, string? Address, bool IsActive = true);

// ─── UC_HRM_006 / 010 / 012 / 017 — Lịch làm việc, Cấp bậc, Mã nhân sự & Giấy tờ ───

public sealed record JobLevelPolishDto(
    Guid Id, string Code, string Name, int LevelOrder, string DefaultScopeType, string? Description, bool IsActive);

public sealed record UpsertJobLevelRequest(
    Guid? Id, string Code, string Name, int LevelOrder, string DefaultScopeType = "Own", string? Description = null, bool IsActive = true);

public sealed record EmployeeCodeGenerateRequest(string DocType = "EMP", string? Pattern = null);

public sealed record EmployeeCodeGeneratedDto(string Code, int SequenceValue, string Pattern);
