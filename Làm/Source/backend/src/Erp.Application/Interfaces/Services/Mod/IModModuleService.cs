using Erp.Application.DTOs.Mod;

namespace Erp.Application.Interfaces.Services.Mod;

public interface IModModuleService
{
    Task<IReadOnlyList<ModMasterDto>> ListMastersAsync(Guid tenantId, string moduleCode, string? recordType, CancellationToken ct = default);
    Task<ModMasterDto> UpsertMasterAsync(Guid tenantId, Guid? actorId, string moduleCode, ModMasterUpsertRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<ModDocumentDto>> ListDocumentsAsync(Guid tenantId, string moduleCode, string? docType, string? status, CancellationToken ct = default);
    Task<ModDocumentDto> UpsertDocumentAsync(Guid tenantId, Guid? actorId, string moduleCode, ModDocumentUpsertRequest req, CancellationToken ct = default);
    Task<ModDocumentDto> TransitionDocumentAsync(Guid tenantId, Guid id, string newStatus, CancellationToken ct = default);
}
