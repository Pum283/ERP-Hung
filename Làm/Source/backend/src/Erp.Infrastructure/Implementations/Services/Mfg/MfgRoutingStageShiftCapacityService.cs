using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Mfg;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class MfgRoutingStageShiftCapacityService : IMfgRoutingStageShiftCapacityService
{
    private readonly AppDbContext _db;

    public MfgRoutingStageShiftCapacityService(AppDbContext db)
    {
        _db = db;
    }

    // UC_MFG_004: Danh mục công đoạn
    public async Task<MfgRoutingStageDto> CreateRoutingStageAsync(Guid tenantId, MfgCreateRoutingStageRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.StageCode) || string.IsNullOrWhiteSpace(req.StageName))
            throw new AppException("Mã và tên công đoạn không được để trống.", 400);

        var entity = new MfgRoutingStage
        {
            TenantId = tenantId,
            StageCode = req.StageCode,
            StageName = req.StageName,
            WorkCenterCode = req.WorkCenterCode ?? "WC-ASSEMBLY-01",
            StandardCycleTimeMinutes = req.StandardCycleTimeMinutes > 0 ? req.StandardCycleTimeMinutes : 15m,
            StandardSetupTimeMinutes = req.StandardSetupTimeMinutes >= 0 ? req.StandardSetupTimeMinutes : 30m,
            IsOutsourced = req.IsOutsourced,
            IsActive = true
        };

        _db.MfgRoutingStages.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgRoutingStageDto(entity.Id, entity.StageCode, entity.StageName, entity.WorkCenterCode, entity.StandardCycleTimeMinutes, entity.StandardSetupTimeMinutes, entity.IsOutsourced, entity.IsActive);
    }

    public async Task<IReadOnlyList<MfgRoutingStageDto>> GetRoutingStagesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.MfgRoutingStages.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<MfgRoutingStageDto>
            {
                new(Guid.NewGuid(), "OP-10-CUT", "Cắt Phôi Kim Loại CNC", "WC-CNC-01", 12m, 20m, false, true),
                new(Guid.NewGuid(), "OP-20-WELD", "Hàn Khung Định Hình", "WC-WELD-02", 25m, 15m, false, true),
                new(Guid.NewGuid(), "OP-30-PAINT", "Sơn Tĩnh Điện Bề Mặt", "WC-PAINT-OUT", 45m, 60m, true, true),
                new(Guid.NewGuid(), "OP-40-ASSEMBLE", "Lắp Ráp Thành Phẩm", "WC-ASSY-03", 30m, 10m, false, true)
            };
        }

        return list.Select(s => new MfgRoutingStageDto(s.Id, s.StageCode, s.StageName, s.WorkCenterCode, s.StandardCycleTimeMinutes, s.StandardSetupTimeMinutes, s.IsOutsourced, s.IsActive)).ToList();
    }

    // UC_MFG_005: Ca sản xuất / năng lực
    public async Task<MfgShiftCapacityDto> CreateShiftCapacityAsync(Guid tenantId, MfgCreateShiftCapacityRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ShiftCode) || string.IsNullOrWhiteSpace(req.WorkCenterCode))
            throw new AppException("Mã ca và mã trung tâm sản xuất không được để trống.", 400);

        var entity = new MfgShiftCapacity
        {
            TenantId = tenantId,
            ShiftCode = req.ShiftCode,
            ShiftName = req.ShiftName ?? "Ca Tiêu Chuẩn",
            WorkCenterCode = req.WorkCenterCode,
            AvailableHoursPerShift = req.AvailableHoursPerShift > 0 ? req.AvailableHoursPerShift : 8m,
            EfficiencyFactorPct = req.EfficiencyFactorPct > 0 ? req.EfficiencyFactorPct : 85m,
            MaxCapacityOutputUnits = req.MaxCapacityOutputUnits > 0 ? req.MaxCapacityOutputUnits : 500m,
            IsActive = true
        };

        _db.MfgShiftCapacities.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new MfgShiftCapacityDto(entity.Id, entity.ShiftCode, entity.ShiftName, entity.WorkCenterCode, entity.AvailableHoursPerShift, entity.EfficiencyFactorPct, entity.MaxCapacityOutputUnits, entity.IsActive);
    }

    public async Task<IReadOnlyList<MfgShiftCapacityDto>> GetShiftCapacitiesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.MfgShiftCapacities.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<MfgShiftCapacityDto>
            {
                new(Guid.NewGuid(), "MFG-SHIFT-1", "Ca Sáng (06:00 - 14:00)", "WC-CNC-01", 8m, 90m, 450m, true),
                new(Guid.NewGuid(), "MFG-SHIFT-2", "Ca Chiều (14:00 - 22:00)", "WC-CNC-01", 8m, 85m, 420m, true),
                new(Guid.NewGuid(), "MFG-SHIFT-3", "Ca Đêm (22:00 - 06:00)", "WC-ASSY-03", 8m, 75m, 350m, true)
            };
        }

        return list.Select(c => new MfgShiftCapacityDto(c.Id, c.ShiftCode, c.ShiftName, c.WorkCenterCode, c.AvailableHoursPerShift, c.EfficiencyFactorPct, c.MaxCapacityOutputUnits, c.IsActive)).ToList();
    }
}
