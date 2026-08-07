using Erp.Application.DTOs.Bi;
using Erp.Domain.Entities.Bi;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Fin;
using Erp.Infrastructure.Implementations.Services.Bi;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.UnitTests;

/// <summary>UC_BI_002/008/014/016 — refresh nguồn thật · widget FIN · chạy BC · tải export.</summary>
public sealed class BiAnalyticsPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly BiAnalyticsService _svc;
    private readonly Guid _tenant = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private readonly Guid _user = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    public BiAnalyticsPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("bi-polish-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new BiAnalyticsService(_db);
    }

    public void Dispose() => _db.Dispose();

    private async Task<BiDatasetDto> SeedDatasetAsync(string module = "CRM")
    {
        return await _svc.UpsertDatasetAsync(_tenant, _user, new BiDatasetUpsertRequest(
            null, "DS-" + module, "Dataset " + module, module, null, "Ready"));
    }

    private async Task<BiReportDto> SeedReportAsync(string module = "CRM", Guid? datasetId = null)
    {
        return await _svc.UpsertReportAsync(_tenant, _user, new BiReportUpsertRequest(
            null, "RPT-" + module, "BC " + module, module, datasetId, null, null, "Active", false));
    }

    [Fact]
    public async Task RefreshDataset_CountsCrmOrders_NotRandom()
    {
        _db.CrmSalesOrders.AddRange(
            new CrmSalesOrder { TenantId = _tenant, Code = "SO-1", TotalAmount = 100, CreatedBy = _user },
            new CrmSalesOrder { TenantId = _tenant, Code = "SO-2", TotalAmount = 200, CreatedBy = _user });
        await _db.SaveChangesAsync();
        var ds = await SeedDatasetAsync("CRM");

        var refreshed = await _svc.RefreshDatasetAsync(
            _tenant, _user, ds.Id, new BiRefreshRequest("manual"));

        Assert.Equal(2, refreshed.RowCountEstimate);
        Assert.Equal("Ready", refreshed.Status);
        Assert.Contains("CrmSalesOrders", refreshed.LastRefreshNote ?? "", StringComparison.OrdinalIgnoreCase);

        var logs = await _svc.ListRefreshesAsync(_tenant, ds.Id);
        Assert.Single(logs);
        Assert.Equal("Succeeded", logs[0].Status);
        Assert.Equal(2, logs[0].RowsAffected);
    }

    [Fact]
    public async Task RefreshDataset_FinCountsJournals()
    {
        _db.FinJournals.Add(new FinJournal
        {
            TenantId = _tenant, Code = "JE-1", PeriodId = Guid.NewGuid(),
            Description = "t", Status = "Posted", CreatedByUserId = _user, CreatedBy = _user,
        });
        await _db.SaveChangesAsync();
        var ds = await SeedDatasetAsync("FIN");

        var refreshed = await _svc.RefreshDatasetAsync(_tenant, _user, ds.Id, new BiRefreshRequest(null));
        Assert.Equal(1, refreshed.RowCountEstimate);
        Assert.Contains("FinJournals", refreshed.LastRefreshNote ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DashboardWidget_RevenueProfit_FromFinRevenueDocs()
    {
        _db.FinRevenueDocuments.AddRange(
            new FinRevenueDocument
            {
                TenantId = _tenant, Code = "RV-1", Kind = "PosRevenue", SourceModule = "POS",
                RevenueAmount = 1_000_000, CogsAmount = 400_000, TotalAmount = 1_000_000,
                Status = "Posted", CreatedByUserId = _user, CreatedBy = _user,
            },
            new FinRevenueDocument
            {
                TenantId = _tenant, Code = "CG-1", Kind = "Cogs", SourceModule = "POS",
                RevenueAmount = 0, CogsAmount = 0, TotalAmount = 50_000,
                Status = "Posted", CreatedByUserId = _user, CreatedBy = _user,
            });
        await _db.SaveChangesAsync();

        var dash = await _svc.UpsertDashboardAsync(_tenant, _user, new BiDashboardUpsertRequest(
            null, "EXEC", "Exec", "Executive", null, "Active", null, 0));
        await _svc.UpsertWidgetAsync(_tenant, _user, new BiWidgetUpsertRequest(
            null, dash.Id, "REV", "Doanh thu", "Kpi", "Revenue", 99, "VND", 0, "Active"));
        await _svc.UpsertWidgetAsync(_tenant, _user, new BiWidgetUpsertRequest(
            null, dash.Id, "PRF", "Lãi", "Kpi", "Profit", 99, "VND", 1, "Active"));
        await _svc.UpsertWidgetAsync(_tenant, _user, new BiWidgetUpsertRequest(
            null, dash.Id, "CUS", "Custom", "Kpi", "Custom", 42, "VND", 2, "Active"));

        var detail = await _svc.GetDashboardDetailAsync(_tenant, dash.Id);
        Assert.Equal(1_000_000m, detail.Widgets.Single(w => w.MetricKey == "Revenue").StubValue);
        Assert.Equal(550_000m, detail.Widgets.Single(w => w.MetricKey == "Profit").StubValue);
        Assert.Equal(42m, detail.Widgets.Single(w => w.MetricKey == "Custom").StubValue);
    }

    [Fact]
    public async Task DashboardWidget_FallsBackToPostedJournalAccounts()
    {
        var periodId = Guid.NewGuid();
        var rev = new FinAccount
        {
            TenantId = _tenant, Code = "5111", Name = "DT", AccountType = "Revenue", CreatedBy = _user,
        };
        var cogs = new FinAccount
        {
            TenantId = _tenant, Code = "6321", Name = "GV", AccountType = "Expense", CreatedBy = _user,
        };
        _db.FinAccounts.AddRange(rev, cogs);
        var je = new FinJournal
        {
            TenantId = _tenant, Code = "JE-R", PeriodId = periodId,
            Description = "rev", Status = "Posted", CreatedByUserId = _user, CreatedBy = _user,
        };
        _db.FinJournals.Add(je);
        await _db.SaveChangesAsync();
        _db.FinJournalLines.AddRange(
            new FinJournalLine
            {
                TenantId = _tenant, JournalId = je.Id, AccountId = rev.Id,
                Debit = 0, Credit = 800_000, LineNo = 1, CreatedBy = _user,
            },
            new FinJournalLine
            {
                TenantId = _tenant, JournalId = je.Id, AccountId = cogs.Id,
                Debit = 300_000, Credit = 0, LineNo = 2, CreatedBy = _user,
            });
        await _db.SaveChangesAsync();

        var dash = await _svc.UpsertDashboardAsync(_tenant, _user, new BiDashboardUpsertRequest(
            null, "EXEC2", "Exec2", "Executive", null, "Active", null, 0));
        await _svc.UpsertWidgetAsync(_tenant, _user, new BiWidgetUpsertRequest(
            null, dash.Id, "REV", "DT", "Kpi", "Revenue", 1, "VND", 0, "Active"));
        await _svc.UpsertWidgetAsync(_tenant, _user, new BiWidgetUpsertRequest(
            null, dash.Id, "PRF", "LN", "Kpi", "Profit", 1, "VND", 1, "Active"));

        var detail = await _svc.GetDashboardDetailAsync(_tenant, dash.Id);
        Assert.Equal(800_000m, detail.Widgets.Single(w => w.MetricKey == "Revenue").StubValue);
        Assert.Equal(500_000m, detail.Widgets.Single(w => w.MetricKey == "Profit").StubValue);
    }

    [Fact]
    public async Task RunReport_AggregatesCrmByDateFilter()
    {
        var inside = new DateTimeOffset(2026, 3, 10, 0, 0, 0, TimeSpan.Zero);
        var outside = new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero);
        _db.CrmSalesOrders.AddRange(
            new CrmSalesOrder
            {
                TenantId = _tenant, Code = "SO-IN", OrderDate = inside, TotalAmount = 500, CreatedBy = _user,
            },
            new CrmSalesOrder
            {
                TenantId = _tenant, Code = "SO-OUT", OrderDate = outside, TotalAmount = 999, CreatedBy = _user,
            });
        await _db.SaveChangesAsync();
        var rpt = await SeedReportAsync("CRM");

        var run = await _svc.RunReportAsync(_tenant, _user, rpt.Id, new BiReportRunRequest(
            """{"from":"2026-03-01","to":"2026-03-31"}""", "None"));

        Assert.Equal("Succeeded", run.Status);
        Assert.Equal(1, run.RowCount);
        Assert.Contains("CRM", run.ResultPreviewJson ?? "", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stub", run.Note ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunReport_ExportExcel_SetsCsvFileName()
    {
        var rpt = await SeedReportAsync("FIN");
        var run = await _svc.RunReportAsync(_tenant, _user, rpt.Id, new BiReportRunRequest("{}", "Excel"));
        Assert.Equal("Excel", run.ExportFormat);
        Assert.NotNull(run.ExportFileName);
        Assert.EndsWith(".csv", run.ExportFileName!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DownloadRunExport_Excel_IsCsvWithHeader()
    {
        _db.CrmSalesOrders.Add(new CrmSalesOrder
        {
            TenantId = _tenant, Code = "SO-X", TotalAmount = 10, CreatedBy = _user,
        });
        await _db.SaveChangesAsync();
        var rpt = await SeedReportAsync("CRM");
        var run = await _svc.RunReportAsync(_tenant, _user, rpt.Id, new BiReportRunRequest("{}", "Excel"));

        var (name, contentType, content) = await _svc.DownloadRunExportAsync(_tenant, run.Id);
        Assert.EndsWith(".csv", name, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("text/csv", contentType, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Label,Value", content);
        Assert.Contains("Số đơn", content);
    }

    [Fact]
    public async Task DownloadRunExport_Pdf_IsPdfFormat()
    {
        var rpt = await SeedReportAsync("FIN");
        var run = await _svc.RunReportAsync(_tenant, _user, rpt.Id, new BiReportRunRequest("{}", "Pdf"));
        var (name, contentType, content) = await _svc.DownloadRunExportAsync(_tenant, run.Id);
        Assert.EndsWith(".pdf", name, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("application/pdf", contentType, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("%PDF-1.4", content);
        Assert.Contains("BÁO CÁO", content);
        Assert.Contains(rpt.Code, content);
    }

    [Fact]
    public async Task DownloadRunExport_None_Throws()
    {
        var rpt = await SeedReportAsync("CRM");
        var run = await _svc.RunReportAsync(_tenant, _user, rpt.Id, new BiReportRunRequest("{}", "None"));
        await Assert.ThrowsAsync<Erp.Application.Common.Exceptions.AppException>(
            () => _svc.DownloadRunExportAsync(_tenant, run.Id));
    }

    [Fact]
    public async Task RunReport_InvalidFilterJson_Throws()
    {
        var rpt = await SeedReportAsync("CRM");
        await Assert.ThrowsAsync<Erp.Application.Common.Exceptions.AppException>(
            () => _svc.RunReportAsync(_tenant, _user, rpt.Id, new BiReportRunRequest("{bad", "None")));
    }

    [Fact]
    public async Task KpiTarget_ComputesActualFromFinRevenueDocuments()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _db.FinRevenueDocuments.Add(new FinRevenueDocument
        {
            TenantId = _tenant, Code = "RV-KPI", Kind = "SalesRevenue", SourceModule = "FIN",
            RevenueAmount = 15_000_000, TotalAmount = 15_000_000, CogsAmount = 4_000_000,
            DocDate = DateTimeOffset.UtcNow, Status = "Posted", CreatedBy = _user
        });
        await _db.SaveChangesAsync();

        await _svc.UpsertKpiTargetAsync(_tenant, _user, new BiKpiTargetUpsertRequest(
            null, "KPI-REV", "DT Tháng", "FIN", "Revenue", "2026-08",
            today.AddDays(-5), today.AddDays(5), 20_000_000, 0, "VND", "Active", null));

        var list = await _svc.ListKpiTargetsAsync(_tenant, "2026-08", "FIN");
        Assert.Single(list);
        Assert.Equal(15_000_000m, list[0].ActualStubValue);
    }

    [Fact]
    public async Task KpiTarget_ComputesActualFromPosSales()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _db.PosSales.Add(new Domain.Entities.Pos.PosSale
        {
            TenantId = _tenant, Code = "POS-S1", ShiftId = Guid.NewGuid(), StoreId = Guid.NewGuid(),
            TotalAmount = 8_500_000, Status = "Paid", CreatedBy = _user
        });
        await _db.SaveChangesAsync();

        await _svc.UpsertKpiTargetAsync(_tenant, _user, new BiKpiTargetUpsertRequest(
            null, "KPI-POS", "Doanh số POS", "POS", "Revenue", "2026-08",
            today.AddDays(-5), today.AddDays(5), 10_000_000, 0, "VND", "Active", null));

        var board = await _svc.ListTargetVsActualAsync(_tenant, "2026-08", "POS");
        Assert.Single(board);
        Assert.Equal(8_500_000m, board[0].ActualValue);
    }
}
