using Erp.Domain.Entities.Bi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Erp.Infrastructure.Persistence.Configurations.Bi;

public sealed class BiDatasetConfig : IEntityTypeConfiguration<BiDataset>
{
    public void Configure(EntityTypeBuilder<BiDataset> b)
    {
        b.ToTable("dataset", "bi");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.ModuleCode).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.LastRefreshNote).HasMaxLength(500);
    }
}

public sealed class BiDatasetRefreshConfig : IEntityTypeConfiguration<BiDatasetRefresh>
{
    public void Configure(EntityTypeBuilder<BiDatasetRefresh> b)
    {
        b.ToTable("dataset_refresh", "bi");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.DatasetId, x.StartedAt });
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class BiReportConfig : IEntityTypeConfiguration<BiReport>
{
    public void Configure(EntityTypeBuilder<BiReport> b)
    {
        b.ToTable("report", "bi");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.ModuleCode).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.FilterSchemaJson).HasMaxLength(4000);
    }
}

public sealed class BiReportPermissionConfig : IEntityTypeConfiguration<BiReportPermission>
{
    public void Configure(EntityTypeBuilder<BiReportPermission> b)
    {
        b.ToTable("report_permission", "bi");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ReportId, x.PrincipalType, x.PrincipalCode }).IsUnique();
        b.Property(x => x.PrincipalType).HasMaxLength(20).IsRequired();
        b.Property(x => x.PrincipalCode).HasMaxLength(80).IsRequired();
        b.Property(x => x.AccessLevel).HasMaxLength(20).IsRequired();
    }
}

public sealed class BiDashboardConfig : IEntityTypeConfiguration<BiDashboard>
{
    public void Configure(EntityTypeBuilder<BiDashboard> b)
    {
        b.ToTable("dashboard", "bi");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.DashboardType).HasMaxLength(30).IsRequired();
        b.Property(x => x.ModuleCode).HasMaxLength(20);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class BiWidgetConfig : IEntityTypeConfiguration<BiWidget>
{
    public void Configure(EntityTypeBuilder<BiWidget> b)
    {
        b.ToTable("widget", "bi");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.DashboardId, x.Code }).IsUnique();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.WidgetType).HasMaxLength(30).IsRequired();
        b.Property(x => x.MetricKey).HasMaxLength(40).IsRequired();
        b.Property(x => x.StubValue).HasPrecision(18, 2);
        b.Property(x => x.Unit).HasMaxLength(40);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
    }
}

public sealed class BiReportRunConfig : IEntityTypeConfiguration<BiReportRun>
{
    public void Configure(EntityTypeBuilder<BiReportRun> b)
    {
        b.ToTable("report_run", "bi");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.ReportId, x.RunAt });
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.ExportFormat).HasMaxLength(20).IsRequired();
        b.Property(x => x.ExportFileName).HasMaxLength(200);
        b.Property(x => x.FilterJson).HasMaxLength(4000);
        b.Property(x => x.ResultPreviewJson).HasMaxLength(8000);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

public sealed class BiKpiTargetConfig : IEntityTypeConfiguration<BiKpiTarget>
{
    public void Configure(EntityTypeBuilder<BiKpiTarget> b)
    {
        b.ToTable("kpi_target", "bi");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.PeriodKey, x.MetricKey });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.ModuleCode).HasMaxLength(20).IsRequired();
        b.Property(x => x.MetricKey).HasMaxLength(40).IsRequired();
        b.Property(x => x.PeriodKey).HasMaxLength(20).IsRequired();
        b.Property(x => x.TargetValue).HasPrecision(18, 2);
        b.Property(x => x.ActualStubValue).HasPrecision(18, 2);
        b.Property(x => x.Unit).HasMaxLength(40);
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

public sealed class BiAlertThresholdConfig : IEntityTypeConfiguration<BiAlertThreshold>
{
    public void Configure(EntityTypeBuilder<BiAlertThreshold> b)
    {
        b.ToTable("alert_threshold", "bi");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.MetricKey, x.Status });
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.MetricKey).HasMaxLength(40).IsRequired();
        b.Property(x => x.Operator).HasMaxLength(10).IsRequired();
        b.Property(x => x.ThresholdValue).HasPrecision(18, 2);
        b.Property(x => x.Severity).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(30).IsRequired();
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}
