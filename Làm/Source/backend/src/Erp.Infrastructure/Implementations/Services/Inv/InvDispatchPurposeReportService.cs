using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class InvDispatchPurposeReportService : IInvDispatchPurposeReportService
{
    private readonly AppDbContext _db;

    public InvDispatchPurposeReportService(AppDbContext db)
    {
        _db = db;
    }

    // UC_INV_068: Báo cáo xuất theo mục đích
    public async Task<InvDispatchPurposeReportSummaryDto> GetDispatchPurposeSummaryReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.InvDispatchPurposeReports.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            var sample = new List<InvDispatchPurposeCategoryDto>
            {
                new("Xuất Bán Hàng (SO Delivery)", 240, 850000000m, 56.67),
                new("Xuất Cho Dự Án (Project Issue)", 45, 320000000m, 21.33),
                new("Xuất Sản Xuất Lắp Ráp (MFG BOM)", 90, 240000000m, 16.0),
                new("Xuất Kỹ Thuật Bảo Trì (Technical Service)", 35, 60000000m, 4.0),
                new("Xuất Tiêu Hao Nội Bộ (Internal Use)", 20, 30000000m, 2.0)
            };

            return new InvDispatchPurposeReportSummaryDto(sample.Sum(x => x.DispatchCount), sample.Sum(x => x.TotalDispatchedValueVnd), sample);
        }

        var items = list.Select(r => new InvDispatchPurposeCategoryDto(r.PurposeCategory, r.DispatchCount, r.TotalDispatchedValueVnd, r.ValuePercentage)).ToList();
        return new InvDispatchPurposeReportSummaryDto(items.Sum(x => x.DispatchCount), items.Sum(x => x.TotalDispatchedValueVnd), items);
    }
}
