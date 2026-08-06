using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Hrm;

[ApiController]
[Authorize]
[Route("api/hrm/headcount-plans")]
public sealed class HrmHeadcountController : ControllerBase
{
    private readonly IHrmHeadcountService _svc;

    public HrmHeadcountController(IHrmHeadcountService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HeadcountPlanDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HeadcountPlanDto>>.Ok(await _svc.ListAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<HeadcountPlanDto>>> Upsert(
        [FromBody] HeadcountPlanUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HeadcountPlanDto>.Ok(await _svc.UpsertAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/submit")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<HeadcountPlanDto>>> Submit(Guid id, CancellationToken ct)
        => Ok(ApiResponse<HeadcountPlanDto>.Ok(await _svc.SubmitAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/approve")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<HeadcountPlanDto>>> Approve(Guid id, CancellationToken ct)
        => Ok(ApiResponse<HeadcountPlanDto>.Ok(await _svc.DecideAsync(TenantId, UserId, id, true, ct)));

    [HttpPost("{id:guid}/reject")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<HeadcountPlanDto>>> Reject(Guid id, CancellationToken ct)
        => Ok(ApiResponse<HeadcountPlanDto>.Ok(await _svc.DecideAsync(TenantId, UserId, id, false, ct)));

    [HttpGet("compare")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HeadcountCompareRowDto>>>> Compare(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HeadcountCompareRowDto>>.Ok(await _svc.CompareAsync(TenantId, ct)));

    [HttpGet("shortages")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HeadcountCompareRowDto>>>> Shortages(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HeadcountCompareRowDto>>.Ok(await _svc.ShortagesAsync(TenantId, ct)));
}
