"use client";

import { useCallback, useEffect, useState } from "react";
import { fetchMfgWorkshops, upsertMfgWorkshop, type MfgWorkshopDto } from "@/shared/api/mfg-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function MfgWorkshopsPage() {
  const { can } = usePermissions();
  const canRead = can("mfg.catalog.read");
  const canManage = can("mfg.catalog.manage");

  const [workshops, setWorkshops] = useState<MfgWorkshopDto[]>([]);
  const [code, setCode] = useState("WS-01");
  const [name, setName] = useState("Xưởng Lắp Ráp Trung Tâm");
  const [capacity, setCapacity] = useState("500");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      setError(null);
      const list = await fetchMfgWorkshops();
      setWorkshops(list);
    } catch (e) {
      setError((e as Error).message);
    }
  }, []);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    loadData().finally(() => setLoading(false));
  }, [canRead, loadData]);

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    try {
      await upsertMfgWorkshop({ code, name, capacityPerDay: Number(capacity) || 0 });
      await loadData();
      setOk("Đã tạo xưởng sản xuất mới!");
      setTimeout(() => setOk(null), 3000);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền truy cập Xưởng Sản Xuất.</div>;

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Quản Lý Xưởng & Dây Chuyền Sản Xuất (UC_MFG_003–005)</h1>
          <p className="text-sm text-slate-500">Quản lý năng lực sản xuất theo ca/ngày và phân bổ chuyền máy.</p>
        </div>
        <button type="button" className={btn.ghost} onClick={() => void loadData()}>🔄 Tải lại</button>
      </div>

      {error && <div className="rounded-lg bg-red-50 p-4 text-sm font-medium text-red-800 border border-red-200">{error}</div>}
      {ok && <div className="rounded-lg bg-emerald-50 p-4 text-sm font-medium text-emerald-800 border border-emerald-200">{ok}</div>}

      <div className="grid gap-6 xl:grid-cols-3">
        <section className={`${panel} xl:col-span-2`}>
          <h2 className="text-lg font-bold text-slate-900 mb-4">Danh Sách Xưởng Sản Xuất</h2>
          {loading ? (
            <div className="p-6 text-center text-sm text-slate-500">Đang tải xưởng sản xuất...</div>
          ) : (
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã Xưởng</th>
                    <th className={th}>Tên Xưởng</th>
                    <th className={th}>Năng Lực (SP/Ngày)</th>
                    <th className={th}>Trạng Thái</th>
                  </tr>
                </thead>
                <tbody>
                  {workshops.map((w) => (
                    <tr key={w.id} className="hover:bg-slate-50">
                      <td className={`${td} font-bold text-slate-900`}>{w.code}</td>
                      <td className={`${td} font-semibold text-slate-800`}>{w.name}</td>
                      <td className={`${td} font-bold text-indigo-900`}>{(w.capacityPerDay ?? 0).toLocaleString("vi-VN")} SP</td>
                      <td className={td}><span className={statusPill(w.status === "Active" ? "success" : "brand")}>{w.status}</span></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        {canManage && (
          <section className={panel}>
            <h2 className="text-base font-bold text-slate-900 border-b pb-2 mb-4">➕ Thêm Xưởng Mới</h2>
            <form onSubmit={handleCreate} className="space-y-4">
              <div>
                <label className="text-xs font-semibold text-slate-700">Mã xưởng (*)</label>
                <input className={field} value={code} onChange={(e) => setCode(e.target.value)} required />
              </div>
              <div>
                <label className="text-xs font-semibold text-slate-700">Tên xưởng (*)</label>
                <input className={field} value={name} onChange={(e) => setName(e.target.value)} required />
              </div>
              <div>
                <label className="text-xs font-semibold text-slate-700">Năng lực sản xuất (SP/Ngày)</label>
                <input className={field} type="number" value={capacity} onChange={(e) => setCapacity(e.target.value)} required />
              </div>
              <button type="submit" className={`${btn.primary} w-full justify-center`}>🚀 Lưu Xưởng Sản Xuất</button>
            </form>
          </section>
        )}
      </div>
    </div>
  );
}
