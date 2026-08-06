using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Fin;
using Erp.Application.Interfaces.Services.Fin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Fin;

[ApiController]
[Authorize]
[Route("api/fin/revenue")]
public sealed class FinRevenueController : ControllerBase
{
    private readonly IFinRevenueService _svc;
    public FinRevenueController(IFinRevenueService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("documents")]
    [AuthorizePermission("fin.revenue.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinRevenueDocumentDto>>>> List(
        [FromQuery] string? kind, [FromQuery] Guid? periodId, [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinRevenueDocumentDto>>.Ok(
            await _svc.ListAsync(TenantId, kind, periodId, status, ct)));

    [HttpGet("summary")]
    [AuthorizePermission("fin.revenue.read")]
    public async Task<ActionResult<ApiResponse<FinRevenueSummaryDto>>> Summary(
        [FromQuery] Guid? periodId, CancellationToken ct)
        => Ok(ApiResponse<FinRevenueSummaryDto>.Ok(await _svc.GetSummaryAsync(TenantId, periodId, ct)));

    [HttpPost("recognize-from-pos/{saleId:guid}")]
    [AuthorizePermission("fin.revenue.manage")]
    public async Task<ActionResult<ApiResponse<FinRevenueDocumentDto>>> FromPos(
        Guid saleId, [FromBody] FinRevenueRecognizeRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinRevenueDocumentDto>.Ok(
            await _svc.RecognizeFromPosAsync(TenantId, UserId, saleId, req, ct)));

    [HttpPost("recognize-from-order/{orderId:guid}")]
    [AuthorizePermission("fin.revenue.manage")]
    public async Task<ActionResult<ApiResponse<FinRevenueDocumentDto>>> FromOrder(
        Guid orderId, [FromBody] FinRevenueRecognizeRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinRevenueDocumentDto>.Ok(
            await _svc.RecognizeFromSalesOrderAsync(TenantId, UserId, orderId, req, ct)));

    [HttpPost("recognize-from-ar/{arInvoiceId:guid}")]
    [AuthorizePermission("fin.revenue.manage")]
    public async Task<ActionResult<ApiResponse<FinRevenueDocumentDto>>> FromAr(
        Guid arInvoiceId, [FromBody] FinRevenueRecognizeRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinRevenueDocumentDto>.Ok(
            await _svc.RecognizeFromArInvoiceAsync(TenantId, UserId, arInvoiceId, req, ct)));

    [HttpPost("recognize-cogs/{invStockDocId:guid}")]
    [AuthorizePermission("fin.revenue.manage")]
    public async Task<ActionResult<ApiResponse<FinRevenueDocumentDto>>> Cogs(
        Guid invStockDocId, [FromBody] FinRevenueRecognizeRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinRevenueDocumentDto>.Ok(
            await _svc.RecognizeCogsAsync(TenantId, UserId, invStockDocId, req, ct)));

    [HttpPost("documents/{id:guid}/void")]
    [AuthorizePermission("fin.revenue.manage")]
    public async Task<ActionResult<ApiResponse<FinRevenueDocumentDto>>> Void(
        Guid id, [FromBody] FinRevenueNoteRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinRevenueDocumentDto>.Ok(
            await _svc.VoidAsync(TenantId, UserId, id, req?.Note, ct)));
}
