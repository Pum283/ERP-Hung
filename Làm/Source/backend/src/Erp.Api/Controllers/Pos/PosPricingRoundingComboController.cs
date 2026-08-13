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
[Route("api/pos/pricing-rounding-combo")]
public sealed class PosPricingRoundingComboController : ControllerBase
{
    private readonly IPosPricingRoundingComboService _svc;

    public PosPricingRoundingComboController(IPosPricingRoundingComboService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_POS_017 & UC_POS_018: Giá theo khung giờ & ngày trong tuần
    [HttpGet("time-slot-rules")]
    [AuthorizePermission("pos.pricing.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosTimeSlotPriceRuleDto>>>> GetTimeSlotPriceRules(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosTimeSlotPriceRuleDto>>.Ok(await _svc.GetTimeSlotPriceRulesAsync(TenantId, ct)));

    [HttpPost("time-slot-rules")]
    [AuthorizePermission("pos.pricing.write")]
    public async Task<ActionResult<ApiResponse<PosTimeSlotPriceRuleDto>>> SaveTimeSlotPriceRule([FromBody] PosSaveTimeSlotPriceRuleRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosTimeSlotPriceRuleDto>.Ok(await _svc.SaveTimeSlotPriceRuleAsync(TenantId, req, ct)));

    // UC_POS_020: Làm tròn tiền thanh toán
    [HttpGet("calculate-cash-rounding")]
    [AuthorizePermission("pos.checkout.read")]
    public async Task<ActionResult<ApiResponse<PosCashRoundingCalculationDto>>> CalculateCashRounding([FromQuery] decimal originalTotalVnd, [FromQuery] int roundingInterval = 500, CancellationToken ct = default)
        => Ok(ApiResponse<PosCashRoundingCalculationDto>.Ok(await _svc.CalculateCashRoundingAsync(originalTotalVnd, roundingInterval, ct)));

    // UC_POS_023: Khuyến mại theo combo
    [HttpGet("combo-rules")]
    [AuthorizePermission("pos.promo.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosComboPromotionRuleDto>>>> GetComboPromotionRules(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosComboPromotionRuleDto>>.Ok(await _svc.GetComboPromotionRulesAsync(TenantId, ct)));

    [HttpPost("combo-rules")]
    [AuthorizePermission("pos.promo.write")]
    public async Task<ActionResult<ApiResponse<PosComboPromotionRuleDto>>> SaveComboPromotionRule([FromBody] PosSaveComboPromotionRuleRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosComboPromotionRuleDto>.Ok(await _svc.SaveComboPromotionRuleAsync(TenantId, req, ct)));
}
