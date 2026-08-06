using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmTransferService
{
    Task<IReadOnlyList<StaffTransferDto>> ListAsync(
        Guid tenantId, string? kind, string? status, Guid? orgUnitId, CancellationToken ct = default);

    Task<IReadOnlyList<StaffTransferDto>> MyOrdersAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<StaffTransferDto>> ActiveTrackingAsync(Guid tenantId, CancellationToken ct = default);

    Task<StaffTransferDto> CreateRequestAsync(
        Guid tenantId, Guid userId, TransferRequestCreateRequest req, CancellationToken ct = default);

    Task<StaffTransferDto> CreateOrderAsync(
        Guid tenantId, Guid userId, TransferOrderCreateRequest req, CancellationToken ct = default);

    Task<StaffTransferDto> SubmitRequestAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<StaffTransferDto> DecideRequestAsync(Guid tenantId, Guid userId, Guid id, bool approve, CancellationToken ct = default);

    Task<StaffTransferDto> IssueOrderAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<StaffTransferDto> AcknowledgeAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<StaffTransferDto> ActivateAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<StaffTransferDto> CompleteAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<StaffTransferDto> CancelAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);

    Task<StaffTransferDto> SetActualHoursAsync(
        Guid tenantId, Guid userId, Guid id, TransferActualHoursRequest req, CancellationToken ct = default);

    Task<StaffTransferDto> SetAttendanceTagAsync(
        Guid tenantId, Guid userId, Guid id, bool tagged, CancellationToken ct = default);

    Task<IReadOnlyList<TransferCostReportRowDto>> CostReportAsync(
        Guid tenantId, DateOnly? from, DateOnly? to, CancellationToken ct = default);
}
