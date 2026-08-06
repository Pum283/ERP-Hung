using System.Security.Claims;
using System.Text;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Pur;
using Erp.Application.Interfaces.Services.Pur;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Pur;

[ApiController]
[Authorize]
[Route("api/pur/reports")]
public sealed class PurReportController : ControllerBase
{
    private readonly IPurReportService _svc;
    public PurReportController(IPurReportService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("by-vendor")]
    [AuthorizePermission("pur.grn.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurPurchaseByVendorRowDto>>>> ByVendor(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        [FromQuery] Guid? vendorId = null, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<PurPurchaseByVendorRowDto>>.Ok(
            await _svc.PurchaseByVendorAsync(TenantId, from, to, vendorId, ct)));

    [HttpGet("by-product")]
    [AuthorizePermission("pur.grn.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurPurchaseByProductRowDto>>>> ByProduct(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        [FromQuery] Guid? vendorId = null, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<PurPurchaseByProductRowDto>>.Ok(
            await _svc.PurchaseByProductAsync(TenantId, from, to, vendorId, ct)));

    [HttpGet("open-pr")]
    [AuthorizePermission("pur.pr.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurOpenPrAgingRowDto>>>> OpenPr(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurOpenPrAgingRowDto>>.Ok(await _svc.OpenPrAgingAsync(TenantId, ct)));

    [HttpGet("open-po")]
    [AuthorizePermission("pur.po.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurOpenPoAgingRowDto>>>> OpenPo(
        [FromQuery] Guid? vendorId = null, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<PurOpenPoAgingRowDto>>.Ok(
            await _svc.OpenPoAgingAsync(TenantId, vendorId, ct)));

    [HttpGet("export.csv")]
    [AuthorizePermission("pur.grn.read")]
    public async Task<IActionResult> Export(
        [FromQuery] string report, [FromQuery] DateTimeOffset? from = null, [FromQuery] DateTimeOffset? to = null,
        [FromQuery] Guid? vendorId = null, CancellationToken ct = default)
    {
        var csv = await _svc.ExportCsvAsync(TenantId, report, from, to, vendorId, ct);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"pur-{report}-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
