"use client";

import { useCallback, useEffect, useState } from "react";
import {
  downloadBiRunExport,
  fetchBiReportRuns,
  type BiReportRunDto,
} from "@/shared/api/bi-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function BiExportPage() {
  const { can } = usePermissions();
  const canRead = can("bi.report.read");

  const [runs, setRuns] = useState<BiReportRunDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [downloadingId, setDownloadingId] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      setError(null);
      const list = await fetchBiReportRuns();
      setRuns(list);
    } catch (e) {
      setError((e as Error).message);
    }
  }, []);

  useEffect(() => {
    if (!canRead) {
      setLoading(false);
      return;
    }
    setLoading(true);
    loadData().finally(() => setLoading(false));
  }, [canRead, loadData]);

  async function handleDownload(run: BiReportRunDto) {
    try {
      setDownloadingId(run.id);
      const { fileName, contentType, content } = await downloadBiRunExport(run.id);
      const blob = new Blob([content], { type: contentType });
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = fileName;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setDownloadingId(null);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem Lịch sử xuất báo cáo BI.</div>;
  }

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Trung Tâm Xuất Báo Cáo & Tải File PDF / Excel (UC_BI_014–016)</h1>
          <p className="mt-1 text-sm text-slate-500">
            Xem nhật ký thực thi báo cáo, tải xuống kết quả dưới dạng CSV (UTF-8 BOM Excel) hoặc PDF (%PDF-1.4 chuẩn mực).
          </p>
        </div>
        <button type="button" className={btn.ghost} onClick={() => void loadData()}>
          🔄 Tải lại lịch sử
        </button>
      </div>

      {error && <div className="rounded-lg bg-red-50 p-4 text-sm font-medium text-red-800 border border-red-200">{error}</div>}

      {/* Report Runs History Table */}
      <section className={panel}>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-bold text-slate-900">Nhật Ký Thực Thi & Xuất File</h2>
          <span className="text-xs font-semibold text-slate-500">{runs.length} lượt chạy</span>
        </div>

        {loading ? (
          <div className="p-8 text-center text-sm text-slate-500">Đang tải nhật ký xuất báo cáo...</div>
        ) : runs.length === 0 ? (
          <div className="p-8 text-center text-sm text-slate-500">Chưa có lượt xuất báo cáo nào.</div>
        ) : (
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Báo Cáo</th>
                  <th className={th}>Thời Gian Thực Thi</th>
                  <th className={th}>Số Dòng Trả Về</th>
                  <th className={th}>Định Dạng Export</th>
                  <th className={th}>Tên File Đã Tạo</th>
                  <th className={th}>Ghi Chú Nguồn</th>
                  <th className={th}>Tải Xuống</th>
                </tr>
              </thead>
              <tbody>
                {runs.map((r) => (
                  <tr key={r.id} className="hover:bg-slate-50">
                    <td className={`${td} font-bold text-slate-900`}>
                      <div>{r.reportName || r.reportCode}</div>
                      <div className="text-xs text-slate-400 font-mono">{r.reportCode}</div>
                    </td>
                    <td className={td}>
                      {r.runAt ? new Date(r.runAt).toLocaleString("vi-VN") : "—"}
                    </td>
                    <td className={`${td} font-semibold text-indigo-900`}>
                      {r.rowCount.toLocaleString("vi-VN")} dòng
                    </td>
                    <td className={td}>
                      <span className={`rounded px-2 py-0.5 text-xs font-bold ${
                        r.exportFormat === "Excel"
                          ? "bg-emerald-100 text-emerald-800"
                          : r.exportFormat === "Pdf"
                          ? "bg-red-100 text-red-800"
                          : "bg-slate-100 text-slate-700"
                      }`}>
                        {r.exportFormat || "None"}
                      </span>
                    </td>
                    <td className={`${td} font-mono text-xs text-slate-700`}>
                      {r.exportFileName || "—"}
                    </td>
                    <td className={`${td} text-xs text-slate-500`}>
                      {r.note || "Nguồn dữ liệu thật"}
                    </td>
                    <td className={td}>
                      {r.exportFormat && r.exportFormat !== "None" ? (
                        <button
                          type="button"
                          disabled={downloadingId === r.id}
                          className={`${btn.primary} text-xs py-1 px-2.5`}
                          onClick={() => void handleDownload(r)}
                        >
                          {downloadingId === r.id ? "⌛ Đang tải..." : `📥 Tải ${r.exportFormat}`}
                        </button>
                      ) : (
                        <span className="text-xs text-slate-400">Không có file</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}
