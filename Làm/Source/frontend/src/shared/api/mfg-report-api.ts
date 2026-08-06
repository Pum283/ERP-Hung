import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type MfgWoProgressRowDto = {
  workOrderId: string; code: string; itemCode: string; itemName: string;
  workshopCode?: string | null; workshopName?: string | null; status: string;
  qtyPlanned: number; qtyFgReceived: number; qtyScrap: number;
  progressPercent: number; releasedAt?: string | null; closedAt?: string | null;
};
export type MfgOutputRowDto = {
  day: string; shiftLabel: string; workshopId?: string | null;
  workshopCode?: string | null; workshopName?: string | null;
  qtyFg: number; receiptCount: number; workOrderCount: number;
};
export type MfgMaterialVarianceRowDto = {
  workOrderId: string; workOrderCode: string; status: string;
  itemId: string; itemCode: string; itemName: string;
  qtyPlanned: number; qtyActual: number; qtyVariance: number; variancePercent: number;
};
export type MfgDashboardDto = {
  draftCount: number; releasedCount: number; inProgressCount: number; pausedCount: number;
  completedCount: number; closedCount: number;
  qtyPlannedOpen: number; qtyFgPeriod: number; qtyScrapPeriod: number;
  openWoCount: number; varianceOverCount: number;
};

export async function fetchMfgWoProgress(params?: { status?: string; workshopId?: string }) {
  const { data } = await api.get<Envelope<MfgWoProgressRowDto[]>>("/api/mfg/reports/wo-progress", { params });
  return data.data;
}
export async function fetchMfgOutput(params: { from: string; to: string; workshopId?: string }) {
  const { data } = await api.get<Envelope<MfgOutputRowDto[]>>("/api/mfg/reports/output", { params });
  return data.data;
}
export async function fetchMfgMaterialVariance(params?: { workOrderId?: string }) {
  const { data } = await api.get<Envelope<MfgMaterialVarianceRowDto[]>>("/api/mfg/reports/material-variance", { params });
  return data.data;
}
export async function fetchMfgDashboard(params?: { from?: string; to?: string }) {
  const { data } = await api.get<Envelope<MfgDashboardDto>>("/api/mfg/reports/dashboard", { params });
  return data.data;
}
export async function downloadMfgReportCsv(params: {
  report: string; status?: string; workshopId?: string; workOrderId?: string; from?: string; to?: string;
}) {
  const { data } = await api.get<Blob>("/api/mfg/reports/export.csv", { params, responseType: "blob" });
  const url = URL.createObjectURL(data);
  const a = document.createElement("a");
  a.href = url;
  a.download = `mfg-${params.report}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}
