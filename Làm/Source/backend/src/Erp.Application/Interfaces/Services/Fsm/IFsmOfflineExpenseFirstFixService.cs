using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IFsmOfflineExpenseFirstFixService
{
    // UC_FSM_040: Cảnh báo thất thoát
    Task<IReadOnlyList<FsmSparePartLossWarningDto>> GetSparePartLossWarningsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FSM_043: Làm việc offline
    Task<FsmOfflineSyncAuditLogDto> RecordOfflineSyncAsync(Guid tenantId, FsmSyncOfflineDataRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FsmOfflineSyncAuditLogDto>> GetOfflineSyncLogsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FSM_044: Nộp quyết toán ngày
    Task<FsmDailyExpenseSettlementDto> SubmitDailySettlementAsync(Guid tenantId, FsmSubmitDailySettlementRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FsmDailyExpenseSettlementDto>> GetDailySettlementsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FSM_048: Tỷ lệ sửa lần đầu
    Task<FsmFirstTimeFixRateReportDto> GetFirstTimeFixRateReportAsync(Guid tenantId, CancellationToken ct = default);
}
