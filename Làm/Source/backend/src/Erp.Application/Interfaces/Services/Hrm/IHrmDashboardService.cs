using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmDashboardService
{
    Task<HrmDashboardHeadcountDto> HeadcountAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<HrmAttendanceReportRowDto>> AttendanceReportAsync(
        Guid tenantId, DateOnly? from, DateOnly? to, CancellationToken ct = default);
    Task<IReadOnlyList<HrmRecruitFunnelRowDto>> RecruitFunnelAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<HrmLeaveSummaryRowDto>> LeaveSummaryAsync(Guid tenantId, int? year, CancellationToken ct = default);
    Task<HrmCostSummaryDto> CostSummaryAsync(Guid tenantId, Guid? periodId, CancellationToken ct = default);
    Task<IReadOnlyList<HeadcountCompareRowDto>> HeadcountVsPlanAsync(Guid tenantId, CancellationToken ct = default);
    Task<HrmDashboardBundleDto> BundleAsync(
        Guid tenantId, DateOnly? attFrom, DateOnly? attTo, int? leaveYear, Guid? periodId, CancellationToken ct = default);
}
