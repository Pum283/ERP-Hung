using Erp.Domain.Entities.Hrm;
using Erp.Domain.Entities.Mod;
using Erp.Domain.Entities.Wf;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Mod;

public sealed class ModMasterConfig : IEntityTypeConfiguration<ModMaster>
{
    public void Configure(EntityTypeBuilder<ModMaster> b)
    {
        b.ToTable("mod_master", "erp_sys");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ModuleCode, x.RecordType, x.Code }).IsUnique();
        b.Property(x => x.ModuleCode).HasMaxLength(10);
        b.Property(x => x.RecordType).HasMaxLength(40);
        b.Property(x => x.Code).HasMaxLength(60);
        b.Property(x => x.Name).HasMaxLength(200);
        b.Property(x => x.Status).HasMaxLength(30);
        b.Property(x => x.PayloadJson).HasMaxLength(8000);
    }
}

public sealed class ModDocumentConfig : IEntityTypeConfiguration<ModDocument>
{
    public void Configure(EntityTypeBuilder<ModDocument> b)
    {
        b.ToTable("mod_document", "erp_sys");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ModuleCode, x.DocType, x.DocNo }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.ModuleCode, x.Status });
        b.Property(x => x.ModuleCode).HasMaxLength(10);
        b.Property(x => x.DocType).HasMaxLength(40);
        b.Property(x => x.DocNo).HasMaxLength(60);
        b.Property(x => x.Title).HasMaxLength(300);
        b.Property(x => x.Status).HasMaxLength(30);
        b.Property(x => x.PayloadJson).HasMaxLength(8000);
    }
}

public sealed class EmploymentStatusChangeConfig : IEntityTypeConfiguration<EmploymentStatusChange>
{
    public void Configure(EntityTypeBuilder<EmploymentStatusChange> b)
    {
        b.ToTable("employment_status_change", "hrm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.EmployeeId, x.EffectiveDate });
        b.Property(x => x.FromStatus).HasMaxLength(40);
        b.Property(x => x.ToStatus).HasMaxLength(40);
        b.Property(x => x.Reason).HasMaxLength(400);
    }
}

public sealed class WorkTypeConfig : IEntityTypeConfiguration<WorkType>
{
    public void Configure(EntityTypeBuilder<WorkType> b)
    {
        b.ToTable("work_type", "wf");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40);
        b.Property(x => x.Name).HasMaxLength(200);
    }
}

public sealed class WorkProjectConfig : IEntityTypeConfiguration<WorkProject>
{
    public void Configure(EntityTypeBuilder<WorkProject> b)
    {
        b.ToTable("work_project", "wf");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40);
        b.Property(x => x.Name).HasMaxLength(200);
    }
}

public sealed class WorkItemConfig : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> b)
    {
        b.ToTable("work_item", "wf");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Status, x.AssigneeUserId });
        b.Property(x => x.Kind).HasMaxLength(20);
        b.Property(x => x.Title).HasMaxLength(300);
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.Status).HasMaxLength(30);
        b.Property(x => x.Priority).HasMaxLength(20);
    }
}
