using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/log/fleet-pricing-zone")]
public sealed class LogFleetPricingZoneController : ControllerBase
{
    private readonly ILogFleetPricingZoneService _svc;

    public LogFleetPricingZoneController(ILogFleetPricingZoneService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_LOG_002: Danh mục tài xế / xe
    [HttpPost("driver-vehicles")]
    [AuthorizePermission("log.fleet.write")]
    public async Task<ActionResult<ApiResponse<LogDriverVehicleDto>>> CreateDriverVehicle([FromBody] LogCreateDriverVehicleRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDriverVehicleDto>.Ok(await _svc.CreateDriverVehicleAsync(TenantId, req, ct)));

    [HttpGet("driver-vehicles")]
    [AuthorizePermission("log.fleet.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LogDriverVehicleDto>>>> GetDriverVehicles(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LogDriverVehicleDto>>.Ok(await _svc.GetDriverVehiclesAsync(TenantId, ct)));

    // UC_LOG_003: Bảng giá cước vận chuyển
    [HttpPost("freight-pricing-rates")]
    [AuthorizePermission("log.pricing.write")]
    public async Task<ActionResult<ApiResponse<LogFreightPricingRateDto>>> CreateFreightPricingRate([FromBody] LogCreateFreightPricingRateRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogFreightPricingRateDto>.Ok(await _svc.CreateFreightPricingRateAsync(TenantId, req, ct)));

    [HttpGet("freight-pricing-rates")]
    [AuthorizePermission("log.pricing.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LogFreightPricingRateDto>>>> GetFreightPricingRates(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LogFreightPricingRateDto>>.Ok(await _svc.GetFreightPricingRatesAsync(TenantId, ct)));

    // UC_LOG_004: Cấu hình khu vực giao
    [HttpPost("delivery-zones")]
    [AuthorizePermission("log.zone.write")]
    public async Task<ActionResult<ApiResponse<LogDeliveryZoneConfigDto>>> CreateDeliveryZoneConfig([FromBody] LogCreateDeliveryZoneConfigRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryZoneConfigDto>.Ok(await _svc.CreateDeliveryZoneConfigAsync(TenantId, req, ct)));

    [HttpGet("delivery-zones")]
    [AuthorizePermission("log.zone.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LogDeliveryZoneConfigDto>>>> GetDeliveryZoneConfigs(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LogDeliveryZoneConfigDto>>.Ok(await _svc.GetDeliveryZoneConfigsAsync(TenantId, ct)));
}
