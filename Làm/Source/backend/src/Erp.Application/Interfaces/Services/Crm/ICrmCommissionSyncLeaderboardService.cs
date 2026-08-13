using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ICrmCommissionSyncLeaderboardService
{
    // UC_CRM_121: Tính hoa hồng theo kỳ
    Task<CrmCommissionPeriodDto> CalculateCommissionPeriodAsync(Guid tenantId, CrmCalculateCommissionRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmCommissionPeriodDto>> GetCommissionPeriodsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_CRM_122: Duyệt bảng hoa hồng
    Task<CrmCommissionApprovalResultDto> ApproveCommissionPeriodAsync(Guid tenantId, CrmApproveCommissionRequest req, CancellationToken ct = default);

    // UC_CRM_123: Đồng bộ hoa hồng sang HRM/FIN
    Task<CrmCommissionSyncResultDto> SyncCommissionToHrmFinAsync(Guid tenantId, CrmSyncCommissionHrmFinRequest req, CancellationToken ct = default);

    // UC_CRM_125: Bảng xếp hạng sales
    Task<IReadOnlyList<CrmSalesLeaderboardEntryDto>> GetSalesLeaderboardAsync(Guid tenantId, string rankingPeriod = "Monthly", CancellationToken ct = default);
}
