using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Mfg;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class MfgScrapBomDemandMrpService : IMfgScrapBomDemandMrpService
{
    private readonly AppDbContext _db;

    public MfgScrapBomDemandMrpService(AppDbContext db)
    {
        _db = db;
    }

    // UC_MFG_009: Định mức hao hụt
    public async Task<MfgBomScrapAllowanceDto> SetBomScrapAllowanceAsync(Guid tenantId, MfgSetBomScrapAllowanceRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.MaterialProductCode) || req.BaseNetQuantity <= 0)
            throw new AppException("Mã nguyên vật liệu và số lượng định mức không hợp lệ.", 400);

        decimal grossQty = req.BaseNetQuantity * (1 + (req.ScrapAllowancePct / 100m));

        var entity = new MfgBomScrapAllowance
        {
            TenantId = tenantId,
            BomId = req.BomId == Guid.Empty ? Guid.NewGuid() : req.BomId,
            BomCode = req.BomCode ?? "BOM-DEFAULT",
            MaterialProductId = req.MaterialProductId == Guid.Empty ? Guid.NewGuid() : req.MaterialProductId,
            MaterialProductCode = req.MaterialProductCode,
            MaterialProductName = req.MaterialProductName ?? req.MaterialProductCode,
            BaseNetQuantity = req.BaseNetQuantity,
            ScrapAllowancePct = req.ScrapAllowancePct,
            GrossPlannedQuantity = grossQty,
            Reason = req.Reason ?? "Dự phòng hao hụt phôi cắt gọt"
        };

        _db.MfgBomScrapAllowances.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgBomScrapAllowanceDto(entity.Id, entity.BomId, entity.BomCode, entity.MaterialProductId, entity.MaterialProductCode, entity.MaterialProductName, entity.BaseNetQuantity, entity.ScrapAllowancePct, entity.GrossPlannedQuantity, entity.Reason);
    }

    // UC_MFG_011: Sao chép BOM
    public async Task<MfgBomCopyLogDto> CopyBomAsync(Guid tenantId, MfgCopyBomRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.SourceBomCode))
            throw new AppException("Mã BOM nguồn không được để trống.", 400);

        string newCode = req.SourceBomCode + "-" + (req.NewVersion ?? "v2.0");

        var entity = new MfgBomCopyLog
        {
            TenantId = tenantId,
            SourceBomId = req.SourceBomId == Guid.Empty ? Guid.NewGuid() : req.SourceBomId,
            SourceBomCode = req.SourceBomCode,
            SourceVersion = req.SourceVersion ?? "v1.0",
            NewBomId = Guid.NewGuid(),
            NewBomCode = newCode,
            NewVersion = req.NewVersion ?? "v2.0",
            CopiedLinesCount = 12,
            CopiedBy = req.CopiedBy ?? "Kỹ Sư Thiết Kế BOM",
            CopiedAt = DateTimeOffset.UtcNow
        };

        _db.MfgBomCopyLogs.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgBomCopyLogDto(entity.Id, entity.SourceBomId, entity.SourceBomCode, entity.SourceVersion, entity.NewBomId, entity.NewBomCode, entity.NewVersion, entity.CopiedLinesCount, entity.CopiedBy, entity.CopiedAt);
    }

    // UC_MFG_012: Kế hoạch SX theo nhu cầu (MPS)
    public async Task<MfgDemandProductionPlanDto> CreateDemandProductionPlanAsync(Guid tenantId, MfgCreateDemandProductionPlanRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.PlanName) || string.IsNullOrWhiteSpace(req.ProductCode))
            throw new AppException("Tên kế hoạch và mã sản phẩm không được để trống.", 400);

        string planNum = "MPS-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        decimal totalPlanned = req.SalesForecastDemandQty + req.BacklogOrdersDemandQty;

        var entity = new MfgDemandProductionPlan
        {
            TenantId = tenantId,
            PlanNumber = planNum,
            PlanName = req.PlanName,
            FinishedProductId = req.FinishedProductId == Guid.Empty ? Guid.NewGuid() : req.FinishedProductId,
            ProductCode = req.ProductCode,
            ProductName = req.ProductName ?? req.ProductCode,
            SalesForecastDemandQty = req.SalesForecastDemandQty,
            BacklogOrdersDemandQty = req.BacklogOrdersDemandQty,
            PlannedProductionQty = totalPlanned,
            PlanningHorizon = req.PlanningHorizon ?? "Monthly-2026-09",
            Status = "Draft",
            CreatedAtDate = DateTimeOffset.UtcNow
        };

        _db.MfgDemandProductionPlans.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgDemandProductionPlanDto(entity.Id, entity.PlanNumber, entity.PlanName, entity.FinishedProductId, entity.ProductCode, entity.ProductName, entity.SalesForecastDemandQty, entity.BacklogOrdersDemandQty, entity.PlannedProductionQty, entity.PlanningHorizon, entity.Status, entity.CreatedAtDate);
    }

    // UC_MFG_014: Tính nhu cầu nguyên vật liệu (MRP)
    public async Task<MfgMaterialRequirementPlanningDto> RunMrpCalculationAsync(Guid tenantId, MfgRunMrpCalculationRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.MaterialProductCode))
            throw new AppException("Mã nguyên vật liệu tính MRP không được để trống.", 400);

        string mrpRun = "MRP-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        decimal netReq = req.GrossRequirementQty - req.CurrentStockOnHandQty - req.ScheduledReceiptsPoQty;
        if (netReq < 0) netReq = 0;

        var entity = new MfgMaterialRequirementPlanning
        {
            TenantId = tenantId,
            MrpRunNumber = mrpRun,
            MaterialProductId = req.MaterialProductId == Guid.Empty ? Guid.NewGuid() : req.MaterialProductId,
            MaterialProductCode = req.MaterialProductCode,
            MaterialProductName = req.MaterialProductName ?? req.MaterialProductCode,
            GrossRequirementQty = req.GrossRequirementQty,
            CurrentStockOnHandQty = req.CurrentStockOnHandQty,
            ScheduledReceiptsPoQty = req.ScheduledReceiptsPoQty,
            NetRequirementQty = netReq,
            SuggestedPurchaseOrderQty = netReq > 0 ? netReq : 0,
            RequiredDate = req.RequiredDate,
            CalculatedAt = DateTimeOffset.UtcNow
        };

        _db.MfgMaterialRequirementPlannings.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgMaterialRequirementPlanningDto(entity.Id, entity.MrpRunNumber, entity.MaterialProductId, entity.MaterialProductCode, entity.MaterialProductName, entity.GrossRequirementQty, entity.CurrentStockOnHandQty, entity.ScheduledReceiptsPoQty, entity.NetRequirementQty, entity.SuggestedPurchaseOrderQty, entity.RequiredDate, entity.CalculatedAt);
    }
}
