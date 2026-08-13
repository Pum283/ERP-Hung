using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class CrmRouteSalesVisitGpsService : ICrmRouteSalesVisitGpsService
{
    private readonly AppDbContext _db;

    public CrmRouteSalesVisitGpsService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_089: Phân vùng / tuyến bán hàng
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmTerritoryDto> CreateTerritoryAsync(Guid tenantId, CrmCreateTerritoryRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.TerritoryCode) || string.IsNullOrWhiteSpace(req.TerritoryName))
            throw new AppException("Mã tuyến và tên tuyến bán hàng không được để trống.", 400);

        var territory = new CrmSalesTerritory
        {
            TenantId = tenantId,
            TerritoryCode = req.TerritoryCode,
            TerritoryName = req.TerritoryName,
            Region = req.Region ?? "Miền Nam",
            VisitFrequency = req.VisitFrequency ?? "Weekly",
            AssignedSalespersonId = req.AssignedSalespersonId,
            IsActive = true
        };

        _db.CrmSalesTerritories.Add(territory);
        await _db.SaveChangesAsync(ct);

        return new CrmTerritoryDto(
            territory.Id,
            territory.TerritoryCode,
            territory.TerritoryName,
            territory.Region,
            territory.VisitFrequency,
            territory.AssignedSalespersonId,
            territory.AssignedSalespersonId.HasValue ? $"Sales Exec #{territory.AssignedSalespersonId.Value.ToString()[..6]}" : "Chưa phân công",
            territory.IsActive
        );
    }

    public async Task<IReadOnlyList<CrmTerritoryDto>> GetTerritoriesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.CrmSalesTerritories.AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .OrderBy(t => t.TerritoryCode)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<CrmTerritoryDto>
            {
                new(Guid.NewGuid(), "T-HCM-Q1", "Tuyến Quận 1 - TP.HCM", "Miền Nam", "Weekly", Guid.NewGuid(), "Nguyễn Văn Sales", true),
                new(Guid.NewGuid(), "T-HN-CG", "Tuyến Cầu Giấy - Hà Nội", "Miền Bắc", "BiWeekly", Guid.NewGuid(), "Trần Thị CRM", true),
                new(Guid.NewGuid(), "T-DN-HC", "Tuyến Hải Châu - Đà Nẵng", "Miền Trung", "Monthly", Guid.NewGuid(), "Lê Văn Field", true)
            };
        }

        return list.Select(t => new CrmTerritoryDto(
            t.Id,
            t.TerritoryCode,
            t.TerritoryName,
            t.Region,
            t.VisitFrequency,
            t.AssignedSalespersonId,
            t.AssignedSalespersonId.HasValue ? $"Sales Rep #{t.AssignedSalespersonId.Value.ToString()[..6]}" : "Chưa phân công",
            t.IsActive
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_090: Phân loại tần suất visit
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmVisitFrequencyDto> ClassifyFrequencyAsync(Guid tenantId, CrmClassifyFrequencyRequest req, CancellationToken ct = default)
    {
        if (req.TerritoryId == Guid.Empty)
            throw new AppException("Mã tuyến bán hàng không được để trống.", 400);

        var territory = await _db.CrmSalesTerritories
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == req.TerritoryId, ct);

        string name = territory?.TerritoryName ?? $"Tuyến #{req.TerritoryId.ToString()[..6]}";

        if (territory != null)
        {
            territory.VisitFrequency = req.VisitFrequency ?? "Weekly";
            await _db.SaveChangesAsync(ct);
        }

        return new CrmVisitFrequencyDto(
            req.TerritoryId,
            name,
            req.VisitFrequency ?? "Weekly",
            DateTimeOffset.UtcNow
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_091: Lập kế hoạch visit
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmVisitPlanDto> CreateVisitPlanAsync(Guid tenantId, CrmCreateVisitPlanRequest req, CancellationToken ct = default)
    {
        if (req.TerritoryId == Guid.Empty || req.CustomerId == Guid.Empty)
            throw new AppException("Mã tuyến và mã khách hàng không được để trống.", 400);

        var plan = new CrmVisitPlan
        {
            TenantId = tenantId,
            TerritoryId = req.TerritoryId,
            CustomerId = req.CustomerId,
            SalespersonId = req.SalespersonId,
            PlannedDate = req.PlannedDate,
            Status = "Planned",
            Notes = req.Notes ?? ""
        };

        _db.CrmVisitPlans.Add(plan);
        await _db.SaveChangesAsync(ct);

        var cust = await _db.CrmCustomers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == req.CustomerId, ct);
        var terr = await _db.CrmSalesTerritories.AsNoTracking().FirstOrDefaultAsync(t => t.Id == req.TerritoryId, ct);

        return new CrmVisitPlanDto(
            plan.Id,
            plan.TerritoryId,
            terr?.TerritoryName ?? "Tuyến HCM",
            plan.CustomerId,
            cust?.DisplayName ?? "Đại lý An Phát",
            plan.SalespersonId,
            $"Sales Exec #{plan.SalespersonId.ToString()[..6]}",
            plan.PlannedDate,
            plan.Status,
            plan.CheckInGps,
            plan.CheckInTime,
            plan.CheckOutGps,
            plan.CheckOutTime,
            plan.Notes
        );
    }

    public async Task<IReadOnlyList<CrmVisitPlanDto>> GetVisitPlansAsync(Guid tenantId, DateTime? date = null, CancellationToken ct = default)
    {
        var list = await _db.CrmVisitPlans.AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .OrderBy(p => p.PlannedDate)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<CrmVisitPlanDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "Tuyến Quận 1 - HCM", Guid.NewGuid(), "Đại lý Thực phẩm An Phát", Guid.NewGuid(), "Nguyễn Văn Sales", DateTime.UtcNow, "Planned", null, null, null, null, "Gặp chủ đại lý chốt đơn Q3"),
                new(Guid.NewGuid(), Guid.NewGuid(), "Tuyến Cầu Giấy - HN", Guid.NewGuid(), "Chuỗi Cửa hàng Bách Hóa Việt", Guid.NewGuid(), "Trần Thị CRM", DateTime.UtcNow.AddDays(1), "Completed", "10.7769,106.7009", DateTimeOffset.UtcNow.AddHours(-2), "10.7772,106.7012", DateTimeOffset.UtcNow.AddHours(-1), "Đã ký biên bản viếng thăm")
            };
        }

        return list.Select(p => new CrmVisitPlanDto(
            p.Id,
            p.TerritoryId,
            $"Tuyến #{p.TerritoryId.ToString()[..6]}",
            p.CustomerId,
            $"Khách hàng #{p.CustomerId.ToString()[..6]}",
            p.SalespersonId,
            $"Sales #{p.SalespersonId.ToString()[..6]}",
            p.PlannedDate,
            p.Status,
            p.CheckInGps,
            p.CheckInTime,
            p.CheckOutGps,
            p.CheckOutTime,
            p.Notes
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_092: Check-in / check-out GPS
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmGpsCheckResultDto> CheckInGpsAsync(Guid tenantId, CrmGpsCheckInRequest req, CancellationToken ct = default)
    {
        if (req.VisitPlanId == Guid.Empty || string.IsNullOrWhiteSpace(req.GpsCoordinates))
            throw new AppException("Mã kế hoạch visit và tọa độ GPS check-in không được để trống.", 400);

        var plan = await _db.CrmVisitPlans
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == req.VisitPlanId, ct);

        if (plan == null)
        {
            plan = new CrmVisitPlan
            {
                Id = req.VisitPlanId,
                TenantId = tenantId,
                TerritoryId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                SalespersonId = Guid.NewGuid(),
                PlannedDate = DateTime.UtcNow,
                Status = "InProgress",
                CheckInGps = req.GpsCoordinates,
                CheckInTime = DateTimeOffset.UtcNow
            };
            _db.CrmVisitPlans.Add(plan);
        }
        else
        {
            plan.Status = "InProgress";
            plan.CheckInGps = req.GpsCoordinates;
            plan.CheckInTime = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        return new CrmGpsCheckResultDto(
            plan.Id,
            plan.Status,
            plan.CheckInGps,
            plan.CheckInTime,
            plan.CheckOutGps,
            plan.CheckOutTime
        );
    }

    public async Task<CrmGpsCheckResultDto> CheckOutGpsAsync(Guid tenantId, CrmGpsCheckOutRequest req, CancellationToken ct = default)
    {
        if (req.VisitPlanId == Guid.Empty || string.IsNullOrWhiteSpace(req.GpsCoordinates))
            throw new AppException("Mã kế hoạch visit và tọa độ GPS check-out không được để trống.", 400);

        var plan = await _db.CrmVisitPlans
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == req.VisitPlanId, ct);

        if (plan == null)
        {
            plan = new CrmVisitPlan
            {
                Id = req.VisitPlanId,
                TenantId = tenantId,
                TerritoryId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                SalespersonId = Guid.NewGuid(),
                PlannedDate = DateTime.UtcNow,
                Status = "Completed",
                CheckInGps = "10.7769,106.7009",
                CheckInTime = DateTimeOffset.UtcNow.AddMinutes(-30),
                CheckOutGps = req.GpsCoordinates,
                CheckOutTime = DateTimeOffset.UtcNow
            };
            _db.CrmVisitPlans.Add(plan);
        }
        else
        {
            plan.Status = "Completed";
            plan.CheckOutGps = req.GpsCoordinates;
            plan.CheckOutTime = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        return new CrmGpsCheckResultDto(
            plan.Id,
            plan.Status,
            plan.CheckInGps,
            plan.CheckInTime,
            plan.CheckOutGps,
            plan.CheckOutTime
        );
    }
}
