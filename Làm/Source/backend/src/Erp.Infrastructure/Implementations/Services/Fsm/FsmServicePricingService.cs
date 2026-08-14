using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Fsm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class FsmServicePricingService : IFsmServicePricingService
{
    private readonly AppDbContext _db;

    public FsmServicePricingService(AppDbContext db)
    {
        _db = db;
    }

    // UC_FSM_004: Bảng giá dịch vụ
    public async Task<FsmServicePriceRateDto> CreateServicePriceRateAsync(Guid tenantId, FsmCreateServicePriceRateRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ServiceCode) || string.IsNullOrWhiteSpace(req.ServiceName))
            throw new AppException("Mã và tên dịch vụ kỹ thuật không được để trống.", 400);

        var entity = new FsmServicePriceRate
        {
            TenantId = tenantId,
            ServiceCode = req.ServiceCode,
            ServiceName = req.ServiceName,
            ServiceCategory = req.ServiceCategory ?? "Bảo Trì Định Kỳ",
            BaseHourlyRateVnd = req.BaseHourlyRateVnd > 0 ? req.BaseHourlyRateVnd : 250000,
            StandardTravelFeeVnd = req.StandardTravelFeeVnd >= 0 ? req.StandardTravelFeeVnd : 150000,
            EmergencySurchargePct = req.EmergencySurchargePct >= 0 ? req.EmergencySurchargePct : 30,
            IsActive = true
        };

        _db.FsmServicePriceRates.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FsmServicePriceRateDto(entity.Id, entity.ServiceCode, entity.ServiceName, entity.ServiceCategory, entity.BaseHourlyRateVnd, entity.StandardTravelFeeVnd, entity.EmergencySurchargePct, entity.IsActive);
    }

    public async Task<IReadOnlyList<FsmServicePriceRateDto>> GetServicePriceRatesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FsmServicePriceRates.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<FsmServicePriceRateDto>
            {
                new(Guid.NewGuid(), "FSM-MAINT-STD", "Bảo Trì & Vệ Sinh Máy Định Kỳ", "Bảo Trì", 250000m, 150000m, 30m, true),
                new(Guid.NewGuid(), "FSM-REPAIR-ELEC", "Sửa Chữa Hệ Thống Điện & Bo Mạch", "Sửa Chữa", 350000m, 200000m, 50m, true),
                new(Guid.NewGuid(), "FSM-INSTALL-RACK", "Lắp Đặt & Cấu Hình Tủ Server", "Lắp Đặt Mới", 300000m, 150000m, 20m, true)
            };
        }

        return list.Select(s => new FsmServicePriceRateDto(s.Id, s.ServiceCode, s.ServiceName, s.ServiceCategory, s.BaseHourlyRateVnd, s.StandardTravelFeeVnd, s.EmergencySurchargePct, s.IsActive)).ToList();
    }
}
