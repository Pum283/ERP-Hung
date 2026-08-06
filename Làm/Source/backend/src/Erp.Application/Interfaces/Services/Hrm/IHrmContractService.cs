using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmContractService
{
    Task<IReadOnlyList<ContractDto>> ListAsync(Guid tenantId, Guid? employeeId, CancellationToken ct = default);
    Task<ContractDto> UpsertAsync(Guid tenantId, Guid? actorId, ContractUpsertRequest req, CancellationToken ct = default);
    Task<ContractDto> RenewAsync(Guid tenantId, Guid? actorId, Guid id, ContractRenewRequest req, CancellationToken ct = default);
    Task<ContractDto> TerminateAsync(Guid tenantId, Guid? actorId, Guid id, ContractTerminateRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<ContractDto>> ListExpiringAsync(Guid tenantId, int withinDays, CancellationToken ct = default);
}
