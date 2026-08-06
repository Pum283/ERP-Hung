using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Ast;
using Erp.Application.Interfaces.Services.Ast;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Ast;

[ApiController]
[Authorize]
[Route("api/ast/groups")]
public sealed class AstGroupController : ControllerBase
{
    private readonly IAstAssetService _svc;
    public AstGroupController(IAstAssetService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("ast.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AstAssetGroupDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AstAssetGroupDto>>.Ok(await _svc.ListGroupsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("ast.master.manage")]
    public async Task<ActionResult<ApiResponse<AstAssetGroupDto>>> Upsert(
        [FromBody] AstAssetGroupUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<AstAssetGroupDto>.Ok(await _svc.UpsertGroupAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/ast/locations")]
public sealed class AstLocationController : ControllerBase
{
    private readonly IAstAssetService _svc;
    public AstLocationController(IAstAssetService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("ast.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AstLocationDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AstLocationDto>>.Ok(await _svc.ListLocationsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("ast.master.manage")]
    public async Task<ActionResult<ApiResponse<AstLocationDto>>> Upsert(
        [FromBody] AstLocationUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<AstLocationDto>.Ok(await _svc.UpsertLocationAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/ast/depreciation-methods")]
public sealed class AstDepreciationMethodController : ControllerBase
{
    private readonly IAstAssetService _svc;
    public AstDepreciationMethodController(IAstAssetService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("ast.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AstDepreciationMethodDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AstDepreciationMethodDto>>.Ok(await _svc.ListMethodsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("ast.master.manage")]
    public async Task<ActionResult<ApiResponse<AstDepreciationMethodDto>>> Upsert(
        [FromBody] AstDepreciationMethodUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<AstDepreciationMethodDto>.Ok(await _svc.UpsertMethodAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/ast/assets")]
public sealed class AstAssetController : ControllerBase
{
    private readonly IAstAssetService _svc;
    public AstAssetController(IAstAssetService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("ast.asset.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AstAssetDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AstAssetDto>>.Ok(await _svc.ListAssetsAsync(TenantId, q, ct)));

    [HttpPost]
    [AuthorizePermission("ast.asset.manage")]
    public async Task<ActionResult<ApiResponse<AstAssetDto>>> Upsert(
        [FromBody] AstAssetUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<AstAssetDto>.Ok(await _svc.UpsertAssetAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/ast/depreciation-runs")]
public sealed class AstDepreciationRunController : ControllerBase
{
    private readonly IAstAssetService _svc;
    public AstDepreciationRunController(IAstAssetService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("ast.asset.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AstDepreciationRunDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AstDepreciationRunDto>>.Ok(await _svc.ListRunsAsync(TenantId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("ast.asset.read")]
    public async Task<ActionResult<ApiResponse<AstDepreciationRunDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<AstDepreciationRunDetailDto>.Ok(await _svc.GetRunDetailAsync(TenantId, id, ct)));

    [HttpPost("calculate")]
    [AuthorizePermission("ast.asset.manage")]
    public async Task<ActionResult<ApiResponse<AstDepreciationRunDto>>> Calculate(
        [FromBody] AstDepreciationCalcRequest req, CancellationToken ct)
        => Ok(ApiResponse<AstDepreciationRunDto>.Ok(await _svc.CalculatePeriodAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/push-fin")]
    [AuthorizePermission("ast.asset.manage")]
    public async Task<ActionResult<ApiResponse<AstDepreciationRunDto>>> PushFin(
        Guid id, [FromBody] AstPushFinRequest req, CancellationToken ct)
        => Ok(ApiResponse<AstDepreciationRunDto>.Ok(await _svc.PushToFinStubAsync(TenantId, UserId, id, req, ct)));
}
