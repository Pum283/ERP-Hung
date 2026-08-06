using Erp.Application.DTOs.Ast;

namespace Erp.Application.Interfaces.Services.Ast;

public interface IAstMovementService
{
    Task<IReadOnlyList<AstMovementDocDto>> ListAsync(
        Guid tenantId, string? docType = null, string? status = null, CancellationToken ct = default);

    Task<AstMovementDocDto> UpsertAsync(
        Guid tenantId, Guid userId, AstMovementUpsertRequest req, CancellationToken ct = default);

    Task<AstMovementDocDto> PostAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);

    Task<AstMovementDocDto> VoidAsync(
        Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default);
}
