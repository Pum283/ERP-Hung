using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IInvProjectTransferSerialTrackingService
{
    // UC_INV_028: Xuất cho dự án
    Task<InvProjectDispatchDto> CreateProjectDispatchAsync(Guid tenantId, InvCreateProjectDispatchRequest req, CancellationToken ct = default);

    // UC_INV_032: Duyệt chuyển kho
    Task<InvTransferApprovalDto> CreateTransferApprovalAsync(Guid tenantId, InvCreateTransferApprovalRequest req, CancellationToken ct = default);
    Task<InvTransferApprovalDto> DecideTransferApprovalAsync(Guid tenantId, InvDecideTransferApprovalRequest req, CancellationToken ct = default);

    // UC_INV_034: Chuyển kho một bước
    Task<InvOneStepTransferDto> ExecuteOneStepTransferAsync(Guid tenantId, InvExecuteOneStepTransferRequest req, CancellationToken ct = default);

    // UC_INV_046: Theo dõi serial
    Task<InvSerialTrackingHistoryDto> RecordSerialEventAsync(Guid tenantId, InvRecordSerialEventRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<InvSerialTrackingHistoryDto>> GetSerialHistoryAsync(Guid tenantId, string serialNumber, CancellationToken ct = default);
}
