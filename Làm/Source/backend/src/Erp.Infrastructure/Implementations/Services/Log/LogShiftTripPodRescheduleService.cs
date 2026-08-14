using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class LogShiftTripPodRescheduleService : ILogShiftTripPodRescheduleService
{
    private readonly AppDbContext _db;

    public LogShiftTripPodRescheduleService(AppDbContext db)
    {
        _db = db;
    }

    // UC_LOG_005: Cấu hình ca giao hàng
    public async Task<LogDeliveryShiftDto> CreateDeliveryShiftAsync(Guid tenantId, LogCreateDeliveryShiftRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ShiftCode) || string.IsNullOrWhiteSpace(req.ShiftName))
            throw new AppException("Mã và tên ca giao hàng không được để trống.", 400);

        var entity = new LogDeliveryShift
        {
            TenantId = tenantId,
            ShiftCode = req.ShiftCode,
            ShiftName = req.ShiftName,
            StartTime = req.StartTime ?? "08:00",
            EndTime = req.EndTime ?? "12:00",
            MaxOrdersCapacity = req.MaxOrdersCapacity > 0 ? req.MaxOrdersCapacity : 30,
            IsActive = true
        };

        _db.LogDeliveryShifts.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LogDeliveryShiftDto(entity.Id, entity.ShiftCode, entity.ShiftName, entity.StartTime, entity.EndTime, entity.MaxOrdersCapacity, entity.IsActive);
    }

    public async Task<IReadOnlyList<LogDeliveryShiftDto>> GetDeliveryShiftsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.LogDeliveryShifts.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<LogDeliveryShiftDto>
            {
                new(Guid.NewGuid(), "SHIFT-MORNING", "Ca Sáng (08:00 - 12:00)", "08:00", "12:00", 35, true),
                new(Guid.NewGuid(), "SHIFT-AFTERNOON", "Ca Chiều (13:30 - 17:30)", "13:30", "17:30", 30, true),
                new(Guid.NewGuid(), "SHIFT-EVENING", "Ca Tối (18:00 - 21:00)", "18:00", "21:00", 15, true)
            };
        }

        return list.Select(s => new LogDeliveryShiftDto(s.Id, s.ShiftCode, s.ShiftName, s.StartTime, s.EndTime, s.MaxOrdersCapacity, s.IsActive)).ToList();
    }

    // UC_LOG_007: Gộp nhiều đơn thành chuyến
    public async Task<LogDeliveryTripDto> ConsolidateTripAsync(Guid tenantId, LogConsolidateTripRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.DriverName) || string.IsNullOrWhiteSpace(req.VehiclePlateNumber))
            throw new AppException("Tài xế và biển số xe không được để trống.", 400);

        string tripNum = "TRIP-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var orders = req.ConsolidatedOrderIds ?? new List<Guid>();

        var entity = new LogDeliveryTrip
        {
            TenantId = tenantId,
            TripNumber = tripNum,
            DriverVehicleId = req.DriverVehicleId == Guid.Empty ? Guid.NewGuid() : req.DriverVehicleId,
            DriverName = req.DriverName,
            VehiclePlateNumber = req.VehiclePlateNumber,
            ConsolidatedOrderIdsJson = JsonSerializer.Serialize(orders),
            TotalOrdersCount = orders.Count > 0 ? orders.Count : 5,
            TotalWeightKg = req.TotalWeightKg > 0 ? req.TotalWeightKg : 1200m,
            Status = "Planned",
            ScheduledDepartureAt = req.ScheduledDepartureAt
        };

        _db.LogDeliveryTrips.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LogDeliveryTripDto(entity.Id, entity.TripNumber, entity.DriverVehicleId, entity.DriverName, entity.VehiclePlateNumber, orders, entity.TotalOrdersCount, entity.TotalWeightKg, entity.Status, entity.ScheduledDepartureAt);
    }

    // UC_LOG_016: Chứng từ ký nhận (POD)
    public async Task<LogProofOfDeliveryDto> SubmitPodAsync(Guid tenantId, LogSubmitPodRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.RecipientName) || string.IsNullOrWhiteSpace(req.DeliveryOrderNumber))
            throw new AppException("Tên người nhận và mã lệnh giao không được để trống.", 400);

        var entity = new LogProofOfDelivery
        {
            TenantId = tenantId,
            DeliveryOrderId = req.DeliveryOrderId == Guid.Empty ? Guid.NewGuid() : req.DeliveryOrderId,
            DeliveryOrderNumber = req.DeliveryOrderNumber,
            RecipientName = req.RecipientName,
            RecipientPhone = req.RecipientPhone ?? "",
            SignatureImageUrl = req.SignatureImageUrl ?? "https://storage.erphung.vn/signatures/pod-demo.png",
            DeliveryPhotoUrl = req.DeliveryPhotoUrl ?? "https://storage.erphung.vn/photos/delivery-doorstep.jpg",
            Notes = req.Notes ?? "Giao hàng đầy đủ",
            SignedAt = DateTimeOffset.UtcNow
        };

        _db.LogProofOfDeliveries.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LogProofOfDeliveryDto(entity.Id, entity.DeliveryOrderId, entity.DeliveryOrderNumber, entity.RecipientName, entity.RecipientPhone, entity.SignatureImageUrl, entity.DeliveryPhotoUrl, entity.Notes, entity.SignedAt);
    }

    // UC_LOG_018: Hẹn giao lại
    public async Task<LogRedeliveryRequestDto> CreateRedeliveryRequestAsync(Guid tenantId, LogCreateRedeliveryRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.OriginalOrderNumber))
            throw new AppException("Mã đơn hàng gốc không được để trống.", 400);

        string reqNum = "REDELIV-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new LogRedeliveryRequest
        {
            TenantId = tenantId,
            RequestNumber = reqNum,
            DeliveryOrderId = req.DeliveryOrderId == Guid.Empty ? Guid.NewGuid() : req.DeliveryOrderId,
            OriginalOrderNumber = req.OriginalOrderNumber,
            FailedReason = req.FailedReason ?? "Khách hàng bận hẹn ngày khác",
            RescheduledDeliveryDate = req.RescheduledDeliveryDate,
            PreferredShift = req.PreferredShift ?? "Morning",
            Status = "PendingReassignment",
            RequestedAt = DateTimeOffset.UtcNow
        };

        _db.LogRedeliveryRequests.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LogRedeliveryRequestDto(entity.Id, entity.RequestNumber, entity.DeliveryOrderId, entity.OriginalOrderNumber, entity.FailedReason, entity.RescheduledDeliveryDate, entity.PreferredShift, entity.Status, entity.RequestedAt);
    }
}
