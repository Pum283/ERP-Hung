using Erp.Application.DTOs.Lms;

namespace Erp.Application.Interfaces.Services.Lms;

public interface ILmsInstructorService
{
    Task<IReadOnlyList<LmsInstructorDto>> ListAsync(Guid tenantId, CancellationToken ct = default);
    Task<LmsInstructorDto> UpsertAsync(
        Guid tenantId, Guid userId, LmsInstructorUpsertRequest req, CancellationToken ct = default);
    Task<LmsInstructorDto> SetStatusAsync(
        Guid tenantId, Guid userId, Guid id, string status, CancellationToken ct = default);
    Task GrantRoleAsync(Guid tenantId, Guid actorId, Guid instructorId, CancellationToken ct = default);
}
