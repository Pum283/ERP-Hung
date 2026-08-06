using Erp.Domain.Entities.Mfg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Mfg;

public sealed class MfgItemConfig : IEntityTypeConfiguration<MfgItem>
{
    public void Configure(EntityTypeBuilder<MfgItem> b)
    {
        b.ToTable("item", "mfg");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.ItemType).HasMaxLength(10).IsRequired();
        b.Property(x => x.Unit).HasMaxLength(20).IsRequired();
        b.Property(x => x.StandardCost).HasPrecision(18, 4);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class MfgWorkshopConfig : IEntityTypeConfiguration<MfgWorkshop>
{
    public void Configure(EntityTypeBuilder<MfgWorkshop> b)
    {
        b.ToTable("workshop", "mfg");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.WorkshopType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class MfgBomConfig : IEntityTypeConfiguration<MfgBom>
{
    public void Configure(EntityTypeBuilder<MfgBom> b)
    {
        b.ToTable("bom", "mfg");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.ParentItemId, x.Version }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Version).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class MfgBomLineConfig : IEntityTypeConfiguration<MfgBomLine>
{
    public void Configure(EntityTypeBuilder<MfgBomLine> b)
    {
        b.ToTable("bom_line", "mfg");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.BomId, x.ComponentItemId });
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.Unit).HasMaxLength(20).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class MfgPlanConfig : IEntityTypeConfiguration<MfgPlan>
{
    public void Configure(EntityTypeBuilder<MfgPlan> b)
    {
        b.ToTable("plan", "mfg");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.SourceOrderCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class MfgPlanLineConfig : IEntityTypeConfiguration<MfgPlanLine>
{
    public void Configure(EntityTypeBuilder<MfgPlanLine> b)
    {
        b.ToTable("plan_line", "mfg");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PlanId });
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class MfgWorkOrderConfig : IEntityTypeConfiguration<MfgWorkOrder>
{
    public void Configure(EntityTypeBuilder<MfgWorkOrder> b)
    {
        b.ToTable("work_order", "mfg");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.QtyIssuedMaterial).HasPrecision(18, 4);
        b.Property(x => x.QtyFgReceived).HasPrecision(18, 4);
        b.Property(x => x.QtyScrap).HasPrecision(18, 4);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
        b.Property(x => x.CancelReason).HasMaxLength(500);
        b.Property(x => x.ResumeStatus).HasMaxLength(30);
    }
}

public sealed class MfgScrapConfig : IEntityTypeConfiguration<MfgScrap>
{
    public void Configure(EntityTypeBuilder<MfgScrap> b)
    {
        b.ToTable("scrap", "mfg");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.WorkOrderId });
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.Unit).HasMaxLength(20).IsRequired();
        b.Property(x => x.ScrapType).HasMaxLength(20).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class MfgMaterialIssueConfig : IEntityTypeConfiguration<MfgMaterialIssue>
{
    public void Configure(EntityTypeBuilder<MfgMaterialIssue> b)
    {
        b.ToTable("material_issue", "mfg");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.WorkOrderId });
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.UnitCost).HasPrecision(18, 4);
        b.Property(x => x.Unit).HasMaxLength(20).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class MfgFgReceiptConfig : IEntityTypeConfiguration<MfgFgReceipt>
{
    public void Configure(EntityTypeBuilder<MfgFgReceipt> b)
    {
        b.ToTable("fg_receipt", "mfg");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.WorkOrderId });
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.Unit).HasMaxLength(20).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class MfgCostSheetConfig : IEntityTypeConfiguration<MfgCostSheet>
{
    public void Configure(EntityTypeBuilder<MfgCostSheet> b)
    {
        b.ToTable("cost_sheet", "mfg");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.WorkOrderId });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.MaterialCost).HasPrecision(18, 2);
        b.Property(x => x.LaborCost).HasPrecision(18, 2);
        b.Property(x => x.OverheadCost).HasPrecision(18, 2);
        b.Property(x => x.TotalCost).HasPrecision(18, 2);
        b.Property(x => x.GoodQty).HasPrecision(18, 4);
        b.Property(x => x.UnitCost).HasPrecision(18, 4);
        b.Property(x => x.InvSkuCode).HasMaxLength(40);
        b.Property(x => x.FinJournalCode).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class MfgCostSheetLineConfig : IEntityTypeConfiguration<MfgCostSheetLine>
{
    public void Configure(EntityTypeBuilder<MfgCostSheetLine> b)
    {
        b.ToTable("cost_sheet_line", "mfg");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.CostSheetId });
        b.Property(x => x.Source).HasMaxLength(20).IsRequired();
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.UnitCost).HasPrecision(18, 4);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}
