using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmLeaveService
{
    Task<IReadOnlyList<LeaveBalanceDto>> ListBalancesAsync(Guid tenantId, Guid currentUserId, Guid? employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequestDto>> ListRequestsAsync(Guid tenantId, Guid currentUserId, Guid? employeeId, CancellationToken ct = default);
    Task<LeaveRequestDto> CreateAndOptionallySubmitAsync(Guid tenantId, Guid userId, LeaveRequestCreateRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<LeaveEntitlementRuleDto>> ListEntitlementRulesAsync(Guid tenantId, CancellationToken ct = default);
    Task<LeaveEntitlementRuleDto> UpsertEntitlementRuleAsync(Guid tenantId, Guid userId, LeaveEntitlementRuleUpsertRequest req, CancellationToken ct = default);

    Task<LeaveBalanceDto> AdjustBalanceAsync(Guid tenantId, Guid userId, LeaveBalanceAdjustRequest req, CancellationToken ct = default);
    Task<int> AllocateYearAsync(Guid tenantId, Guid userId, LeaveAllocateYearRequest req, CancellationToken ct = default);

    Task<LeaveRequestDto> CancelRequestAsync(Guid tenantId, Guid userId, Guid requestId, CancellationToken ct = default);

    Task<IReadOnlyList<LeaveCalendarItemDto>> CalendarAsync(
        Guid tenantId, Guid? orgUnitId, DateOnly? from, DateOnly? to, CancellationToken ct = default);

    Task<IReadOnlyList<HolidayDto>> ListHolidaysAsync(Guid tenantId, int? year, CancellationToken ct = default);
    Task<HolidayDto> UpsertHolidayAsync(Guid tenantId, Guid userId, HolidayUpsertRequest req, CancellationToken ct = default);
    Task<int> ImportHolidaysAsync(Guid tenantId, Guid userId, IReadOnlyList<HolidayImportItem> items, CancellationToken ct = default);

    Task<IReadOnlyList<LeaveReportRowDto>> ReportAsync(Guid tenantId, int year, Guid? orgUnitId, CancellationToken ct = default);
}
