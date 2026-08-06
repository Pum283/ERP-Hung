using Erp.Domain.Entities.Log;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Log;

public sealed class LogCarrierConfig : IEntityTypeConfiguration<LogCarrier>
{
    public void Configure(EntityTypeBuilder<LogCarrier> b)
    {
        b.ToTable("carrier", "log");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Phone).HasMaxLength(40);
        b.Property(x => x.ContactName).HasMaxLength(120);
        b.Property(x => x.Note).HasMaxLength(1000);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class LogDeliveryOrderConfig : IEntityTypeConfiguration<LogDeliveryOrder>
{
    public void Configure(EntityTypeBuilder<LogDeliveryOrder> b)
    {
        b.ToTable("delivery_order", "log");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.SourceOrderCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        b.Property(x => x.ShipAddress).HasMaxLength(500);
        b.Property(x => x.Phone).HasMaxLength(40);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.DriverName).HasMaxLength(120);
        b.Property(x => x.Note).HasMaxLength(1000);
        b.Property(x => x.FailureReason).HasMaxLength(500);
        b.Property(x => x.WaybillNo).HasMaxLength(60);
        b.Property(x => x.CodAmount).HasPrecision(18, 2);
        b.Property(x => x.CodStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.CodNote).HasMaxLength(500);
        b.HasIndex(x => new { x.TenantId, x.CodStatus });
        b.HasIndex(x => new { x.TenantId, x.PromisedAt });
    }
}

public sealed class LogCodHandoverConfig : IEntityTypeConfiguration<LogCodHandover>
{
    public void Configure(EntityTypeBuilder<LogCodHandover> b)
    {
        b.ToTable("cod_handover", "log");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.DriverName).HasMaxLength(120);
        b.Property(x => x.ExpectedAmount).HasPrecision(18, 2);
        b.Property(x => x.CollectedAmount).HasPrecision(18, 2);
        b.Property(x => x.RemittedAmount).HasPrecision(18, 2);
        b.Property(x => x.VarianceAmount).HasPrecision(18, 2);
        b.Property(x => x.Note).HasMaxLength(1000);
        b.Property(x => x.VarianceNote).HasMaxLength(1000);
    }
}

public sealed class LogCodHandoverLineConfig : IEntityTypeConfiguration<LogCodHandoverLine>
{
    public void Configure(EntityTypeBuilder<LogCodHandoverLine> b)
    {
        b.ToTable("cod_handover_line", "log");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.HandoverId });
        b.HasIndex(x => new { x.TenantId, x.DeliveryOrderId }).IsUnique();
        b.Property(x => x.CodAmount).HasPrecision(18, 2);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class LogDeliveryLineConfig : IEntityTypeConfiguration<LogDeliveryLine>
{
    public void Configure(EntityTypeBuilder<LogDeliveryLine> b)
    {
        b.ToTable("delivery_line", "log");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.DeliveryOrderId });
        b.Property(x => x.ProductCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.QtyPicked).HasPrecision(18, 4);
        b.Property(x => x.Unit).HasMaxLength(20).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class LogShipmentEventConfig : IEntityTypeConfiguration<LogShipmentEvent>
{
    public void Configure(EntityTypeBuilder<LogShipmentEvent> b)
    {
        b.ToTable("shipment_event", "log");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.DeliveryOrderId, x.OccurredAt });
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class LogReturnNoteConfig : IEntityTypeConfiguration<LogReturnNote>
{
    public void Configure(EntityTypeBuilder<LogReturnNote> b)
    {
        b.ToTable("return_note", "log");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.DeliveryOrderId, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(500);
        b.Property(x => x.Note).HasMaxLength(1000);
        b.Property(x => x.InvStockDocCode).HasMaxLength(40);
    }
}

public sealed class LogReturnLineConfig : IEntityTypeConfiguration<LogReturnLine>
{
    public void Configure(EntityTypeBuilder<LogReturnLine> b)
    {
        b.ToTable("return_line", "log");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ReturnNoteId });
        b.Property(x => x.ProductCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Unit).HasMaxLength(20).IsRequired();
        b.Property(x => x.QtyExpected).HasPrecision(18, 4);
        b.Property(x => x.QtyCounted).HasPrecision(18, 4);
        b.Property(x => x.QtyAccepted).HasPrecision(18, 4);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}
