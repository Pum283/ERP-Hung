using Erp.Domain.Entities.Inv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Inv;

public sealed class InvItemGroupConfig : IEntityTypeConfiguration<InvItemGroup>
{
    public void Configure(EntityTypeBuilder<InvItemGroup> b)
    {
        b.ToTable("item_group", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
    }
}

public sealed class InvUnitOfMeasureConfig : IEntityTypeConfiguration<InvUnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<InvUnitOfMeasure> b)
    {
        b.ToTable("uom", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
    }
}

public sealed class InvUnitConversionConfig : IEntityTypeConfiguration<InvUnitConversion>
{
    public void Configure(EntityTypeBuilder<InvUnitConversion> b)
    {
        b.ToTable("uom_conversion", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.FromUnitId, x.ToUnitId }).IsUnique();
        b.Property(x => x.Factor).HasPrecision(18, 6);
    }
}

public sealed class InvSkuConfig : IEntityTypeConfiguration<InvSku>
{
    public void Configure(EntityTypeBuilder<InvSku> b)
    {
        b.ToTable("sku", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.CostingMethod).HasMaxLength(30).IsRequired();
        b.Property(x => x.StandardCost).HasPrecision(18, 4);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.MinQty).HasPrecision(18, 4);
        b.Property(x => x.MaxQty).HasPrecision(18, 4);
        b.Property(x => x.ReorderQty).HasPrecision(18, 4);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class InvWarehouseTypeConfig : IEntityTypeConfiguration<InvWarehouseType>
{
    public void Configure(EntityTypeBuilder<InvWarehouseType> b)
    {
        b.ToTable("warehouse_type", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
    }
}

public sealed class InvWarehouseConfig : IEntityTypeConfiguration<InvWarehouse>
{
    public void Configure(EntityTypeBuilder<InvWarehouse> b)
    {
        b.ToTable("warehouse", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Address).HasMaxLength(500);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.PickPolicy).HasMaxLength(20).IsRequired();
    }
}

public sealed class InvWarehouseKeeperConfig : IEntityTypeConfiguration<InvWarehouseKeeper>
{
    public void Configure(EntityTypeBuilder<InvWarehouseKeeper> b)
    {
        b.ToTable("warehouse_keeper", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.WarehouseId, x.UserId }).IsUnique();
        b.Property(x => x.Role).HasMaxLength(30).IsRequired();
    }
}

public sealed class InvStockBalanceConfig : IEntityTypeConfiguration<InvStockBalance>
{
    public void Configure(EntityTypeBuilder<InvStockBalance> b)
    {
        b.ToTable("stock_balance", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.WarehouseId, x.SkuId, x.LotCode }).IsUnique();
        b.Property(x => x.LotCode).HasMaxLength(60);
        b.Property(x => x.QtyOnHand).HasPrecision(18, 4);
        b.Property(x => x.QtyReserved).HasPrecision(18, 4);
        b.Property(x => x.QtyInTransit).HasPrecision(18, 4);
    }
}

public sealed class InvStockDocConfig : IEntityTypeConfiguration<InvStockDoc>
{
    public void Configure(EntityTypeBuilder<InvStockDoc> b)
    {
        b.ToTable("stock_doc", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.DocType).HasMaxLength(20).IsRequired();
        b.Property(x => x.SourceType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.RefModule).HasMaxLength(20);
        b.Property(x => x.RefCode).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class InvStockDocLineConfig : IEntityTypeConfiguration<InvStockDocLine>
{
    public void Configure(EntityTypeBuilder<InvStockDocLine> b)
    {
        b.ToTable("stock_doc_line", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.DocId });
        b.Property(x => x.SkuCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.SkuName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.LotCode).HasMaxLength(60);
        b.Property(x => x.UnitCost).HasPrecision(18, 2);
    }
}

public sealed class InvTransferConfig : IEntityTypeConfiguration<InvTransfer>
{
    public void Configure(EntityTypeBuilder<InvTransfer> b)
    {
        b.ToTable("transfer", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class InvTransferLineConfig : IEntityTypeConfiguration<InvTransferLine>
{
    public void Configure(EntityTypeBuilder<InvTransferLine> b)
    {
        b.ToTable("transfer_line", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.TransferId });
        b.Property(x => x.SkuCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.SkuName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.LotCode).HasMaxLength(60);
    }
}

public sealed class InvStocktakeConfig : IEntityTypeConfiguration<InvStocktake>
{
    public void Configure(EntityTypeBuilder<InvStocktake> b)
    {
        b.ToTable("stocktake", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class InvStocktakeLineConfig : IEntityTypeConfiguration<InvStocktakeLine>
{
    public void Configure(EntityTypeBuilder<InvStocktakeLine> b)
    {
        b.ToTable("stocktake_line", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.StocktakeId });
        b.Property(x => x.SkuCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.SkuName).HasMaxLength(200).IsRequired();
        b.Property(x => x.LotCode).HasMaxLength(60);
        b.Property(x => x.SystemQty).HasPrecision(18, 4);
        b.Property(x => x.CountedQty).HasPrecision(18, 4);
        b.Property(x => x.VarianceQty).HasPrecision(18, 4);
    }
}

public sealed class InvStockReservationConfig : IEntityTypeConfiguration<InvStockReservation>
{
    public void Configure(EntityTypeBuilder<InvStockReservation> b)
    {
        b.ToTable("stock_reservation", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Status, x.WarehouseId });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.RefModule).HasMaxLength(40);
        b.Property(x => x.RefCode).HasMaxLength(40);
        b.Property(x => x.Note).HasMaxLength(1000);
        b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class InvStockReservationLineConfig : IEntityTypeConfiguration<InvStockReservationLine>
{
    public void Configure(EntityTypeBuilder<InvStockReservationLine> b)
    {
        b.ToTable("stock_reservation_line", "inv");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ReservationId });
        b.Property(x => x.SkuCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.SkuName).HasMaxLength(200).IsRequired();
        b.Property(x => x.LotCode).HasMaxLength(60);
        b.Property(x => x.Qty).HasPrecision(18, 4);
    }
}
