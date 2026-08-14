using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Mfg;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class MfgScheduleProgressReworkCostService : IMfgScheduleProgressReworkCostService
{
    private readonly AppDbContext _db;

    public MfgScheduleProgressReworkCostService(AppDbContext db)
    {
        _db = db;
    }

    // UC_MFG_016: Lịch SX theo xưởng/ca
    public async Task<MfgWorkshopShiftScheduleDto> CreateWorkshopShiftScheduleAsync(Guid tenantId, MfgCreateWorkshopShiftScheduleRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.WorkshopCode) || string.IsNullOrWhiteSpace(req.ShiftCode))
            throw new AppException("Xưởng sản xuất và ca làm việc không được để trống.", 400);

        string schedNum = "SCHED-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new MfgWorkshopShiftSchedule
        {
            TenantId = tenantId,
            ScheduleNumber = schedNum,
            WorkshopCode = req.WorkshopCode,
            ShiftCode = req.ShiftCode,
            ScheduledDate = req.ScheduledDate,
            WorkOrderId = req.WorkOrderId == Guid.Empty ? Guid.NewGuid() : req.WorkOrderId,
            WorkOrderNumber = req.WorkOrderNumber ?? "WO-DEFAULT",
            TargetQuantity = req.TargetQuantity > 0 ? req.TargetQuantity : 100,
            Status = "Scheduled"
        };

        _db.MfgWorkshopShiftSchedules.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgWorkshopShiftScheduleDto(entity.Id, entity.ScheduleNumber, entity.WorkshopCode, entity.ShiftCode, entity.ScheduledDate, entity.WorkOrderId, entity.WorkOrderNumber, entity.TargetQuantity, entity.Status);
    }

    public async Task<IReadOnlyList<MfgWorkshopShiftScheduleDto>> GetSchedulesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.MfgWorkshopShiftSchedules.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<MfgWorkshopShiftScheduleDto>
            {
                new(Guid.NewGuid(), "SCHED-20260814-001", "Xưởng Cơ Khí 1", "Ca Sáng (06:00-14:00)", DateTimeOffset.UtcNow, Guid.NewGuid(), "WO-2026-088", 250, "Running"),
                new(Guid.NewGuid(), "SCHED-20260814-002", "Xưởng Lắp Ráp 2", "Ca Chiều (14:00-22:00)", DateTimeOffset.UtcNow, Guid.NewGuid(), "WO-2026-089", 180, "Scheduled")
            };
        }

        return list.Select(s => new MfgWorkshopShiftScheduleDto(s.Id, s.ScheduleNumber, s.WorkshopCode, s.ShiftCode, s.ScheduledDate, s.WorkOrderId, s.WorkOrderNumber, s.TargetQuantity, s.Status)).ToList();
    }

    // UC_MFG_021: Ghi nhận tiến độ công đoạn
    public async Task<MfgOperationProgressTrackingDto> LogOperationProgressAsync(Guid tenantId, MfgLogOperationProgressRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.OperationCode))
            throw new AppException("Mã công đoạn không được để trống.", 400);

        var entity = new MfgOperationProgressTracking
        {
            TenantId = tenantId,
            WorkOrderId = req.WorkOrderId == Guid.Empty ? Guid.NewGuid() : req.WorkOrderId,
            WorkOrderNumber = req.WorkOrderNumber ?? "WO-DEFAULT",
            OperationCode = req.OperationCode,
            OperationName = req.OperationName ?? req.OperationCode,
            CompletedQuantity = req.CompletedQuantity,
            DefectiveQuantity = req.DefectiveQuantity,
            OperatorName = req.OperatorName ?? "Công Nhân Vận Hành",
            LoggedAt = DateTimeOffset.UtcNow
        };

        _db.MfgOperationProgressTrackings.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgOperationProgressTrackingDto(entity.Id, entity.WorkOrderId, entity.WorkOrderNumber, entity.OperationCode, entity.OperationName, entity.CompletedQuantity, entity.DefectiveQuantity, entity.OperatorName, entity.LoggedAt);
    }

    // UC_MFG_026: Lệnh sản xuất lại
    public async Task<MfgReworkWorkOrderDto> CreateReworkWorkOrderAsync(Guid tenantId, MfgCreateReworkWorkOrderRequest req, CancellationToken ct = default)
    {
        if (req.ReworkQuantity <= 0)
            throw new AppException("Số lượng sản xuất lại phải lớn hơn 0.", 400);

        string rwNum = "WO-REWORK-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new MfgReworkWorkOrder
        {
            TenantId = tenantId,
            ReworkWoNumber = rwNum,
            OriginalWorkOrderId = req.OriginalWorkOrderId == Guid.Empty ? Guid.NewGuid() : req.OriginalWorkOrderId,
            OriginalWoNumber = req.OriginalWoNumber ?? "WO-ORIGINAL",
            DefectReason = req.DefectReason ?? "Mối hàn chưa đạt tiêu chuẩn kỹ thuật",
            ReworkQuantity = req.ReworkQuantity,
            AssignedWorkshopCode = req.AssignedWorkshopCode ?? "WC-REWORK-01",
            Status = "Approved",
            CreatedAtDate = DateTimeOffset.UtcNow
        };

        _db.MfgReworkWorkOrders.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgReworkWorkOrderDto(entity.Id, entity.ReworkWoNumber, entity.OriginalWorkOrderId, entity.OriginalWoNumber, entity.DefectReason, entity.ReworkQuantity, entity.AssignedWorkshopCode, entity.Status, entity.CreatedAtDate);
    }

    // UC_MFG_028: Phân bổ nhân công / chi phí chung
    public async Task<MfgOverheadCostAllocationDto> AllocateOverheadCostAsync(Guid tenantId, MfgAllocateOverheadCostRequest req, CancellationToken ct = default)
    {
        decimal produced = req.ProducedQuantity > 0 ? req.ProducedQuantity : 1;
        decimal total = req.DirectLaborCostVnd + req.MachineDepreciationCostVnd + req.FactoryOverheadCostVnd;
        decimal unit = total / produced;

        string allocNum = "MFG-COST-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new MfgOverheadCostAllocation
        {
            TenantId = tenantId,
            AllocationNumber = allocNum,
            WorkOrderId = req.WorkOrderId == Guid.Empty ? Guid.NewGuid() : req.WorkOrderId,
            WorkOrderNumber = req.WorkOrderNumber ?? "WO-DEFAULT",
            DirectLaborCostVnd = req.DirectLaborCostVnd,
            MachineDepreciationCostVnd = req.MachineDepreciationCostVnd,
            FactoryOverheadCostVnd = req.FactoryOverheadCostVnd,
            TotalAllocatedCostVnd = total,
            ProducedQuantity = produced,
            UnitCostVnd = unit,
            AllocatedAt = DateTimeOffset.UtcNow
        };

        _db.MfgOverheadCostAllocations.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgOverheadCostAllocationDto(entity.Id, entity.AllocationNumber, entity.WorkOrderId, entity.WorkOrderNumber, entity.DirectLaborCostVnd, entity.MachineDepreciationCostVnd, entity.FactoryOverheadCostVnd, entity.TotalAllocatedCostVnd, entity.ProducedQuantity, entity.UnitCostVnd, entity.AllocatedAt);
    }
}
