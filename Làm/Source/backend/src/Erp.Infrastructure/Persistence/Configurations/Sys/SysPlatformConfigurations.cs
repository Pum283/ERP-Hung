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
