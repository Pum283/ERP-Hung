using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmShiftService
{
    Task<IReadOnlyList<WorkShiftDto>> ListTemplatesAsync(Guid tenantId, CancellationToken ct = default);
    Task<WorkShiftDto> UpsertTemplateAsync(Guid tenantId, Guid userId, WorkShiftUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<ShiftAssignmentDto>> ListAssignmentsAsync(
        Guid tenantId, Guid? orgUnitId, Guid? employeeId, DateOnly? from, DateOnly? to, CancellationToken ct = default);

    Task<IReadOnlyList<ShiftAssignmentDto>> MyAssignmentsAsync(
        Guid tenantId, Guid userId, DateOnly? from, DateOnly? to, CancellationToken ct = default);

    Task<ShiftAssignmentDto> AssignAsync(Guid tenantId, Guid userId, ShiftAssignRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<ShiftAssignmentDto>> AssignRangeAsync(
        Guid tenantId, Guid userId, ShiftAssignRangeRequest req, CancellationToken ct = default);

    Task SwapAsync(Guid tenantId, Guid userId, ShiftSwapRequest req, CancellationToken ct = default);
    Task CancelAsync(Guid tenantId, Guid userId, Guid assignmentId, CancellationToken ct = default);

    Task<int> CopyAsync(Guid tenantId, Guid userId, ShiftCopyRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<ShiftPeriodLockDto>> ListLocksAsync(Guid tenantId, CancellationToken ct = default);
    Task<ShiftPeriodLockDto> LockPeriodAsync(Guid tenantId, Guid userId, ShiftLockRequest req, CancellationToken ct = default);

    Task<string> ExportCsvAsync(
        Guid tenantId, Guid? orgUnitId, DateOnly? from, DateOnly? to, CancellationToken ct = default);
}
