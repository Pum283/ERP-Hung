using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Sys;

[ApiController]
[Route("api/sys")]
public sealed class HealthController : ControllerBase
{
    [HttpGet("health")]
    [AllowAnonymous]
    public ActionResult<object> Health() =>
        Ok(new { success = true, service = "Erp.Api", utc = DateTimeOffset.UtcNow });

    [HttpGet("ping-secure")]
    [Authorize]
    [AuthorizePermission("sys.user.read")]
    public ActionResult<ApiResponse<string>> PingSecure() =>
        Ok(ApiResponse<string>.Ok("pong", "OK — JWT + permission"));
}
