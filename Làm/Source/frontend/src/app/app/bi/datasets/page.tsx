"use client";

import { useCallback, useEffect, useState } from "react";
import {
  fetchBiDatasets,
  refreshBiDataset,
  type BiDatasetDto,
} from "@/shared/api/bi-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function BiDatasetsPage() {
  const { can } = usePermissions();
  const canRead = can("bi.catalog.read") || can("bi.report.read");
  const canManage = can("bi.catalog.manage");

  const [datasets, setDatasets] = useState<BiDatasetDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [refreshingId, setRefreshingId] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      setError(null);
      const list = await fetchBiDatasets();
      setDatasets(list);
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

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 3000);
  }

  async function handleRefresh(datasetId: string) {
    try {
      setRefreshingId(datasetId);
      const updated = await refreshBiDataset(datasetId);
      await loadData();
      flash(`Đã làm mới dữ liệu cho Dataset: ${updated.name} (${updated.rowCountEstimate} bản ghi)`);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setRefreshingId(null);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem Danh mục Datasets BI.</div>;
  }

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Quản Lý Nguồn Dữ Liệu BI Lakehouse (UC_BI_001–002)</h1>
          <p className="mt-1 text-sm text-slate-500">
            Quản lý các nguồn dữ liệu tập trung từ FIN, POS, CRM, INV, MFG và kích hoạt làm mới (Refresh ETL) định kỳ.
          </p>
        </div>
        <button type="button" className={btn.ghost} onClick={() => void loadData()}>
          🔄 Tải lại danh sách
        </button>
      </div>

      {error && <div className="rounded-lg bg-red-50 p-4 text-sm font-medium text-red-800 border border-red-200">{error}</div>}
      {ok && <div className="rounded-lg bg-emerald-50 p-4 text-sm font-medium text-emerald-800 border border-emerald-200">{ok}</div>}

      {/* Datasets Master Table */}
      <section className={panel}>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-bold text-slate-900">Danh Sách Datasets Hiện Có</h2>
          <span className="text-xs font-semibold text-slate-500">{datasets.length} Datasets</span>
        </div>

        {loading ? (
          <div className="p-8 text-center text-sm text-slate-500">Đang tải danh mục Dataset BI...</div>
        ) : datasets.length === 0 ? (
          <div className="p-8 text-center text-sm text-slate-500">Chưa có Dataset nào được đăng ký.</div>
        ) : (
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã Dataset</th>
                  <th className={th}>Tên Nguồn Dữ Liệu</th>
                  <th className={th}>Module Nguồn</th>
                  <th className={th}>Số Dòng Ước Tính</th>
                  <th className={th}>Lần Làm Mới Cuối</th>
                  <th className={th}>Trạng Thái</th>
                  {canManage && <th className={th}>Thao Tác ETL</th>}
                </tr>
              </thead>
              <tbody>
                {datasets.map((d) => (
                  <tr key={d.id} className="hover:bg-slate-50">
                    <td className={`${td} font-bold text-indigo-900`}>{d.code}</td>
                    <td className={td}>
                      <div className="font-semibold text-slate-900">{d.name}</div>
                      <div className="text-xs text-slate-500">{d.description || "Nguồn dữ liệu phân tích tập trung"}</div>
                    </td>
                    <td className={td}>
                      <span className="rounded bg-indigo-50 px-2 py-0.5 text-xs font-bold text-indigo-700">
                        {d.moduleCode}
                      </span>
                    </td>
                    <td className={`${td} font-semibold text-slate-800`}>
                      {(d.rowCountEstimate ?? 0).toLocaleString("vi-VN")} dòng
                    </td>
                    <td className={td}>
                      <div className="text-xs text-slate-700">
                        {d.lastRefreshedAt ? new Date(d.lastRefreshedAt).toLocaleString("vi-VN") : "Chưa refresh"}
                      </div>
                      <div className="text-[11px] text-slate-400 italic">{d.lastRefreshNote || "Mặc định"}</div>
                    </td>
                    <td className={td}>
                      <span className={statusPill(d.status === "Active" ? "success" : "brand")}>{d.status}</span>
                    </td>
                    {canManage && (
                      <td className={td}>
                        <button
                          type="button"
                          disabled={refreshingId === d.id}
                          className={`${btn.primary} text-xs py-1 px-2.5`}
                          onClick={() => void handleRefresh(d.id)}
                        >
                          {refreshingId === d.id ? "⌛ Đang refresh..." : "🔄 Refresh ETL"}
                        </button>
                      </td>
                    )}
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
