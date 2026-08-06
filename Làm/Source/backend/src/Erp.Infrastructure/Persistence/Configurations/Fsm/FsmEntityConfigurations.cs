using Erp.Domain.Entities.Fsm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Fsm;

public sealed class FsmServiceTypeConfig : IEntityTypeConfiguration<FsmServiceType>
{
    public void Configure(EntityTypeBuilder<FsmServiceType> b)
    {
        b.ToTable("service_type", "fsm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FsmFaultCodeConfig : IEntityTypeConfiguration<FsmFaultCode>
{
    public void Configure(EntityTypeBuilder<FsmFaultCode> b)
    {
        b.ToTable("fault_code", "fsm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Severity).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FsmPartConfig : IEntityTypeConfiguration<FsmPart>
{
    public void Configure(EntityTypeBuilder<FsmPart> b)
    {
        b.ToTable("part", "fsm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Unit).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FsmSlaPolicyConfig : IEntityTypeConfiguration<FsmSlaPolicy>
{
    public void Configure(EntityTypeBuilder<FsmSlaPolicy> b)
    {
        b.ToTable("sla_policy", "fsm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Priority });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Priority).HasMaxLength(20).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FsmAssetConfig : IEntityTypeConfiguration<FsmAsset>
{
    public void Configure(EntityTypeBuilder<FsmAsset> b)
    {
        b.ToTable("asset", "fsm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.SerialNo });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        b.Property(x => x.CustomerPhone).HasMaxLength(40);
        b.Property(x => x.SerialNo).HasMaxLength(80).IsRequired();
        b.Property(x => x.Model).HasMaxLength(120);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Address).HasMaxLength(500);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class FsmAssetHistoryConfig : IEntityTypeConfiguration<FsmAssetHistory>
{
    public void Configure(EntityTypeBuilder<FsmAssetHistory> b)
    {
        b.ToTable("asset_history", "fsm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.AssetId, x.OccurredAt });
        b.Property(x => x.EventType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Summary).HasMaxLength(500).IsRequired();
    }
}

public sealed class FsmTicketConfig : IEntityTypeConfiguration<FsmTicket>
{
    public void Configure(EntityTypeBuilder<FsmTicket> b)
    {
        b.ToTable("ticket", "fsm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Channel).HasMaxLength(30).IsRequired();
        b.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        b.Property(x => x.CustomerPhone).HasMaxLength(40);
        b.Property(x => x.Priority).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.AssignedTechName).HasMaxLength(120);
        b.Property(x => x.EscalateReason).HasMaxLength(500);
        b.Property(x => x.AppointmentNote).HasMaxLength(500);
        b.Property(x => x.RootCause).HasMaxLength(1000);
        b.Property(x => x.ResolutionNote).HasMaxLength(2000);
        b.Property(x => x.AcceptanceSignerName).HasMaxLength(120);
        b.Property(x => x.AcceptanceNote).HasMaxLength(500);
        b.HasIndex(x => new { x.TenantId, x.Status, x.DueResolveAt });
    }
}

public sealed class FsmPartStockConfig : IEntityTypeConfiguration<FsmPartStock>
{
    public void Configure(EntityTypeBuilder<FsmPartStock> b)
    {
        b.ToTable("part_stock", "fsm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PartId, x.LocationType, x.TechUserId }).IsUnique();
        b.Property(x => x.LocationType).HasMaxLength(20).IsRequired();
        b.Property(x => x.TechName).HasMaxLength(120);
        b.Property(x => x.QtyOnHand).HasPrecision(18, 3);
        b.Property(x => x.UnitCost).HasPrecision(18, 2);
    }
}

public sealed class FsmPartIssueDocConfig : IEntityTypeConfiguration<FsmPartIssueDoc>
{
    public void Configure(EntityTypeBuilder<FsmPartIssueDoc> b)
    {
        b.ToTable("part_issue_doc", "fsm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.TechName).HasMaxLength(120).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
        b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.IssueDocId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class FsmPartIssueLineConfig : IEntityTypeConfiguration<FsmPartIssueLine>
{
    public void Configure(EntityTypeBuilder<FsmPartIssueLine> b)
    {
        b.ToTable("part_issue_line", "fsm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.IssueDocId });
        b.Property(x => x.Qty).HasPrecision(18, 3);
        b.Property(x => x.UnitCost).HasPrecision(18, 2);
    }
}

public sealed class FsmPartReconcileDocConfig : IEntityTypeConfiguration<FsmPartReconcileDoc>
{
    public void Configure(EntityTypeBuilder<FsmPartReconcileDoc> b)
    {
        b.ToTable("part_reconcile_doc", "fsm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Scope).HasMaxLength(20).IsRequired();
        b.Property(x => x.TechName).HasMaxLength(120);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
        b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.ReconcileDocId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class FsmPartReconcileLineConfig : IEntityTypeConfiguration<FsmPartReconcileLine>
{
    public void Configure(EntityTypeBuilder<FsmPartReconcileLine> b)
    {
        b.ToTable("part_reconcile_line", "fsm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ReconcileDocId });
        b.Property(x => x.SystemQty).HasPrecision(18, 3);
        b.Property(x => x.CountedQty).HasPrecision(18, 3);
        b.Property(x => x.DiffQty).HasPrecision(18, 3);
        b.Property(x => x.UnitCost).HasPrecision(18, 2);
    }
}

public sealed class FsmTicketPartLineConfig : IEntityTypeConfiguration<FsmTicketPartLine>
{
    public void Configure(EntityTypeBuilder<FsmTicketPartLine> b)
    {
        b.ToTable("ticket_part_line", "fsm");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.TicketId, x.IssuedAt });
        b.Property(x => x.Source).HasMaxLength(20).IsRequired();
        b.Property(x => x.TechName).HasMaxLength(120);
        b.Property(x => x.Note).HasMaxLength(500);
        b.Property(x => x.Qty).HasPrecision(18, 3);
        b.Property(x => x.UnitCost).HasPrecision(18, 2);
    }
}
