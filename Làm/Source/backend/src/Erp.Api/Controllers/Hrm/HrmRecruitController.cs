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
[Route("api/hrm/recruitment-requests")]
public sealed class HrmRecruitController : ControllerBase
{
    private readonly IHrmRecruitService _svc;

    public HrmRecruitController(IHrmRecruitService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("hrm.recruit.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RecruitmentRequestDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<RecruitmentRequestDto>>.Ok(await _svc.ListAsync(TenantId, UserId, ct)));

    [HttpPost]
    [AuthorizePermission("hrm.recruit.manage")]
    public async Task<ActionResult<ApiResponse<RecruitmentRequestDto>>> Create(
        [FromBody] RecruitmentRequestCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<RecruitmentRequestDto>.Ok(await _svc.CreateAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/submit")]
    [AuthorizePermission("hrm.recruit.manage")]
    public async Task<ActionResult<ApiResponse<RecruitmentRequestDto>>> Submit(Guid id, CancellationToken ct)
        => Ok(ApiResponse<RecruitmentRequestDto>.Ok(await _svc.SubmitAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/close")]
    [AuthorizePermission("hrm.recruit.manage")]
    public async Task<ActionResult<ApiResponse<RecruitmentRequestDto>>> Close(Guid id, CancellationToken ct)
        => Ok(ApiResponse<RecruitmentRequestDto>.Ok(await _svc.CancelOrCloseAsync(TenantId, UserId, id, ct)));
}
