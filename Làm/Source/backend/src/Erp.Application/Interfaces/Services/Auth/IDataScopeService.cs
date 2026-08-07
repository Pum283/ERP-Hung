using Erp.Domain.Enums.Sys;

namespace Erp.Application.Interfaces.Services.Auth;

public sealed record UserScopeContext(
    ScopeType Scope,
    bool BypassDataScope,
    Guid UserId,
    Guid? DepartmentId,
    IReadOnlyList<Guid> AccessibleDepartmentIds,
    IReadOnlyList<Guid>? AccessibleSalesPointIds = null);

public interface IDataScopeService
{
    Task<UserScopeContext> GetUserScopeContextAsync(Guid userId, CancellationToken ct = default);
}
