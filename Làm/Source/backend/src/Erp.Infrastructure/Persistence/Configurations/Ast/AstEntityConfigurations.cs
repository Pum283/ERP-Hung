using Erp.Domain.Entities.Ast;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Ast;

public sealed class AstAssetGroupConfig : IEntityTypeConfiguration<AstAssetGroup>
{
    public void Configure(EntityTypeBuilder<AstAssetGroup> b)
    {
        b.ToTable("asset_group", "ast");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.DefaultDepreciationRate).HasPrecision(9, 4);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class AstLocationConfig : IEntityTypeConfiguration<AstLocation>
{
    public void Configure(EntityTypeBuilder<AstLocation> b)
    {
        b.ToTable("location", "ast");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.BranchName).HasMaxLength(200);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class AstDepreciationMethodConfig : IEntityTypeConfiguration<AstDepreciationMethod>
{
    public void Configure(EntityTypeBuilder<AstDepreciationMethod> b)
    {
        b.ToTable("depreciation_method", "ast");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.MethodType).HasMaxLength(40).IsRequired();
        b.Property(x => x.DefaultRatePercent).HasPrecision(9, 4);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class AstAssetConfig : IEntityTypeConfiguration<AstAsset>
{
    public void Configure(EntityTypeBuilder<AstAsset> b)
    {
        b.ToTable("asset", "ast");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.OriginalCost).HasPrecision(18, 2);
        b.Property(x => x.DepreciationRatePercent).HasPrecision(9, 4);
        b.Property(x => x.AccumulatedDepreciation).HasPrecision(18, 2);
        b.Property(x => x.BookValue).HasPrecision(18, 2);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.AssignedEmployeeName).HasMaxLength(200);
        b.Property(x => x.DisposalAmount).HasPrecision(18, 2);
        b.Property(x => x.PurchaseRef).HasMaxLength(80);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class AstMovementDocConfig : IEntityTypeConfiguration<AstMovementDoc>
{
    public void Configure(EntityTypeBuilder<AstMovementDoc> b)
    {
        b.ToTable("movement_doc", "ast");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.DocType, x.Status });
        b.HasIndex(x => new { x.TenantId, x.AssetId });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.DocType).HasMaxLength(20).IsRequired();
        b.Property(x => x.FromEmployeeName).HasMaxLength(200);
        b.Property(x => x.ToEmployeeName).HasMaxLength(200);
        b.Property(x => x.DisposalKind).HasMaxLength(20);
        b.Property(x => x.DisposalAmount).HasPrecision(18, 2);
        b.Property(x => x.BookValueSnapshot).HasPrecision(18, 2);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class AstStocktakeConfig : IEntityTypeConfiguration<AstStocktake>
{
    public void Configure(EntityTypeBuilder<AstStocktake> b)
    {
        b.ToTable("stocktake", "ast");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class AstStocktakeLineConfig : IEntityTypeConfiguration<AstStocktakeLine>
{
    public void Configure(EntityTypeBuilder<AstStocktakeLine> b)
    {
        b.ToTable("stocktake_line", "ast");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.StocktakeId, x.AssetId });
        b.Property(x => x.AssetCode).HasMaxLength(40).IsRequired();
        b.Property(x => x.AssetName).HasMaxLength(200).IsRequired();
        b.Property(x => x.LocationName).HasMaxLength(200);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class AstDepreciationRunConfig : IEntityTypeConfiguration<AstDepreciationRun>
{
    public void Configure(EntityTypeBuilder<AstDepreciationRun> b)
    {
        b.ToTable("depreciation_run", "ast");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.Year, x.Month }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
    }
}

public sealed class AstDepreciationLineConfig : IEntityTypeConfiguration<AstDepreciationLine>
{
    public void Configure(EntityTypeBuilder<AstDepreciationLine> b)
    {
        b.ToTable("depreciation_line", "ast");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.RunId, x.LineNo });
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.BookValueBefore).HasPrecision(18, 2);
        b.Property(x => x.BookValueAfter).HasPrecision(18, 2);
    }
}
