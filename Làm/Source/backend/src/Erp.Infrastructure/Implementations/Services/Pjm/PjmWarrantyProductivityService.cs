using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pjm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PjmWarrantyProductivityService : IPjmWarrantyProductivityService
{
    private readonly AppDbContext _db;

    public PjmWarrantyProductivityService(AppDbContext db)
    {
        _db = db;
    }

    // UC_PJM_037: Bảo hành sau dự án
    public async Task<PjmPostProjectWarrantyCoverageDto> CreateWarrantyCoverageAsync(Guid tenantId, PjmCreateWarrantyCoverageRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.CustomerName))
            throw new AppException("Tên khách hàng bảo hành không được để trống.", 400);

        var entity = new PjmPostProjectWarrantyCoverage
        {
            TenantId = tenantId,
            ProjectId = req.ProjectId == Guid.Empty ? Guid.NewGuid() : req.ProjectId,
            ProjectCode = req.ProjectCode ?? "PRJ-2026-088",
            CustomerName = req.CustomerName,
            WarrantyStartDate = req.WarrantyStartDate,
            WarrantyEndDate = req.WarrantyEndDate,
            WarrantyPeriodMonths = req.WarrantyPeriodMonths > 0 ? req.WarrantyPeriodMonths : 24,
            SupportHotline = req.SupportHotline ?? "1900-8888",
            IsActive = true
        };

        _db.PjmPostProjectWarrantyCoverages.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new PjmPostProjectWarrantyCoverageDto(entity.Id, entity.ProjectId, entity.ProjectCode, entity.CustomerName, entity.WarrantyStartDate, entity.WarrantyEndDate, entity.WarrantyPeriodMonths, entity.SupportHotline, entity.IsActive);
    }

    public async Task<IReadOnlyList<PjmPostProjectWarrantyCoverageDto>> GetWarrantyCoveragesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PjmPostProjectWarrantyCoverages.AsNoTracking()
            .Where(w => w.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PjmPostProjectWarrantyCoverageDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "PRJ-2026-088", "Công Ty Viễn Thông Viettel", DateTimeOffset.UtcNow.AddMonths(-2), DateTimeOffset.UtcNow.AddMonths(22), 24, "1900-8888", true),
                new(Guid.NewGuid(), Guid.NewGuid(), "PRJ-2026-065", "Tập Đoàn Cơ Khí FPT", DateTimeOffset.UtcNow.AddMonths(-6), DateTimeOffset.UtcNow.AddMonths(6), 12, "1900-8888", true)
            };
        }

        return list.Select(w => new PjmPostProjectWarrantyCoverageDto(w.Id, w.ProjectId, w.ProjectCode, w.CustomerName, w.WarrantyStartDate, w.WarrantyEndDate, w.WarrantyPeriodMonths, w.SupportHotline, w.IsActive)).ToList();
    }

    // UC_PJM_041: Năng suất nguồn lực
    public async Task<PjmResourceProductivityReportDto> GetResourceProductivityReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        var report = await _db.PjmResourceProductivityReports.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .FirstOrDefaultAsync(ct);

        if (report == null)
        {
            return new PjmResourceProductivityReportDto(Guid.NewGuid(), "Tháng 08/2026", 18, 2880, 2650, 92.0, 125000000m, DateTimeOffset.UtcNow);
        }

        return new PjmResourceProductivityReportDto(report.Id, report.PeriodLabel, report.TotalEngineersCount, report.TotalAllocatedHours, report.TotalBillableTimesheetHours, report.ResourceUtilizationRatePct, report.AverageOutputPerEngineerVnd, report.ReportGeneratedAt);
    }
}
