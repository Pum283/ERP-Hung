import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type FsmDashboardDto = {
  openCount: number; assignedCount: number; inProgressCount: number; escalatedCount: number;
  resolvedCount: number; closedCount: number; overdueOpenCount: number;
  closedTodayCount: number; slaHitRatePercent: number; appointmentTodayCount: number;
};
export type FsmSlaComplianceRowDto = {
  priority: string; openCount: number; overdueOpenCount: number;
  closedCount: number; slaMetCount: number; slaMissCount: number; slaHitRatePercent: number;
};
export type FsmTechProductivityRowDto = {
  techUserId?: string | null; techName: string;
  assignedCount: number; resolvedCount: number; closedCount: number;
  onSlaCount: number; onSlaPercent: number; avgResolveHours: number;
};
export type FsmPartCostRowDto = {
  partId: string; partCode: string; partName: string;
  qty: number; amount: number; ticketCount: number;
};
export type FsmPartCostSummaryDto = {
  totalQty: number; totalAmount: number; lineCount: number; ticketCount: number;
  byPart: FsmPartCostRowDto[];
};

export async function fetchFsmDashboard() {
  const { data } = await api.get<Envelope<FsmDashboardDto>>("/api/fsm/reports/dashboard");
  return data.data;
}
export async function fetchFsmSlaCompliance() {
  const { data } = await api.get<Envelope<FsmSlaComplianceRowDto[]>>("/api/fsm/reports/sla");
  return data.data;
}
export async function fetchFsmTechProductivity() {
  const { data } = await api.get<Envelope<FsmTechProductivityRowDto[]>>("/api/fsm/reports/productivity");
  return data.data;
}
export async function fetchFsmPartCost() {
  const { data } = await api.get<Envelope<FsmPartCostSummaryDto>>("/api/fsm/reports/parts");
  return data.data;
}
export async function downloadFsmReportCsv(report: string) {
  const { data } = await api.get<Blob>("/api/fsm/reports/export.csv", { params: { report }, responseType: "blob" });
  const url = URL.createObjectURL(data);
  const a = document.createElement("a");
  a.href = url;
  a.download = `fsm-${report}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}
