using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ILogDriverProductivityCostService
{
    // UC_LOG_036: Năng suất tài xế / chuyến
    Task<LogDriverProductivitySummaryDto> GetDriverProductivityReportAsync(Guid tenantId, CancellationToken ct = default);

    // UC_LOG_037: Chi phí vận chuyển
    Task<LogShippingCostAllocationDto> CalculateTripCostAsync(Guid tenantId, LogCalculateTripCostRequest req, CancellationToken ct = default);
}
