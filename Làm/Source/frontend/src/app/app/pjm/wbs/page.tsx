"use client";

import { useCallback, useEffect, useState } from "react";
import { fetchPjmProjects, fetchPjmWbsItems, type PjmProjectDto, type PjmWbsItemDto } from "@/shared/api/pjm-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PjmWbsPage() {
  const { can } = usePermissions();
  const canRead = can("pjm.project.read");

  const [projects, setProjects] = useState<PjmProjectDto[]>([]);
  const [projectId, setProjectId] = useState("");
  const [wbsItems, setWbsItems] = useState<PjmWbsItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      setError(null);
      const projList = await fetchPjmProjects();
      setProjects(projList);
      const pid = projectId || projList[0]?.id || "";
      if (!projectId && pid) setProjectId(pid);
      if (!pid) { setWbsItems([]); return; }
      const items = await fetchPjmWbsItems(pid);
      setWbsItems(items);
    } catch (e) { setError((e as Error).message); }
  }, [projectId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    loadData().finally(() => setLoading(false));
  }, [canRead, loadData]);

  if (!canRead) return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền truy cập WBS Dự Án.</div>;

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Cấu Trúc Phân Chia Công Việc Dự Án (WBS) (UC_PJM_011–013)</h1>
          <p className="text-sm text-slate-500">Quản lý các hạng mục công việc, mốc tiến độ (Milestone) và phân công phụ trách.</p>
        </div>
        <select className={`${field} font-semibold text-slate-900 min-w-[260px]`} value={projectId} onChange={(e) => setProjectId(e.target.value)}>
          <option value="">— Chọn dự án —</option>
          {projects.map((p) => <option key={p.id} value={p.id}>{p.name} ({p.code})</option>)}
        </select>
      </div>

      {error && <div className="rounded-lg bg-red-50 p-4 text-sm font-medium text-red-800 border border-red-200">{error}</div>}

      <section className={panel}>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-bold text-slate-900">Danh Sách Hạng Mục WBS</h2>
          <span className="text-xs font-semibold text-slate-500">{wbsItems.length} hạng mục</span>
        </div>

        {loading ? (
          <div className="p-6 text-center text-sm text-slate-500">Đang tải WBS...</div>
        ) : wbsItems.length === 0 ? (
          <div className="p-6 text-center text-sm text-slate-500">Chưa có WBS cho dự án này.</div>
        ) : (
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã WBS</th>
                  <th className={th}>Tên Hạng Mục Công Việc</th>
                  <th className={th}>Tiến Độ (%)</th>
                  <th className={th}>Trọng Số (%)</th>
                  <th className={th}>Trạng Thái</th>
                </tr>
              </thead>
              <tbody>
                {wbsItems.map((w) => (
                  <tr key={w.id} className="hover:bg-slate-50">
                    <td className={`${td} font-bold text-slate-900`}>{w.code}</td>
                    <td className={`${td} font-semibold text-slate-800`}>{w.name}</td>
                    <td className={td}>
                      <div className="flex items-center gap-2">
                        <span className="font-bold text-indigo-700">{w.progressPercent}%</span>
                        <div className="h-2 w-16 bg-slate-200 rounded-full overflow-hidden">
                          <div className="h-full bg-indigo-600" style={{ width: `${w.progressPercent}%` }} />
                        </div>
                      </div>
                    </td>
                    <td className={`${td} font-medium text-slate-700`}>{w.weightPercent}%</td>
                    <td className={td}><span className={statusPill(w.status === "Done" ? "success" : "brand")}>{w.status}</span></td>
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
