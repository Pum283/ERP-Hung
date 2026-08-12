using Erp.Domain.Entities.Sys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Sys;

public sealed class TenantConfig : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("tenant");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Code).HasMaxLength(30).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20);
        b.Property(x => x.Timezone).HasMaxLength(64);
        b.Property(x => x.DefaultLocale).HasMaxLength(10);
        b.Property(x => x.DefaultCurrency).HasMaxLength(3);
        b.Property(x => x.LogoUrl).HasMaxLength(1000);
        b.Property(x => x.LogoStorageKey).HasMaxLength(500);
        b.Property(x => x.PrimaryColor).HasMaxLength(16);
        b.Property(x => x.AccentColor).HasMaxLength(16);
        b.Property(x => x.FaviconUrl).HasMaxLength(1000);
        b.Property(x => x.FaviconStorageKey).HasMaxLength(500);
    }
}

public sealed class OrgUnitConfig : IEntityTypeConfiguration<OrgUnit>
{
    public void Configure(EntityTypeBuilder<OrgUnit> b)
    {
        b.ToTable("org_unit");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.UnitType).HasMaxLength(30);
        b.Property(x => x.Path).HasMaxLength(500);
    }
}

public sealed class DepartmentConfig : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.ToTable("department");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Path });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Path).HasMaxLength(500);
    }
}

public sealed class JobLevelConfig : IEntityTypeConfiguration<JobLevel>
{
    public void Configure(EntityTypeBuilder<JobLevel> b)
    {
        b.ToTable("job_level");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.DefaultScopeType).HasConversion<int>();
    }
}

public sealed class AppUserConfig : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.ToTable("app_user");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Username }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Email });
        b.Property(x => x.Username).HasMaxLength(100).IsRequired();
        b.Property(x => x.DisplayName).HasMaxLength(200);
        b.Property(x => x.Email).HasMaxLength(255);
        b.Property(x => x.Phone).HasMaxLength(30);
        b.Property(x => x.PasswordHash).HasMaxLength(255);
        b.Property(x => x.Status).HasConversion<int>();
    }
}

public sealed class RoleConfig : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("role");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
    }
}

public sealed class PermissionConfig : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("permission");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.ModuleCode).HasMaxLength(10).IsRequired();
        b.Property(x => x.Code).HasMaxLength(100).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Resource).HasMaxLength(80).IsRequired();
        b.Property(x => x.Action).HasMaxLength(40).IsRequired();
    }
}

public sealed class RolePermissionConfig : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> b)
    {
        b.ToTable("role_permission");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
    }
}

public sealed class UserRoleConfig : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.ToTable("user_role");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.UserId, x.RoleId });
    }
}

public sealed class LicenseConfig : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> b)
    {
        b.ToTable("license");
        b.HasKey(x => x.Id);
        b.Property(x => x.PlanCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20);
    }
}

public sealed class LicenseModuleConfig : IEntityTypeConfiguration<LicenseModule>
{
    public void Configure(EntityTypeBuilder<LicenseModule> b)
    {
        b.ToTable("license_module");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.LicenseId, x.ModuleCode }).IsUnique();
        b.Property(x => x.ModuleCode).HasMaxLength(10).IsRequired();
    }
}

public sealed class UserDepartmentConfig : IEntityTypeConfiguration<UserDepartment>
{
    public void Configure(EntityTypeBuilder<UserDepartment> b)
    {
        b.ToTable("user_department");
        b.HasKey(x => x.Id);
        b.Property(x => x.JobLevelId);
        b.HasIndex(x => new { x.UserId, x.DepartmentId }).IsUnique();
        b.HasIndex(x => new { x.UserId, x.IsPrimary });
    }
}

public sealed class UserDataScopeConfig : IEntityTypeConfiguration<UserDataScope>
{
    public void Configure(EntityTypeBuilder<UserDataScope> b)
    {
        b.ToTable("user_data_scope");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.UserId, x.Dimension, x.ScopeId });
        b.Property(x => x.Dimension).HasMaxLength(30);
        b.Property(x => x.AccessLevel).HasMaxLength(20);
        b.Property(x => x.Source).HasMaxLength(30);
    }
}

public sealed class MenuItemConfig : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> b)
    {
        b.ToTable("menu_item");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(80);
        b.Property(x => x.ModuleCode).HasMaxLength(10);
        b.Property(x => x.Title).HasMaxLength(200);
        b.Property(x => x.RoutePath).HasMaxLength(255);
        b.Property(x => x.PermissionCode).HasMaxLength(100);
        b.Property(x => x.Icon).HasMaxLength(80);
    }
}

public sealed class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("audit_log");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.CreatedAt });
        b.Property(x => x.EntityType).HasMaxLength(80);
        b.Property(x => x.Action).HasMaxLength(30);
        b.Property(x => x.IpAddress).HasMaxLength(45);
    }
}

public sealed class OutboxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("outbox_message");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Status, x.NextAttemptAt });
        b.HasIndex(x => x.CorrelationId);
        b.Property(x => x.EventType).HasMaxLength(120).IsRequired();
        b.Property(x => x.SourceModule).HasMaxLength(10).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.LastError).HasMaxLength(1000);
    }
}

public sealed class InboxMessageConfig : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> b)
    {
        b.ToTable("inbox_message");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.EventId, x.Consumer }).IsUnique();
        b.Property(x => x.Consumer).HasMaxLength(80).IsRequired();
        b.Property(x => x.EventType).HasMaxLength(120).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.ResultNote).HasMaxLength(500);
    }
}

public sealed class IdempotencyRecordConfig : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> b)
    {
        b.ToTable("idempotency_record");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
        b.Property(x => x.Key).HasMaxLength(120).IsRequired();
        b.Property(x => x.RequestPath).HasMaxLength(255).IsRequired();
    }
}

public sealed class ConversationConfig : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> b)
    {
        b.ToTable("conversation");
        b.HasKey(x => x.Id);
        b.Property(x => x.Kind).HasMaxLength(20).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200);
        b.Property(x => x.DirectKey).HasMaxLength(80);
        b.HasIndex(x => new { x.TenantId, x.DirectKey }).IsUnique().HasFilter("[DirectKey] IS NOT NULL AND [IsDeleted] = 0");
    }
}

public sealed class ConversationMemberConfig : IEntityTypeConfiguration<ConversationMember>
{
    public void Configure(EntityTypeBuilder<ConversationMember> b)
    {
        b.ToTable("conversation_member");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ConversationId, x.UserId }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.UserId });
    }
}

public sealed class ChatMessageConfig : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> b)
    {
        b.ToTable("chat_message");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ConversationId, x.SentAt });
        b.Property(x => x.Body).HasMaxLength(4000).IsRequired();
        b.Property(x => x.AttachmentStorageKey).HasMaxLength(500);
    }
}

public sealed class ChatMessageReactionConfig : IEntityTypeConfiguration<ChatMessageReaction>
{
    public void Configure(EntityTypeBuilder<ChatMessageReaction> b)
    {
        b.ToTable("chat_message_reaction");
        b.HasKey(x => x.Id);
        b.Property(x => x.ReactionType).HasMaxLength(32).IsRequired();
        b.HasIndex(x => new { x.TenantId, x.MessageId, x.UserId, x.ReactionType }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.MessageId });
    }
}
