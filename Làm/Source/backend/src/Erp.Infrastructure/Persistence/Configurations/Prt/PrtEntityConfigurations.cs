using Erp.Domain.Entities.Prt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Prt;

public sealed class PrtAccountConfig : IEntityTypeConfiguration<PrtAccount>
{
    public void Configure(EntityTypeBuilder<PrtAccount> b)
    {
        b.ToTable("account", "prt");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Email).HasMaxLength(200).IsRequired();
        b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        b.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
        b.Property(x => x.CustomerCode).HasMaxLength(40);
        b.Property(x => x.CustomerName).HasMaxLength(200);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.ResetTokenStub).HasMaxLength(80);
    }
}

public sealed class PrtOrderConfig : IEntityTypeConfiguration<PrtOrder>
{
    public void Configure(EntityTypeBuilder<PrtOrder> b)
    {
        b.ToTable("order", "prt");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.ShippingAddress).HasMaxLength(500);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class PrtOrderLineConfig : IEntityTypeConfiguration<PrtOrderLine>
{
    public void Configure(EntityTypeBuilder<PrtOrderLine> b)
    {
        b.ToTable("order_line", "prt");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.OrderId, x.LineNo });
        b.Property(x => x.ItemCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 3);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.LineAmount).HasPrecision(18, 2);
    }
}

public sealed class PrtInvoiceConfig : IEntityTypeConfiguration<PrtInvoice>
{
    public void Configure(EntityTypeBuilder<PrtInvoice> b)
    {
        b.ToTable("invoice", "prt");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.PaidAmount).HasPrecision(18, 2);
        b.Property(x => x.OpenAmount).HasPrecision(18, 2);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class PrtPaymentConfig : IEntityTypeConfiguration<PrtPayment>
{
    public void Configure(EntityTypeBuilder<PrtPayment> b)
    {
        b.ToTable("payment", "prt");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Method).HasMaxLength(40).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class PrtTicketConfig : IEntityTypeConfiguration<PrtTicket>
{
    public void Configure(EntityTypeBuilder<PrtTicket> b)
    {
        b.ToTable("ticket", "prt");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class PrtPortalPackageConfig : IEntityTypeConfiguration<PrtPortalPackage>
{
    public void Configure(EntityTypeBuilder<PrtPortalPackage> b)
    {
        b.ToTable("portal_package", "prt");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PlanCode }).IsUnique();
        b.Property(x => x.PlanCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.FeaturesJson).HasMaxLength(4000).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}
