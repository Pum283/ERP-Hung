using Erp.Application.DTOs.Ast;

namespace Erp.Application.Interfaces.Services.Ast;

public interface IAstStocktakeService
{
    Task<IReadOnlyList<AstStocktakeDto>> ListAsync(Guid tenantId, CancellationToken ct = default);
    Task<AstStocktakeDetailDto> GetDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<AstStocktakeDto> CreateAsync(Guid tenantId, Guid userId, AstStocktakeCreateRequest req, CancellationToken ct = default);
    Task<AstStocktakeLineDto> CountLineAsync(Guid tenantId, Guid userId, Guid stocktakeId, AstStocktakeCountRequest req, CancellationToken ct = default);
    Task<AstStocktakeDto> ReviewAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<AstStocktakeDto> CloseAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<AstStocktakeLineDto>> ListVariancesAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}
