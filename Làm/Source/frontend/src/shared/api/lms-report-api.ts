import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type LmsInstructorDto = {
  id: string; code: string; displayName: string;
  employeeId?: string | null; employeeCode?: string | null; employeeName?: string | null;
  userId?: string | null; title?: string | null; specialty?: string | null; bio?: string | null;
  email?: string | null; phone?: string | null; status: string; roleGranted: boolean; classCount: number;
};

export type LmsLearnerRowDto = {
  source: string; classId?: string | null; classCode?: string | null; className?: string | null;
  courseId?: string | null; courseCode?: string | null; courseName?: string | null;
  employeeId?: string | null; userId?: string | null; learnerCode: string; learnerName: string;
  orgUnitName?: string | null; status: string; enrolledAt: string;
  progressPercent: number; presentSessions?: number | null; totalSessions?: number | null;
};

export type LmsDashboardDto = {
  courseCount: number; publishedCourseCount: number;
  openClassCount: number; closedClassCount: number;
  offlineEnrollmentCount: number; offlineCompletedCount: number;
  onlineEnrollmentCount: number; onlineCompletedCount: number;
  activeCertificateCount: number; instructorCount: number;
  avgOnlineProgressPercent: number; examPassRatePercent: number;
};

export type LmsCompletionByOrgRowDto = {
  orgUnitId?: string | null; orgUnitCode: string; orgUnitName: string;
  offlineTotal: number; offlineCompleted: number;
  onlineTotal: number; onlineCompleted: number; completionRatePercent: number;
};

export async function fetchLmsInstructors() {
  const { data } = await api.get<Envelope<LmsInstructorDto[]>>("/api/lms/instructors");
  return data.data;
}

export async function upsertLmsInstructor(body: {
  id?: string; code: string; displayName: string; employeeId?: string | null;
  userId?: string | null; title?: string; specialty?: string; bio?: string;
  email?: string; phone?: string; status?: string; grantInstructorRole?: boolean;
}) {
  const { data } = await api.post<Envelope<LmsInstructorDto>>("/api/lms/instructors", body);
  return data.data;
}

export async function setLmsInstructorStatus(id: string, status: string) {
  const { data } = await api.post<Envelope<LmsInstructorDto>>(`/api/lms/instructors/${id}/status`, { status });
  return data.data;
}

export async function grantLmsInstructorRole(id: string) {
  const { data } = await api.post<Envelope<{ granted: boolean }>>(`/api/lms/instructors/${id}/grant-role`);
  return data.data;
}

export async function fetchLmsDashboard() {
  const { data } = await api.get<Envelope<LmsDashboardDto>>("/api/lms/reports/dashboard");
  return data.data;
}

export async function fetchLmsCompletionByOrg() {
  const { data } = await api.get<Envelope<LmsCompletionByOrgRowDto[]>>("/api/lms/reports/by-org");
  return data.data;
}

export async function fetchLmsLearners(params?: { classId?: string; courseId?: string; instructorId?: string }) {
  const { data } = await api.get<Envelope<LmsLearnerRowDto[]>>("/api/lms/reports/learners", { params });
  return data.data;
}

export async function downloadLmsReportCsv(params: {
  report: string; classId?: string; courseId?: string; instructorId?: string;
}) {
  const { data } = await api.get<Blob>("/api/lms/reports/export.csv", { params, responseType: "blob" });
  const url = URL.createObjectURL(data);
  const a = document.createElement("a");
  a.href = url;
  a.download = `lms-${params.report}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}
