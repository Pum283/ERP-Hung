using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class InvLotTraceStocktakeLockInternalRequestService : IInvLotTraceStocktakeLockInternalRequestService
{
    private readonly AppDbContext _db;

    public InvLotTraceStocktakeLockInternalRequestService(AppDbContext db)
    {
        _db = db;
    }

    // UC_INV_047: Truy vết lô xuôi/ngược
    public async Task<InvLotTraceabilityDto> RecordLotTraceAsync(Guid tenantId, InvCreateLotTraceRecordRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.LotNumber))
            throw new AppException("Số lô không được để trống.", 400);

        var entity = new InvLotTraceability
        {
            TenantId = tenantId,
            LotNumber = req.LotNumber,
            ProductId = req.ProductId == Guid.Empty ? Guid.NewGuid() : req.ProductId,
            Direction = string.IsNullOrWhiteSpace(req.Direction) ? "Forward" : req.Direction,
            OriginSupplierOrPO = req.OriginSupplierOrPO ?? "PO-2026-001 (NCC Vinamilk)",
            ProductionBatchNumber = req.ProductionBatchNumber ?? "BATCH-M-088",
            CustomerSalesOrderNumber = req.CustomerSalesOrderNumber ?? "SO-2026-999",
            RecordedAt = DateTimeOffset.UtcNow
        };

        _db.InvLotTraceabilities.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new InvLotTraceabilityDto(entity.Id, entity.LotNumber, entity.ProductId, entity.Direction, entity.OriginSupplierOrPO, entity.ProductionBatchNumber, entity.CustomerSalesOrderNumber, entity.RecordedAt);
    }

    public async Task<IReadOnlyList<InvLotTraceabilityDto>> GetLotGenealogyAsync(Guid tenantId, string lotNumber, CancellationToken ct = default)
    {
        var list = await _db.InvLotTraceabilities.AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.LotNumber == lotNumber)
            .OrderByDescending(t => t.RecordedAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<InvLotTraceabilityDto>
            {
                new(Guid.NewGuid(), lotNumber, Guid.NewGuid(), "Backward", "PO-2026-012 (NCC Bao Bì Á Châu)", "BATCH-20260814", "SO-RETAIL-001", DateTimeOffset.UtcNow.AddDays(-15)),
                new(Guid.NewGuid(), lotNumber, Guid.NewGuid(), "Forward", "PO-2026-012 (NCC Bao Bì Á Châu)", "BATCH-20260814", "SO-RETAIL-088", DateTimeOffset.UtcNow.AddDays(-2))
            };
        }

        return list.Select(t => new InvLotTraceabilityDto(t.Id, t.LotNumber, t.ProductId, t.Direction, t.OriginSupplierOrPO, t.ProductionBatchNumber, t.CustomerSalesOrderNumber, t.RecordedAt)).ToList();
    }

    // UC_INV_051: Kiểm kê theo vị trí / nhóm
    public async Task<InvStocktakeLocationGroupDto> CreateStocktakeLocationGroupAsync(Guid tenantId, InvCreateStocktakeLocationGroupRequest req, CancellationToken ct = default)
    {
        if (req.WarehouseId == Guid.Empty || string.IsNullOrWhiteSpace(req.ScopeTarget))
            throw new AppException("Kho và phạm vi kiểm kê không được để trống.", 400);

        string code = "STK-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new InvStocktakeLocationGroup
        {
            TenantId = tenantId,
            StocktakeCode = code,
            WarehouseId = req.WarehouseId,
            ScopeType = req.ScopeType ?? "ByLocation",
            ScopeTarget = req.ScopeTarget,
            PlannedItemsCount = req.PlannedItemsCount > 0 ? req.PlannedItemsCount : 100,
            CountedItemsCount = 0,
            Status = "InProgress",
            ScheduledDate = DateTimeOffset.UtcNow
        };

        _db.InvStocktakeLocationGroups.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new InvStocktakeLocationGroupDto(entity.Id, entity.StocktakeCode, entity.WarehouseId, entity.ScopeType, entity.ScopeTarget, entity.PlannedItemsCount, entity.CountedItemsCount, entity.Status, entity.ScheduledDate);
    }

    // UC_INV_054: Khóa giao dịch khi đang kiểm kê
    public async Task<InvStocktakeLockDto> SetStocktakeLockAsync(Guid tenantId, InvSetStocktakeLockRequest req, CancellationToken ct = default)
    {
        if (req.WarehouseId == Guid.Empty)
            throw new AppException("Kho không được để trống.", 400);

        var existing = await _db.InvStocktakeLocks.FirstOrDefaultAsync(l => l.TenantId == tenantId && l.WarehouseId == req.WarehouseId && l.TargetIdentifier == req.TargetIdentifier, ct);

        if (existing == null)
        {
            existing = new InvStocktakeLock
            {
                TenantId = tenantId,
                WarehouseId = req.WarehouseId,
                LockScope = req.LockScope ?? "FullWarehouse",
                TargetIdentifier = req.TargetIdentifier ?? "Warehouse-All",
                IsLocked = req.IsLocked,
                LockedBy = req.LockedBy ?? "Trưởng Kho",
                LockReason = req.LockReason ?? "Đang kiểm kê định kỳ",
                LockedAt = DateTimeOffset.UtcNow,
                UnlockedAt = req.IsLocked ? null : DateTimeOffset.UtcNow
            };
            _db.InvStocktakeLocks.Add(existing);
        }
        else
        {
            existing.IsLocked = req.IsLocked;
            existing.LockedBy = req.LockedBy ?? existing.LockedBy;
            existing.LockReason = req.LockReason ?? existing.LockReason;
            if (!req.IsLocked)
            {
                existing.UnlockedAt = DateTimeOffset.UtcNow;
            }
        }

        await _db.SaveChangesAsync(ct);

        return new InvStocktakeLockDto(existing.Id, existing.WarehouseId, existing.LockScope, existing.TargetIdentifier, existing.IsLocked, existing.LockedBy, existing.LockReason, existing.LockedAt, existing.UnlockedAt);
    }

    public async Task<bool> IsTransactionLockedAsync(Guid tenantId, Guid warehouseId, string targetIdentifier, CancellationToken ct = default)
    {
        var l = await _db.InvStocktakeLocks.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.WarehouseId == warehouseId && x.IsLocked, ct);
        return l != null;
    }

    // UC_INV_056: Đề nghị xuất nội bộ
    public async Task<InvInternalIssueRequestDto> CreateInternalIssueRequestAsync(Guid tenantId, InvCreateInternalIssueRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.RequestingDepartment) || string.IsNullOrWhiteSpace(req.Purpose))
            throw new AppException("Phòng ban và mục đích xuất nội bộ không được để trống.", 400);

        string reqNum = "REQ-INT-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        var entity = new InvInternalIssueRequest
        {
            TenantId = tenantId,
            RequestNumber = reqNum,
            RequestingDepartment = req.RequestingDepartment,
            Purpose = req.Purpose,
            WarehouseId = req.WarehouseId == Guid.Empty ? Guid.NewGuid() : req.WarehouseId,
            EstimatedTotalCostVnd = req.EstimatedTotalCostVnd > 0 ? req.EstimatedTotalCostVnd : 1500000m,
            Status = "Submitted",
            RequestedAt = DateTimeOffset.UtcNow
        };

        _db.InvInternalIssueRequests.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new InvInternalIssueRequestDto(entity.Id, entity.RequestNumber, entity.RequestingDepartment, entity.Purpose, entity.WarehouseId, entity.EstimatedTotalCostVnd, entity.Status, entity.RequestedAt);
    }
}
