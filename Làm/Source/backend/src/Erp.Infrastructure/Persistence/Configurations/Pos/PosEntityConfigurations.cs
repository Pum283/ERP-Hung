using Erp.Domain.Entities.Pos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Pos;

public sealed class PosStoreConfig : IEntityTypeConfiguration<PosStore>
{
    public void Configure(EntityTypeBuilder<PosStore> b)
    {
        b.ToTable("store", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Address).HasMaxLength(500);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.MonthlyRevenueTarget).HasPrecision(18, 2);
        b.HasIndex(x => new { x.TenantId, x.WarehouseId });
    }
}

public sealed class PosTerminalConfig : IEntityTypeConfiguration<PosTerminal>
{
    public void Configure(EntityTypeBuilder<PosTerminal> b)
    {
        b.ToTable("terminal", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.StoreId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class PosPrinterConfig : IEntityTypeConfiguration<PosPrinter>
{
    public void Configure(EntityTypeBuilder<PosPrinter> b)
    {
        b.ToTable("printer", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.StoreId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.PrinterType).HasMaxLength(30).IsRequired();
        b.Property(x => x.ConnectionInfo).HasMaxLength(300);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class PosCashierAssignmentConfig : IEntityTypeConfiguration<PosCashierAssignment>
{
    public void Configure(EntityTypeBuilder<PosCashierAssignment> b)
    {
        b.ToTable("cashier_assignment", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.StoreId, x.UserId }).IsUnique();
        b.Property(x => x.Role).HasMaxLength(30).IsRequired();
    }
}

public sealed class PosProductCategoryConfig : IEntityTypeConfiguration<PosProductCategory>
{
    public void Configure(EntityTypeBuilder<PosProductCategory> b)
    {
        b.ToTable("product_category", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
    }
}

public sealed class PosProductConfig : IEntityTypeConfiguration<PosProduct>
{
    public void Configure(EntityTypeBuilder<PosProduct> b)
    {
        b.ToTable("product", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Unit).HasMaxLength(30);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class PosBomLineConfig : IEntityTypeConfiguration<PosBomLine>
{
    public void Configure(EntityTypeBuilder<PosBomLine> b)
    {
        b.ToTable("bom_line", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ProductId, x.MaterialCode });
        b.Property(x => x.MaterialCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.MaterialName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Qty).HasPrecision(18, 4);
        b.Property(x => x.Unit).HasMaxLength(30).IsRequired();
    }
}

public sealed class PosTaxRateConfig : IEntityTypeConfiguration<PosTaxRate>
{
    public void Configure(EntityTypeBuilder<PosTaxRate> b)
    {
        b.ToTable("tax_rate", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.RatePct).HasPrecision(18, 4);
    }
}

public sealed class PosPriceListConfig : IEntityTypeConfiguration<PosPriceList>
{
    public void Configure(EntityTypeBuilder<PosPriceList> b)
    {
        b.ToTable("price_list", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.StoreId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class PosPriceListItemConfig : IEntityTypeConfiguration<PosPriceListItem>
{
    public void Configure(EntityTypeBuilder<PosPriceListItem> b)
    {
        b.ToTable("price_list_item", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.PriceListId, x.ProductId }).IsUnique();
        b.Property(x => x.Price).HasPrecision(18, 2);
    }
}

public sealed class PosShiftConfig : IEntityTypeConfiguration<PosShift>
{
    public void Configure(EntityTypeBuilder<PosShift> b)
    {
        b.ToTable("shift", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.StoreId, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.OpeningCash).HasPrecision(18, 2);
        b.Property(x => x.ClosingCashCounted).HasPrecision(18, 2);
        b.Property(x => x.ExpectedCash).HasPrecision(18, 2);
        b.Property(x => x.Variance).HasPrecision(18, 2);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class PosSaleConfig : IEntityTypeConfiguration<PosSale>
{
    public void Configure(EntityTypeBuilder<PosSale> b)
    {
        b.ToTable("sale", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.ShiftId, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.AreaName).HasMaxLength(100);
        b.Property(x => x.SubTotal).HasPrecision(18, 2);
        b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.PaidAmount).HasPrecision(18, 2);
        b.Property(x => x.ReturnedAmount).HasPrecision(18, 2);
        b.Property(x => x.Note).HasMaxLength(1000);
        b.Property(x => x.DiscountSource).HasMaxLength(30).IsRequired();
        b.Property(x => x.AppliedVoucherCode).HasMaxLength(40);
        b.Property(x => x.ManualDiscountType).HasMaxLength(20);
        b.Property(x => x.ManualDiscountValue).HasPrecision(18, 2);
        b.Property(x => x.DiscountApprovalStatus).HasMaxLength(30).IsRequired();
        b.Property(x => x.DiscountNote).HasMaxLength(500);
    }
}

public sealed class PosPromotionConfig : IEntityTypeConfiguration<PosPromotion>
{
    public void Configure(EntityTypeBuilder<PosPromotion> b)
    {
        b.ToTable("promotion", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.DiscountType).HasMaxLength(20).IsRequired();
        b.Property(x => x.DiscountValue).HasPrecision(18, 2);
        b.Property(x => x.MinOrderAmount).HasPrecision(18, 2);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class PosVoucherConfig : IEntityTypeConfiguration<PosVoucher>
{
    public void Configure(EntityTypeBuilder<PosVoucher> b)
    {
        b.ToTable("voucher", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.PromotionId });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class PosSaleLineConfig : IEntityTypeConfiguration<PosSaleLine>
{
    public void Configure(EntityTypeBuilder<PosSaleLine> b)
    {
        b.ToTable("sale_line", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.SaleId, x.LineNo });
        b.Property(x => x.ProductCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 3);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.TaxRatePct).HasPrecision(9, 4);
        b.Property(x => x.LineAmount).HasPrecision(18, 2);
    }
}

public sealed class PosSalePaymentConfig : IEntityTypeConfiguration<PosSalePayment>
{
    public void Configure(EntityTypeBuilder<PosSalePayment> b)
    {
        b.ToTable("sale_payment", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Method).HasMaxLength(30).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class PosReturnConfig : IEntityTypeConfiguration<PosReturn>
{
    public void Configure(EntityTypeBuilder<PosReturn> b)
    {
        b.ToTable("sale_return", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.RefundMethod).HasMaxLength(30).IsRequired();
        b.Property(x => x.RefundAmount).HasPrecision(18, 2);
        b.Property(x => x.Reason).HasMaxLength(500);
    }
}

public sealed class PosReturnLineConfig : IEntityTypeConfiguration<PosReturnLine>
{
    public void Configure(EntityTypeBuilder<PosReturnLine> b)
    {
        b.ToTable("return_line", "pos");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ReturnId });
        b.Property(x => x.ProductCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 3);
        b.Property(x => x.LineAmount).HasPrecision(18, 2);
    }
}
