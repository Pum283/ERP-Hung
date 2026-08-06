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

    Task<IReadOnlyList<ApiKeyDto>> ListApiKeysAsync(Guid tenantId, CancellationToken ct = default);
    Task<ApiKeyCreatedDto> CreateApiKeyAsync(Guid tenantId, Guid? actorId, string name, CancellationToken ct = default);
    Task RevokeApiKeyAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<WebhookDto>> ListWebhooksAsync(Guid tenantId, CancellationToken ct = default);
    Task<WebhookDto> UpsertWebhookAsync(Guid tenantId, Guid? actorId, WebhookDto req, CancellationToken ct = default);
    Task<IReadOnlyList<IntegrationLogDto>> ListIntegrationLogsAsync(Guid tenantId, int take, CancellationToken ct = default);
    Task<IReadOnlyList<ExternalIntegrationDto>> ListIntegrationsAsync(Guid tenantId, CancellationToken ct = default);
    Task<ExternalIntegrationDto> UpsertIntegrationAsync(Guid tenantId, Guid? actorId, ExternalIntegrationDto req, CancellationToken ct = default);

    Task<IReadOnlyList<LocalePackDto>> ListLocalePacksAsync(Guid tenantId, CancellationToken ct = default);
    Task SetUserLocaleAsync(Guid tenantId, Guid userId, string localeCode, CancellationToken ct = default);

    Task<IReadOnlyList<RoleMatrixRowDto>> GetPermissionMatrixAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<PermissionChangeLogDto>> ListPermissionChangeLogsAsync(Guid tenantId, int take, CancellationToken ct = default);

    Task EnsureMaxUsersAsync(Guid tenantId, CancellationToken ct = default);
    Task EnsureMaxOrgUnitsAsync(Guid tenantId, CancellationToken ct = default);
}
