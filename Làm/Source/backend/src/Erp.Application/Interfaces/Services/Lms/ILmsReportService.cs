using Erp.Application.DTOs.Lms;

namespace Erp.Application.Interfaces.Services.Lms;

public interface ILmsReportService
{
    Task<LmsDashboardDto> DashboardAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<LmsCompletionByOrgRowDto>> CompletionByOrgAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<LmsLearnerRowDto>> LearnersAsync(
        Guid tenantId, Guid? classId = null, Guid? courseId = null, Guid? instructorId = null,
        CancellationToken ct = default);
    Task<string> ExportCsvAsync(Guid tenantId, string report, Guid? classId = null, Guid? courseId = null,
        Guid? instructorId = null, CancellationToken ct = default);
}
