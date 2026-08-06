using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmAttendanceService
{
    Task<AttendancePolicyDto> GetPolicyAsync(Guid tenantId, CancellationToken ct = default);
    Task<AttendancePolicyDto> UpsertPolicyAsync(Guid tenantId, Guid userId, AttendancePolicyUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<AttendanceDeviceDto>> ListDevicesAsync(Guid tenantId, CancellationToken ct = default);
    Task<AttendanceDeviceDto> UpsertDeviceAsync(Guid tenantId, Guid userId, AttendanceDeviceUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<AttendanceGeoFenceDto>> ListGeoFencesAsync(Guid tenantId, CancellationToken ct = default);
    Task<AttendanceGeoFenceDto> UpsertGeoFenceAsync(Guid tenantId, Guid userId, AttendanceGeoFenceUpsertRequest req, CancellationToken ct = default);

    Task<AttendanceRecordDto> CheckInAsync(Guid tenantId, Guid userId, AttendancePunchRequest req, CancellationToken ct = default);
    Task<AttendanceRecordDto> CheckOutAsync(Guid tenantId, Guid userId, AttendancePunchRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<AttendanceRecordDto>> MyHistoryAsync(
        Guid tenantId, Guid userId, DateOnly? from, DateOnly? to, CancellationToken ct = default);

    Task<IReadOnlyList<AttendanceRecordDto>> BoardAsync(
        Guid tenantId, Guid? orgUnitId, DateOnly? from, DateOnly? to, CancellationToken ct = default);

    Task<IReadOnlyList<AttendanceMissingAlertDto>> MissingAlertsAsync(Guid tenantId, DateOnly? date, CancellationToken ct = default);

    Task<int> MarkMissingAsync(Guid tenantId, Guid userId, DateOnly date, CancellationToken ct = default);
    Task<int> SyncDeviceAsync(Guid tenantId, Guid userId, AttendanceDeviceSyncRequest req, CancellationToken ct = default);
    Task<int> RecalcOtAsync(Guid tenantId, Guid userId, DateOnly from, DateOnly to, CancellationToken ct = default);

    Task<IReadOnlyList<AttendanceAdjustDto>> ListAdjustsAsync(Guid tenantId, CancellationToken ct = default);
    Task<AttendanceAdjustDto> CreateAdjustAsync(Guid tenantId, Guid userId, AttendanceAdjustCreateRequest req, CancellationToken ct = default);
    Task<AttendanceAdjustDto> DecideAdjustAsync(Guid tenantId, Guid userId, Guid id, bool approve, CancellationToken ct = default);

    Task<IReadOnlyList<AttendancePeriodLockDto>> ListLocksAsync(Guid tenantId, CancellationToken ct = default);
    Task<AttendancePeriodLockDto> LockPeriodAsync(Guid tenantId, Guid userId, AttendanceLockRequest req, CancellationToken ct = default);
    Task<AttendancePeriodLockDto> UnlockPeriodAsync(Guid tenantId, Guid userId, string periodKey, CancellationToken ct = default);

    Task ConfirmRecordAsync(Guid tenantId, Guid userId, Guid recordId, CancellationToken ct = default);
}
