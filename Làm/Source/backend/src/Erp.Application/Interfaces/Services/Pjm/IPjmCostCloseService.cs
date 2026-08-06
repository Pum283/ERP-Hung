using Erp.Application.DTOs.Pjm;

namespace Erp.Application.Interfaces.Services.Pjm;

public interface IPjmCostCloseService
{
    Task<PjmExpenseDto> UpsertExpenseAsync(
        Guid tenantId, Guid userId, Guid projectId, PjmExpenseUpsertRequest req, CancellationToken ct = default);

    Task<PjmMaterialIssueDto> CreateMaterialIssueAsync(
        Guid tenantId, Guid userId, Guid projectId, PjmMaterialIssueCreateRequest req, CancellationToken ct = default);

    Task<PjmAcceptanceDto> CreateAcceptanceAsync(
        Guid tenantId, Guid userId, Guid projectId, PjmAcceptanceCreateRequest req, CancellationToken ct = default);

    Task<PjmAcceptanceDto> SignAcceptanceAsync(
        Guid tenantId, Guid userId, Guid projectId, Guid acceptanceId, PjmAcceptanceSignRequest req, CancellationToken ct = default);

    Task<PjmProjectDto> RecognizeRevenueAsync(
        Guid tenantId, Guid userId, Guid projectId, PjmRecognizeRevenueRequest req, CancellationToken ct = default);

    Task<PjmProjectDto> CloseProjectAsync(
        Guid tenantId, Guid userId, Guid projectId, PjmCloseProjectRequest req, CancellationToken ct = default);
}
