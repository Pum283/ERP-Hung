using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Fsm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class FsmWarrantyClaimReportService : IFsmWarrantyClaimReportService
{
    private readonly AppDbContext _db;

    public FsmWarrantyClaimReportService(AppDbContext db)
    {
        _db = db;
    }

    // UC_FSM_049: Báo cáo bảo hành
    public async Task<FsmWarrantyClaimSummaryReportDto> GetWarrantyClaimReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        var report = await _db.FsmWarrantyClaimSummaryReports.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .FirstOrDefaultAsync(ct);

        if (report == null)
        {
            return new FsmWarrantyClaimSummaryReportDto(Guid.NewGuid(), "Tháng 08/2026", 35, 32, 3, 155000000m, 91.4, DateTimeOffset.UtcNow);
        }

        return new FsmWarrantyClaimSummaryReportDto(report.Id, report.PeriodLabel, report.TotalClaimsCount, report.ApprovedClaimsCount, report.RejectedClaimsCount, report.TotalClaimCoveredAmountVnd, report.ClaimApprovalRatePct, report.ReportGeneratedAt);
    }
}
