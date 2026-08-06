using Erp.Domain.Entities.Pjm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Pjm;

public sealed class PjmProjectTypeConfig : IEntityTypeConfiguration<PjmProjectType>
{
    public void Configure(EntityTypeBuilder<PjmProjectType> b)
    {
        b.ToTable("project_type", "pjm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class PjmProjectStatusConfig : IEntityTypeConfiguration<PjmProjectStatus>
{
    public void Configure(EntityTypeBuilder<PjmProjectStatus> b)
    {
        b.ToTable("project_status", "pjm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
    }
}

public sealed class PjmWbsTemplateConfig : IEntityTypeConfiguration<PjmWbsTemplate>
{
    public void Configure(EntityTypeBuilder<PjmWbsTemplate> b)
    {
        b.ToTable("wbs_template", "pjm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class PjmWbsTemplateItemConfig : IEntityTypeConfiguration<PjmWbsTemplateItem>
{
    public void Configure(EntityTypeBuilder<PjmWbsTemplateItem> b)
    {
        b.ToTable("wbs_template_item", "pjm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.TemplateId, x.Code });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
    }
}

public sealed class PjmProjectConfig : IEntityTypeConfiguration<PjmProject>
{
    public void Configure(EntityTypeBuilder<PjmProject> b)
    {
        b.ToTable("project", "pjm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.StatusCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.CustomerName).HasMaxLength(200);
        b.Property(x => x.ContractCode).HasMaxLength(40);
        b.Property(x => x.SourceOpportunityCode).HasMaxLength(40);
        b.Property(x => x.PmName).HasMaxLength(120);
        b.Property(x => x.Budget).HasPrecision(18, 2);
        b.Property(x => x.RecognizedRevenue).HasPrecision(18, 2);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class PjmProjectMemberConfig : IEntityTypeConfiguration<PjmProjectMember>
{
    public void Configure(EntityTypeBuilder<PjmProjectMember> b)
    {
        b.ToTable("project_member", "pjm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ProjectId, x.UserId }).IsUnique();
        b.Property(x => x.Role).HasMaxLength(30).IsRequired();
        b.Property(x => x.AllocationPct).HasPrecision(5, 2);
    }
}

public sealed class PjmExpenseConfig : IEntityTypeConfiguration<PjmExpense>
{
    public void Configure(EntityTypeBuilder<PjmExpense> b)
    {
        b.ToTable("expense", "pjm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.ProjectId, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Category).HasMaxLength(40).IsRequired();
        b.Property(x => x.Description).HasMaxLength(300).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class PjmMaterialIssueConfig : IEntityTypeConfiguration<PjmMaterialIssue>
{
    public void Configure(EntityTypeBuilder<PjmMaterialIssue> b)
    {
        b.ToTable("material_issue", "pjm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.ProjectId });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
        b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.MaterialIssueId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class PjmMaterialIssueLineConfig : IEntityTypeConfiguration<PjmMaterialIssueLine>
{
    public void Configure(EntityTypeBuilder<PjmMaterialIssueLine> b)
    {
        b.ToTable("material_issue_line", "pjm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.MaterialIssueId });
        b.Property(x => x.ProductCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Unit).HasMaxLength(20).IsRequired();
        b.Property(x => x.Qty).HasPrecision(18, 3);
        b.Property(x => x.UnitCost).HasPrecision(18, 2);
    }
}

public sealed class PjmAcceptanceConfig : IEntityTypeConfiguration<PjmAcceptance>
{
    public void Configure(EntityTypeBuilder<PjmAcceptance> b)
    {
        b.ToTable("acceptance", "pjm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.ProjectId, x.Kind });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Kind).HasMaxLength(20).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.SignerName).HasMaxLength(120);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class PjmWbsItemConfig : IEntityTypeConfiguration<PjmWbsItem>
{
    public void Configure(EntityTypeBuilder<PjmWbsItem> b)
    {
        b.ToTable("wbs_item", "pjm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ProjectId, x.Code });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.AssigneeName).HasMaxLength(120);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
        b.Property(x => x.PercentComplete).HasPrecision(5, 2);
        b.HasIndex(x => new { x.TenantId, x.DueDate });
    }
}
