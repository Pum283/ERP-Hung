namespace Erp.Application.DTOs.Hrm;

public sealed record EmployeeDto(
    Guid Id,
    string EmployeeCode,
    Guid? UserId,
    string FullName,
    DateOnly? Dob,
    string? Gender,
    string? Email,
    string? Phone,
    Guid OrgUnitId,
    string? OrgUnitName,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? JobLevelId,
    string? JobLevelName,
    Guid? JobTitleId,
    string? JobTitleName,
    Guid? EmployeeTypeId,
    string? EmployeeTypeName,
    Guid? ManagerEmployeeId,
    string? ManagerName,
    string Status,
    DateOnly? HireDate,
    DateOnly? TerminateDate);

public sealed record EmployeeUpsertRequest(
    Guid? Id,
    string EmployeeCode,
    Guid? UserId,
    string FullName,
    DateOnly? Dob,
    string? Gender,
    string? Email,
    string? Phone,
    Guid OrgUnitId,
    Guid? DepartmentId,
    Guid? JobLevelId,
    Guid? JobTitleId,
    Guid? EmployeeTypeId,
    Guid? ManagerEmployeeId,
    string Status,
    DateOnly? HireDate,
    DateOnly? TerminateDate);

public sealed record EmployeeDocumentDto(
    Guid Id, Guid EmployeeId, string DocType, string Title, string StorageKey,
    DateOnly? IssuedOn, DateOnly? ExpiresOn, DateTimeOffset CreatedAt);

public sealed record EmployeeDocumentUploadRequest(
    string DocType, string Title, string StorageKey, DateOnly? IssuedOn, DateOnly? ExpiresOn);

public sealed record JobTitleDto(Guid Id, string Code, string Name, Guid? DefaultJobLevelId, bool IsActive);
public sealed record EmployeeTypeDto(Guid Id, string Code, string Name, bool IsActive);
public sealed record LeaveTypeDto(Guid Id, string Code, string Name, bool IsPaid, decimal DefaultDaysPerYear, bool IsActive);
public sealed record ContractDto(
    Guid Id, Guid EmployeeId, string? EmployeeName, string ContractNo, string ContractType,
    DateOnly StartDate, DateOnly? EndDate, string Status,
    Guid? ParentContractId, decimal? BaseSalary, Guid? ScanFileId);

public sealed record ContractUpsertRequest(
    Guid? Id, Guid EmployeeId, string ContractNo, string ContractType,
    DateOnly StartDate, DateOnly? EndDate, string Status,
    Guid? ParentContractId = null, decimal? BaseSalary = null, Guid? ScanFileId = null);

public sealed record ContractRenewRequest(DateOnly NewEndDate, decimal? BaseSalary = null);
public sealed record ContractTerminateRequest(DateOnly TerminateDate, string? Reason = null);

public sealed record LeaveBalanceDto(
    Guid Id, Guid EmployeeId, Guid LeaveTypeId, string LeaveTypeName,
    int Year, decimal Entitled, decimal Used, decimal Remaining);

public sealed record LeaveRequestDto(
    Guid Id, Guid EmployeeId, string EmployeeName, Guid LeaveTypeId, string LeaveTypeName,
    DateOnly FromDate, DateOnly ToDate, decimal Days, string? Reason, string Status, Guid? WfInstanceId);

public sealed record LeaveRequestCreateRequest(
    Guid? EmployeeId, Guid LeaveTypeId, DateOnly FromDate, DateOnly ToDate,
    decimal Days, string? Reason, bool Submit);

public sealed record LeaveEntitlementRuleDto(
    Guid Id, Guid LeaveTypeId, string LeaveTypeName, Guid? EmployeeTypeId, string? EmployeeTypeName,
    decimal DaysPerYear, bool IsActive, string? Note);

public sealed record LeaveEntitlementRuleUpsertRequest(
    Guid? Id, Guid LeaveTypeId, Guid? EmployeeTypeId, decimal DaysPerYear, bool IsActive, string? Note);

public sealed record LeaveBalanceAdjustRequest(
    Guid EmployeeId, Guid LeaveTypeId, int Year, decimal Entitled, string? Note);

public sealed record LeaveAllocateYearRequest(int Year, Guid? EmployeeTypeId, Guid? LeaveTypeId);

public sealed record LeaveCalendarItemDto(
    Guid RequestId, Guid EmployeeId, string EmployeeCode, string EmployeeName,
    Guid OrgUnitId, string OrgUnitName, Guid LeaveTypeId, string LeaveTypeName,
    DateOnly FromDate, DateOnly ToDate, decimal Days, string Status);

public sealed record HolidayDto(Guid Id, DateOnly Date, string Name, bool IsPaid, int Year, string? Note);

public sealed record HolidayUpsertRequest(Guid? Id, DateOnly Date, string Name, bool IsPaid, string? Note);

public sealed record HolidayImportItem(DateOnly Date, string Name, bool IsPaid);

public sealed record LeaveReportRowDto(
    Guid EmployeeId, string EmployeeCode, string EmployeeName, Guid OrgUnitId, string OrgUnitName,
    Guid LeaveTypeId, string LeaveTypeName, int Year, decimal Entitled, decimal Used, decimal Remaining,
    int ApprovedRequests);

public sealed record RecruitmentRequestDto(
    Guid Id, string DocNo, Guid JobTitleId, string JobTitleName, int Headcount,
    string Reason, Guid OrgUnitId, string OrgUnitName, string Status, Guid? WfInstanceId,
    Guid RequestedByUserId, string RequesterName, DateTimeOffset CreatedAt,
    IReadOnlyList<RecruitmentApprovalStepDto> ApprovalHistory);

public sealed record RecruitmentApprovalStepDto(
    Guid ActionId, Guid ActorUserId, string ActorName, string Action, string? Comment, DateTimeOffset At);

public sealed record RecruitmentRequestCreateRequest(
    Guid JobTitleId, int Headcount, string Reason, Guid OrgUnitId, bool Submit);

public sealed record JobPostingDto(
    Guid Id, Guid RecruitmentRequestId, string RequestDocNo, string Title, string Channel,
    string Status, string JobTitleName, int Headcount, DateTimeOffset CreatedAt);

public sealed record JobPostingCreateRequest(Guid RecruitmentRequestId, string Title, string Channel);

public sealed record CandidateDto(
    Guid Id, Guid JobPostingId, string JobPostingTitle, string FullName, string? Email, string? Phone,
    string? CvStorageKey, string PipelineStatus, Guid? EvalOrgUnitId, string? EvalOrgUnitName,
    int? EvalScore, string? EvalComment, string? CareNotes, Guid? ConvertedEmployeeId, DateTimeOffset CreatedAt);

public sealed record CandidateCreateRequest(
    Guid JobPostingId, string FullName, string? Email, string? Phone, string? CvStorageKey);

public sealed record CandidatePipelineUpdateRequest(string PipelineStatus);

public sealed record CandidateEvalRequest(Guid? EvalOrgUnitId, int? EvalScore, string? EvalComment);

public sealed record CandidateCareNoteRequest(string Note);

public sealed record RecruitChannelStatDto(string Channel, int PostingCount, int CandidateCount);

public sealed record OnboardingSettingDto(int OnboardingDays, int TrialDays);

public sealed record OnboardingSettingUpsertRequest(int OnboardingDays, int TrialDays);

public sealed record OnboardingChecklistItemDto(string Key, string Label, bool Done);

public sealed record OnboardingDocumentDto(Guid Id, string Title, string StorageKey, DateTimeOffset CreatedAt);

public sealed record OnboardingCaseDto(
    Guid Id, Guid EmployeeId, string EmployeeCode, string EmployeeName, string EmployeeStatus,
    Guid? CandidateId, string? CandidateName, Guid? MentorEmployeeId, string? MentorName,
    DateOnly StartDate, DateOnly OnboardingDueDate, DateOnly TrialEndDate, string Status,
    int? TrialScore, string? TrialComment, IReadOnlyList<OnboardingChecklistItemDto> Checklist,
    IReadOnlyList<OnboardingDocumentDto> Documents);

public sealed record HireFromCandidateRequest(Guid CandidateId, Guid? OrgUnitId = null, Guid? JobTitleId = null);

public sealed record AssignMentorRequest(Guid MentorEmployeeId);

public sealed record OnboardingChecklistUpdateRequest(IReadOnlyList<OnboardingChecklistItemDto> Items);

public sealed record OnboardingDocUploadRequest(string Title, string StorageKey);

public sealed record TrialEvalRequest(int Score, string? Comment);

public sealed record TrialExpiringDto(
    Guid OnboardingCaseId, Guid EmployeeId, string EmployeeCode, string FullName,
    DateOnly TrialEndDate, int DaysLeft);

public sealed record HeadcountPlanDto(
    Guid Id, string ScopeType, Guid OrgUnitId, string OrgUnitName,
    Guid? DepartmentId, string? DepartmentName, string? ShiftCode,
    int PlannedHeadcount, string Status, DateOnly EffectiveFrom, DateOnly? EffectiveTo,
    string? Note, Guid RequestedByUserId, string RequesterName,
    Guid? DecidedByUserId, string? DeciderName, DateTimeOffset? DecidedAt, DateTimeOffset CreatedAt);

public sealed record HeadcountPlanUpsertRequest(
    Guid? Id, string ScopeType, Guid OrgUnitId, Guid? DepartmentId, string? ShiftCode,
    int PlannedHeadcount, DateOnly EffectiveFrom, DateOnly? EffectiveTo, string? Note, bool Submit);

public sealed record HeadcountCompareRowDto(
    string ScopeType, Guid OrgUnitId, string OrgUnitName,
    Guid? DepartmentId, string? DepartmentName, string? ShiftCode,
    int Planned, int Actual, int Gap, bool Shortage);

public sealed record WorkShiftDto(
    Guid Id, string Code, string Name, TimeOnly StartTime, TimeOnly EndTime,
    int BreakMinutes, bool IsOvernight, bool IsActive, string? Note);

public sealed record WorkShiftUpsertRequest(
    Guid? Id, string Code, string Name, TimeOnly StartTime, TimeOnly EndTime,
    int BreakMinutes, bool? IsOvernight, bool IsActive, string? Note);

public sealed record ShiftAssignmentDto(
    Guid Id, Guid EmployeeId, string EmployeeCode, string EmployeeName,
    Guid OrgUnitId, string OrgUnitName, Guid WorkShiftId, string ShiftCode, string ShiftName,
    TimeOnly StartTime, TimeOnly EndTime, DateOnly WorkDate, string Status, string? Note);

public sealed record ShiftAssignRequest(Guid EmployeeId, Guid WorkShiftId, DateOnly WorkDate, string? Note);

public sealed record ShiftAssignRangeRequest(
    IReadOnlyList<Guid> EmployeeIds, Guid WorkShiftId,
    DateOnly From, DateOnly To, IReadOnlyList<int>? Weekdays, string? Note);

public sealed record ShiftSwapRequest(Guid AssignmentAId, Guid AssignmentBId);

public sealed record ShiftCopyRequest(DateOnly SourceFrom, DateOnly SourceTo, DateOnly TargetStart, Guid? OrgUnitId);

public sealed record ShiftLockRequest(Guid OrgUnitId, string PeriodKey, string? Note);

public sealed record ShiftPeriodLockDto(
    Guid Id, Guid OrgUnitId, string OrgUnitName, string PeriodKey,
    DateOnly PeriodFrom, DateOnly PeriodTo, Guid LockedByUserId, string LockerName,
    DateTimeOffset LockedAt, string? Note);

public sealed record StaffTransferDto(
    Guid Id, string DocNo, string Kind, Guid? EmployeeId, string? EmployeeCode, string? EmployeeName,
    Guid FromOrgUnitId, string FromOrgUnitName, Guid ToOrgUnitId, string ToOrgUnitName,
    DateOnly StartDate, DateOnly? EndDate, string Reason, int? RequestedHeadcount, string Status,
    bool AttendanceTagged, string AttendanceTag, decimal? PlannedHours, decimal? ActualHours,
    decimal? CostRate, decimal? EstimatedCost, Guid RequestedByUserId, string RequesterName,
    Guid? AcknowledgedByUserId, string? AcknowledgerName, DateTimeOffset? AcknowledgedAt,
    Guid? SourceRequestId, string? Note, DateTimeOffset CreatedAt);

public sealed record TransferRequestCreateRequest(
    Guid FromOrgUnitId, Guid ToOrgUnitId, DateOnly StartDate, DateOnly? EndDate,
    int RequestedHeadcount, string Reason, string? Note, bool Submit);

public sealed record TransferOrderCreateRequest(
    Guid EmployeeId, Guid FromOrgUnitId, Guid ToOrgUnitId, DateOnly StartDate, DateOnly? EndDate,
    string Reason, decimal? PlannedHours, decimal? CostRate, bool AttendanceTagged,
    string? Note, bool Issue, Guid? SourceRequestId);

public sealed record TransferActualHoursRequest(decimal ActualHours);

public sealed record TransferCostReportRowDto(
    Guid OrgUnitId, string OrgUnitName, int OrderCount, decimal PlannedHours,
    decimal ActualHours, decimal EstimatedCost, decimal ActualCost);

public sealed record AttendancePolicyDto(
    bool EnableFingerprint, bool EnableApp, bool EnableQr, bool EnableGeoFence,
    int LateGraceMinutes, int LateDeductEveryMinutes, decimal LateDeductWorkUnit,
    int ForgotCheckoutHours, int AdjustDeadlineDays, bool EnableOt, int OtAfterMinutes,
    bool EnableNightShiftRule, bool EnableHolidayRule,
    TimeOnly DefaultShiftStart, TimeOnly DefaultShiftEnd);

public sealed record AttendancePolicyUpsertRequest(
    bool EnableFingerprint, bool EnableApp, bool EnableQr, bool EnableGeoFence,
    int LateGraceMinutes, int LateDeductEveryMinutes, decimal LateDeductWorkUnit,
    int ForgotCheckoutHours, int AdjustDeadlineDays, bool EnableOt, int OtAfterMinutes,
    bool EnableNightShiftRule, bool EnableHolidayRule,
    TimeOnly DefaultShiftStart, TimeOnly DefaultShiftEnd);

public sealed record AttendanceDeviceDto(
    Guid Id, string Code, string Name, string DeviceType, Guid? OrgUnitId,
    string? OrgUnitName, string? SerialNo, bool IsActive, string? Note);

public sealed record AttendanceDeviceUpsertRequest(
    Guid? Id, string Code, string Name, string DeviceType, Guid? OrgUnitId,
    string? SerialNo, bool IsActive, string? Note);

public sealed record AttendanceGeoFenceDto(
    Guid Id, string Name, Guid? OrgUnitId, string? OrgUnitName,
    double Latitude, double Longitude, int RadiusMeters, bool IsActive);

public sealed record AttendanceGeoFenceUpsertRequest(
    Guid? Id, string Name, Guid? OrgUnitId, double Latitude, double Longitude,
    int RadiusMeters, bool IsActive);

public sealed record AttendanceRecordDto(
    Guid Id, Guid EmployeeId, string EmployeeCode, string EmployeeName,
    Guid OrgUnitId, string OrgUnitName, DateOnly WorkDate,
    DateTimeOffset? CheckInAt, DateTimeOffset? CheckOutAt,
    string? CheckInMethod, string? CheckOutMethod, int LateMinutes,
    decimal DeductedWorkUnit, int OtMinutes, decimal WorkUnit, string Status,
    string? Tag, string? Note, bool IsConfirmed);

public sealed record AttendancePunchRequest(
    string Method, Guid? DeviceId, double? Latitude, double? Longitude, string? Note);

public sealed record AttendanceDeviceSyncItem(
    string EmployeeCode, DateTimeOffset PunchedAt, string PunchType, string? DeviceCode);

public sealed record AttendanceDeviceSyncRequest(IReadOnlyList<AttendanceDeviceSyncItem> Items);

public sealed record AttendanceMissingAlertDto(
    Guid EmployeeId, string EmployeeCode, string EmployeeName, Guid OrgUnitId,
    string OrgUnitName, DateOnly WorkDate, string AlertType);

public sealed record AttendanceAdjustDto(
    Guid Id, Guid EmployeeId, string EmployeeCode, string EmployeeName, DateOnly WorkDate,
    DateTimeOffset? RequestedCheckInAt, DateTimeOffset? RequestedCheckOutAt,
    string Reason, string? EvidenceStorageKey, string Status,
    Guid RequestedByUserId, string RequesterName, DateTimeOffset CreatedAt);

public sealed record AttendanceAdjustCreateRequest(
    Guid EmployeeId, DateOnly WorkDate, DateTimeOffset? RequestedCheckInAt,
    DateTimeOffset? RequestedCheckOutAt, string Reason, string? EvidenceStorageKey, bool Submit);

public sealed record AttendancePeriodLockDto(
    Guid Id, string PeriodKey, DateOnly PeriodFrom, DateOnly PeriodTo,
    bool IsLocked, Guid LockedByUserId, string LockerName, DateTimeOffset LockedAt, string? Note);

public sealed record AttendanceLockRequest(string PeriodKey, string? Note);

public sealed record SalaryGradeDto(Guid Id, string Code, string Name, int Level, decimal BaseAmount, bool IsActive, string? Note);
public sealed record SalaryGradeUpsertRequest(Guid? Id, string Code, string Name, int Level, decimal BaseAmount, bool IsActive, string? Note);

public sealed record EmployeeSalaryDto(
    Guid Id, Guid EmployeeId, string EmployeeCode, string EmployeeName, Guid? SalaryGradeId, string? SalaryGradeName,
    decimal BaseSalary, decimal? HourlyRate, decimal? DailyRate, string? AppliesToStatus,
    DateOnly EffectiveFrom, DateOnly? EffectiveTo, bool IsActive, string? Note);

public sealed record EmployeeSalaryUpsertRequest(
    Guid? Id, Guid EmployeeId, Guid? SalaryGradeId, decimal BaseSalary, decimal? HourlyRate, decimal? DailyRate,
    string? AppliesToStatus, DateOnly EffectiveFrom, DateOnly? EffectiveTo, bool IsActive, string? Note);

public sealed record AllowanceTypeDto(Guid Id, string Code, string Name, decimal DefaultAmount, bool IsTaxable, bool IsActive);
public sealed record AllowanceTypeUpsertRequest(Guid? Id, string Code, string Name, decimal DefaultAmount, bool IsTaxable, bool IsActive);

public sealed record AllowanceRuleDto(
    Guid Id, Guid AllowanceTypeId, string AllowanceTypeName, string? ShiftCode, decimal Amount, bool IsActive, string? Note);
public sealed record AllowanceRuleUpsertRequest(
    Guid? Id, Guid AllowanceTypeId, string? ShiftCode, decimal Amount, bool IsActive, string? Note);

public sealed record PayrollPolicyDto(
    decimal SocialInsuranceEmpRate, decimal HealthInsuranceEmpRate, decimal UnemploymentEmpRate,
    decimal PersonalDeduction, decimal FlatTaxRate, int StandardWorkDays, decimal OtMultiplier);

public sealed record PayrollPolicyUpsertRequest(
    decimal SocialInsuranceEmpRate, decimal HealthInsuranceEmpRate, decimal UnemploymentEmpRate,
    decimal PersonalDeduction, decimal FlatTaxRate, int StandardWorkDays, decimal OtMultiplier);

public sealed record PayrollPeriodDto(
    Guid Id, string PeriodKey, DateOnly PeriodFrom, DateOnly PeriodTo, string Status, string? Note,
    int LineCount, decimal TotalNet, DateTimeOffset CreatedAt);

public sealed record PayrollPeriodCreateRequest(string PeriodKey, string? Note);

public sealed record PayrollLineDto(
    Guid Id, Guid PayrollPeriodId, Guid EmployeeId, string EmployeeCode, string EmployeeName, string OrgUnitName,
    decimal WorkUnits, int OtMinutes, decimal BaseSalary, decimal AttendancePay, decimal OtPay,
    decimal AllowanceTotal, decimal Bonus, decimal DeductionTotal, decimal InsuranceEmployee,
    decimal Tax, decimal GrossPay, decimal NetPay, bool IsConfirmed, string? Note);

public sealed record PayrollLinePatchRequest(decimal? Bonus, decimal? DeductionTotal, decimal? AllowanceTotal, string? Note);

public sealed record PayrollAdjustmentDto(
    Guid Id, Guid PayrollPeriodId, Guid EmployeeId, string EmployeeName, string Kind, string Title, decimal Amount, string? Note);

public sealed record PayrollAdjustmentCreateRequest(
    Guid PayrollPeriodId, Guid EmployeeId, string Kind, string Title, decimal Amount, string? Note);

public sealed record PayrollCostByOrgDto(Guid OrgUnitId, string OrgUnitName, int Headcount, decimal Gross, decimal Net, decimal Insurance);

public sealed record PayrollCompareDto(string PeriodKey, decimal TotalGross, decimal TotalNet, decimal TotalInsurance, int LineCount);

public sealed record RewardDisciplineDto(
    Guid Id, Guid EmployeeId, string EmployeeCode, string EmployeeName, string Kind, string Title,
    DateOnly DecisionDate, string? Reason, decimal PayrollImpactAmount, string PayrollImpactKind,
    string? DecisionStorageKey, string Status, Guid? AppliedPayrollPeriodId, string? Note, DateTimeOffset CreatedAt);

public sealed record RewardDisciplineCreateRequest(
    Guid EmployeeId, string Kind, string Title, DateOnly DecisionDate, string? Reason,
    decimal PayrollImpactAmount, string PayrollImpactKind, string? DecisionStorageKey, string? Note);

public sealed record RewardDisciplineAttachRequest(string DecisionStorageKey);

public sealed record RewardDisciplineReportRowDto(string Kind, int Count, decimal TotalImpact);

public sealed record OffboardingSettingDto(int NoticeDays, bool RequireChecklistComplete, bool AutoRevokeAccessOnComplete);
public sealed record OffboardingSettingUpsertRequest(int NoticeDays, bool RequireChecklistComplete, bool AutoRevokeAccessOnComplete);

public sealed record OffboardingChecklistItemDto(string Key, string Label, bool Done);

public sealed record OffboardingCaseDto(
    Guid Id, Guid EmployeeId, string EmployeeCode, string EmployeeName, string OrgUnitName,
    DateOnly RequestDate, DateOnly LastWorkingDay, string ReasonCode, string? ReasonDetail, string Status,
    bool NoticeSatisfied, int RequiredNoticeDays, IReadOnlyList<OffboardingChecklistItemDto> Checklist,
    bool AccessRevoked, decimal? LeaveDaysRemaining, decimal? LeaveSettlementAmount, decimal? FinalPayEstimate,
    string? SettlementNote, string? InterviewNotes, string? RejectReason, DateTimeOffset CreatedAt);

public sealed record OffboardingCreateRequest(
    Guid EmployeeId, DateOnly RequestDate, DateOnly LastWorkingDay, string ReasonCode, string? ReasonDetail);

public sealed record OffboardingChecklistUpdateRequest(IReadOnlyList<OffboardingChecklistItemDto> Items);
public sealed record OffboardingSettleRequest(decimal? LeaveSettlementAmount, decimal? FinalPayEstimate, string? SettlementNote);
public sealed record OffboardingInterviewRequest(string InterviewNotes);
public sealed record OffboardingRejectRequest(string? Reason);

public sealed record OffboardingReportRowDto(string ReasonCode, int Count);

public sealed record HrmHeadcountByStatusDto(string Status, int Count);
public sealed record HrmHeadcountByOrgDto(Guid OrgUnitId, string OrgUnitName, int Count);
public sealed record HrmHeadcountMovementDto(string PeriodKey, int Hired, int Resigned, int Net);

public sealed record HrmDashboardHeadcountDto(
    int TotalActive, int TotalProbation, int TotalInactiveOrLeft,
    IReadOnlyList<HrmHeadcountByStatusDto> ByStatus,
    IReadOnlyList<HrmHeadcountByOrgDto> ByOrg,
    IReadOnlyList<HrmHeadcountMovementDto> Movements);

public sealed record HrmAttendanceReportRowDto(
    Guid OrgUnitId, string OrgUnitName, int RecordCount, decimal WorkUnits,
    int OtMinutes, int LateMinutes, int LateCount);

public sealed record HrmRecruitFunnelRowDto(string PipelineStatus, int Count);

public sealed record HrmLeaveSummaryRowDto(
    Guid OrgUnitId, string OrgUnitName, decimal Entitled, decimal Used, decimal Remaining, int EmployeeCount);

public sealed record HrmCostSummaryDto(
    Guid? PeriodId, string? PeriodKey, string? PeriodStatus,
    decimal TotalGross, decimal TotalNet, decimal TotalInsurance, int LineCount,
    IReadOnlyList<PayrollCostByOrgDto> ByOrg);

public sealed record HrmDashboardBundleDto(
    HrmDashboardHeadcountDto Headcount,
    IReadOnlyList<HrmAttendanceReportRowDto> Attendance,
    IReadOnlyList<HrmRecruitFunnelRowDto> RecruitFunnel,
    IReadOnlyList<HrmLeaveSummaryRowDto> LeaveSummary,
    HrmCostSummaryDto Cost,
    IReadOnlyList<HeadcountCompareRowDto> HeadcountVsPlan);

public sealed record ChangeEmployeeStatusRequest(string Status, string? Note);

