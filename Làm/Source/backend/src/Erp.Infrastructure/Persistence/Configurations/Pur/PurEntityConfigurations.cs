using Erp.Domain.Entities.Pur;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Pur;

public sealed class PurVendorConfig : IEntityTypeConfiguration<PurVendor>
{
    public void Configure(EntityTypeBuilder<PurVendor> b)
    {
        b.ToTable("vendor", "pur");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.TaxCode).HasMaxLength(40);
        b.Property(x => x.Phone).HasMaxLength(40);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Address).HasMaxLength(500);
        b.Property(x => x.PaymentTerms).HasMaxLength(200);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class PurVendorContactConfig : IEntityTypeConfiguration<PurVendorContact>
{
    public void Configure(EntityTypeBuilder<PurVendorContact> b)
    {
        b.ToTable("vendor_contact", "pur");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.VendorId });
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Title).HasMaxLength(100);
        b.Property(x => x.Phone).HasMaxLength(40);
        b.Property(x => x.Email).HasMaxLength(200);
    }
}

public sealed class PurVendorProductConfig : IEntityTypeConfiguration<PurVendorProduct>
{
    public void Configure(EntityTypeBuilder<PurVendorProduct> b)
    {
        b.ToTable("vendor_product", "pur");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.VendorId, x.ProductCode }).IsUnique();
        b.Property(x => x.ProductCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
    }
}

public sealed class PurVendorPriceConfig : IEntityTypeConfiguration<PurVendorPrice>
{
    public void Configure(EntityTypeBuilder<PurVendorPrice> b)
    {
        b.ToTable("vendor_price", "pur");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.VendorId, x.ProductCode, x.EffectiveFrom });
        b.Property(x => x.ProductCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.Currency).HasMaxLength(10).IsRequired();
    }
}

public sealed class PurPurchaseRequestConfig : IEntityTypeConfiguration<PurPurchaseRequest>
{
    public void Configure(EntityTypeBuilder<PurPurchaseRequest> b)
    {
        b.ToTable("purchase_request", "pur");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.RequestingUnit).HasMaxLength(200);
        b.Property(x => x.Note).HasMaxLength(2000);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.DecisionNote).HasMaxLength(1000);
    }
}

public sealed class PurPrLineConfig : IEntityTypeConfiguration<PurPrLine>
{
    public void Configure(EntityTypeBuilder<PurPrLine> b)
    {
        b.ToTable("pr_line", "pur");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PrId });
        b.Property(x => x.ProductCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.Unit).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class PurPurchaseOrderConfig : IEntityTypeConfiguration<PurPurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurPurchaseOrder> b)
    {
        b.ToTable("purchase_order", "pur");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        b.Property(x => x.Note).HasMaxLength(2000);
        b.Property(x => x.CancelReason).HasMaxLength(500);
    }
}

public sealed class PurPoLineConfig : IEntityTypeConfiguration<PurPoLine>
{
    public void Configure(EntityTypeBuilder<PurPoLine> b)
    {
        b.ToTable("po_line", "pur");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PoId });
        b.Property(x => x.ProductCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.ReceivedQty).HasPrecision(18, 4);
        b.Property(x => x.InvoicedQty).HasPrecision(18, 4);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.Unit).HasMaxLength(30).IsRequired();
    }
}

public sealed class PurGoodsReceiptConfig : IEntityTypeConfiguration<PurGoodsReceipt>
{
    public void Configure(EntityTypeBuilder<PurGoodsReceipt> b)
    {
        b.ToTable("goods_receipt", "pur");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.PoId });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.InventoryPushStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.QualityNote).HasMaxLength(1000);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class PurGrnLineConfig : IEntityTypeConfiguration<PurGrnLine>
{
    public void Configure(EntityTypeBuilder<PurGrnLine> b)
    {
        b.ToTable("grn_line", "pur");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.GrnId });
        b.Property(x => x.ProductCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        b.Property(x => x.OrderedQty).HasPrecision(18, 4);
        b.Property(x => x.ReceivedQty).HasPrecision(18, 4);
        b.Property(x => x.AcceptedQty).HasPrecision(18, 4);
        b.Property(x => x.RejectedQty).HasPrecision(18, 4);
        b.Property(x => x.Unit).HasMaxLength(30).IsRequired();
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
    }
}

public sealed class PurVendorInvoiceConfig : IEntityTypeConfiguration<PurVendorInvoice>
{
    public void Configure(EntityTypeBuilder<PurVendorInvoice> b)
    {
        b.ToTable("vendor_invoice", "pur");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.InvoiceNumber).HasMaxLength(80).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.MatchStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.ApPushStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.SubTotal).HasPrecision(18, 2);
        b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.MatchNote).HasMaxLength(1000);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class PurInvoiceLineConfig : IEntityTypeConfiguration<PurInvoiceLine>
{
    public void Configure(EntityTypeBuilder<PurInvoiceLine> b)
    {
        b.ToTable("invoice_line", "pur");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.InvoiceId });
        b.Property(x => x.ProductCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.LineAmount).HasPrecision(18, 2);
    }
}
