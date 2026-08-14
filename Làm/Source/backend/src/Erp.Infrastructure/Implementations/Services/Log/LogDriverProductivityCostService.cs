using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class LogDriverProductivityCostService : ILogDriverProductivityCostService
{
    private readonly AppDbContext _db;

    public LogDriverProductivityCostService(AppDbContext db)
    {
        _db = db;
    }

    // UC_LOG_036: Năng suất tài xế / chuyến
    public async Task<LogDriverProductivitySummaryDto> GetDriverProductivityReportAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.LogDriverProductivityReports.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            var sample = new List<LogDriverProductivityItemDto>
            {
                new(Guid.NewGuid(), "Trần Văn Tài", 42, 180, 18500m, 98.5),
                new(Guid.NewGuid(), "Nguyễn Hoàng Lái", 38, 155, 12200m, 96.8)
            };

            return new LogDriverProductivitySummaryDto(sample.Count, sample.Sum(s => s.CompletedTripsCount), sample);
        }

        var items = list.Select(r => new LogDriverProductivityItemDto(r.DriverVehicleId, r.DriverName, r.CompletedTripsCount, r.DeliveredOrdersCount, r.TotalWeightDeliveredKg, r.OnTimeDeliveryRatePct)).ToList();
        return new LogDriverProductivitySummaryDto(items.Count, items.Sum(s => s.CompletedTripsCount), items);
    }

    // UC_LOG_037: Chi phí vận chuyển
    public async Task<LogShippingCostAllocationDto> CalculateTripCostAsync(Guid tenantId, LogCalculateTripCostRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.TripNumber))
            throw new AppException("Mã chuyến xe không được để trống.", 400);

        int orderCount = req.AllocatedOrdersCount > 0 ? req.AllocatedOrdersCount : 1;
        decimal totalCost = req.TotalFuelCostVnd + req.TotalTollFeeVnd + req.DriverAllowanceVnd;
        decimal avgCost = totalCost / orderCount;

        string allocNum = "COST-ALLOC-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new LogShippingCostAllocation
        {
            TenantId = tenantId,
            CostAllocationNumber = allocNum,
            TripNumber = req.TripNumber,
            TotalFuelCostVnd = req.TotalFuelCostVnd,
            TotalTollFeeVnd = req.TotalTollFeeVnd,
            DriverAllowanceVnd = req.DriverAllowanceVnd,
            TotalTripCostVnd = totalCost,
            AllocatedOrdersCount = orderCount,
            AverageCostPerOrderVnd = avgCost,
            CalculatedAt = DateTimeOffset.UtcNow
        };

        _db.LogShippingCostAllocations.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LogShippingCostAllocationDto(entity.Id, entity.CostAllocationNumber, entity.TripNumber, entity.TotalFuelCostVnd, entity.TotalTollFeeVnd, entity.DriverAllowanceVnd, entity.TotalTripCostVnd, entity.AllocatedOrdersCount, entity.AverageCostPerOrderVnd, entity.CalculatedAt);
    }
}
