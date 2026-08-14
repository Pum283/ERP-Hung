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
[Route("api/mfg/pack-blend-oee")]
public sealed class MfgPackBlendOeeController : ControllerBase
{
    private readonly IMfgPackBlendOeeService _svc;

    public MfgPackBlendOeeController(IMfgPackBlendOeeService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_MFG_039: Đóng gói & gắn tem
    [HttpPost("packaging-labels")]
    [AuthorizePermission("mfg.pack.write")]
    public async Task<ActionResult<ApiResponse<MfgPackagingLabelTagDto>>> CreatePackagingLabelTag([FromBody] MfgCreatePackagingLabelRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgPackagingLabelTagDto>.Ok(await _svc.CreatePackagingLabelTagAsync(TenantId, req, ct)));

    [HttpGet("packaging-labels")]
    [AuthorizePermission("mfg.pack.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgPackagingLabelTagDto>>>> GetPackagingLabelTags(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgPackagingLabelTagDto>>.Ok(await _svc.GetPackagingLabelTagsAsync(TenantId, ct)));

    // UC_MFG_040: Định mức phối trộn
    [HttpPost("blending-recipes")]
    [AuthorizePermission("mfg.blend.write")]
    public async Task<ActionResult<ApiResponse<MfgBlendingRecipeRatioDto>>> CreateBlendingRecipeRatio([FromBody] MfgCreateBlendingRecipeRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgBlendingRecipeRatioDto>.Ok(await _svc.CreateBlendingRecipeRatioAsync(TenantId, req, ct)));

    [HttpGet("blending-recipes")]
    [AuthorizePermission("mfg.blend.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgBlendingRecipeRatioDto>>>> GetBlendingRecipeRatios(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgBlendingRecipeRatioDto>>.Ok(await _svc.GetBlendingRecipeRatiosAsync(TenantId, ct)));

    // UC_MFG_044: Hiệu suất / OEE
    [HttpPost("oee/calculate")]
    [AuthorizePermission("mfg.oee.write")]
    public async Task<ActionResult<ApiResponse<MfgOverallEquipmentEffectivenessDto>>> CalculateOee([FromBody] MfgCalculateOeeRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgOverallEquipmentEffectivenessDto>.Ok(await _svc.CalculateOeeAsync(TenantId, req, ct)));
}
