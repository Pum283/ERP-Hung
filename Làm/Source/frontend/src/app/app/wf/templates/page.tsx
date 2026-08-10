"use client";

import { useCallback, useEffect, useState } from "react";
import { fetchWfWorks, type WfWorkDto } from "@/shared/api/wf-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function WfTemplatesPage() {
  const { can } = usePermissions();
  const canRead = can("wf.work.read");

  const [works, setWorks] = useState<WfWorkDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      setError(null);
      const list = await fetchWfWorks();
      setWorks(list);
    } catch (e) { setError((e as Error).message); }
  }, []);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    loadData().finally(() => setLoading(false));
  }, [canRead, loadData]);

  if (!canRead) return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền truy cập Mẫu Quy Trình Phê Duyệt.</div>;

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Mẫu Quy Trình Duyệt & Luồng Phê Duyệt Workflow (UC_WF_001–005)</h1>
          <p className="text-sm text-slate-500">Định nghĩa ma trận các bước phê duyệt cho đơn xin nghỉ, đề xuất mua sắm và thanh toán.</p>
        </div>
        <button type="button" className={btn.ghost} onClick={() => void loadData()}>🔄 Tải lại</button>
      </div>

      {error && <div className="rounded-lg bg-red-50 p-4 text-sm font-medium text-red-800 border border-red-200">{error}</div>}

      <section className={panel}>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-bold text-slate-900">Danh Sách Luồng Quy Trình Duyệt</h2>
          <span className="text-xs font-semibold text-slate-500">{works.length} luồng</span>
        </div>

        {loading ? (
          <div className="p-6 text-center text-sm text-slate-500">Đang tải luồng quy trình...</div>
        ) : (
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã Luồng WF</th>
                  <th className={th}>Tên Quy Trình Phê Duyệt</th>
                  <th className={th}>Loại Công Việc</th>
                  <th className={th}>Trạng Thái</th>
                </tr>
              </thead>
              <tbody>
                {works.map((w) => (
                  <tr key={w.id} className="hover:bg-slate-50">
                    <td className={`${td} font-bold text-slate-900`}>{w.code}</td>
                    <td className={`${td} font-semibold text-slate-800`}>{w.title}</td>
                    <td className={td}>
                      <span className="rounded bg-indigo-50 px-2 py-0.5 text-xs font-bold text-indigo-700">{w.workType || "APPROVAL"}</span>
                    </td>
                    <td className={td}><span className={statusPill(w.status === "Completed" ? "success" : "brand")}>{w.status}</span></td>
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
