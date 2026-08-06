using System.Security.Claims;
using Erp.Application.Interfaces.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Erp.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AuthorizePermissionAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _permissionCode;

    public AuthorizePermissionAttribute(string permissionCode) => _permissionCode = permissionCode;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userIdValue = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? context.HttpContext.User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var authz = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
        try
        {
            await authz.EnsurePermissionAsync(userId, _permissionCode, context.HttpContext.RequestAborted);
        }
        catch (Erp.Application.Common.Exceptions.ForbiddenException ex)
        {
            context.Result = new ObjectResult(new { success = false, message = ex.Message }) { StatusCode = 403 };
            return;
        }

        await next();
    }
}
