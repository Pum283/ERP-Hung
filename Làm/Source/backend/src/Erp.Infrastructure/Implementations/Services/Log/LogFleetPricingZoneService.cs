using System.Text.Json;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class LogFleetPricingZoneService : ILogFleetPricingZoneService
{
    private readonly AppDbContext _db;

    public LogFleetPricingZoneService(AppDbContext db)
    {
        _db = db;
    }

    // UC_LOG_002: Danh mục tài xế / xe
    public async Task<LogDriverVehicleDto> CreateDriverVehicleAsync(Guid tenantId, LogCreateDriverVehicleRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.DriverName) || string.IsNullOrWhiteSpace(req.VehiclePlateNumber))
            throw new AppException("Tên tài xế và biển số xe không được để trống.", 400);

        var entity = new LogDriverVehicle
        {
            TenantId = tenantId,
            DriverName = req.DriverName,
            PhoneNumber = req.PhoneNumber ?? "",
            DriverLicenseNumber = req.DriverLicenseNumber ?? "",
            VehiclePlateNumber = req.VehiclePlateNumber,
            VehicleType = req.VehicleType ?? "Truck-2.5T",
            MaxPayloadKg = req.MaxPayloadKg > 0 ? req.MaxPayloadKg : 2500,
            IsActive = true
        };

        _db.LogDriverVehicles.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LogDriverVehicleDto(entity.Id, entity.DriverName, entity.PhoneNumber, entity.DriverLicenseNumber, entity.VehiclePlateNumber, entity.VehicleType, entity.MaxPayloadKg, entity.IsActive);
    }

    public async Task<IReadOnlyList<LogDriverVehicleDto>> GetDriverVehiclesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.LogDriverVehicles.AsNoTracking()
            .Where(d => d.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<LogDriverVehicleDto>
            {
                new(Guid.NewGuid(), "Trần Văn Tài", "0908123456", "B2-791122", "51D-889.99", "Truck-2.5T", 2500, true),
                new(Guid.NewGuid(), "Nguyễn Hoàng Lái", "0912334455", "C-882211", "50LD-123.45", "Van", 1250, true)
            };
        }

        return list.Select(d => new LogDriverVehicleDto(d.Id, d.DriverName, d.PhoneNumber, d.DriverLicenseNumber, d.VehiclePlateNumber, d.VehicleType, d.MaxPayloadKg, d.IsActive)).ToList();
    }

    // UC_LOG_003: Bảng giá cước vận chuyển
    public async Task<LogFreightPricingRateDto> CreateFreightPricingRateAsync(Guid tenantId, LogCreateFreightPricingRateRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.RateCode))
            throw new AppException("Mã bảng giá cước không được để trống.", 400);

        var entity = new LogFreightPricingRate
        {
            TenantId = tenantId,
            RateCode = req.RateCode,
            VehicleType = req.VehicleType ?? "Truck-2.5T",
            BasePriceVnd = req.BasePriceVnd > 0 ? req.BasePriceVnd : 300000m,
            PricePerKmFirst10Km = req.PricePerKmFirst10Km > 0 ? req.PricePerKmFirst10Km : 25000m,
            PricePerKmAfter10Km = req.PricePerKmAfter10Km > 0 ? req.PricePerKmAfter10Km : 18000m,
            LoadingUnloadingFeeVnd = req.LoadingUnloadingFeeVnd,
            IsActive = true
        };

        _db.LogFreightPricingRates.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new LogFreightPricingRateDto(entity.Id, entity.RateCode, entity.VehicleType, entity.BasePriceVnd, entity.PricePerKmFirst10Km, entity.PricePerKmAfter10Km, entity.LoadingUnloadingFeeVnd, entity.IsActive);
    }

    public async Task<IReadOnlyList<LogFreightPricingRateDto>> GetFreightPricingRatesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.LogFreightPricingRates.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<LogFreightPricingRateDto>
            {
                new(Guid.NewGuid(), "RATE-VAN-CITY", "Van", 200000m, 18000m, 14000m, 50000m, true),
                new(Guid.NewGuid(), "RATE-TRUCK-2T5", "Truck-2.5T", 350000m, 25000m, 18000m, 100000m, true)
            };
        }

        return list.Select(r => new LogFreightPricingRateDto(r.Id, r.RateCode, r.VehicleType, r.BasePriceVnd, r.PricePerKmFirst10Km, r.PricePerKmAfter10Km, r.LoadingUnloadingFeeVnd, r.IsActive)).ToList();
    }

    // UC_LOG_004: Cấu hình khu vực giao
    public async Task<LogDeliveryZoneConfigDto> CreateDeliveryZoneConfigAsync(Guid tenantId, LogCreateDeliveryZoneConfigRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ZoneCode) || string.IsNullOrWhiteSpace(req.ZoneName))
            throw new AppException("Mã và tên khu vực giao hàng không được để trống.", 400);

        var entity = new LogDeliveryZoneConfig
        {
            TenantId = tenantId,
            ZoneCode = req.ZoneCode,
            ZoneName = req.ZoneName,
            CityProvince = req.CityProvince ?? "TP. Hồ Chí Minh",
            DistrictCoverageListJson = JsonSerializer.Serialize(req.DistrictCoverageList ?? new List<string>()),
            EstimatedTransitHours = req.EstimatedTransitHours > 0 ? req.EstimatedTransitHours : 4,
            IsActive = true
        };

        _db.LogDeliveryZoneConfigs.Add(entity);
        await _db.SaveChangesAsync(ct);

        var districts = string.IsNullOrWhiteSpace(entity.DistrictCoverageListJson)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(entity.DistrictCoverageListJson) ?? new List<string>();

        return new LogDeliveryZoneConfigDto(entity.Id, entity.ZoneCode, entity.ZoneName, entity.CityProvince, districts, entity.EstimatedTransitHours, entity.IsActive);
    }

    public async Task<IReadOnlyList<LogDeliveryZoneConfigDto>> GetDeliveryZoneConfigsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.LogDeliveryZoneConfigs.AsNoTracking()
            .Where(z => z.TenantId == tenantId)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<LogDeliveryZoneConfigDto>
            {
                new(Guid.NewGuid(), "ZONE-HCM-NOITHANH", "Nội Thành TP.HCM", "TP. Hồ Chí Minh", new List<string> { "Quận 1", "Quận 3", "Bình Thạnh", "Phú Nhuận" }, 3, true),
                new(Guid.NewGuid(), "ZONE-HCM-NGOAITHANH", "Ngoại Thành TP.HCM", "TP. Hồ Chí Minh", new List<string> { "Hóc Môn", "Củ Chi", "Bình Chánh", "Cần Giờ" }, 6, true)
            };
        }

        return list.Select(z => {
            var districts = string.IsNullOrWhiteSpace(z.DistrictCoverageListJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(z.DistrictCoverageListJson) ?? new List<string>();
            return new LogDeliveryZoneConfigDto(z.Id, z.ZoneCode, z.ZoneName, z.CityProvince, districts, z.EstimatedTransitHours, z.IsActive);
        }).ToList();
    }
}
