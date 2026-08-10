using Erp.Application.DTOs.Hrm;
using Erp.Application.DTOs.Mod;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmEmployeeService
{
    Task<IReadOnlyList<EmployeeDto>> ListAsync(Guid tenantId, Guid currentUserId, string? q, CancellationToken ct = default);
    Task<EmployeeDto> GetAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<EmployeeDto> GetWithScopeAsync(Guid tenantId, Guid currentUserId, Guid id, CancellationToken ct = default);
    Task<EmployeeDto> UpsertAsync(Guid tenantId, Guid? actorId, EmployeeUpsertRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<JobTitleDto>> ListJobTitlesAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeTypeDto>> ListEmployeeTypesAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveTypeDto>> ListLeaveTypesAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<ContractDto>> ListContractsAsync(Guid tenantId, Guid? employeeId, CancellationToken ct = default);
    Task<EmployeeDto> ChangeStatusAsync(Guid tenantId, Guid actorId, Guid employeeId, ChangeEmploymentStatusRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<EmploymentStatusChangeDto>> ListStatusHistoryAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default);
    Task<byte[]> ExportEmployeesCsvAsync(Guid tenantId, Guid currentUserId, CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeDocumentDto>> ListDocumentsAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default);
    Task<EmployeeDocumentDto> AddDocumentAsync(Guid tenantId, Guid? actorId, Guid employeeId, EmployeeDocumentUploadRequest req, CancellationToken ct = default);
    Task DeleteDocumentAsync(Guid tenantId, Guid employeeId, Guid documentId, CancellationToken ct = default);

    // ── UC_HRM_034 / 036 — Điều chuyển & Cảnh báo hết hạn thử việc ──
    Task<EmployeeDto> TransferEmployeeAsync(Guid tenantId, Guid actorId, Guid employeeId, EmployeeTransferRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<ProbationExpiringEmployeeDto>> ListExpiringProbationEmployeesAsync(Guid tenantId, int daysAhead = 15, CancellationToken ct = default);
}
