using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Prt;
using Erp.Application.Interfaces.Services.Prt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Prt;

[ApiController]
[Authorize]
[Route("api/prt/packages")]
public sealed class PrtPackageController : ControllerBase
{
    private readonly IPrtPackageService _svc;
    public PrtPackageController(IPrtPackageService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("prt.portal.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PrtPortalPackageDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PrtPortalPackageDto>>.Ok(await _svc.ListPackagesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("prt.portal.manage")]
    public async Task<ActionResult<ApiResponse<PrtPortalPackageDto>>> Upsert(
        [FromBody] PrtPortalPackageUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PrtPortalPackageDto>.Ok(await _svc.UpsertPackageAsync(TenantId, UserId, req, ct)));

    [HttpGet("enabled")]
    [AuthorizePermission("prt.portal.read")]
    public async Task<ActionResult<ApiResponse<PrtEnabledFeaturesDto>>> Enabled(
        [FromQuery] string? planCode, CancellationToken ct)
        => Ok(ApiResponse<PrtEnabledFeaturesDto>.Ok(await _svc.GetEnabledFeaturesAsync(TenantId, planCode, ct)));
}
