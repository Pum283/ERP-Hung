using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPjmTimesheetBudgetChecklistService
{
    // UC_PJM_020: Timesheet theo dự án
    Task<PjmProjectTimesheetEntryDto> CreateTimesheetEntryAsync(Guid tenantId, PjmCreateTimesheetRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PjmProjectTimesheetEntryDto>> GetTimesheetEntriesAsync(Guid tenantId, Guid projectId, CancellationToken ct = default);

    // UC_PJM_024: Cảnh báo vượt ngân sách
    Task<IReadOnlyList<PjmBudgetOverrunWarningDto>> GetBudgetOverrunWarningsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_PJM_025: Checklist khảo sát
    Task<PjmSurveyChecklistItemDto> CreateSurveyChecklistAsync(Guid tenantId, PjmCreateSurveyChecklistRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PjmSurveyChecklistItemDto>> GetSurveyChecklistsAsync(Guid tenantId, Guid projectId, CancellationToken ct = default);

    // UC_PJM_026: Checklist lắp đặt
    Task<PjmInstallationChecklistItemDto> CreateInstallationChecklistAsync(Guid tenantId, PjmCreateInstallationChecklistRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PjmInstallationChecklistItemDto>> GetInstallationChecklistsAsync(Guid tenantId, Guid projectId, CancellationToken ct = default);
}
