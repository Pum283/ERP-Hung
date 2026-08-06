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
[Route("api/fin/vat")]
public sealed class FinVatController : ControllerBase
{
    private readonly IFinVatService _svc;
    public FinVatController(IFinVatService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpPost("calculate")]
    [AuthorizePermission("fin.tax.read")]
    public async Task<ActionResult<ApiResponse<FinVatCalcResult>>> Calculate(
        [FromBody] FinVatCalcRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinVatCalcResult>.Ok(await _svc.CalculateAsync(TenantId, req, ct)));

    [HttpGet("documents")]
    [AuthorizePermission("fin.tax.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FinVatDocumentDto>>>> List(
        [FromQuery] string? direction, [FromQuery] Guid? periodId, [FromQuery] string? status,
        [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FinVatDocumentDto>>.Ok(
            await _svc.ListDocumentsAsync(TenantId, direction, periodId, status, from, to, ct)));

    [HttpPost("documents")]
    [AuthorizePermission("fin.tax.manage")]
    public async Task<ActionResult<ApiResponse<FinVatDocumentDto>>> Upsert(
        [FromBody] FinVatDocumentUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<FinVatDocumentDto>.Ok(await _svc.UpsertDocumentAsync(TenantId, UserId, req, ct)));

    [HttpPost("documents/{id:guid}/post")]
    [AuthorizePermission("fin.tax.manage")]
    public async Task<ActionResult<ApiResponse<FinVatDocumentDto>>> Post(Guid id, CancellationToken ct)
        => Ok(ApiResponse<FinVatDocumentDto>.Ok(await _svc.PostDocumentAsync(TenantId, UserId, id, ct)));

    [HttpPost("documents/{id:guid}/void")]
    [AuthorizePermission("fin.tax.manage")]
    public async Task<ActionResult<ApiResponse<FinVatDocumentDto>>> Void(
        Guid id, [FromBody] FinVatNoteRequest? req, CancellationToken ct)
        => Ok(ApiResponse<FinVatDocumentDto>.Ok(await _svc.VoidDocumentAsync(TenantId, UserId, id, req?.Note, ct)));

    [HttpGet("summary")]
    [AuthorizePermission("fin.tax.read")]
    public async Task<ActionResult<ApiResponse<FinVatSummaryDto>>> Summary(
        [FromQuery] Guid? periodId, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken ct)
        => Ok(ApiResponse<FinVatSummaryDto>.Ok(await _svc.GetSummaryAsync(TenantId, periodId, from, to, ct)));

    [HttpPost("register-from-ar/{arInvoiceId:guid}")]
    [AuthorizePermission("fin.tax.manage")]
    public async Task<ActionResult<ApiResponse<FinVatDocumentDto>>> FromAr(
        Guid arInvoiceId, [FromQuery] Guid? taxId, CancellationToken ct)
        => Ok(ApiResponse<FinVatDocumentDto>.Ok(
            await _svc.RegisterFromArAsync(TenantId, UserId, arInvoiceId, taxId, ct)));

    [HttpPost("register-from-ap/{apInvoiceId:guid}")]
    [AuthorizePermission("fin.tax.manage")]
    public async Task<ActionResult<ApiResponse<FinVatDocumentDto>>> FromAp(
        Guid apInvoiceId, [FromQuery] Guid? taxId, CancellationToken ct)
        => Ok(ApiResponse<FinVatDocumentDto>.Ok(
            await _svc.RegisterFromApAsync(TenantId, UserId, apInvoiceId, taxId, ct)));
}
