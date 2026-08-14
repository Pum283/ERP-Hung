using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IFinStatementOffsetDunningBadDebtService
{
    // UC_FIN_028: Import sao kê
    Task<FinBankStatementImportRecordDto> ImportBankStatementAsync(Guid tenantId, FinImportBankStatementRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FinBankStatementImportRecordDto>> GetBankStatementImportsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FIN_033: Bù trừ công nợ
    Task<FinArApOffsetSettlementDto> CreateArApOffsetAsync(Guid tenantId, FinCreateArApOffsetRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FinArApOffsetSettlementDto>> GetArApOffsetsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FIN_034: Nhắc nợ tự động
    Task<FinDebtDunningNotificationDto> SendDunningNotificationAsync(Guid tenantId, FinSendDunningNotificationRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FinDebtDunningNotificationDto>> GetDunningNotificationsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FIN_037: Xử lý nợ khó đòi
    Task<FinBadDebtProvisionWriteOffDto> ProcessBadDebtAsync(Guid tenantId, FinProcessBadDebtRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FinBadDebtProvisionWriteOffDto>> GetBadDebtRecordsAsync(Guid tenantId, CancellationToken ct = default);
}
