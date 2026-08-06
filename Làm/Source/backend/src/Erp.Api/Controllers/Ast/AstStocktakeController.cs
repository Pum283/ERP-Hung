using System.Security.Claims;
using System.Text;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Ast;
using Erp.Application.Interfaces.Services.Ast;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Ast;

[ApiController]
[Authorize]
[Route("api/ast/stocktakes")]
public sealed class AstStocktakeController : ControllerBase
{
    private readonly IAstStocktakeService _svc;
    public AstStocktakeController(IAstStocktakeService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("ast.asset.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AstStocktakeDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AstStocktakeDto>>.Ok(await _svc.ListAsync(TenantId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("ast.asset.read")]
    public async Task<ActionResult<ApiResponse<AstStocktakeDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<AstStocktakeDetailDto>.Ok(await _svc.GetDetailAsync(TenantId, id, ct)));

    [HttpGet("{id:guid}/variances")]
    [AuthorizePermission("ast.asset.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AstStocktakeLineDto>>>> Variances(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AstStocktakeLineDto>>.Ok(await _svc.ListVariancesAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("ast.asset.manage")]
    public async Task<ActionResult<ApiResponse<AstStocktakeDto>>> Create(
        [FromBody] AstStocktakeCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<AstStocktakeDto>.Ok(await _svc.CreateAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/count")]
    [AuthorizePermission("ast.asset.manage")]
    public async Task<ActionResult<ApiResponse<AstStocktakeLineDto>>> Count(
        Guid id, [FromBody] AstStocktakeCountRequest req, CancellationToken ct)
        => Ok(ApiResponse<AstStocktakeLineDto>.Ok(await _svc.CountLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/review")]
    [AuthorizePermission("ast.asset.manage")]
    public async Task<ActionResult<ApiResponse<AstStocktakeDto>>> Review(Guid id, CancellationToken ct)
        => Ok(ApiResponse<AstStocktakeDto>.Ok(await _svc.ReviewAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/close")]
    [AuthorizePermission("ast.asset.manage")]
    public async Task<ActionResult<ApiResponse<AstStocktakeDto>>> Close(Guid id, CancellationToken ct)
        => Ok(ApiResponse<AstStocktakeDto>.Ok(await _svc.CloseAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/ast/reports")]
public sealed class AstReportController : ControllerBase
{
    private readonly IAstReportService _svc;
    public AstReportController(IAstReportService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("register")]
    [AuthorizePermission("ast.asset.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AstRegisterRowDto>>>> Register(
        [FromQuery] string? status, [FromQuery] Guid? locationId, [FromQuery] Guid? groupId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AstRegisterRowDto>>.Ok(
            await _svc.RegisterAsync(TenantId, status, locationId, groupId, ct)));

    [HttpGet("depreciation")]
    [AuthorizePermission("ast.asset.read")]
    public async Task<ActionResult<ApiResponse<AstDepreciationReportDto>>> Depreciation(
        [FromQuery] int year, [FromQuery] int month, CancellationToken ct)
        => Ok(ApiResponse<AstDepreciationReportDto>.Ok(await _svc.DepreciationAsync(TenantId, year, month, ct)));

    [HttpGet("by-location")]
    [AuthorizePermission("ast.asset.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AstByLocationRowDto>>>> ByLocation(
        [FromQuery] Guid? locationId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AstByLocationRowDto>>.Ok(
            await _svc.ByLocationAsync(TenantId, locationId, ct)));

    [HttpGet("export.csv")]
    [AuthorizePermission("ast.asset.read")]
    public async Task<IActionResult> Export(
        [FromQuery] string report, [FromQuery] string? status, [FromQuery] Guid? locationId,
        [FromQuery] Guid? groupId, [FromQuery] int? year, [FromQuery] int? month, CancellationToken ct)
    {
        var csv = await _svc.ExportCsvAsync(TenantId, report, status, locationId, groupId, year, month, ct);
        var name = $"ast-{report}-{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", name);
    }
}
