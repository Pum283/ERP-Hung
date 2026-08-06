using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Mod;
using Erp.Application.Interfaces.Services.Mod;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Mod;

/// <summary>CRUD Day-1 masters/docs cho LMS…PRT (và tái dùng HRM docs).</summary>
[ApiController]
[Authorize]
[Route("api/{moduleCode}")]
public sealed class ModuleCrudController : ControllerBase
{
    private readonly IModModuleService _svc;

    public ModuleCrudController(IModModuleService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("masters")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ModMasterDto>>>> Masters(
        string moduleCode, [FromQuery] string? type, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ModMasterDto>>.Ok(await _svc.ListMastersAsync(TenantId, moduleCode, type, ct)));

    [HttpPost("masters")]
    public async Task<ActionResult<ApiResponse<ModMasterDto>>> UpsertMaster(
        string moduleCode, [FromBody] ModMasterUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<ModMasterDto>.Ok(await _svc.UpsertMasterAsync(TenantId, UserId, moduleCode, req, ct)));

    [HttpGet("documents")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ModDocumentDto>>>> Documents(
        string moduleCode, [FromQuery] string? docType, [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ModDocumentDto>>.Ok(await _svc.ListDocumentsAsync(TenantId, moduleCode, docType, status, ct)));

    [HttpPost("documents")]
    public async Task<ActionResult<ApiResponse<ModDocumentDto>>> UpsertDoc(
        string moduleCode, [FromBody] ModDocumentUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<ModDocumentDto>.Ok(await _svc.UpsertDocumentAsync(TenantId, UserId, moduleCode, req, ct)));

    [HttpPost("documents/{id:guid}/transition")]
    public async Task<ActionResult<ApiResponse<ModDocumentDto>>> Transition(
        string moduleCode, Guid id, [FromBody] TransitionBody body, CancellationToken ct)
        => Ok(ApiResponse<ModDocumentDto>.Ok(await _svc.TransitionDocumentAsync(TenantId, id, body.Status, ct)));

    public sealed record TransitionBody(string Status);
}
