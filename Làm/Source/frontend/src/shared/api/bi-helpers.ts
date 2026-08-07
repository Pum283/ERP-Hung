/** Pure helpers — BI refresh / widget / run / export (UC_BI_002/008/014/016). */

export type BiRefreshLike = {
  rowCountEstimate: number;
  lastRefreshNote?: string | null;
  status: string;
};

/** Thông điệp sau refresh dataset — hiển thị số dòng nguồn thật. */
export function formatDatasetRefreshFlash(ds: BiRefreshLike): string {
  const note = (ds.lastRefreshNote ?? "").trim();
  const rows = Number.isFinite(ds.rowCountEstimate) ? ds.rowCountEstimate : 0;
  if (note) return `Refresh OK · ${rows.toLocaleString()} dòng · ${note}`;
  return `Refresh OK · ${rows.toLocaleString()} dòng · ${ds.status}`;
}

/** Metric Revenue/Profit lấy số live từ FIN; Custom dùng stubValue cấu hình. */
export function isLiveBiMetric(metricKey: string): boolean {
  return metricKey === "Revenue" || metricKey === "Profit";
}

export function widgetValueHint(metricKey: string): string {
  return isLiveBiMetric(metricKey) ? "Live FIN" : "Custom";
}

/** Filter JSON gửi BE khi chạy BC (from/to ISO date). */
export function buildBiReportFilterJson(from: string, to: string): string {
  return JSON.stringify({
    from: from.trim() || null,
    to: to.trim() || null,
  });
}

/** Chỉ tải file khi lần chạy có Export Excel/Pdf. */
export function canDownloadBiExport(exportFormat: string | null | undefined): boolean {
  return exportFormat === "Excel" || exportFormat === "Pdf";
}

/** Tên file tải về — ưu tiên ExportFileName từ BE. */
export function biExportDownloadName(
  exportFileName: string | null | undefined,
  reportCode: string,
  exportFormat: string,
): string {
  if (exportFileName?.trim()) return exportFileName.trim();
  if (exportFormat === "Excel") return `${reportCode || "report"}.csv`;
  if (exportFormat === "Pdf") return `${reportCode || "report"}.txt`;
  return `${reportCode || "report"}.bin`;
}

/** Nhãn nút xuất — không còn “stub”. */
export function biExportActionLabel(format: "Excel" | "Pdf"): string {
  return format === "Excel" ? "Xuất Excel (CSV)" : "Xuất PDF (text)";
}
