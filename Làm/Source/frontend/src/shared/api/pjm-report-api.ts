import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type PjmDashboardDto = {
  activeCount: number; draftCount: number; closedCount: number;
  overdueProjectCount: number; overdueWbsCount: number; overdueMilestoneCount: number;
  avgActiveProgressPercent: number;
};
export type PjmPortfolioRowDto = {
  projectId: string; code: string; name: string; statusCode: string; pmName?: string | null;
  budget: number; startDate?: string | null; endDate?: string | null;
  progressPercent: number; health: string; wbsCount: number; overdueCount: number; milestoneCount: number;
};
export type PjmProgressHealthRowDto = {
  projectId: string; code: string; name: string; statusCode: string;
  progressPercent: number; health: string; openWbs: number; doneWbs: number;
  overdueWbs: number; overdueMilestones: number; endDate?: string | null; projectEndOverdue: boolean;
};
export type PjmOverdueRowDto = {
  projectId: string; projectCode: string; projectName: string;
  wbsItemId: string; wbsCode: string; wbsName: string;
  isMilestone: boolean; dueDate: string; percentComplete: number; assigneeName?: string | null;
};
export type PjmProfitRowDto = {
  projectId: string; code: string; name: string; statusCode: string;
  budget: number; actualCost: number; recognizedRevenue: number; margin: number; marginPct: number;
  budgetVariance: number; overBudget: boolean;
};

export async function fetchPjmDashboard() {
  const { data } = await api.get<Envelope<PjmDashboardDto>>("/api/pjm/reports/dashboard");
  return data.data;
}
export async function fetchPjmPortfolio() {
  const { data } = await api.get<Envelope<PjmPortfolioRowDto[]>>("/api/pjm/reports/portfolio");
  return data.data;
}
export async function fetchPjmProgressHealth() {
  const { data } = await api.get<Envelope<PjmProgressHealthRowDto[]>>("/api/pjm/reports/progress");
  return data.data;
}
export async function fetchPjmOverdue() {
  const { data } = await api.get<Envelope<PjmOverdueRowDto[]>>("/api/pjm/reports/overdue");
  return data.data;
}
export async function fetchPjmProfit() {
  const { data } = await api.get<Envelope<PjmProfitRowDto[]>>("/api/pjm/reports/profit");
  return data.data;
}
export async function downloadPjmReportCsv(report: string) {
  const { data } = await api.get<Blob>("/api/pjm/reports/export.csv", { params: { report }, responseType: "blob" });
  const url = URL.createObjectURL(data);
  const a = document.createElement("a");
  a.href = url;
  a.download = `pjm-${report}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}
