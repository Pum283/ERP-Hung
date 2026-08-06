using System.Security.Claims;
using System.Text;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Crm;
using Erp.Application.Interfaces.Services.Crm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Crm;

[ApiController]
[Authorize]
[Route("api/crm/customers")]
public sealed class CrmCustomerController : ControllerBase
{
    private readonly ICrmCustomerService _svc;
    public CrmCustomerController(ICrmCustomerService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("crm.customer.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmCustomerDto>>>> Search(
        [FromQuery] string? q,
        [FromQuery] string? customerType,
        [FromQuery] string? segment,
        [FromQuery] string? status,
        [FromQuery] Guid? ownerUserId,
        [FromQuery] string? phone,
        [FromQuery] string? taxCode,
        [FromQuery] bool includeMerged = false,
        CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<CrmCustomerDto>>.Ok(await _svc.SearchAsync(TenantId,
            new CrmCustomerSearchRequest(q, customerType, segment, status, ownerUserId, phone, taxCode, includeMerged), ct)));

    [HttpPost]
    [AuthorizePermission("crm.customer.manage")]
    public async Task<ActionResult<ApiResponse<CrmCustomerDto>>> Upsert(
        [FromBody] CrmCustomerUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCustomerDto>.Ok(await _svc.UpsertAsync(TenantId, UserId, req, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("crm.customer.read")]
    public async Task<ActionResult<ApiResponse<CrmCustomer360Dto>>> Get360(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmCustomer360Dto>.Ok(await _svc.Get360Async(TenantId, id, ct)));

    [HttpGet("duplicates")]
    [AuthorizePermission("crm.customer.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmDuplicateHitDto>>>> Duplicates(
        [FromQuery] string? phone, [FromQuery] string? taxCode, [FromQuery] Guid? excludeId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmDuplicateHitDto>>.Ok(
            await _svc.FindDuplicatesAsync(TenantId, phone, taxCode, excludeId, ct)));

    [HttpPost("{id:guid}/assign-owner")]
    [AuthorizePermission("crm.customer.manage")]
    public async Task<ActionResult<ApiResponse<CrmCustomerDto>>> AssignOwner(
        Guid id, [FromBody] CrmAssignOwnerRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCustomerDto>.Ok(await _svc.AssignOwnerAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/handover")]
    [AuthorizePermission("crm.customer.manage")]
    public async Task<ActionResult<ApiResponse<CrmHandoverDto>>> Handover(
        Guid id, [FromBody] CrmHandoverRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmHandoverDto>.Ok(await _svc.HandoverAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("merge")]
    [AuthorizePermission("crm.customer.manage")]
    public async Task<ActionResult<ApiResponse<CrmCustomerDto>>> Merge(
        [FromBody] CrmMergeRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCustomerDto>.Ok(await _svc.MergeAsync(TenantId, UserId, req, ct)));

    [HttpGet("{id:guid}/contacts")]
    [AuthorizePermission("crm.customer.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmContactDto>>>> Contacts(
        Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmContactDto>>.Ok(await _svc.ListContactsAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/contacts")]
    [AuthorizePermission("crm.customer.manage")]
    public async Task<ActionResult<ApiResponse<CrmContactDto>>> UpsertContact(
        Guid id, [FromBody] CrmContactUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmContactDto>.Ok(await _svc.UpsertContactAsync(TenantId, UserId, id, req, ct)));

    [HttpGet("export.csv")]
    [AuthorizePermission("crm.customer.read")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var csv = await _svc.ExportCsvAsync(TenantId, ct);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "crm-customers.csv");
    }

    [HttpPost("import")]
    [AuthorizePermission("crm.customer.manage")]
    public async Task<ActionResult<ApiResponse<CrmImportResult>>> Import(
        [FromBody] CrmImportRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmImportResult>.Ok(await _svc.ImportCsvAsync(TenantId, UserId, req, ct)));
}
