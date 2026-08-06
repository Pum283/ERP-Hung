using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Exceptions;
using Erp.Application.Common.Models;
using Erp.Application.Interfaces.Services.Auth;
using Erp.Application.Interfaces.Services.Sys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAppAuthz = Erp.Application.Interfaces.Services.Auth.IAuthorizationService;

namespace Erp.Api.Controllers.Sys;

[ApiController]
[Authorize]
[Route("api/sys/files")]
public sealed class FilesController : ControllerBase
{
    private readonly IFileStorageService _files;
    private readonly IAppAuthz _authz;

    public FilesController(IFileStorageService files, IAppAuthz authz)
    {
        _files = files;
        _authz = authz;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpPost("upload")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<ApiResponse<object>>> Upload(IFormFile file, CancellationToken ct)
    {
        // Digi chat attach: cho phép sys.msg.send ngoài sys.user.manage
        if (!await _authz.HasPermissionAsync(UserId, "sys.user.manage", ct)
            && !await _authz.HasPermissionAsync(UserId, "sys.msg.send", ct))
            throw new ForbiddenException("Thiếu quyền upload (sys.user.manage | sys.msg.send).");

        if (file.Length == 0) return BadRequest(ApiResponse<object>.Fail("File rỗng."));
        await using var stream = file.OpenReadStream();
        var saved = await _files.SaveAsync(stream, file.FileName, file.ContentType, TenantId, "files", ct);
        return Ok(ApiResponse<object>.Ok(new
        {
            storageKey = saved.StorageKey,
            fileName = saved.FileName,
            sizeBytes = saved.SizeBytes,
            publicUrl = saved.PublicUrl
        }));
    }

    [HttpGet("{**storageKey}")]
    public async Task<IActionResult> Download(string storageKey, CancellationToken ct)
    {
        if (!await _authz.HasPermissionAsync(UserId, "sys.user.read", ct)
            && !await _authz.HasPermissionAsync(UserId, "sys.msg.read", ct))
            throw new ForbiddenException("Thiếu quyền tải file.");

        var opened = await _files.OpenReadAsync(storageKey, TenantId, ct);
        if (opened is null) return NotFound();
        return File(opened.Value.Content, opened.Value.ContentType ?? "application/octet-stream", opened.Value.FileName);
    }
}
