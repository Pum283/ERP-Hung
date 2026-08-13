using Erp.Application.DTOs;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class CrmPosReceivablesHardwarePolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmSalesReceivablesReportService _crmSvc;
    private readonly PosHardwarePrinterDrawerService _posSvc;
    private readonly Guid _tenant = Guid.NewGuid();

    public CrmPosReceivablesHardwarePolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-pos-receivables-hardware-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TCP178", Name = "Tenant CRM POS 178" });
        _db.SaveChanges();

        _crmSvc = new CrmSalesReceivablesReportService(_db);
        _posSvc = new PosHardwarePrinterDrawerService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_130: Báo cáo công nợ bán
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetReceivablesAgingReport_ReturnsAgingSummary()
    {
        var report = await _crmSvc.GetReceivablesAgingReportAsync(_tenant);

        Assert.NotNull(report);
        Assert.True(report.TotalReceivablesAmount > 0);
        Assert.NotEmpty(report.CustomerAgingDetails);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_131: Xuất báo cáo định kỳ
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ScheduleReportExport_CreatesNewSchedule()
    {
        var req = new CrmScheduleReportExportRequest(
            "Báo Cáo Phân Tích Tuổi Nợ",
            "ReceivablesAging",
            "PDF",
            "Monthly",
            "giamdoc@erphung.vn"
        );

        var res = await _crmSvc.ScheduleReportExportAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Báo Cáo Phân Tích Tuổi Nợ", res.ReportName);
        Assert.Equal("PDF", res.ExportFormat);

        var list = await _crmSvc.GetScheduledReportExportsAsync(_tenant);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_004: Cấu hình máy in bếp/khu vực
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveKitchenPrinterConfig_SavesPrinterConfig()
    {
        var req = new PosSaveKitchenPrinterConfigRequest(
            "Máy in Bếp Nóng 01",
            "Kitchen",
            "LAN_IP",
            "192.168.1.210",
            80,
            true
        );

        var res = await _posSvc.SaveKitchenPrinterConfigAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Máy in Bếp Nóng 01", res.PrinterName);
        Assert.True(res.AutoCutPaper);

        var list = await _posSvc.GetKitchenPrinterConfigsAsync(_tenant);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_005: Cấu hình ngăn kéo tiền
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveCashDrawerConfig_SavesDrawerConfig()
    {
        var req = new PosSaveCashDrawerConfigRequest(
            "Ngăn Kéo Quầy 01",
            "PrinterKickout",
            "1B700019FA",
            true
        );

        var res = await _posSvc.SaveCashDrawerConfigAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Ngăn Kéo Quầy 01", res.DrawerName);
        Assert.True(res.AutoOpenOnCashPayment);

        var list = await _posSvc.GetCashDrawerConfigsAsync(_tenant);
        Assert.NotEmpty(list);
    }
}
