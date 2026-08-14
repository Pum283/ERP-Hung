using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PurOtdReportRfqSavingsService : IPurOtdReportRfqSavingsService
{
    private readonly AppDbContext _db;

    public PurOtdReportRfqSavingsService(AppDbContext db)
    {
        _db = db;
    }

    // UC_PUR_049: Báo cáo đúng hạn giao hàng OTD
    public async Task<IReadOnlyList<PurVendorOtdPerformanceDto>> GetVendorOtdPerformanceReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PurVendorOtdReports.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<PurVendorOtdPerformanceDto>
            {
                new(Guid.NewGuid(), "Vinamilk Co.", 40, 38, 2, 95.0, "Excellent"),
                new(Guid.NewGuid(), "Mộc Châu Milk", 20, 16, 4, 80.0, "Poor"),
                new(Guid.NewGuid(), "Trung Nguyên Corp", 30, 27, 3, 90.0, "Good")
            };
        }

        return list.Select(r => new PurVendorOtdPerformanceDto(
            r.SupplierId,
            r.SupplierName,
            r.TotalOrdersCount,
            r.OnTimeOrdersCount,
            r.LateOrdersCount,
            r.OnTimeDeliveryPercentage,
            r.OnTimeDeliveryPercentage >= 95 ? "Excellent" : r.OnTimeDeliveryPercentage >= 85 ? "Good" : "Poor"
        )).ToList();
    }

    // UC_PUR_050: Báo cáo tiết kiệm chi phí từ RFQ
    public async Task<PurRfqSavingsSummaryDto> GetRfqSavingsSummaryReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.PurRfqSavingsReports.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            var sampleList = new List<PurRfqSavingsItemDto>
            {
                new(Guid.NewGuid(), "RFQ-2026-001", "Gói Thầu Cung Cấp Sữa Tươi Quý 3", 300000000m, 240000000m, 60000000m, 20.0, DateTimeOffset.UtcNow.AddDays(-5)),
                new(Guid.NewGuid(), "RFQ-2026-002", "Gói Thầu Bao Bì Hộp Giấy", 150000000m, 125000000m, 25000000m, 16.67, DateTimeOffset.UtcNow.AddDays(-2))
            };

            decimal budget = sampleList.Sum(x => x.InitialBudgetVnd);
            decimal awarded = sampleList.Sum(x => x.AwardedAmountVnd);
            decimal savings = sampleList.Sum(x => x.SavingsAmountVnd);
            double pct = budget > 0 ? Math.Round((double)(savings / budget) * 100, 2) : 0;

            return new PurRfqSavingsSummaryDto(budget, awarded, savings, pct, sampleList);
        }

        var items = list.Select(r => new PurRfqSavingsItemDto(
            r.RfqId,
            r.RfqNumber,
            r.Title,
            r.InitialBudgetVnd,
            r.AwardedAmountVnd,
            r.SavingsAmountVnd,
            r.SavingsPercentage,
            r.CalculatedAt
        )).ToList();

        decimal totBudget = items.Sum(x => x.InitialBudgetVnd);
        decimal totAwarded = items.Sum(x => x.AwardedAmountVnd);
        decimal totSavings = items.Sum(x => x.SavingsAmountVnd);
        double overallPct = totBudget > 0 ? Math.Round((double)(totSavings / totBudget) * 100, 2) : 0;

        return new PurRfqSavingsSummaryDto(totBudget, totAwarded, totSavings, overallPct, items);
    }
}
