using Erp.Application.DTOs.Fin;

namespace Erp.Application.Interfaces.Services.Fin;

public interface IFinCashService
{
    Task<IReadOnlyList<FinCashFundDto>> ListFundsAsync(Guid tenantId, CancellationToken ct = default);
    Task<FinCashFundDto> UpsertFundAsync(Guid tenantId, Guid userId, FinCashFundUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FinCashVoucherDto>> ListVouchersAsync(Guid tenantId, Guid? fundId = null, string? type = null, CancellationToken ct = default);
    Task<FinCashVoucherDto> UpsertVoucherAsync(Guid tenantId, Guid userId, FinCashVoucherUpsertRequest req, CancellationToken ct = default);
    Task<FinCashVoucherDto> PostVoucherAsync(Guid tenantId, Guid userId, Guid voucherId, CancellationToken ct = default);
    Task<FinCashVoucherDto> VoidVoucherAsync(Guid tenantId, Guid userId, Guid voucherId, string? note = null, CancellationToken ct = default);

    Task<FinCashBookDto> GetCashBookAsync(Guid tenantId, Guid fundId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default);
}
