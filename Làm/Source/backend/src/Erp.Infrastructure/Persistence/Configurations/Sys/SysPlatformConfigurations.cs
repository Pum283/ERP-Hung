using Erp.Domain.Entities.Sys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Sys;

public sealed class SystemSettingConfig : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> b)
    {
        b.ToTable("system_setting");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        b.Property(x => x.Key).HasMaxLength(100).IsRequired();
        b.Property(x => x.ValueJson).HasMaxLength(8000);
    }
}

public sealed class LoginAuditConfig : IEntityTypeConfiguration<LoginAudit>
{
    public void Configure(EntityTypeBuilder<LoginAudit> b)
    {
        b.ToTable("login_audit");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.AttemptedAt });
        b.Property(x => x.Username).HasMaxLength(100);
        b.Property(x => x.IpAddress).HasMaxLength(64);
        b.Property(x => x.UserAgent).HasMaxLength(400);
        b.Property(x => x.FailureReason).HasMaxLength(200);
    }
}

public sealed class LookupCategoryConfig : IEntityTypeConfiguration<LookupCategory>
{
    public void Configure(EntityTypeBuilder<LookupCategory> b)
    {
        b.ToTable("lookup_category");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(50);
        b.Property(x => x.Name).HasMaxLength(200);
    }
}

public sealed class LookupItemConfig : IEntityTypeConfiguration<LookupItem>
{
    public void Configure(EntityTypeBuilder<LookupItem> b)
    {
        b.ToTable("lookup_item");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.CategoryId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(50);
        b.Property(x => x.Name).HasMaxLength(200);
    }
}

public sealed class NumberSequenceConfig : IEntityTypeConfiguration<NumberSequence>
{
    public void Configure(EntityTypeBuilder<NumberSequence> b)
    {
        b.ToTable("number_sequence");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.DocType }).IsUnique();
        b.Property(x => x.DocType).HasMaxLength(80);
        b.Property(x => x.Pattern).HasMaxLength(100);
    }
}

public sealed class AppNotificationConfig : IEntityTypeConfiguration<AppNotification>
{
    public void Configure(EntityTypeBuilder<AppNotification> b)
    {
        b.ToTable("app_notification");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.UserId, x.IsRead });
        b.Property(x => x.Title).HasMaxLength(200);
        b.Property(x => x.Body).HasMaxLength(2000);
        b.Property(x => x.Link).HasMaxLength(400);
        b.Property(x => x.EventType).HasMaxLength(80);
    }
}

public sealed class NotificationRuleConfig : IEntityTypeConfiguration<NotificationRule>
{
    public void Configure(EntityTypeBuilder<NotificationRule> b)
    {
        b.ToTable("notification_rule");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.EventType }).IsUnique();
        b.Property(x => x.EventType).HasMaxLength(80);
        b.Property(x => x.TitleTemplate).HasMaxLength(200);
        b.Property(x => x.BodyTemplate).HasMaxLength(2000);
    }
}

public sealed class PasswordResetTokenConfig : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> b)
    {
        b.ToTable("password_reset_token");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.UserId });
        b.Property(x => x.TokenHash).HasMaxLength(128);
        b.Property(x => x.OtpCode).HasMaxLength(12);
    }
}

public sealed class UserSessionConfig : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> b)
    {
        b.ToTable("user_session");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.UserId });
        b.HasIndex(x => x.SessionKey).IsUnique();
        b.Property(x => x.SessionKey).HasMaxLength(64);
        b.Property(x => x.IpAddress).HasMaxLength(64);
        b.Property(x => x.UserAgent).HasMaxLength(400);
    }
}

public sealed class LegalEntityConfig : IEntityTypeConfiguration<LegalEntity>
{
    public void Configure(EntityTypeBuilder<LegalEntity> b)
    {
        b.ToTable("legal_entity");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40);
        b.Property(x => x.Name).HasMaxLength(200);
        b.Property(x => x.TaxCode).HasMaxLength(40);
    }
}

public sealed class SalesPointConfig : IEntityTypeConfiguration<SalesPoint>
{
    public void Configure(EntityTypeBuilder<SalesPoint> b)
    {
        b.ToTable("sales_point");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40);
        b.Property(x => x.Name).HasMaxLength(200);
        b.Property(x => x.Address).HasMaxLength(400);
    }
}

public sealed class ProvinceConfig : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> b)
    {
        b.ToTable("province");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(20);
        b.Property(x => x.Name).HasMaxLength(100);
    }
}

public sealed class WorkCalendarConfig : IEntityTypeConfiguration<WorkCalendar>
{
    public void Configure(EntityTypeBuilder<WorkCalendar> b)
    {
        b.ToTable("work_calendar");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40);
        b.Property(x => x.Name).HasMaxLength(200);
        b.Property(x => x.WeekMask).HasMaxLength(7);
    }
}

public sealed class MessageTemplateConfig : IEntityTypeConfiguration<MessageTemplate>
{
    public void Configure(EntityTypeBuilder<MessageTemplate> b)
    {
        b.ToTable("message_template");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(50);
        b.Property(x => x.Channel).HasMaxLength(20);
        b.Property(x => x.Subject).HasMaxLength(300);
        b.Property(x => x.Body).HasMaxLength(8000);
    }
}

public sealed class ApiKeyConfig : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> b)
    {
        b.ToTable("api_key");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.KeyPrefix });
        b.Property(x => x.Name).HasMaxLength(100);
        b.Property(x => x.KeyPrefix).HasMaxLength(16);
        b.Property(x => x.KeyHash).HasMaxLength(128);
    }
}

public sealed class WebhookSubscriptionConfig : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(EntityTypeBuilder<WebhookSubscription> b)
    {
        b.ToTable("webhook_subscription");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(100);
        b.Property(x => x.TargetUrl).HasMaxLength(500);
        b.Property(x => x.EventTypes).HasMaxLength(400);
        b.Property(x => x.Secret).HasMaxLength(100);
    }
}

public sealed class IntegrationCallLogConfig : IEntityTypeConfiguration<IntegrationCallLog>
{
    public void Configure(EntityTypeBuilder<IntegrationCallLog> b)
    {
        b.ToTable("integration_call_log");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.CalledAt });
        b.Property(x => x.Kind).HasMaxLength(30);
        b.Property(x => x.Target).HasMaxLength(500);
        b.Property(x => x.RequestSummary).HasMaxLength(1000);
        b.Property(x => x.ResponseSummary).HasMaxLength(1000);
    }
}

public sealed class PermissionChangeLogConfig : IEntityTypeConfiguration<PermissionChangeLog>
{
    public void Configure(EntityTypeBuilder<PermissionChangeLog> b)
    {
        b.ToTable("permission_change_log");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.CreatedAt });
        b.Property(x => x.ChangeType).HasMaxLength(40);
        b.Property(x => x.DetailJson).HasMaxLength(4000);
    }
}

public sealed class LocalePackConfig : IEntityTypeConfiguration<LocalePack>
{
    public void Configure(EntityTypeBuilder<LocalePack> b)
    {
        b.ToTable("locale_pack");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(10);
        b.Property(x => x.Name).HasMaxLength(100);
    }
}

public sealed class FileObjectConfig : IEntityTypeConfiguration<FileObject>
{
    public void Configure(EntityTypeBuilder<FileObject> b)
    {
        b.ToTable("file_object");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.StorageKey }).IsUnique();
        b.Property(x => x.StorageKey).HasMaxLength(400);
        b.Property(x => x.FileName).HasMaxLength(260);
        b.Property(x => x.ContentType).HasMaxLength(120);
        b.Property(x => x.LinkedEntityType).HasMaxLength(80);
    }
}

public sealed class FileFolderConfig : IEntityTypeConfiguration<FileFolder>
{
    public void Configure(EntityTypeBuilder<FileFolder> b)
    {
        b.ToTable("file_folder");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200);
    }
}

public sealed class ExternalIntegrationConfig : IEntityTypeConfiguration<ExternalIntegration>
{
    public void Configure(EntityTypeBuilder<ExternalIntegration> b)
    {
        b.ToTable("external_integration");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40);
        b.Property(x => x.Name).HasMaxLength(200);
        b.Property(x => x.Kind).HasMaxLength(40);
        b.Property(x => x.ConfigJson).HasMaxLength(8000);
    }
}

public sealed class SysConfigVersionConfig : IEntityTypeConfiguration<SysConfigVersion>
{
    public void Configure(EntityTypeBuilder<SysConfigVersion> b)
    {
        b.ToTable("sys_config_version");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ConfigKey, x.VersionNumber }).IsUnique();
        b.Property(x => x.ConfigKey).HasMaxLength(100).IsRequired();
        b.Property(x => x.ConfigValue).HasMaxLength(8000);
        b.Property(x => x.CommitNote).HasMaxLength(400);
    }
}

public sealed class SysSsoProviderConfig : IEntityTypeConfiguration<SysSsoProvider>
{
    public void Configure(EntityTypeBuilder<SysSsoProvider> b)
    {
        b.ToTable("sys_sso_provider");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.DisplayName).HasMaxLength(120);
        b.Property(x => x.ClientId).HasMaxLength(200);
        b.Property(x => x.ClientSecret).HasMaxLength(400);
        b.Property(x => x.AuthorityUrl).HasMaxLength(400);
        b.Property(x => x.RedirectUri).HasMaxLength(400);
        b.Property(x => x.Scopes).HasMaxLength(200);
        b.Property(x => x.Note).HasMaxLength(400);
    }
}

public sealed class SysExternalLoginConfig : IEntityTypeConfiguration<SysExternalLogin>
{
    public void Configure(EntityTypeBuilder<SysExternalLogin> b)
    {
        b.ToTable("sys_external_login");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ProviderCode, x.ProviderSubject }).IsUnique();
        b.Property(x => x.ProviderCode).HasMaxLength(40);
        b.Property(x => x.ProviderSubject).HasMaxLength(200);
        b.Property(x => x.Email).HasMaxLength(200);
    }
}

public sealed class SysSensitiveFieldConfig : IEntityTypeConfiguration<SysSensitiveField>
{
    public void Configure(EntityTypeBuilder<SysSensitiveField> b)
    {
        b.ToTable("sys_sensitive_field");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ModuleCode, x.EntityName, x.FieldKey }).IsUnique();
        b.Property(x => x.ModuleCode).HasMaxLength(20);
        b.Property(x => x.EntityName).HasMaxLength(80);
        b.Property(x => x.FieldKey).HasMaxLength(80);
        b.Property(x => x.DisplayName).HasMaxLength(160);
        b.Property(x => x.DefaultMask).HasMaxLength(20);
    }
}

public sealed class SysRoleFieldPermissionConfig : IEntityTypeConfiguration<SysRoleFieldPermission>
{
    public void Configure(EntityTypeBuilder<SysRoleFieldPermission> b)
    {
        b.ToTable("sys_role_field_permission");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.RoleId, x.SensitiveFieldId }).IsUnique();
        b.Property(x => x.Access).HasMaxLength(20);
    }
}

public sealed class SysPushDeviceConfig : IEntityTypeConfiguration<SysPushDevice>
{
    public void Configure(EntityTypeBuilder<SysPushDevice> b)
    {
        b.ToTable("sys_push_device");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.DeviceToken }).IsUnique();
        b.Property(x => x.Platform).HasMaxLength(20);
        b.Property(x => x.DeviceToken).HasMaxLength(500);
        b.Property(x => x.AppVersion).HasMaxLength(40);
    }
}

public sealed class SysUserNotificationPreferenceConfig : IEntityTypeConfiguration<SysUserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<SysUserNotificationPreference> b)
    {
        b.ToTable("sys_user_notification_preference");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();
        b.Property(x => x.QuietHoursStart).HasMaxLength(8);
        b.Property(x => x.QuietHoursEnd).HasMaxLength(8);
    }
}

public sealed class SysFileScanLogConfig : IEntityTypeConfiguration<SysFileScanLog>
{
    public void Configure(EntityTypeBuilder<SysFileScanLog> b)
    {
        b.ToTable("sys_file_scan_log");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.FileObjectId, x.ScannedAt });
        b.Property(x => x.ScanStatus).HasMaxLength(20);
        b.Property(x => x.Engine).HasMaxLength(80);
        b.Property(x => x.ThreatName).HasMaxLength(200);
        b.Property(x => x.Detail).HasMaxLength(1000);
    }
}

public sealed class SysIpRuleConfig : IEntityTypeConfiguration<SysIpRule>
{
    public void Configure(EntityTypeBuilder<SysIpRule> b)
    {
        b.ToTable("sys_ip_rule");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.IpAddressOrCidr, x.RuleType });
        b.Property(x => x.IpAddressOrCidr).HasMaxLength(64).IsRequired();
        b.Property(x => x.RuleType).HasMaxLength(16);
        b.Property(x => x.Description).HasMaxLength(400);
    }
}

public sealed class SysRoleHomeConfigConfig : IEntityTypeConfiguration<SysRoleHomeConfig>
{
    public void Configure(EntityTypeBuilder<SysRoleHomeConfig> b)
    {
        b.ToTable("sys_role_home_config");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.RoleId }).IsUnique();
        b.Property(x => x.LandingPath).HasMaxLength(200).IsRequired();
        b.Property(x => x.Note).HasMaxLength(400);
    }
}
