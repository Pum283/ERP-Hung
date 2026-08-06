using System.Security.Claims;
using System.Text;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Inv;
using Erp.Application.Interfaces.Services.Inv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Inv;

[ApiController]
[Authorize]
[Route("api/inv/reports")]
public sealed class InvReportController : ControllerBase
{
    private readonly IInvReportService _svc;
    public InvReportController(IInvReportService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("stock-value")]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvStockValueRowDto>>>> StockValue(
        [FromQuery] Guid? warehouseId = null, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<InvStockValueRowDto>>.Ok(
            await _svc.StockValueAsync(TenantId, warehouseId, ct)));

    [HttpGet("movement")]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvMovementPeriodRowDto>>>> Movement(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        [FromQuery] Guid? warehouseId = null, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<InvMovementPeriodRowDto>>.Ok(
            await _svc.MovementByPeriodAsync(TenantId, from, to, warehouseId, ct)));

    [HttpGet("sku-card")]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvSkuCardLineDto>>>> SkuCard(
        [FromQuery] Guid skuId, [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTimeOffset? from = null, [FromQuery] DateTimeOffset? to = null, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<InvSkuCardLineDto>>.Ok(
            await _svc.SkuCardAsync(TenantId, skuId, warehouseId, from, to, ct)));

    [HttpGet("min-max")]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvMinMaxAlertRowDto>>>> MinMax(
        [FromQuery] Guid? warehouseId = null, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<InvMinMaxAlertRowDto>>.Ok(
            await _svc.MinMaxAlertsAsync(TenantId, warehouseId, ct)));

    [HttpGet("stocktake")]
    [AuthorizePermission("inv.stocktake.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvStocktakeReportRowDto>>>> Stocktake(
        [FromQuery] Guid? stocktakeId = null, [FromQuery] Guid? warehouseId = null, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<InvStocktakeReportRowDto>>.Ok(
            await _svc.StocktakeResultAsync(TenantId, stocktakeId, warehouseId, ct)));

    [HttpGet("dashboard")]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<InvDashboardDto>>> Dashboard(
        [FromQuery] Guid? warehouseId = null, CancellationToken ct = default)
        => Ok(ApiResponse<InvDashboardDto>.Ok(await _svc.DashboardAsync(TenantId, warehouseId, ct)));

    [HttpGet("near-expiry")]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvNearExpiryRowDto>>>> NearExpiry(
        [FromQuery] int withinDays = 30, [FromQuery] Guid? warehouseId = null, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<InvNearExpiryRowDto>>.Ok(
            await _svc.NearExpiryAsync(TenantId, withinDays, warehouseId, ct)));

    [HttpGet("export.csv")]
    [AuthorizePermission("inv.stock.read")]
    public async Task<IActionResult> Export(
        [FromQuery] string report, [FromQuery] Guid? warehouseId = null, [FromQuery] Guid? skuId = null,
        [FromQuery] Guid? stocktakeId = null, [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null, [FromQuery] int? withinDays = null, CancellationToken ct = default)
    {
        var csv = await _svc.ExportCsvAsync(
            TenantId, report, warehouseId, skuId, stocktakeId, from, to, withinDays, ct);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"inv-{report}-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
