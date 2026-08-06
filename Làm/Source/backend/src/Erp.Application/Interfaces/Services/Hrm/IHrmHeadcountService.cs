using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmHeadcountService
{
    Task<IReadOnlyList<HeadcountPlanDto>> ListAsync(Guid tenantId, CancellationToken ct = default);
    Task<HeadcountPlanDto> UpsertAsync(Guid tenantId, Guid userId, HeadcountPlanUpsertRequest req, CancellationToken ct = default);
    Task<HeadcountPlanDto> SubmitAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<HeadcountPlanDto> DecideAsync(Guid tenantId, Guid userId, Guid id, bool approve, CancellationToken ct = default);
    Task<IReadOnlyList<HeadcountCompareRowDto>> CompareAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<HeadcountCompareRowDto>> ShortagesAsync(Guid tenantId, CancellationToken ct = default);
}
