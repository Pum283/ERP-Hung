using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Erp.Api.Controllers.Sys;

/// <summary>Ops tối thiểu G4 — outbox gần đây + bật/tắt soft module (license).</summary>
[ApiController]
[Authorize]
[Route("api/sys")]
public sealed class SysOpsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SysOpsController(AppDbContext db) => _db = db;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("outbox/recent")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<object>>> OutboxRecent([FromQuery] int take = 20, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 100);
        var rows = await _db.OutboxMessages.AsNoTracking()
            .Where(x => x.TenantId == TenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new
            {
                x.Id,
                x.EventType,
                x.SourceModule,
                x.Status,
                x.AttemptCount,
                x.CorrelationId,
                x.CreatedAt,
                x.PublishedAt,
                x.LastError
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(rows));
    }

    [HttpGet("license-modules")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<object>>> ListLicenseModules(CancellationToken ct)
    {
        var rows = await _db.LicenseModules.AsNoTracking()
            .Where(x => x.TenantId == TenantId && !x.IsDeleted)
            .OrderBy(x => x.ModuleCode)
            .Select(x => new { x.Id, x.ModuleCode, x.IsEnabled, x.LicenseId })
            .ToListAsync(ct);
        return Ok(ApiResponse<object>.Ok(rows));
    }

    [HttpPut("license-modules/{moduleCode}")]
    [AuthorizePermission("sys.license.manage")]
    public async Task<ActionResult<ApiResponse<object>>> SetLicenseModule(
        string moduleCode, [FromBody] SetLicenseModuleRequest req, CancellationToken ct)
    {
        var code = moduleCode.Trim().ToUpperInvariant();
        if (code is "SYS")
            return BadRequest(ApiResponse<object>.Fail("Không được tắt module SYS (hard path)."));

        var rows = await _db.LicenseModules
            .Where(x => x.TenantId == TenantId && x.ModuleCode == code && !x.IsDeleted)
            .ToListAsync(ct);

        if (rows.Count == 0)
            return NotFound(ApiResponse<object>.Fail($"Không tìm thấy license module `{code}`."));

        foreach (var row in rows)
            row.IsEnabled = req.IsEnabled;

        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { moduleCode = code, isEnabled = req.IsEnabled, updated = rows.Count }));
    }
}
