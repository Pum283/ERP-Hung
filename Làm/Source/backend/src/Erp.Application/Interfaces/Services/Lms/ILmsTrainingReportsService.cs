using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ILmsTrainingReportsService
{
    // UC_LMS_064: Cảnh báo quá hạn đào tạo
    Task<IReadOnlyList<LmsOverdueTrainingAlertDto>> GetOverdueTrainingAlertsAsync(Guid tenantId, Guid? userId = null, CancellationToken ct = default);
    Task<IReadOnlyList<LmsOverdueTrainingAlertDto>> TriggerOverdueCheckAsync(Guid tenantId, CancellationToken ct = default);

    // UC_LMS_067: Báo cáo điểm thi / tỷ lệ đạt
    Task<IReadOnlyList<LmsExamAnalyticsReportDto>> GetExamAnalyticsReportAsync(Guid tenantId, Guid? examId = null, CancellationToken ct = default);

    // UC_LMS_068: Báo cáo học viên bỏ dở
    Task<IReadOnlyList<LmsDropoutAnalyticsReportDto>> GetDropoutAnalyticsReportAsync(Guid tenantId, Guid? courseId = null, int inactiveDaysThreshold = 14, CancellationToken ct = default);

    // UC_LMS_069: Báo cáo hiệu quả khóa
    Task<IReadOnlyList<LmsCourseEngagementReportDto>> GetCourseEngagementReportAsync(Guid tenantId, Guid? courseId = null, CancellationToken ct = default);
}
