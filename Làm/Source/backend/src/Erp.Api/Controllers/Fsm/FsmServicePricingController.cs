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
[Route("api/fsm/service-pricing")]
public sealed class FsmServicePricingController : ControllerBase
{
    private readonly IFsmServicePricingService _svc;

    public FsmServicePricingController(IFsmServicePricingService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_FSM_004: Bảng giá dịch vụ
    [HttpPost("price-rates")]
    [AuthorizePermission("fsm.pricing.write")]
    public async Task<ActionResult<ApiResponse<FsmServicePriceRateDto>>> CreateServicePriceRate([FromBody] FsmCreateServicePriceRateRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmServicePriceRateDto>.Ok(await _svc.CreateServicePriceRateAsync(TenantId, req, ct)));

    [HttpGet("price-rates")]
    [AuthorizePermission("fsm.pricing.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmServicePriceRateDto>>>> GetServicePriceRates(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmServicePriceRateDto>>.Ok(await _svc.GetServicePriceRatesAsync(TenantId, ct)));
}
