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
[Route("api/fin/currency-cashflow-category")]
public sealed class FinCurrencyCashFlowCategoryController : ControllerBase
{
    private readonly IFinCurrencyCashFlowCategoryService _svc;

    public FinCurrencyCashFlowCategoryController(IFinCurrencyCashFlowCategoryService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_FIN_005: Đồng tiền hạch toán & tỷ giá
    [HttpPost("exchange-rates")]
    [AuthorizePermission("fin.currency.write")]
    public async Task<ActionResult<ApiResponse<FinCurrencyExchangeRateDto>>> CreateExchangeRate([FromBody] FinCreateExchangeRateRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinCurrencyExchangeRateDto>.Ok(await _svc.CreateExchangeRateAsync(TenantId, req, ct)));

    [HttpGet("exchange-rates")]
    [AuthorizePermission("fin.currency.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinCurrencyExchangeRateDto>>>> GetExchangeRates(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinCurrencyExchangeRateDto>>.Ok(await _svc.GetExchangeRatesAsync(TenantId, ct)));

    // UC_FIN_007: Khoản mục thu/chi
    [HttpPost("cashflow-categories")]
    [AuthorizePermission("fin.category.write")]
    public async Task<ActionResult<ApiResponse<FinCashFlowCategoryDto>>> CreateCashFlowCategory([FromBody] FinCreateCashFlowCategoryRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinCashFlowCategoryDto>.Ok(await _svc.CreateCashFlowCategoryAsync(TenantId, req, ct)));

    [HttpGet("cashflow-categories")]
    [AuthorizePermission("fin.category.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinCashFlowCategoryDto>>>> GetCashFlowCategories(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinCashFlowCategoryDto>>.Ok(await _svc.GetCashFlowCategoriesAsync(TenantId, ct)));
}
