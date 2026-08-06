using Erp.Application.DTOs.Crm;

namespace Erp.Application.Interfaces.Services.Crm;

public interface ICrmCustomerService
{
    Task<IReadOnlyList<CrmCustomerDto>> SearchAsync(
        Guid tenantId, CrmCustomerSearchRequest req, CancellationToken ct = default);
    Task<CrmCustomerDto> UpsertAsync(
        Guid tenantId, Guid userId, CrmCustomerUpsertRequest req, CancellationToken ct = default);
    Task<CrmCustomer360Dto> Get360Async(Guid tenantId, Guid customerId, CancellationToken ct = default);
    Task<IReadOnlyList<CrmDuplicateHitDto>> FindDuplicatesAsync(
        Guid tenantId, string? phone, string? taxCode, Guid? excludeId, CancellationToken ct = default);
    Task<CrmCustomerDto> AssignOwnerAsync(
        Guid tenantId, Guid userId, Guid customerId, CrmAssignOwnerRequest req, CancellationToken ct = default);
    Task<CrmHandoverDto> HandoverAsync(
        Guid tenantId, Guid userId, Guid customerId, CrmHandoverRequest req, CancellationToken ct = default);
    Task<CrmCustomerDto> MergeAsync(
        Guid tenantId, Guid userId, CrmMergeRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<CrmContactDto>> ListContactsAsync(
        Guid tenantId, Guid customerId, CancellationToken ct = default);
    Task<CrmContactDto> UpsertContactAsync(
        Guid tenantId, Guid userId, Guid customerId, CrmContactUpsertRequest req, CancellationToken ct = default);

    Task<string> ExportCsvAsync(Guid tenantId, CancellationToken ct = default);
    Task<CrmImportResult> ImportCsvAsync(
        Guid tenantId, Guid userId, CrmImportRequest req, CancellationToken ct = default);
}
