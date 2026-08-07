import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type EmployeeDto = {
  id: string;
  employeeCode: string;
  userId?: string | null;
  fullName: string;
  dob?: string | null;
  gender?: string | null;
  email?: string | null;
  phone?: string | null;
  orgUnitId: string;
  orgUnitName?: string | null;
  departmentId?: string | null;
  departmentName?: string | null;
  jobLevelId?: string | null;
  jobLevelName?: string | null;
  jobTitleId?: string | null;
  jobTitleName?: string | null;
  employeeTypeId?: string | null;
  employeeTypeName?: string | null;
  managerEmployeeId?: string | null;
  managerName?: string | null;
  status: string;
  hireDate?: string | null;
  terminateDate?: string | null;
};

export type EmployeeUpsertRequest = {
  id?: string | null;
  employeeCode: string;
  userId?: string | null;
  fullName: string;
  dob?: string | null;
  gender?: string | null;
  email?: string | null;
  phone?: string | null;
  orgUnitId: string;
  departmentId?: string | null;
  jobLevelId?: string | null;
  jobTitleId?: string | null;
  employeeTypeId?: string | null;
  managerEmployeeId?: string | null;
  status: string;
  hireDate?: string | null;
  terminateDate?: string | null;
};

export type LeaveTypeDto = {
  id: string;
  code: string;
  name: string;
  isPaid: boolean;
  defaultDaysPerYear: number;
  isActive: boolean;
};

export type LeaveBalanceDto = {
  id: string;
  employeeId: string;
  leaveTypeId: string;
  leaveTypeName: string;
  year: number;
  entitled: number;
  used: number;
  remaining: number;
};

export type LeaveRequestDto = {
  id: string;
  employeeId: string;
  employeeName: string;
  leaveTypeId: string;
  leaveTypeName: string;
  fromDate: string;
  toDate: string;
  days: number;
  reason?: string | null;
  status: string;
  wfInstanceId?: string | null;
};

export type LeaveRequestCreateRequest = {
  employeeId?: string | null;
  leaveTypeId: string;
  fromDate: string;
  toDate: string;
  days: number;
  reason?: string | null;
  submit: boolean;
};

export type ContractDto = {
  id: string;
  employeeId: string;
  employeeName?: string | null;
  contractNo: string;
  contractType: string;
  startDate: string;
  endDate?: string | null;
  status: string;
};

export type ContractUpsertRequest = {
  id?: string | null;
  employeeId: string;
  contractNo: string;
  contractType: string;
  startDate: string;
  endDate?: string | null;
  status: string;
};

export async function fetchEmployees(q?: string) {
  const { data } = await api.get<Envelope<EmployeeDto[]>>("/api/hrm/employees", {
    params: q ? { q } : undefined,
  });
  return data.data;
}

export async function fetchEmployee(id: string) {
  const { data } = await api.get<Envelope<EmployeeDto>>(`/api/hrm/employees/${id}`);
  return data.data;
}

export type EmployeeDocumentDto = {
  id: string;
  employeeId: string;
  docType: string;
  title: string;
  storageKey: string;
  issuedOn?: string | null;
  expiresOn?: string | null;
  createdAt: string;
};

export async function fetchEmployeeDocuments(employeeId: string) {
  const { data } = await api.get<Envelope<EmployeeDocumentDto[]>>(
    `/api/hrm/employees/${employeeId}/documents`
  );
  return data.data;
}

export async function addEmployeeDocument(
  employeeId: string,
  body: {
    docType: string;
    title: string;
    storageKey: string;
    issuedOn?: string | null;
    expiresOn?: string | null;
  }
) {
  const { data } = await api.post<Envelope<EmployeeDocumentDto>>(
    `/api/hrm/employees/${employeeId}/documents`,
    body
  );
  return data.data;
}

export async function deleteEmployeeDocument(employeeId: string, docId: string) {
  await api.delete(`/api/hrm/employees/${employeeId}/documents/${docId}`);
}

export async function upsertEmployee(body: EmployeeUpsertRequest) {
  const { data } = await api.post<Envelope<EmployeeDto>>("/api/hrm/employees", body);
  return data.data;
}

export async function fetchJobTitles() {
  const { data } = await api.get<Envelope<{ id: string; code: string; name: string }[]>>("/api/hrm/job-titles");
  return data.data;
}

export async function fetchEmployeeTypes() {
  const { data } = await api.get<Envelope<{ id: string; code: string; name: string }[]>>("/api/hrm/employee-types");
  return data.data;
}

export async function fetchLeaveTypes() {
  const { data } = await api.get<Envelope<LeaveTypeDto[]>>("/api/hrm/leave-types");
  return data.data;
}

export async function fetchLeaveBalances(employeeId?: string) {
  const { data } = await api.get<Envelope<LeaveBalanceDto[]>>("/api/hrm/leave-balances", {
    params: employeeId ? { employeeId } : undefined,
  });
  return data.data;
}

export async function fetchLeaveRequests(employeeId?: string) {
  const { data } = await api.get<Envelope<LeaveRequestDto[]>>("/api/hrm/leave-requests", {
    params: employeeId ? { employeeId } : undefined,
  });
  return data.data;
}

export async function createLeaveRequest(body: LeaveRequestCreateRequest) {
  const { data } = await api.post<Envelope<LeaveRequestDto>>("/api/hrm/leave-requests", body);
  return data.data;
}

export async function cancelLeaveRequest(id: string) {
  const { data } = await api.post<Envelope<LeaveRequestDto>>(`/api/hrm/leave-requests/${id}/cancel`);
  return data.data;
}

export type LeaveEntitlementRuleDto = {
  id: string;
  leaveTypeId: string;
  leaveTypeName: string;
  employeeTypeId?: string | null;
  employeeTypeName?: string | null;
  daysPerYear: number;
  isActive: boolean;
  note?: string | null;
};

export type LeaveCalendarItemDto = {
  requestId: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  orgUnitId: string;
  orgUnitName: string;
  leaveTypeId: string;
  leaveTypeName: string;
  fromDate: string;
  toDate: string;
  days: number;
  status: string;
};

export type HolidayDto = {
  id: string;
  date: string;
  name: string;
  isPaid: boolean;
  year: number;
  note?: string | null;
};

export type LeaveReportRowDto = {
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  orgUnitId: string;
  orgUnitName: string;
  leaveTypeId: string;
  leaveTypeName: string;
  year: number;
  entitled: number;
  used: number;
  remaining: number;
  approvedRequests: number;
};

export async function fetchLeaveEntitlements() {
  const { data } = await api.get<Envelope<LeaveEntitlementRuleDto[]>>("/api/hrm/leave-entitlements");
  return data.data;
}

export async function upsertLeaveEntitlement(body: {
  id?: string | null;
  leaveTypeId: string;
  employeeTypeId?: string | null;
  daysPerYear: number;
  isActive: boolean;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<LeaveEntitlementRuleDto>>("/api/hrm/leave-entitlements", body);
  return data.data;
}

export async function adjustLeaveBalance(body: {
  employeeId: string;
  leaveTypeId: string;
  year: number;
  entitled: number;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<LeaveBalanceDto>>("/api/hrm/leave-balances/adjust", body);
  return data.data;
}

export async function allocateLeaveYear(body: {
  year: number;
  employeeTypeId?: string | null;
  leaveTypeId?: string | null;
}) {
  const { data } = await api.post<Envelope<{ allocated: number }>>("/api/hrm/leave-balances/allocate", body);
  return data.data;
}

export async function fetchLeaveCalendar(params?: {
  orgUnitId?: string;
  from?: string;
  to?: string;
}) {
  const { data } = await api.get<Envelope<LeaveCalendarItemDto[]>>("/api/hrm/leave-calendar", {
    params,
  });
  return data.data;
}

export async function fetchHolidays(year?: number) {
  const { data } = await api.get<Envelope<HolidayDto[]>>("/api/hrm/holidays", {
    params: year ? { year } : undefined,
  });
  return data.data;
}

export async function upsertHoliday(body: {
  id?: string | null;
  date: string;
  name: string;
  isPaid: boolean;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<HolidayDto>>("/api/hrm/holidays", body);
  return data.data;
}

export async function importHolidays(
  items: { date: string; name: string; isPaid: boolean }[],
) {
  const { data } = await api.post<Envelope<{ imported: number }>>("/api/hrm/holidays/import", items);
  return data.data;
}

export async function fetchLeaveReport(params?: { year?: number; orgUnitId?: string }) {
  const { data } = await api.get<Envelope<LeaveReportRowDto[]>>("/api/hrm/leave-report", { params });
  return data.data;
}

export async function fetchContracts(employeeId?: string) {
  const { data } = await api.get<Envelope<ContractDto[]>>("/api/hrm/contracts", {
    params: employeeId ? { employeeId } : undefined,
  });
  return data.data;
}

export async function upsertContract(body: ContractUpsertRequest) {
  const { data } = await api.post<Envelope<ContractDto>>("/api/hrm/contracts", body);
  return data.data;
}

export type RecruitmentApprovalStepDto = {
  actionId: string;
  actorUserId: string;
  actorName: string;
  action: string;
  comment?: string | null;
  at: string;
};

export type RecruitmentRequestDto = {
  id: string;
  docNo: string;
  jobTitleId: string;
  jobTitleName: string;
  headcount: number;
  reason: string;
  orgUnitId: string;
  orgUnitName: string;
  status: string;
  wfInstanceId?: string | null;
  requestedByUserId: string;
  requesterName: string;
  createdAt: string;
  approvalHistory: RecruitmentApprovalStepDto[];
};

export type RecruitmentRequestCreateRequest = {
  jobTitleId: string;
  headcount: number;
  reason: string;
  orgUnitId: string;
  submit: boolean;
};

export async function fetchRecruitmentRequests() {
  const { data } = await api.get<Envelope<RecruitmentRequestDto[]>>("/api/hrm/recruitment-requests");
  return data.data;
}

export async function createRecruitmentRequest(body: RecruitmentRequestCreateRequest) {
  const { data } = await api.post<Envelope<RecruitmentRequestDto>>("/api/hrm/recruitment-requests", body);
  return data.data;
}

export async function submitRecruitmentRequest(id: string) {
  const { data } = await api.post<Envelope<RecruitmentRequestDto>>(
    `/api/hrm/recruitment-requests/${id}/submit`,
  );
  return data.data;
}

export async function closeRecruitmentRequest(id: string) {
  const { data } = await api.post<Envelope<RecruitmentRequestDto>>(
    `/api/hrm/recruitment-requests/${id}/close`,
  );
  return data.data;
}

export type JobPostingDto = {
  id: string;
  recruitmentRequestId: string;
  requestDocNo: string;
  title: string;
  channel: string;
  status: string;
  jobTitleName: string;
  headcount: number;
  createdAt: string;
};

export type CandidateDto = {
  id: string;
  jobPostingId: string;
  jobPostingTitle: string;
  fullName: string;
  email?: string | null;
  phone?: string | null;
  cvStorageKey?: string | null;
  pipelineStatus: string;
  evalOrgUnitId?: string | null;
  evalOrgUnitName?: string | null;
  evalScore?: number | null;
  evalComment?: string | null;
  careNotes?: string | null;
  convertedEmployeeId?: string | null;
  createdAt: string;
};

export type OnboardingSettingDto = { onboardingDays: number; trialDays: number };

export type OnboardingChecklistItemDto = { key: string; label: string; done: boolean };

export type OnboardingDocumentDto = {
  id: string;
  title: string;
  storageKey: string;
  createdAt: string;
};

export type OnboardingCaseDto = {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  employeeStatus: string;
  candidateId?: string | null;
  candidateName?: string | null;
  mentorEmployeeId?: string | null;
  mentorName?: string | null;
  startDate: string;
  onboardingDueDate: string;
  trialEndDate: string;
  status: string;
  trialScore?: number | null;
  trialComment?: string | null;
  checklist: OnboardingChecklistItemDto[];
  documents: OnboardingDocumentDto[];
};

export type TrialExpiringDto = {
  onboardingCaseId: string;
  employeeId: string;
  employeeCode: string;
  fullName: string;
  trialEndDate: string;
  daysLeft: number;
};

export type RecruitChannelStatDto = {
  channel: string;
  postingCount: number;
  candidateCount: number;
};

export async function fetchJobPostings() {
  const { data } = await api.get<Envelope<JobPostingDto[]>>("/api/hrm/job-postings");
  return data.data;
}

export async function createJobPosting(body: {
  recruitmentRequestId: string;
  title: string;
  channel: string;
}) {
  const { data } = await api.post<Envelope<JobPostingDto>>("/api/hrm/job-postings", body);
  return data.data;
}

export async function closeJobPosting(id: string) {
  await api.post(`/api/hrm/job-postings/${id}/close`);
}

export async function fetchCandidates(jobPostingId?: string) {
  const { data } = await api.get<Envelope<CandidateDto[]>>("/api/hrm/candidates", {
    params: jobPostingId ? { jobPostingId } : undefined,
  });
  return data.data;
}

export async function createCandidate(body: {
  jobPostingId: string;
  fullName: string;
  email?: string | null;
  phone?: string | null;
  cvStorageKey?: string | null;
}) {
  const { data } = await api.post<Envelope<CandidateDto>>("/api/hrm/candidates", body);
  return data.data;
}

export async function updateCandidatePipeline(id: string, pipelineStatus: string) {
  const { data } = await api.post<Envelope<CandidateDto>>(`/api/hrm/candidates/${id}/pipeline`, {
    pipelineStatus,
  });
  return data.data;
}

export async function evaluateCandidate(
  id: string,
  body: { evalOrgUnitId?: string | null; evalScore?: number | null; evalComment?: string | null },
) {
  const { data } = await api.post<Envelope<CandidateDto>>(`/api/hrm/candidates/${id}/evaluate`, body);
  return data.data;
}

export async function addCandidateCareNote(id: string, note: string) {
  const { data } = await api.post<Envelope<CandidateDto>>(`/api/hrm/candidates/${id}/care-notes`, {
    note,
  });
  return data.data;
}

export async function fetchRecruitChannelStats() {
  const { data } = await api.get<Envelope<RecruitChannelStatDto[]>>("/api/hrm/recruit/channel-stats");
  return data.data;
}

export async function uploadHrmFile(file: File) {
  const fd = new FormData();
  fd.append("file", file);
  const { data } = await api.post<Envelope<{ storageKey: string; fileName?: string }>>(
    "/api/sys/files/upload",
    fd,
    { headers: { "Content-Type": "multipart/form-data" } },
  );
  return data.data;
}

export async function fetchOnboardingSettings() {
  const { data } = await api.get<Envelope<OnboardingSettingDto>>("/api/hrm/onboarding/settings");
  return data.data;
}

export async function upsertOnboardingSettings(body: OnboardingSettingDto) {
  const { data } = await api.put<Envelope<OnboardingSettingDto>>("/api/hrm/onboarding/settings", body);
  return data.data;
}

export async function fetchOnboardingCases() {
  const { data } = await api.get<Envelope<OnboardingCaseDto[]>>("/api/hrm/onboarding/cases");
  return data.data;
}

export async function fetchTrialExpiring(days = 14) {
  const { data } = await api.get<Envelope<TrialExpiringDto[]>>("/api/hrm/onboarding/trial-expiring", {
    params: { days },
  });
  return data.data;
}

export async function hireFromCandidate(candidateId: string) {
  const { data } = await api.post<Envelope<OnboardingCaseDto>>("/api/hrm/onboarding/hire-from-candidate", {
    candidateId,
  });
  return data.data;
}

export async function assignOnboardingMentor(caseId: string, mentorEmployeeId: string) {
  const { data } = await api.post<Envelope<OnboardingCaseDto>>(
    `/api/hrm/onboarding/cases/${caseId}/mentor`,
    { mentorEmployeeId },
  );
  return data.data;
}

export async function updateOnboardingChecklist(caseId: string, items: OnboardingChecklistItemDto[]) {
  const { data } = await api.put<Envelope<OnboardingCaseDto>>(
    `/api/hrm/onboarding/cases/${caseId}/checklist`,
    { items },
  );
  return data.data;
}

export async function addOnboardingDocument(caseId: string, title: string, storageKey: string) {
  const { data } = await api.post<Envelope<OnboardingCaseDto>>(
    `/api/hrm/onboarding/cases/${caseId}/documents`,
    { title, storageKey },
  );
  return data.data;
}

export async function evaluateOnboardingTrial(caseId: string, score: number, comment?: string | null) {
  const { data } = await api.post<Envelope<OnboardingCaseDto>>(
    `/api/hrm/onboarding/cases/${caseId}/trial-eval`,
    { score, comment: comment ?? null },
  );
  return data.data;
}

export async function convertOnboardingOfficial(caseId: string) {
  const { data } = await api.post<Envelope<OnboardingCaseDto>>(
    `/api/hrm/onboarding/cases/${caseId}/convert`,
  );
  return data.data;
}

export type HeadcountPlanDto = {
  id: string;
  scopeType: string;
  orgUnitId: string;
  orgUnitName: string;
  departmentId?: string | null;
  departmentName?: string | null;
  shiftCode?: string | null;
  plannedHeadcount: number;
  status: string;
  effectiveFrom: string;
  effectiveTo?: string | null;
  note?: string | null;
  requestedByUserId: string;
  requesterName: string;
  decidedByUserId?: string | null;
  deciderName?: string | null;
  decidedAt?: string | null;
  createdAt: string;
};

export type HeadcountCompareRowDto = {
  scopeType: string;
  orgUnitId: string;
  orgUnitName: string;
  departmentId?: string | null;
  departmentName?: string | null;
  shiftCode?: string | null;
  planned: number;
  actual: number;
  gap: number;
  shortage: boolean;
};

export type HeadcountPlanUpsertRequest = {
  id?: string | null;
  scopeType: string;
  orgUnitId: string;
  departmentId?: string | null;
  shiftCode?: string | null;
  plannedHeadcount: number;
  effectiveFrom: string;
  effectiveTo?: string | null;
  note?: string | null;
  submit: boolean;
};

export async function fetchHeadcountPlans() {
  const { data } = await api.get<Envelope<HeadcountPlanDto[]>>("/api/hrm/headcount-plans");
  return data.data;
}

export async function upsertHeadcountPlan(body: HeadcountPlanUpsertRequest) {
  const { data } = await api.post<Envelope<HeadcountPlanDto>>("/api/hrm/headcount-plans", body);
  return data.data;
}

export async function submitHeadcountPlan(id: string) {
  const { data } = await api.post<Envelope<HeadcountPlanDto>>(`/api/hrm/headcount-plans/${id}/submit`);
  return data.data;
}

export async function approveHeadcountPlan(id: string) {
  const { data } = await api.post<Envelope<HeadcountPlanDto>>(`/api/hrm/headcount-plans/${id}/approve`);
  return data.data;
}

export async function rejectHeadcountPlan(id: string) {
  const { data } = await api.post<Envelope<HeadcountPlanDto>>(`/api/hrm/headcount-plans/${id}/reject`);
  return data.data;
}

export async function fetchHeadcountCompare() {
  const { data } = await api.get<Envelope<HeadcountCompareRowDto[]>>("/api/hrm/headcount-plans/compare");
  return data.data;
}

export async function fetchHeadcountShortages() {
  const { data } = await api.get<Envelope<HeadcountCompareRowDto[]>>("/api/hrm/headcount-plans/shortages");
  return data.data;
}

export type WorkShiftDto = {
  id: string;
  code: string;
  name: string;
  startTime: string;
  endTime: string;
  breakMinutes: number;
  isOvernight: boolean;
  isActive: boolean;
  note?: string | null;
};

export type ShiftAssignmentDto = {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  orgUnitId: string;
  orgUnitName: string;
  workShiftId: string;
  shiftCode: string;
  shiftName: string;
  startTime: string;
  endTime: string;
  workDate: string;
  status: string;
  note?: string | null;
};

export type ShiftPeriodLockDto = {
  id: string;
  orgUnitId: string;
  orgUnitName: string;
  periodKey: string;
  periodFrom: string;
  periodTo: string;
  lockedByUserId: string;
  lockerName: string;
  lockedAt: string;
  note?: string | null;
};

export async function fetchWorkShifts() {
  const { data } = await api.get<Envelope<WorkShiftDto[]>>("/api/hrm/shifts/templates");
  return data.data;
}

export async function upsertWorkShift(body: {
  id?: string | null;
  code: string;
  name: string;
  startTime: string;
  endTime: string;
  breakMinutes: number;
  isOvernight?: boolean | null;
  isActive: boolean;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<WorkShiftDto>>("/api/hrm/shifts/templates", body);
  return data.data;
}

export async function fetchShiftAssignments(params?: {
  orgUnitId?: string;
  employeeId?: string;
  from?: string;
  to?: string;
}) {
  const { data } = await api.get<Envelope<ShiftAssignmentDto[]>>("/api/hrm/shifts/assignments", {
    params,
  });
  return data.data;
}

export async function fetchMyShiftAssignments(params?: { from?: string; to?: string }) {
  const { data } = await api.get<Envelope<ShiftAssignmentDto[]>>("/api/hrm/shifts/assignments/mine", {
    params,
  });
  return data.data;
}

export async function assignShift(body: {
  employeeId: string;
  workShiftId: string;
  workDate: string;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<ShiftAssignmentDto>>("/api/hrm/shifts/assignments", body);
  return data.data;
}

export async function assignShiftRange(body: {
  employeeIds: string[];
  workShiftId: string;
  from: string;
  to: string;
  weekdays?: number[] | null;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<ShiftAssignmentDto[]>>(
    "/api/hrm/shifts/assignments/range",
    body,
  );
  return data.data;
}

export async function swapShifts(assignmentAId: string, assignmentBId: string) {
  await api.post("/api/hrm/shifts/assignments/swap", { assignmentAId, assignmentBId });
}

export async function cancelShiftAssignment(id: string) {
  await api.post(`/api/hrm/shifts/assignments/${id}/cancel`);
}

export async function copyShiftAssignments(body: {
  sourceFrom: string;
  sourceTo: string;
  targetStart: string;
  orgUnitId?: string | null;
}) {
  const { data } = await api.post<Envelope<{ copied: number }>>("/api/hrm/shifts/assignments/copy", body);
  return data.data;
}

export async function fetchShiftLocks() {
  const { data } = await api.get<Envelope<ShiftPeriodLockDto[]>>("/api/hrm/shifts/locks");
  return data.data;
}

export async function lockShiftPeriod(body: {
  orgUnitId: string;
  periodKey: string;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<ShiftPeriodLockDto>>("/api/hrm/shifts/locks", body);
  return data.data;
}

export async function exportShiftScheduleCsv(params?: {
  orgUnitId?: string;
  from?: string;
  to?: string;
}) {
  const { data } = await api.get<Blob>("/api/hrm/shifts/export", {
    params,
    responseType: "blob",
  });
  return data;
}

export type StaffTransferDto = {
  id: string;
  docNo: string;
  kind: string;
  employeeId?: string | null;
  employeeCode?: string | null;
  employeeName?: string | null;
  fromOrgUnitId: string;
  fromOrgUnitName: string;
  toOrgUnitId: string;
  toOrgUnitName: string;
  startDate: string;
  endDate?: string | null;
  reason: string;
  requestedHeadcount?: number | null;
  status: string;
  attendanceTagged: boolean;
  attendanceTag: string;
  plannedHours?: number | null;
  actualHours?: number | null;
  costRate?: number | null;
  estimatedCost?: number | null;
  requestedByUserId: string;
  requesterName: string;
  acknowledgedByUserId?: string | null;
  acknowledgerName?: string | null;
  acknowledgedAt?: string | null;
  sourceRequestId?: string | null;
  note?: string | null;
  createdAt: string;
};

export type TransferCostReportRowDto = {
  orgUnitId: string;
  orgUnitName: string;
  orderCount: number;
  plannedHours: number;
  actualHours: number;
  estimatedCost: number;
  actualCost: number;
};

export async function fetchTransfers(params?: {
  kind?: string;
  status?: string;
  orgUnitId?: string;
}) {
  const { data } = await api.get<Envelope<StaffTransferDto[]>>("/api/hrm/transfers", { params });
  return data.data;
}

export async function fetchMyTransfers() {
  const { data } = await api.get<Envelope<StaffTransferDto[]>>("/api/hrm/transfers/mine");
  return data.data;
}

export async function fetchTransferTracking() {
  const { data } = await api.get<Envelope<StaffTransferDto[]>>("/api/hrm/transfers/tracking");
  return data.data;
}

export async function fetchTransferCostReport(params?: { from?: string; to?: string }) {
  const { data } = await api.get<Envelope<TransferCostReportRowDto[]>>(
    "/api/hrm/transfers/cost-report",
    { params },
  );
  return data.data;
}

export async function createTransferRequest(body: {
  fromOrgUnitId: string;
  toOrgUnitId: string;
  startDate: string;
  endDate?: string | null;
  requestedHeadcount: number;
  reason: string;
  note?: string | null;
  submit: boolean;
}) {
  const { data } = await api.post<Envelope<StaffTransferDto>>("/api/hrm/transfers/requests", body);
  return data.data;
}

export async function createTransferOrder(body: {
  employeeId: string;
  fromOrgUnitId: string;
  toOrgUnitId: string;
  startDate: string;
  endDate?: string | null;
  reason: string;
  plannedHours?: number | null;
  costRate?: number | null;
  attendanceTagged: boolean;
  note?: string | null;
  issue: boolean;
  sourceRequestId?: string | null;
}) {
  const { data } = await api.post<Envelope<StaffTransferDto>>("/api/hrm/transfers/orders", body);
  return data.data;
}

export async function submitTransferRequest(id: string) {
  const { data } = await api.post<Envelope<StaffTransferDto>>(`/api/hrm/transfers/${id}/submit`);
  return data.data;
}

export async function approveTransferRequest(id: string) {
  const { data } = await api.post<Envelope<StaffTransferDto>>(`/api/hrm/transfers/${id}/approve`);
  return data.data;
}

export async function rejectTransferRequest(id: string) {
  const { data } = await api.post<Envelope<StaffTransferDto>>(`/api/hrm/transfers/${id}/reject`);
  return data.data;
}

export async function issueTransferOrder(id: string) {
  const { data } = await api.post<Envelope<StaffTransferDto>>(`/api/hrm/transfers/${id}/issue`);
  return data.data;
}

export async function acknowledgeTransfer(id: string) {
  const { data } = await api.post<Envelope<StaffTransferDto>>(`/api/hrm/transfers/${id}/acknowledge`);
  return data.data;
}

export async function activateTransfer(id: string) {
  const { data } = await api.post<Envelope<StaffTransferDto>>(`/api/hrm/transfers/${id}/activate`);
  return data.data;
}

export async function completeTransfer(id: string) {
  const { data } = await api.post<Envelope<StaffTransferDto>>(`/api/hrm/transfers/${id}/complete`);
  return data.data;
}

export async function cancelTransfer(id: string) {
  const { data } = await api.post<Envelope<StaffTransferDto>>(`/api/hrm/transfers/${id}/cancel`);
  return data.data;
}

export async function setTransferActualHours(id: string, actualHours: number) {
  const { data } = await api.post<Envelope<StaffTransferDto>>(
    `/api/hrm/transfers/${id}/actual-hours`,
    { actualHours },
  );
  return data.data;
}

export async function setTransferAttendanceTag(id: string, tagged: boolean) {
  const { data } = await api.post<Envelope<StaffTransferDto>>(
    `/api/hrm/transfers/${id}/attendance-tag`,
    null,
    { params: { tagged } },
  );
  return data.data;
}

export type AttendancePolicyDto = {
  enableFingerprint: boolean;
  enableApp: boolean;
  enableQr: boolean;
  enableGeoFence: boolean;
  lateGraceMinutes: number;
  lateDeductEveryMinutes: number;
  lateDeductWorkUnit: number;
  forgotCheckoutHours: number;
  adjustDeadlineDays: number;
  enableOt: boolean;
  otAfterMinutes: number;
  enableNightShiftRule: boolean;
  enableHolidayRule: boolean;
  defaultShiftStart: string;
  defaultShiftEnd: string;
};

export type AttendanceDeviceDto = {
  id: string;
  code: string;
  name: string;
  deviceType: string;
  orgUnitId?: string | null;
  orgUnitName?: string | null;
  serialNo?: string | null;
  isActive: boolean;
  note?: string | null;
};

export type AttendanceGeoFenceDto = {
  id: string;
  name: string;
  orgUnitId?: string | null;
  orgUnitName?: string | null;
  latitude: number;
  longitude: number;
  radiusMeters: number;
  isActive: boolean;
};

export type AttendanceRecordDto = {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  orgUnitId: string;
  orgUnitName: string;
  workDate: string;
  checkInAt?: string | null;
  checkOutAt?: string | null;
  checkInMethod?: string | null;
  checkOutMethod?: string | null;
  lateMinutes: number;
  deductedWorkUnit: number;
  otMinutes: number;
  workUnit: number;
  status: string;
  tag?: string | null;
  note?: string | null;
  isConfirmed: boolean;
};

export type AttendanceMissingAlertDto = {
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  orgUnitId: string;
  orgUnitName: string;
  workDate: string;
  alertType: string;
};

export type AttendanceAdjustDto = {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  workDate: string;
  requestedCheckInAt?: string | null;
  requestedCheckOutAt?: string | null;
  reason: string;
  evidenceStorageKey?: string | null;
  status: string;
  requestedByUserId: string;
  requesterName: string;
  createdAt: string;
};

export type AttendancePeriodLockDto = {
  id: string;
  periodKey: string;
  periodFrom: string;
  periodTo: string;
  isLocked: boolean;
  lockedByUserId: string;
  lockerName: string;
  lockedAt: string;
  note?: string | null;
};

export async function fetchAttendancePolicy() {
  const { data } = await api.get<Envelope<AttendancePolicyDto>>("/api/hrm/attendance/policy");
  return data.data;
}

export async function upsertAttendancePolicy(body: AttendancePolicyDto) {
  const { data } = await api.put<Envelope<AttendancePolicyDto>>("/api/hrm/attendance/policy", body);
  return data.data;
}

export async function fetchAttendanceDevices() {
  const { data } = await api.get<Envelope<AttendanceDeviceDto[]>>("/api/hrm/attendance/devices");
  return data.data;
}

export async function upsertAttendanceDevice(body: {
  id?: string | null;
  code: string;
  name: string;
  deviceType: string;
  orgUnitId?: string | null;
  serialNo?: string | null;
  isActive: boolean;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<AttendanceDeviceDto>>("/api/hrm/attendance/devices", body);
  return data.data;
}

export async function fetchAttendanceGeoFences() {
  const { data } = await api.get<Envelope<AttendanceGeoFenceDto[]>>("/api/hrm/attendance/geofences");
  return data.data;
}

export async function upsertAttendanceGeoFence(body: {
  id?: string | null;
  name: string;
  orgUnitId?: string | null;
  latitude: number;
  longitude: number;
  radiusMeters: number;
  isActive: boolean;
}) {
  const { data } = await api.post<Envelope<AttendanceGeoFenceDto>>(
    "/api/hrm/attendance/geofences",
    body,
  );
  return data.data;
}

export async function attendanceCheckIn(body: {
  method: string;
  deviceId?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<AttendanceRecordDto>>("/api/hrm/attendance/check-in", body);
  return data.data;
}

export async function attendanceCheckOut(body: {
  method: string;
  deviceId?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<AttendanceRecordDto>>("/api/hrm/attendance/check-out", body);
  return data.data;
}

export async function fetchMyAttendance(params?: { from?: string; to?: string }) {
  const { data } = await api.get<Envelope<AttendanceRecordDto[]>>("/api/hrm/attendance/mine", {
    params,
  });
  return data.data;
}

export async function fetchAttendanceBoard(params?: {
  orgUnitId?: string;
  from?: string;
  to?: string;
}) {
  const { data } = await api.get<Envelope<AttendanceRecordDto[]>>("/api/hrm/attendance/board", {
    params,
  });
  return data.data;
}

export async function fetchAttendanceAlerts(date?: string) {
  const { data } = await api.get<Envelope<AttendanceMissingAlertDto[]>>("/api/hrm/attendance/alerts", {
    params: { date },
  });
  return data.data;
}

export async function markAttendanceMissing(date: string) {
  const { data } = await api.post<Envelope<{ marked: number }>>("/api/hrm/attendance/mark-missing", null, {
    params: { date },
  });
  return data.data;
}

export async function syncAttendanceDevice(items: {
  employeeCode: string;
  punchedAt: string;
  punchType: string;
  deviceCode?: string | null;
}[]) {
  const { data } = await api.post<Envelope<{
    synced: number; skippedUnknownEmployee: number; skippedLocked: number;
    skippedDuplicate: number; skippedInvalidType: number; total: number;
  }>>("/api/hrm/attendance/sync-device", { items });
  return data.data;
}

export async function recalcAttendanceOt(from: string, to: string) {
  const { data } = await api.post<Envelope<{ recalculated: number }>>(
    "/api/hrm/attendance/recalc-ot",
    null,
    { params: { from, to } },
  );
  return data.data;
}

export async function fetchAttendanceAdjusts() {
  const { data } = await api.get<Envelope<AttendanceAdjustDto[]>>("/api/hrm/attendance/adjusts");
  return data.data;
}

export async function createAttendanceAdjust(body: {
  employeeId: string;
  workDate: string;
  requestedCheckInAt?: string | null;
  requestedCheckOutAt?: string | null;
  reason: string;
  evidenceStorageKey?: string | null;
  submit: boolean;
}) {
  const { data } = await api.post<Envelope<AttendanceAdjustDto>>("/api/hrm/attendance/adjusts", body);
  return data.data;
}

export async function approveAttendanceAdjust(id: string) {
  const { data } = await api.post<Envelope<AttendanceAdjustDto>>(
    `/api/hrm/attendance/adjusts/${id}/approve`,
  );
  return data.data;
}

export async function rejectAttendanceAdjust(id: string) {
  const { data } = await api.post<Envelope<AttendanceAdjustDto>>(
    `/api/hrm/attendance/adjusts/${id}/reject`,
  );
  return data.data;
}

export async function fetchAttendanceLocks() {
  const { data } = await api.get<Envelope<AttendancePeriodLockDto[]>>("/api/hrm/attendance/locks");
  return data.data;
}

export async function lockAttendancePeriod(periodKey: string, note?: string | null) {
  const { data } = await api.post<Envelope<AttendancePeriodLockDto>>("/api/hrm/attendance/locks", {
    periodKey,
    note,
  });
  return data.data;
}

export async function unlockAttendancePeriod(periodKey: string) {
  const { data } = await api.post<Envelope<AttendancePeriodLockDto>>(
    `/api/hrm/attendance/locks/${periodKey}/unlock`,
  );
  return data.data;
}

export async function confirmAttendanceRecord(id: string) {
  await api.post(`/api/hrm/attendance/records/${id}/confirm`);
}

/* —— Payroll / lương kỳ (UC_HRM_152+) —— */

export type SalaryGradeDto = {
  id: string;
  code: string;
  name: string;
  level: number;
  baseAmount: number;
  isActive: boolean;
  note?: string | null;
};

export type EmployeeSalaryDto = {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  salaryGradeId?: string | null;
  salaryGradeName?: string | null;
  baseSalary: number;
  hourlyRate?: number | null;
  dailyRate?: number | null;
  appliesToStatus?: string | null;
  effectiveFrom: string;
  effectiveTo?: string | null;
  isActive: boolean;
  note?: string | null;
};

export type AllowanceTypeDto = {
  id: string;
  code: string;
  name: string;
  defaultAmount: number;
  isTaxable: boolean;
  isActive: boolean;
};

export type AllowanceRuleDto = {
  id: string;
  allowanceTypeId: string;
  allowanceTypeName: string;
  shiftCode?: string | null;
  amount: number;
  isActive: boolean;
  note?: string | null;
};

export type PayrollPolicyDto = {
  socialInsuranceEmpRate: number;
  healthInsuranceEmpRate: number;
  unemploymentEmpRate: number;
  personalDeduction: number;
  flatTaxRate: number;
  standardWorkDays: number;
  otMultiplier: number;
};

export type PayrollPeriodDto = {
  id: string;
  periodKey: string;
  periodFrom: string;
  periodTo: string;
  status: string;
  note?: string | null;
  lineCount: number;
  totalNet: number;
  createdAt: string;
};

export type PayrollLineDto = {
  id: string;
  payrollPeriodId: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  orgUnitName: string;
  workUnits: number;
  otMinutes: number;
  baseSalary: number;
  attendancePay: number;
  otPay: number;
  allowanceTotal: number;
  bonus: number;
  deductionTotal: number;
  insuranceEmployee: number;
  tax: number;
  grossPay: number;
  netPay: number;
  isConfirmed: boolean;
  note?: string | null;
};

export type PayrollAdjustmentDto = {
  id: string;
  payrollPeriodId: string;
  employeeId: string;
  employeeName: string;
  kind: string;
  title: string;
  amount: number;
  note?: string | null;
};

export type PayrollCostByOrgDto = {
  orgUnitId: string;
  orgUnitName: string;
  headcount: number;
  gross: number;
  net: number;
  insurance: number;
};

export type PayrollCompareDto = {
  periodKey: string;
  totalGross: number;
  totalNet: number;
  totalInsurance: number;
  lineCount: number;
};

export async function fetchSalaryGrades() {
  const { data } = await api.get<Envelope<SalaryGradeDto[]>>("/api/hrm/payroll/grades");
  return data.data;
}

export async function upsertSalaryGrade(body: {
  id?: string | null;
  code: string;
  name: string;
  level: number;
  baseAmount: number;
  isActive: boolean;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<SalaryGradeDto>>("/api/hrm/payroll/grades", body);
  return data.data;
}

export async function fetchEmployeeSalaries(employeeId?: string) {
  const { data } = await api.get<Envelope<EmployeeSalaryDto[]>>("/api/hrm/payroll/employee-salaries", {
    params: { employeeId },
  });
  return data.data;
}

export async function upsertEmployeeSalary(body: {
  id?: string | null;
  employeeId: string;
  salaryGradeId?: string | null;
  baseSalary: number;
  hourlyRate?: number | null;
  dailyRate?: number | null;
  appliesToStatus?: string | null;
  effectiveFrom: string;
  effectiveTo?: string | null;
  isActive: boolean;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<EmployeeSalaryDto>>("/api/hrm/payroll/employee-salaries", body);
  return data.data;
}

export async function fetchAllowanceTypes() {
  const { data } = await api.get<Envelope<AllowanceTypeDto[]>>("/api/hrm/payroll/allowance-types");
  return data.data;
}

export async function upsertAllowanceType(body: {
  id?: string | null;
  code: string;
  name: string;
  defaultAmount: number;
  isTaxable: boolean;
  isActive: boolean;
}) {
  const { data } = await api.post<Envelope<AllowanceTypeDto>>("/api/hrm/payroll/allowance-types", body);
  return data.data;
}

export async function fetchAllowanceRules() {
  const { data } = await api.get<Envelope<AllowanceRuleDto[]>>("/api/hrm/payroll/allowance-rules");
  return data.data;
}

export async function upsertAllowanceRule(body: {
  id?: string | null;
  allowanceTypeId: string;
  shiftCode?: string | null;
  amount: number;
  isActive: boolean;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<AllowanceRuleDto>>("/api/hrm/payroll/allowance-rules", body);
  return data.data;
}

export async function fetchPayrollPolicy() {
  const { data } = await api.get<Envelope<PayrollPolicyDto>>("/api/hrm/payroll/policy");
  return data.data;
}

export async function upsertPayrollPolicy(body: PayrollPolicyDto) {
  const { data } = await api.put<Envelope<PayrollPolicyDto>>("/api/hrm/payroll/policy", body);
  return data.data;
}

export async function fetchPayrollPeriods() {
  const { data } = await api.get<Envelope<PayrollPeriodDto[]>>("/api/hrm/payroll/periods");
  return data.data;
}

export async function createPayrollPeriod(periodKey: string, note?: string | null) {
  const { data } = await api.post<Envelope<PayrollPeriodDto>>("/api/hrm/payroll/periods", {
    periodKey,
    note,
  });
  return data.data;
}

export async function calculatePayrollPeriod(id: string) {
  const { data } = await api.post<Envelope<PayrollPeriodDto>>(`/api/hrm/payroll/periods/${id}/calculate`);
  return data.data;
}

export async function confirmPayrollPeriod(id: string) {
  await api.post(`/api/hrm/payroll/periods/${id}/confirm`);
}

export async function lockPayrollPeriod(id: string) {
  await api.post(`/api/hrm/payroll/periods/${id}/lock`);
}

export async function fetchPayrollLines(periodId: string) {
  const { data } = await api.get<Envelope<PayrollLineDto[]>>(`/api/hrm/payroll/periods/${periodId}/lines`);
  return data.data;
}

export async function patchPayrollLine(
  id: string,
  body: { bonus?: number | null; deductionTotal?: number | null; allowanceTotal?: number | null; note?: string | null },
) {
  const { data } = await api.patch<Envelope<PayrollLineDto>>(`/api/hrm/payroll/lines/${id}`, body);
  return data.data;
}

export async function fetchMyPayslip(periodId?: string) {
  const { data } = await api.get<Envelope<PayrollLineDto[]>>("/api/hrm/payroll/mine", {
    params: { periodId },
  });
  return data.data;
}

export async function fetchPayrollAdjustments(periodId: string) {
  const { data } = await api.get<Envelope<PayrollAdjustmentDto[]>>(
    `/api/hrm/payroll/periods/${periodId}/adjustments`,
  );
  return data.data;
}

export async function addPayrollAdjustment(body: {
  payrollPeriodId: string;
  employeeId: string;
  kind: string;
  title: string;
  amount: number;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<PayrollAdjustmentDto>>("/api/hrm/payroll/adjustments", body);
  return data.data;
}

export async function fetchPayrollCostByOrg(periodId: string) {
  const { data } = await api.get<Envelope<PayrollCostByOrgDto[]>>(
    `/api/hrm/payroll/periods/${periodId}/cost-by-org`,
  );
  return data.data;
}

export async function fetchPayrollCompare(periodKey: string) {
  const { data } = await api.get<Envelope<PayrollCompareDto[]>>("/api/hrm/payroll/compare", {
    params: { periodKey },
  });
  return data.data;
}

export function payrollExportUrl(periodId: string) {
  return `/api/hrm/payroll/periods/${periodId}/export`;
}

export function payrollExportBankUrl(periodId: string) {
  return `/api/hrm/payroll/periods/${periodId}/export-bank`;
}

/* —— Rewards / Discipline + Offboarding (UC_HRM_139–151) —— */

export type RewardDisciplineDto = {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  kind: string;
  title: string;
  decisionDate: string;
  reason?: string | null;
  payrollImpactAmount: number;
  payrollImpactKind: string;
  decisionStorageKey?: string | null;
  status: string;
  appliedPayrollPeriodId?: string | null;
  note?: string | null;
  createdAt: string;
};

export type RewardDisciplineReportRowDto = { kind: string; count: number; totalImpact: number };

export type OffboardingSettingDto = {
  noticeDays: number;
  requireChecklistComplete: boolean;
  autoRevokeAccessOnComplete: boolean;
};

export type OffboardingChecklistItemDto = { key: string; label: string; done: boolean };

export type OffboardingCaseDto = {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  orgUnitName: string;
  requestDate: string;
  lastWorkingDay: string;
  reasonCode: string;
  reasonDetail?: string | null;
  status: string;
  noticeSatisfied: boolean;
  requiredNoticeDays: number;
  checklist: OffboardingChecklistItemDto[];
  accessRevoked: boolean;
  leaveDaysRemaining?: number | null;
  leaveSettlementAmount?: number | null;
  finalPayEstimate?: number | null;
  settlementNote?: string | null;
  interviewNotes?: string | null;
  rejectReason?: string | null;
  createdAt: string;
};

export type OffboardingReportRowDto = { reasonCode: string; count: number };

export async function fetchRewardDecisions(kind?: string) {
  const { data } = await api.get<Envelope<RewardDisciplineDto[]>>("/api/hrm/rewards", { params: { kind } });
  return data.data;
}

export async function createRewardDecision(body: {
  employeeId: string;
  kind: string;
  title: string;
  decisionDate: string;
  reason?: string | null;
  payrollImpactAmount: number;
  payrollImpactKind: string;
  decisionStorageKey?: string | null;
  note?: string | null;
}) {
  const { data } = await api.post<Envelope<RewardDisciplineDto>>("/api/hrm/rewards", body);
  return data.data;
}

export async function attachRewardDecision(id: string, decisionStorageKey: string) {
  const { data } = await api.post<Envelope<RewardDisciplineDto>>(`/api/hrm/rewards/${id}/attach`, {
    decisionStorageKey,
  });
  return data.data;
}

export async function applyRewardToPayroll(id: string, periodId?: string) {
  const { data } = await api.post<Envelope<RewardDisciplineDto>>(
    `/api/hrm/rewards/${id}/apply-payroll`,
    null,
    { params: { periodId } },
  );
  return data.data;
}

export async function fetchRewardReport(year?: number) {
  const { data } = await api.get<Envelope<RewardDisciplineReportRowDto[]>>("/api/hrm/rewards/report", {
    params: { year },
  });
  return data.data;
}

export async function fetchOffboardingSettings() {
  const { data } = await api.get<Envelope<OffboardingSettingDto>>("/api/hrm/offboarding/settings");
  return data.data;
}

export async function upsertOffboardingSettings(body: OffboardingSettingDto) {
  const { data } = await api.put<Envelope<OffboardingSettingDto>>("/api/hrm/offboarding/settings", body);
  return data.data;
}

export async function fetchOffboardingCases() {
  const { data } = await api.get<Envelope<OffboardingCaseDto[]>>("/api/hrm/offboarding");
  return data.data;
}

export async function createOffboardingCase(body: {
  employeeId: string;
  requestDate: string;
  lastWorkingDay: string;
  reasonCode: string;
  reasonDetail?: string | null;
}) {
  const { data } = await api.post<Envelope<OffboardingCaseDto>>("/api/hrm/offboarding", body);
  return data.data;
}

export async function submitOffboarding(id: string) {
  const { data } = await api.post<Envelope<OffboardingCaseDto>>(`/api/hrm/offboarding/${id}/submit`);
  return data.data;
}

export async function approveOffboarding(id: string) {
  const { data } = await api.post<Envelope<OffboardingCaseDto>>(`/api/hrm/offboarding/${id}/approve`);
  return data.data;
}

export async function rejectOffboarding(id: string, reason?: string | null) {
  const { data } = await api.post<Envelope<OffboardingCaseDto>>(`/api/hrm/offboarding/${id}/reject`, {
    reason,
  });
  return data.data;
}

export async function updateOffboardingChecklist(id: string, items: OffboardingChecklistItemDto[]) {
  const { data } = await api.put<Envelope<OffboardingCaseDto>>(`/api/hrm/offboarding/${id}/checklist`, {
    items,
  });
  return data.data;
}

export async function revokeOffboardingAccess(id: string) {
  const { data } = await api.post<Envelope<OffboardingCaseDto>>(`/api/hrm/offboarding/${id}/revoke-access`);
  return data.data;
}

export async function settleOffboarding(
  id: string,
  body: { leaveSettlementAmount?: number | null; finalPayEstimate?: number | null; settlementNote?: string | null },
) {
  const { data } = await api.post<Envelope<OffboardingCaseDto>>(`/api/hrm/offboarding/${id}/settle`, body);
  return data.data;
}

export async function saveOffboardingInterview(id: string, interviewNotes: string) {
  const { data } = await api.post<Envelope<OffboardingCaseDto>>(`/api/hrm/offboarding/${id}/interview`, {
    interviewNotes,
  });
  return data.data;
}

export async function completeOffboarding(id: string) {
  const { data } = await api.post<Envelope<OffboardingCaseDto>>(`/api/hrm/offboarding/${id}/complete`);
  return data.data;
}

export async function fetchOffboardingReport(year?: number) {
  const { data } = await api.get<Envelope<OffboardingReportRowDto[]>>("/api/hrm/offboarding/report", {
    params: { year },
  });
  return data.data;
}

/* —— Dashboard / reports HRM (UC_HRM_182–187) —— */

export type HrmDashboardBundleDto = {
  headcount: {
    totalActive: number;
    totalProbation: number;
    totalInactiveOrLeft: number;
    byStatus: { status: string; count: number }[];
    byOrg: { orgUnitId: string; orgUnitName: string; count: number }[];
    movements: { periodKey: string; hired: number; resigned: number; net: number }[];
  };
  attendance: {
    orgUnitId: string;
    orgUnitName: string;
    recordCount: number;
    workUnits: number;
    otMinutes: number;
    lateMinutes: number;
    lateCount: number;
  }[];
  recruitFunnel: { pipelineStatus: string; count: number }[];
  leaveSummary: {
    orgUnitId: string;
    orgUnitName: string;
    entitled: number;
    used: number;
    remaining: number;
    employeeCount: number;
  }[];
  cost: {
    periodId?: string | null;
    periodKey?: string | null;
    periodStatus?: string | null;
    totalGross: number;
    totalNet: number;
    totalInsurance: number;
    lineCount: number;
    byOrg: {
      orgUnitId: string;
      orgUnitName: string;
      headcount: number;
      gross: number;
      net: number;
      insurance: number;
    }[];
  };
  headcountVsPlan: HeadcountCompareRowDto[];
};

export async function fetchHrmDashboard(params?: {
  attFrom?: string;
  attTo?: string;
  leaveYear?: number;
  periodId?: string;
}) {
  const { data } = await api.get<Envelope<HrmDashboardBundleDto>>("/api/hrm/dashboard", { params });
  return data.data;
}
