using Erp.Application.DTOs.Fin;

namespace Erp.Application.Interfaces.Services.Fin;

public interface IFinBankService
{
    Task<IReadOnlyList<FinBankAccountDto>> ListAccountsAsync(Guid tenantId, CancellationToken ct = default);
    Task<FinBankAccountDto> UpsertAccountAsync(Guid tenantId, Guid userId, FinBankAccountUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FinBankVoucherDto>> ListVouchersAsync(Guid tenantId, Guid? bankAccountId = null, string? type = null, CancellationToken ct = default);
    Task<FinBankVoucherDto> UpsertVoucherAsync(Guid tenantId, Guid userId, FinBankVoucherUpsertRequest req, CancellationToken ct = default);
    Task<FinBankVoucherDto> PostVoucherAsync(Guid tenantId, Guid userId, Guid voucherId, CancellationToken ct = default);
    Task<FinBankVoucherDto> VoidVoucherAsync(Guid tenantId, Guid userId, Guid voucherId, string? note = null, CancellationToken ct = default);

    Task<IReadOnlyList<FinBankTransferRequestDto>> ListTransfersAsync(Guid tenantId, Guid? bankAccountId = null, CancellationToken ct = default);
    Task<FinBankTransferRequestDto> UpsertTransferAsync(Guid tenantId, Guid userId, FinBankTransferUpsertRequest req, CancellationToken ct = default);
    Task<FinBankTransferRequestDto> SubmitTransferAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<FinBankTransferRequestDto> ApproveTransferAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<FinBankTransferRequestDto> RejectTransferAsync(Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default);
    Task<FinBankTransferRequestDto> ExecuteTransferAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<FinBankTransferRequestDto> VoidTransferAsync(Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default);

    Task<IReadOnlyList<FinBankStatementLineDto>> ListStatementsAsync(Guid tenantId, Guid? bankAccountId = null, string? status = null, CancellationToken ct = default);
    Task<FinBankStatementLineDto> UpsertStatementAsync(Guid tenantId, Guid userId, FinBankStatementUpsertRequest req, CancellationToken ct = default);
    Task<FinBankStatementLineDto> MatchStatementAsync(Guid tenantId, Guid userId, Guid lineId, Guid voucherId, CancellationToken ct = default);
    Task<FinBankStatementLineDto> UnmatchStatementAsync(Guid tenantId, Guid userId, Guid lineId, CancellationToken ct = default);
    Task<FinBankStatementLineDto> IgnoreStatementAsync(Guid tenantId, Guid userId, Guid lineId, CancellationToken ct = default);

    Task<FinBankBookDto> GetBankBookAsync(Guid tenantId, Guid bankAccountId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default);
}
