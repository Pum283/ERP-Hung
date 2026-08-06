using System.Security.Claims;
using System.Text;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Pos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Pos;

[ApiController]
[Authorize]
[Route("api/pos/reports")]
public sealed class PosReportController : ControllerBase
{
    private readonly IPosReportService _svc;
    public PosReportController(IPosReportService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("by-time")]
    [AuthorizePermission("pos.sale.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosRevenueByTimeRowDto>>>> ByTime(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        [FromQuery] string grain = "day", [FromQuery] Guid? storeId = null, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<PosRevenueByTimeRowDto>>.Ok(
            await _svc.RevenueByTimeAsync(TenantId, from, to, grain, storeId, ct)));

    [HttpGet("by-product")]
    [AuthorizePermission("pos.sale.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosRevenueByProductRowDto>>>> ByProduct(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        [FromQuery] Guid? storeId = null, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<PosRevenueByProductRowDto>>.Ok(
            await _svc.RevenueByProductAsync(TenantId, from, to, storeId, ct)));

    [HttpGet("by-cashier")]
    [AuthorizePermission("pos.sale.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosRevenueByCashierRowDto>>>> ByCashier(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        [FromQuery] Guid? storeId = null, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<PosRevenueByCashierRowDto>>.Ok(
            await _svc.RevenueByCashierAsync(TenantId, from, to, storeId, ct)));

    [HttpGet("cancel-discount")]
    [AuthorizePermission("pos.sale.read")]
    public async Task<ActionResult<ApiResponse<PosCancelDiscountReportDto>>> CancelDiscount(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        [FromQuery] Guid? storeId = null, CancellationToken ct = default)
        => Ok(ApiResponse<PosCancelDiscountReportDto>.Ok(
            await _svc.CancelDiscountRatesAsync(TenantId, from, to, storeId, ct)));

    [HttpGet("top-products")]
    [AuthorizePermission("pos.sale.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosTopProductRowDto>>>> TopProducts(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        [FromQuery] int top = 10, [FromQuery] string by = "qty",
        [FromQuery] Guid? storeId = null, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<PosTopProductRowDto>>.Ok(
            await _svc.TopProductsAsync(TenantId, from, to, top, by, storeId, ct)));

    [HttpGet("store-compare")]
    [AuthorizePermission("pos.sale.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosStoreCompareRowDto>>>> StoreCompare(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<PosStoreCompareRowDto>>.Ok(
            await _svc.CompareStoresAsync(TenantId, from, to, ct)));

    [HttpGet("chain-live")]
    [AuthorizePermission("pos.sale.read")]
    public async Task<ActionResult<ApiResponse<PosChainLiveReportDto>>> ChainLive(CancellationToken ct = default)
        => Ok(ApiResponse<PosChainLiveReportDto>.Ok(await _svc.ChainLiveAsync(TenantId, null, ct)));

    [HttpGet("cost-variance")]
    [AuthorizePermission("pos.sale.read")]
    public async Task<ActionResult<ApiResponse<PosCostVarianceReportDto>>> CostVariance(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        [FromQuery] Guid? storeId = null, CancellationToken ct = default)
        => Ok(ApiResponse<PosCostVarianceReportDto>.Ok(
            await _svc.CostVarianceAsync(TenantId, from, to, storeId, ct)));

    [HttpGet("export.csv")]
    [AuthorizePermission("pos.sale.read")]
    public async Task<IActionResult> Export(
        [FromQuery] string report, [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        [FromQuery] string? grain = null, [FromQuery] Guid? storeId = null, CancellationToken ct = default)
    {
        var csv = await _svc.ExportCsvAsync(TenantId, report, from, to, grain, storeId, ct);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"pos-{report}-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
