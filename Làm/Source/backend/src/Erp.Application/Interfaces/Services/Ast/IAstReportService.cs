using Erp.Application.DTOs.Ast;

namespace Erp.Application.Interfaces.Services.Ast;

public interface IAstReportService
{
    Task<IReadOnlyList<AstRegisterRowDto>> RegisterAsync(
        Guid tenantId, string? status = null, Guid? locationId = null, Guid? groupId = null,
        CancellationToken ct = default);

    Task<AstDepreciationReportDto> DepreciationAsync(
        Guid tenantId, int year, int month, CancellationToken ct = default);

    Task<IReadOnlyList<AstByLocationRowDto>> ByLocationAsync(
        Guid tenantId, Guid? locationId = null, CancellationToken ct = default);

    Task<string> ExportCsvAsync(
        Guid tenantId, string report, string? status = null, Guid? locationId = null,
        Guid? groupId = null, int? year = null, int? month = null, CancellationToken ct = default);
}
