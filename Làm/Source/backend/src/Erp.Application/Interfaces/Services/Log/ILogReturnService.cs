using Erp.Application.DTOs.Log;

namespace Erp.Application.Interfaces.Services.Log;

public interface ILogReturnService
{
    Task<IReadOnlyList<LogReturnNoteDto>> ListAsync(Guid tenantId, string? status = null, CancellationToken ct = default);
    Task<LogReturnDetailDto> GetDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<LogReturnDetailDto> CreateAsync(Guid tenantId, Guid userId, LogReturnCreateRequest req, CancellationToken ct = default);
    Task<LogReturnLineDto> CountLineAsync(Guid tenantId, Guid userId, Guid noteId, LogReturnCountRequest req, CancellationToken ct = default);
    Task<LogReturnDetailDto> ConfirmCountAsync(Guid tenantId, Guid userId, Guid noteId, CancellationToken ct = default);
    Task<LogReturnDetailDto> PostAsync(Guid tenantId, Guid userId, Guid noteId, CancellationToken ct = default);
    Task<LogReturnDetailDto> CancelAsync(Guid tenantId, Guid userId, Guid noteId, string? note = null, CancellationToken ct = default);
    Task<LogOpsReportDto> GetOpsReportAsync(Guid tenantId, CancellationToken ct = default);
}