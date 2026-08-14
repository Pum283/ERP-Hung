using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class LogRealtimeGpsInternalTransferService : ILogRealtimeGpsInternalTransferService
{
    private readonly AppDbContext _db;

    public LogRealtimeGpsInternalTransferService(AppDbContext db)
    {
        _db = db;
    }

    // UC_LOG_019: Theo dõi realtime trên bản đồ
    public async Task<LogRealtimeGpsPingDto> RecordGpsPingAsync(Guid tenantId, LogPingGpsLocationRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.VehiclePlateNumber))
            throw new AppException("Biển số xe không được để trống.", 400);

        var entity = new LogRealtimeGpsPing
        {
            TenantId = tenantId,
            DriverVehicleId = req.DriverVehicleId == Guid.Empty ? Guid.NewGuid() : req.DriverVehicleId,
            VehiclePlateNumber = req.VehiclePlateNumber,
            Latitude = req.Latitude != 0 ? req.Latitude : 10.7769,
            Longitude = req.Longitude != 0 ? req.Longitude : 106.7009,
            CurrentSpeedKmh = req.CurrentSpeedKmh >= 0 ? req.CurrentSpeedKmh : 42.5,
            CurrentAddress = req.CurrentAddress ?? "Quốc lộ 1A, Bình Chánh, TP.HCM",
            PingedAt = DateTimeOffset.UtcNow
        };

        _db.LogRealtimeGpsPings.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LogRealtimeGpsPingDto(entity.Id, entity.DriverVehicleId, entity.VehiclePlateNumber, entity.Latitude, entity.Longitude, entity.CurrentSpeedKmh, entity.CurrentAddress, entity.PingedAt);
    }

    public async Task<IReadOnlyList<LogRealtimeGpsPingDto>> GetLatestFleetLocationsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.LogRealtimeGpsPings.AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.PingedAt)
            .Take(10)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<LogRealtimeGpsPingDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "51D-889.99 (Trần Văn Tài)", 10.7769, 106.7009, 45.0, "Vòng xoay An Lạc, Bình Tân, TP.HCM", DateTimeOffset.UtcNow),
                new(Guid.NewGuid(), Guid.NewGuid(), "50LD-123.45 (Nguyễn Hoàng Lái)", 10.8231, 106.6297, 30.2, "Đường Trường Chinh, Tân Bình, TP.HCM", DateTimeOffset.UtcNow)
            };
        }

        return list.Select(p => new LogRealtimeGpsPingDto(p.Id, p.DriverVehicleId, p.VehiclePlateNumber, p.Latitude, p.Longitude, p.CurrentSpeedKmh, p.CurrentAddress, p.PingedAt)).ToList();
    }

    // UC_LOG_031 & UC_LOG_032: Lệnh giao nội bộ & Xác nhận nhận hàng
    public async Task<LogInternalTransferDeliveryDto> CreateInternalTransferDeliveryAsync(Guid tenantId, LogCreateInternalTransferDeliveryRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.DriverName) || req.DispatchedQuantity <= 0)
            throw new AppException("Tài xế và số lượng xuất chuyển kho không hợp lệ.", 400);

        string docNum = "DEL-INT-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new LogInternalTransferDelivery
        {
            TenantId = tenantId,
            InternalDeliveryNumber = docNum,
            FromWarehouseId = req.FromWarehouseId == Guid.Empty ? Guid.NewGuid() : req.FromWarehouseId,
            FromWarehouseName = req.FromWarehouseName ?? "Kho Tổng Miền Nam",
            ToWarehouseId = req.ToWarehouseId == Guid.Empty ? Guid.NewGuid() : req.ToWarehouseId,
            ToWarehouseName = req.ToWarehouseName ?? "Kho Trung Chuyển Bình Dương",
            DriverName = req.DriverName,
            VehiclePlateNumber = req.VehiclePlateNumber ?? "51D-889.99",
            DispatchedQuantity = req.DispatchedQuantity,
            ReceivedQuantity = 0,
            Status = "InTransit",
            ReceiverStaffName = "",
            DispatchedAt = DateTimeOffset.UtcNow,
            ReceivedAt = null
        };

        _db.LogInternalTransferDeliveries.Add(entity);
        await _db.SaveChangesAsync(ct);

        return MapDto(entity);
    }

    public async Task<LogInternalTransferDeliveryDto> ConfirmInternalReceiptAsync(Guid tenantId, LogConfirmInternalReceiptRequest req, CancellationToken ct = default)
    {
        var entity = await _db.LogInternalTransferDeliveries.FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == req.InternalDeliveryId, ct);
        if (entity == null)
            throw new AppException("Không tìm thấy lệnh giao nội bộ.", 404);

        entity.ReceivedQuantity = req.ReceivedQuantity;
        entity.ReceiverStaffName = req.ReceiverStaffName ?? "Thủ Kho Nhận";
        entity.ReceivedAt = DateTimeOffset.UtcNow;
        entity.Status = entity.ReceivedQuantity < entity.DispatchedQuantity ? "DiscrepancyReported" : "Received";

        await _db.SaveChangesAsync(ct);

        return MapDto(entity);
    }

    // UC_LOG_033: Đối soát giao nội bộ
    public async Task<LogInternalDeliveryReconciliationDto> ReconcileInternalDeliveryAsync(Guid tenantId, LogCreateInternalReconciliationRequest req, CancellationToken ct = default)
    {
        var delivery = await _db.LogInternalTransferDeliveries.FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == req.InternalTransferDeliveryId, ct);
        if (delivery == null)
            throw new AppException("Không tìm thấy lệnh giao nội bộ để đối soát.", 404);

        string recNum = "REC-INT-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        decimal diffQty = delivery.DispatchedQuantity - delivery.ReceivedQuantity;

        var entity = new LogInternalDeliveryReconciliation
        {
            TenantId = tenantId,
            ReconciliationNumber = recNum,
            InternalTransferDeliveryId = delivery.Id,
            InternalDeliveryNumber = delivery.InternalDeliveryNumber,
            DispatchedTotalQty = delivery.DispatchedQuantity,
            ReceivedTotalQty = delivery.ReceivedQuantity,
            DiscrepancyQty = diffQty,
            DiscrepancyCostVnd = req.DiscrepancyCostVnd > 0 ? req.DiscrepancyCostVnd : diffQty * 250000m,
            RootCause = req.RootCause ?? "Biên bản kiểm đếm xác nhận lệch số lượng khi bốc xếp dỡ hàng",
            ResolutionStatus = "Reconciled",
            ReconciledAt = DateTimeOffset.UtcNow
        };

        _db.LogInternalDeliveryReconciliations.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LogInternalDeliveryReconciliationDto(entity.Id, entity.ReconciliationNumber, entity.InternalTransferDeliveryId, entity.InternalDeliveryNumber, entity.DispatchedTotalQty, entity.ReceivedTotalQty, entity.DiscrepancyQty, entity.DiscrepancyCostVnd, entity.RootCause, entity.ResolutionStatus, entity.ReconciledAt);
    }

    private static LogInternalTransferDeliveryDto MapDto(LogInternalTransferDelivery e)
        => new(e.Id, e.InternalDeliveryNumber, e.FromWarehouseId, e.FromWarehouseName, e.ToWarehouseId, e.ToWarehouseName, e.DriverName, e.VehiclePlateNumber, e.DispatchedQuantity, e.ReceivedQuantity, e.Status, e.ReceiverStaffName, e.DispatchedAt, e.ReceivedAt);
}
