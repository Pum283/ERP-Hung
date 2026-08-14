using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IInvLotTraceStocktakeLockInternalRequestService
{
    // UC_INV_047: Truy vết lô xuôi/ngược
    Task<InvLotTraceabilityDto> RecordLotTraceAsync(Guid tenantId, InvCreateLotTraceRecordRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<InvLotTraceabilityDto>> GetLotGenealogyAsync(Guid tenantId, string lotNumber, CancellationToken ct = default);

    // UC_INV_051: Kiểm kê theo vị trí / nhóm
    Task<InvStocktakeLocationGroupDto> CreateStocktakeLocationGroupAsync(Guid tenantId, InvCreateStocktakeLocationGroupRequest req, CancellationToken ct = default);

    // UC_INV_054: Khóa giao dịch khi đang kiểm kê
    Task<InvStocktakeLockDto> SetStocktakeLockAsync(Guid tenantId, InvSetStocktakeLockRequest req, CancellationToken ct = default);
    Task<bool> IsTransactionLockedAsync(Guid tenantId, Guid warehouseId, string targetIdentifier, CancellationToken ct = default);

    // UC_INV_056: Đề nghị xuất nội bộ
    Task<InvInternalIssueRequestDto> CreateInternalIssueRequestAsync(Guid tenantId, InvCreateInternalIssueRequest req, CancellationToken ct = default);
}
