using Erp.Application.DTOs.Sys;

namespace Erp.Application.Interfaces.Services.Sys;

public interface ISysPlatformService
{
    Task<PasswordPolicyDto> GetPasswordPolicyAsync(Guid tenantId, CancellationToken ct = default);
    Task<PasswordPolicyDto> SetPasswordPolicyAsync(Guid tenantId, PasswordPolicyDto policy, CancellationToken ct = default);
    Task ValidatePasswordAsync(Guid tenantId, string password, CancellationToken ct = default);

    Task<TenantDto> GetTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<TenantDto> UpdateTenantAsync(Guid tenantId, TenantUpdateRequest req, CancellationToken ct = default);
    Task<TenantDto> SetTenantLogoAsync(Guid tenantId, string? logoUrl, string? storageKey, CancellationToken ct = default);

    Task<IReadOnlyList<ModuleCatalogItemDto>> ListModulesAsync();
    Task<IReadOnlyList<LicenseDto>> ListLicensesAsync(Guid tenantId, CancellationToken ct = default);
    Task<LicenseDto> UpsertLicenseAsync(Guid tenantId, Guid? actorId, LicenseUpsertRequest req, CancellationToken ct = default);
    Task<LicenseStatusDto> GetLicenseStatusAsync(Guid tenantId, CancellationToken ct = default);

    Task<IReadOnlyList<SystemSettingDto>> ListSettingsAsync(Guid tenantId, CancellationToken ct = default);
    Task UpsertSettingAsync(Guid tenantId, string key, string valueJson, CancellationToken ct = default);
    Task<string?> GetSettingValueAsync(Guid tenantId, string key, Guid? orgUnitId = null, CancellationToken ct = default);
    Task UpsertOrgUnitSettingAsync(Guid tenantId, Guid orgUnitId, string key, string valueJson, CancellationToken ct = default);

    Task<IReadOnlyList<LookupCategoryDto>> ListLookupCategoriesAsync(Guid tenantId, CancellationToken ct = default);
    Task<LookupCategoryDto> UpsertLookupCategoryAsync(Guid tenantId, Guid? actorId, LookupCategoryDto req, CancellationToken ct = default);
    Task<IReadOnlyList<LookupItemDto>> ListLookupItemsAsync(Guid tenantId, Guid categoryId, CancellationToken ct = default);
    Task<LookupItemDto> UpsertLookupItemAsync(Guid tenantId, Guid? actorId, LookupItemDto req, CancellationToken ct = default);

    Task<IReadOnlyList<NumberSequenceDto>> ListNumberSequencesAsync(Guid tenantId, CancellationToken ct = default);
    Task<NumberSequenceDto> UpsertNumberSequenceAsync(Guid tenantId, Guid? actorId, NumberSequenceDto req, CancellationToken ct = default);
    Task<string> NextNumberAsync(Guid tenantId, string docType, CancellationToken ct = default);

    Task<IReadOnlyList<LoginAuditDto>> ListLoginAuditsAsync(Guid tenantId, int take, CancellationToken ct = default);

    Task<IReadOnlyList<AppNotificationDto>> ListNotificationsAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task MarkNotificationReadAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task MarkAllNotificationsReadAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<int> UnreadNotificationCountAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationRuleDto>> ListNotificationRulesAsync(Guid tenantId, CancellationToken ct = default);
    Task<NotificationRuleDto> UpsertNotificationRuleAsync(Guid tenantId, Guid? actorId, NotificationRuleDto req, CancellationToken ct = default);
    Task NotifyEventAsync(Guid tenantId, Guid targetUserId, string eventType, string? link, IDictionary<string, string>? vars, CancellationToken ct = default);

    Task<byte[]> ExportUsersCsvAsync(Guid tenantId, Guid currentUserId, CancellationToken ct = default);
    Task<string> GetUserImportTemplateAsync();
    Task<(int Ok, int Fail, IReadOnlyList<string> Errors)> ImportUsersCsvAsync(Guid tenantId, Guid actorId, Stream csv, CancellationToken ct = default);

    Task<IReadOnlyList<LegalEntityDto>> ListLegalEntitiesAsync(Guid tenantId, CancellationToken ct = default);
    Task<LegalEntityDto> UpsertLegalEntityAsync(Guid tenantId, Guid? actorId, LegalEntityDto req, CancellationToken ct = default);
    Task<IReadOnlyList<SalesPointDto>> ListSalesPointsAsync(Guid tenantId, CancellationToken ct = default);
    Task<SalesPointDto> UpsertSalesPointAsync(Guid tenantId, Guid? actorId, SalesPointDto req, CancellationToken ct = default);
    Task<IReadOnlyList<ProvinceDto>> ListProvincesAsync(Guid tenantId, CancellationToken ct = default);
    Task<ProvinceDto> UpsertProvinceAsync(Guid tenantId, Guid? actorId, ProvinceDto req, CancellationToken ct = default);
    Task<IReadOnlyList<OrgNodeDto>> GetOrgChartAsync(Guid tenantId, CancellationToken ct = default);

    Task<IReadOnlyList<WorkCalendarDto>> ListWorkCalendarsAsync(Guid tenantId, CancellationToken ct = default);
    Task<WorkCalendarDto> UpsertWorkCalendarAsync(Guid tenantId, Guid? actorId, WorkCalendarDto req, CancellationToken ct = default);
    Task<IReadOnlyList<MessageTemplateDto>> ListMessageTemplatesAsync(Guid tenantId, CancellationToken ct = default);
    Task<MessageTemplateDto> UpsertMessageTemplateAsync(Guid tenantId, Guid? actorId, MessageTemplateDto req, CancellationToken ct = default);

    Task<IReadOnlyList<FileFolderDto>> ListFoldersAsync(Guid tenantId, CancellationToken ct = default);
    Task<FileFolderDto> UpsertFolderAsync(Guid tenantId, Guid? actorId, FileFolderDto req, CancellationToken ct = default);
    Task SoftDeleteFileAsync(Guid tenantId, Guid fileId, CancellationToken ct = default);
    Task RestoreFileAsync(Guid tenantId, Guid fileId, CancellationToken ct = default);
    Task<IReadOnlyList<FileObjectDto>> ListFilesAsync(Guid tenantId, Guid? folderId, CancellationToken ct = default);
    Task<FileObjectDto> UploadFileMetadataAsync(Guid tenantId, Guid? actorId, FileUploadRequest req, CancellationToken ct = default);
    Task<FileObjectDto> GetFileObjectAsync(Guid tenantId, Guid fileId, CancellationToken ct = default);

    Task<IReadOnlyList<ApiKeyDto>> ListApiKeysAsync(Guid tenantId, CancellationToken ct = default);
    Task<ApiKeyCreatedDto> CreateApiKeyAsync(Guid tenantId, Guid? actorId, string name, CancellationToken ct = default);
    Task RevokeApiKeyAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<WebhookDto>> ListWebhooksAsync(Guid tenantId, CancellationToken ct = default);
    Task<WebhookDto> UpsertWebhookAsync(Guid tenantId, Guid? actorId, WebhookDto req, CancellationToken ct = default);
    Task<IReadOnlyList<IntegrationLogDto>> ListIntegrationLogsAsync(Guid tenantId, int take, CancellationToken ct = default);
    Task<IReadOnlyList<ExternalIntegrationDto>> ListIntegrationsAsync(Guid tenantId, CancellationToken ct = default);
    Task<ExternalIntegrationDto> UpsertIntegrationAsync(Guid tenantId, Guid? actorId, ExternalIntegrationDto req, CancellationToken ct = default);

    /// <summary>UC_SYS_060/061 — render MessageTemplate + ghi IntegrationCallLog (+ outbox), không gọi SMTP/SMS gateway.</summary>
    Task<ChannelSendResultDto> SendChannelMessageAsync(
        Guid tenantId, Guid? actorId, ChannelSendRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<LocalePackDto>> ListLocalePacksAsync(Guid tenantId, CancellationToken ct = default);
    Task SetUserLocaleAsync(Guid tenantId, Guid userId, string localeCode, CancellationToken ct = default);

    Task<IReadOnlyList<RoleMatrixRowDto>> GetPermissionMatrixAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<PermissionChangeLogDto>> ListPermissionChangeLogsAsync(Guid tenantId, int take, CancellationToken ct = default);

    Task EnsureMaxUsersAsync(Guid tenantId, CancellationToken ct = default);
    Task EnsureMaxOrgUnitsAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>UC_SYS_069 — gắn file vào đối tượng nghiệp vụ (LinkedEntity ACL).</summary>
    Task LinkFileToEntityAsync(Guid tenantId, Guid? actorId, LinkFileToEntityRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<LinkedFileDto>> ListLinkedFilesAsync(Guid tenantId, string entityType, Guid entityId, CancellationToken ct = default);
    Task UnlinkFileFromEntityAsync(Guid tenantId, Guid fileId, string entityType, Guid entityId, CancellationToken ct = default);

    /// <summary>UC_SYS_074/075 — export generic CSV/PDF theo EntityType.</summary>
    Task<GenericExportResult> ExportEntityDataAsync(Guid tenantId, Guid? actorId, GenericExportRequest req, CancellationToken ct = default);

    /// <summary>UC_SYS_076 — lịch sử job import/export.</summary>
    Task<IReadOnlyList<ImportExportJobDto>> ListImportExportJobsAsync(Guid tenantId, int take, CancellationToken ct = default);

    // ── UC_SYS_078 / 080 / 081 — Audit Log ──

    /// <summary>UC_SYS_078 — nhật ký thao tác người dùng, lọc đa tiêu chí + phân trang.</summary>
    Task<(IReadOnlyList<AuditLogDto> Items, int Total)> ListAuditLogsAsync(Guid tenantId, AuditLogQueryRequest query, CancellationToken ct = default);

    /// <summary>UC_SYS_080 — xem chi tiết thay đổi từng field của một audit log record.</summary>
    Task<AuditLogDetailDto> GetAuditLogDetailAsync(Guid tenantId, Guid auditLogId, CancellationToken ct = default);

    /// <summary>UC_SYS_081 — xuất audit log ra CSV với bắt buộc có khoảng thời gian, tối đa 10.000 dòng.</summary>
    Task<GenericExportResult> ExportAuditLogCsvAsync(Guid tenantId, AuditLogExportRequest req, CancellationToken ct = default);

    // ── UC_SYS_083 — Chính sách hết hạn phiên ──

    /// <summary>UC_SYS_083 — lấy chính sách phiên hiện tại của tenant.</summary>
    Task<SessionPolicyDto> GetSessionPolicyAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>UC_SYS_083 — cập nhật chính sách phiên; validate min/max và revoke phiên hết hạn theo policy mới.</summary>
    Task<SessionPolicyDto> SetSessionPolicyAsync(Guid tenantId, SessionPolicyUpdateRequest req, CancellationToken ct = default);

    /// <summary>UC_SYS_083 — thu dọn tất cả UserSession đã hết hạn (batch purge).</summary>
    Task<int> PurgeExpiredSessionsAsync(Guid tenantId, CancellationToken ct = default);

    // ── UC_SYS_087 — Hàng đợi sự kiện liên module (Outbox Queue) ──
    Task<OutboxMessageDto> EnqueueOutboxAsync(Guid tenantId, EnqueueOutboxRequest req, CancellationToken ct = default);
    Task<(IReadOnlyList<OutboxMessageDto> Items, int Total)> ListOutboxMessagesAsync(Guid tenantId, OutboxQueryRequest query, CancellationToken ct = default);
    Task<OutboxBatchProcessResultDto> ProcessOutboxQueueAsync(Guid tenantId, int maxBatch = 50, CancellationToken ct = default);
    Task RetryOutboxMessageAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    // ── UC_SYS_088 — Kết nối Email Gateway ──
    Task<ExternalIntegrationDto> UpsertEmailGatewayAsync(Guid tenantId, Guid? actorId, UpsertEmailGatewayRequest req, CancellationToken ct = default);
    Task<TestGatewayResultDto> TestEmailGatewayAsync(Guid tenantId, Guid gatewayId, CancellationToken ct = default);
    Task<ChannelSendResultDto> SendTestEmailAsync(Guid tenantId, Guid? actorId, SendTestEmailRequest req, CancellationToken ct = default);

    // ── UC_SYS_089 — Kết nối SMS Gateway ──
    Task<ExternalIntegrationDto> UpsertSmsGatewayAsync(Guid tenantId, Guid? actorId, UpsertSmsGatewayRequest req, CancellationToken ct = default);
    Task<TestGatewayResultDto> TestSmsGatewayAsync(Guid tenantId, Guid gatewayId, CancellationToken ct = default);
    Task<ChannelSendResultDto> SendTestSmsAsync(Guid tenantId, Guid? actorId, SendTestSmsRequest req, CancellationToken ct = default);

    // ── UC_SYS_101 — Đính kèm file trong tin nhắn ──
    Task<ChatMessageAttachmentDto> SendChatMessageAsync(Guid tenantId, Guid senderUserId, SendChatMessageRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<ChatMessageAttachmentDto>> ListChatMessagesAsync(Guid tenantId, Guid conversationId, int take = 50, CancellationToken ct = default);
    Task RecallChatMessageAsync(Guid tenantId, Guid userId, Guid messageId, CancellationToken ct = default);

    // ── UC_HRM_001 / 002 / 003 / 004 — Cơ cấu tổ chức & Điểm bán ──
    Task<OrgUnitDetailDto> UpsertOrgUnitAsync(Guid tenantId, Guid? actorId, UpsertOrgUnitRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<OrgUnitDetailDto>> ListOrgUnitsAsync(Guid tenantId, string? unitType = null, CancellationToken ct = default);
    Task<OrgUnitDetailDto> GetOrgUnitDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task DeleteOrgUnitAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task DeleteSalesPointAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    // ── UC_HRM_006 / 010 / 012 — Lịch làm việc, Cấp bậc, Mã nhân sự ──
    Task<IReadOnlyList<JobLevelPolishDto>> ListJobLevelsAsync(Guid tenantId, CancellationToken ct = default);
    Task<JobLevelPolishDto> UpsertJobLevelAsync(Guid tenantId, Guid? actorId, UpsertJobLevelRequest req, CancellationToken ct = default);
    Task DeleteJobLevelAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<EmployeeCodeGeneratedDto> GenerateNextEmployeeCodeAsync(Guid tenantId, EmployeeCodeGenerateRequest req, CancellationToken ct = default);
}
