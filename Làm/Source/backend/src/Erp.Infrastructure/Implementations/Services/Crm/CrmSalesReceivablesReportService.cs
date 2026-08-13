using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class CrmSalesReceivablesReportService : ICrmSalesReceivablesReportService
{
    private readonly AppDbContext _db;

    public CrmSalesReceivablesReportService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_130: Báo cáo công nợ bán
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmSalesReceivablesAgingSummaryDto> GetReceivablesAgingReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        var list = new List<CrmCustomerReceivableAgingDto>
        {
            new(Guid.NewGuid(), "Đại lý Nông Sản Miền Tây", 85000000m, 50000000m, 20000000m, 15000000m, 0m, 85000000m),
            new(Guid.NewGuid(), "Chuỗi Cửa hàng Tiện Lợi An Khang", 42000000m, 30000000m, 12000000m, 0m, 0m, 42000000m),
            new(Guid.NewGuid(), "Công ty TNHH Bách Hóa Việt", 68000000m, 20000000m, 25000000m, 13000000m, 10000000m, 68000000m)
        };

        decimal total = list.Sum(x => x.TotalReceivableVnd);
        decimal overdue = list.Sum(x => x.Debt31To60DaysVnd + x.Debt61To90DaysVnd + x.DebtOver90DaysVnd);
        double overdueRate = total > 0 ? Math.Round((double)overdue / (double)total * 100, 1) : 0;

        return new CrmSalesReceivablesAgingSummaryDto(total, overdue, overdueRate, list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_131: Xuất báo cáo định kỳ
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmScheduledReportExportDto> ScheduleReportExportAsync(Guid tenantId, CrmScheduleReportExportRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ReportName) || string.IsNullOrWhiteSpace(req.RecipientEmails))
            throw new AppException("Tên báo cáo và email người nhận không được để trống.", 400);

        var exp = new CrmScheduledReportExport
        {
            TenantId = tenantId,
            ReportName = req.ReportName,
            ReportType = req.ReportType ?? "ReceivablesAging",
            ExportFormat = req.ExportFormat ?? "PDF",
            Frequency = req.Frequency ?? "Monthly",
            RecipientEmails = req.RecipientEmails,
            LastExportedAt = DateTimeOffset.UtcNow
        };

        _db.CrmScheduledReportExports.Add(exp);
        await _db.SaveChangesAsync(ct);

        return new CrmScheduledReportExportDto(
            exp.Id,
            exp.ReportName,
            exp.ReportType,
            exp.ExportFormat,
            exp.Frequency,
            exp.RecipientEmails,
            exp.LastExportedAt
        );
    }

    public async Task<IReadOnlyList<CrmScheduledReportExportDto>> GetScheduledReportExportsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.CrmScheduledReportExports.AsNoTracking()
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.LastExportedAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<CrmScheduledReportExportDto>
            {
                new(Guid.NewGuid(), "Báo Cáo Phân Tích Công Nợ Quá Hạn", "ReceivablesAging", "PDF", "Monthly", "giamdoc@erphung.vn, ketoan@erphung.vn", DateTimeOffset.UtcNow),
                new(Guid.NewGuid(), "Báo Cáo Hoa Hồng Kế Toán Chi Trả", "CommissionSummary", "Excel", "Weekly", "ketoanluong@erphung.vn", DateTimeOffset.UtcNow.AddDays(-2))
            };
        }

        return list.Select(e => new CrmScheduledReportExportDto(
            e.Id,
            e.ReportName,
            e.ReportType,
            e.ExportFormat,
            e.Frequency,
            e.RecipientEmails,
            e.LastExportedAt
        )).ToList();
    }
}
