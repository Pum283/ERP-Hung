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
[Route("api/ast/movements")]
public sealed class AstMovementController : ControllerBase
{
    private readonly IAstMovementService _svc;
    public AstMovementController(IAstMovementService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("ast.asset.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AstMovementDocDto>>>> List(
        [FromQuery] string? docType, [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AstMovementDocDto>>.Ok(
            await _svc.ListAsync(TenantId, docType, status, ct)));

    [HttpPost]
    [AuthorizePermission("ast.asset.manage")]
    public async Task<ActionResult<ApiResponse<AstMovementDocDto>>> Upsert(
        [FromBody] AstMovementUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<AstMovementDocDto>.Ok(await _svc.UpsertAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/post")]
    [AuthorizePermission("ast.asset.manage")]
    public async Task<ActionResult<ApiResponse<AstMovementDocDto>>> Post(Guid id, CancellationToken ct)
        => Ok(ApiResponse<AstMovementDocDto>.Ok(await _svc.PostAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/void")]
    [AuthorizePermission("ast.asset.manage")]
    public async Task<ActionResult<ApiResponse<AstMovementDocDto>>> Void(
        Guid id, [FromBody] AstMovementNoteRequest? req, CancellationToken ct)
        => Ok(ApiResponse<AstMovementDocDto>.Ok(await _svc.VoidAsync(TenantId, UserId, id, req?.Note, ct)));
}
