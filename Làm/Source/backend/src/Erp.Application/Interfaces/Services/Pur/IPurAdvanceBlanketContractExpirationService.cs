using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPurAdvanceBlanketContractExpirationService
{
    // UC_PUR_044: Tạm ứng nhà cung cấp
    Task<PurVendorAdvancePaymentDto> CreateAdvancePaymentAsync(Guid tenantId, PurCreateVendorAdvancePaymentRequest req, CancellationToken ct = default);

    // UC_PUR_045, UC_PUR_046, UC_PUR_047: Hợp đồng mua khung & Cảnh báo hết hạn
    Task<PurBlanketContractDto> CreateBlanketContractAsync(Guid tenantId, PurCreateBlanketContractRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PurBlanketContractDto>> GetBlanketContractsAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<PurBlanketContractDto>> GetExpiringContractsAlertsAsync(Guid tenantId, int warningDaysThreshold = 30, CancellationToken ct = default);
}
