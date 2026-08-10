import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type BiDatasetDto = {
  id: string; code: string; name: string; moduleCode: string; description?: string | null;
  status: string; lastRefreshedAt?: string | null; lastRefreshNote?: string | null; rowCountEstimate: number;
};
export type BiDatasetRefreshDto = {
  id: string; datasetId: string; startedAt: string; finishedAt?: string | null;
  status: string; rowsAffected: number; note?: string | null;
};
export type BiReportDto = {
  id: string; code: string; name: string; moduleCode: string; datasetId?: string | null; datasetName?: string | null;
  description?: string | null; filterSchemaJson?: string | null; status: string;
  requirePermission: boolean; permissionCount: number;
};
export type BiReportPermissionDto = {
  id: string; reportId: string; principalType: string; principalCode: string; accessLevel: string;
};
export type BiDashboardDto = {
  id: string; code: string; name: string; dashboardType: string; moduleCode?: string | null;
  status: string; note?: string | null; sortOrder: number; widgetCount: number;
};
export type BiWidgetDto = {
  id: string; dashboardId: string; code: string; title: string; widgetType: string;
  metricKey: string; stubValue: number; unit?: string | null; sortOrder: number; status: string;
};
export type BiDashboardDetailDto = { dashboard: BiDashboardDto; widgets: BiWidgetDto[] };
export type BiReportRunDto = {
  id: string; reportId: string; reportCode?: string | null; reportName?: string | null;
  runAt: string; status: string; rowCount: number; exportFormat: string;
  exportFileName?: string | null; filterJson?: string | null; resultPreviewJson?: string | null; note?: string | null;
};

export async function fetchBiDatasets(moduleCode?: string) {
  const { data } = await api.get<Envelope<BiDatasetDto[]>>("/api/bi/datasets", { params: { moduleCode } });
  return data.data;
}
export async function upsertBiDataset(body: {
  id?: string | null; code: string; name: string; moduleCode: string; description?: string | null; status?: string;
}) {
  const { data } = await api.post<Envelope<BiDatasetDto>>("/api/bi/datasets", body);
  return data.data;
}
export async function refreshBiDataset(id: string, note?: string) {
  const { data } = await api.post<Envelope<BiDatasetDto>>(`/api/bi/datasets/${id}/refresh`, { note });
  return data.data;
}
export async function fetchBiRefreshes(id: string) {
  const { data } = await api.get<Envelope<BiDatasetRefreshDto[]>>(`/api/bi/datasets/${id}/refreshes`);
  return data.data;
}
export async function fetchBiReports(moduleCode?: string) {
  const { data } = await api.get<Envelope<BiReportDto[]>>("/api/bi/reports", { params: { moduleCode } });
  return data.data;
}
export async function upsertBiReport(body: {
  id?: string | null; code: string; name: string; moduleCode: string; datasetId?: string | null;
  description?: string | null; filterSchemaJson?: string | null; status?: string; requirePermission?: boolean;
}) {
  const { data } = await api.post<Envelope<BiReportDto>>("/api/bi/reports", body);
  return data.data;
}
export async function fetchBiPermissions(reportId: string) {
  const { data } = await api.get<Envelope<BiReportPermissionDto[]>>(`/api/bi/reports/${reportId}/permissions`);
  return data.data;
}
export async function upsertBiPermission(body: {
  id?: string | null; reportId: string; principalType: string; principalCode: string; accessLevel: string;
}) {
  const { data } = await api.post<Envelope<BiReportPermissionDto>>("/api/bi/reports/permissions", body);
  return data.data;
}
export async function runBiReport(id: string, body?: { filterJson?: string; exportFormat?: string }) {
  const { data } = await api.post<Envelope<BiReportRunDto>>(`/api/bi/reports/${id}/run`, body ?? {});
  return data.data;
}
export async function fetchBiRuns(reportId?: string) {
  const { data } = await api.get<Envelope<BiReportRunDto[]>>("/api/bi/reports/runs", { params: { reportId } });
  return data.data;
}
export const fetchBiReportRuns = fetchBiRuns;

/** UC_BI_016 — tải nội dung xuất thật (CSV / text) của lần chạy. */
export async function downloadBiRunExport(runId: string): Promise<Blob> {
  const { data } = await api.get<Blob>(`/api/bi/reports/runs/${runId}/export`, { responseType: "blob" });
  return data;
}
export async function fetchBiDashboards() {
  const { data } = await api.get<Envelope<BiDashboardDto[]>>("/api/bi/dashboards");
  return data.data;
}
export async function fetchBiDashboardDetail(id: string) {
  const { data } = await api.get<Envelope<BiDashboardDetailDto>>(`/api/bi/dashboards/${id}`);
  return data.data;
}
export async function upsertBiDashboard(body: {
  id?: string | null; code: string; name: string; dashboardType: string; moduleCode?: string | null;
  status?: string; note?: string | null; sortOrder?: number;
}) {
  const { data } = await api.post<Envelope<BiDashboardDto>>("/api/bi/dashboards", body);
  return data.data;
}
export async function upsertBiWidget(body: {
  id?: string | null; dashboardId: string; code: string; title: string; widgetType: string;
  metricKey: string; stubValue?: number; unit?: string | null; sortOrder?: number; status?: string;
}) {
  const { data } = await api.post<Envelope<BiWidgetDto>>("/api/bi/dashboards/widgets", body);
  return data.data;
}

export type BiKpiTargetDto = {
  id: string; code: string; name: string; moduleCode: string; metricKey: string; periodKey: string;
  periodFrom: string; periodTo: string; targetValue: number; actualStubValue: number;
  unit?: string | null; status: string; note?: string | null;
  variance: number; variancePercent: number;
};
export type BiAlertThresholdDto = {
  id: string; code: string; name: string; metricKey: string; kpiTargetId?: string | null;
  kpiTargetCode?: string | null; operator: string; thresholdValue: number;
  severity: string; status: string; note?: string | null;
};
export type BiPeriodCompareDto = {
  metricKey: string; currentPeriodKey: string; currentActual: number;
  priorPeriodKey?: string | null; priorActual?: number | null;
  periodDelta?: number | null; periodDeltaPercent?: number | null;
  targetValue?: number | null; vsTargetDelta?: number | null; vsTargetPercent?: number | null;
};
export type BiTargetVsActualRowDto = {
  targetId: string; code: string; name: string; moduleCode: string; metricKey: string; periodKey: string;
  targetValue: number; actualValue: number; variance: number; variancePercent: number;
  unit?: string | null; breached: boolean; breachSeverity?: string | null; breachNote?: string | null;
};

export async function fetchBiKpiTargets(params?: { periodKey?: string; moduleCode?: string }) {
  const { data } = await api.get<Envelope<BiKpiTargetDto[]>>("/api/bi/kpi/targets", { params });
  return data.data;
}
export async function upsertBiKpiTarget(body: {
  id?: string | null; code: string; name: string; moduleCode: string; metricKey: string; periodKey: string;
  periodFrom: string; periodTo: string; targetValue: number; actualStubValue?: number;
  unit?: string | null; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<BiKpiTargetDto>>("/api/bi/kpi/targets", body);
  return data.data;
}
export async function fetchBiAlertThresholds() {
  const { data } = await api.get<Envelope<BiAlertThresholdDto[]>>("/api/bi/kpi/thresholds");
  return data.data;
}
export async function upsertBiAlertThreshold(body: {
  id?: string | null; code: string; name: string; metricKey: string; kpiTargetId?: string | null;
  operator: string; thresholdValue: number; severity?: string; status?: string; note?: string | null;
}) {
  const { data } = await api.post<Envelope<BiAlertThresholdDto>>("/api/bi/kpi/thresholds", body);
  return data.data;
}
export async function compareBiPeriods(body: {
  metricKey: string; currentPeriodKey: string; priorPeriodKey?: string | null; kpiTargetId?: string | null;
}) {
  const { data } = await api.post<Envelope<BiPeriodCompareDto>>("/api/bi/kpi/compare", body);
  return data.data;
}
export async function fetchBiTargetBoard(periodKey: string, moduleCode?: string) {
  const { data } = await api.get<Envelope<BiTargetVsActualRowDto[]>>("/api/bi/kpi/board", {
    params: { periodKey, moduleCode },
  });
  return data.data;
}
