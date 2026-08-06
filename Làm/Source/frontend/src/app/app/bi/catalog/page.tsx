"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchBiDashboardDetail,
  fetchBiDashboards,
  fetchBiDatasets,
  refreshBiDataset,
  upsertBiDashboard,
  upsertBiDataset,
  upsertBiWidget,
  type BiDashboardDetailDto,
  type BiDashboardDto,
  type BiDatasetDto,
} from "@/shared/api/bi-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function BiCatalogPage() {
  const { can } = usePermissions();
  const canRead = can("bi.catalog.read");
  const canManage = can("bi.catalog.manage");

  const [datasets, setDatasets] = useState<BiDatasetDto[]>([]);
  const [dashboards, setDashboards] = useState<BiDashboardDto[]>([]);
  const [selectedDashId, setSelectedDashId] = useState("");
  const [dashDetail, setDashDetail] = useState<BiDashboardDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [dsCode, setDsCode] = useState("SALES_FACT");
  const [dsName, setDsName] = useState("");
  const [dsMod, setDsMod] = useState("CRM");
  const [dbCode, setDbCode] = useState("EXEC");
  const [dbName, setDbName] = useState("Dashboard Ban lãnh đạo");
  const [dbType, setDbType] = useState("Executive");
  const [dbMod, setDbMod] = useState("FIN");
  const [wCode, setWCode] = useState("REV");
  const [wTitle, setWTitle] = useState("Doanh thu");
  const [wMetric, setWMetric] = useState("Revenue");
  const [wValue, setWValue] = useState("850000000");

  const load = useCallback(async () => {
    const [d, db] = await Promise.all([fetchBiDatasets(), fetchBiDashboards()]);
    setDatasets(d); setDashboards(db);
    if (!selectedDashId && db[0]) setSelectedDashId(db[0].id);
  }, [selectedDashId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedDashId || !canRead) return;
    fetchBiDashboardDetail(selectedDashId).then(setDashDetail).catch((e: Error) => setError(e.message));
  }, [selectedDashId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedDashId) setDashDetail(await fetchBiDashboardDetail(selectedDashId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem catalog BI.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Dataset / Dashboard</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Dataset · refresh · DB lãnh đạo/module · widget DT–LN (UC_BI_001–002, 006–008)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Catalog dataset</h2>
          {canManage && (
            <form className="mb-3 flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(() => upsertBiDataset({
                code: dsCode, name: dsName || dsCode, moduleCode: dsMod,
              }), "Đã lưu dataset");
            }}>
              <input className={field} placeholder="Mã" value={dsCode} onChange={(e) => setDsCode(e.target.value)} />
              <input className={field} placeholder="Tên" value={dsName} onChange={(e) => setDsName(e.target.value)} />
              <input className={field} placeholder="Module" value={dsMod} onChange={(e) => setDsMod(e.target.value)} />
              <button className={btn.primary} type="submit">Thêm</button>
            </form>
          )}
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th><th className={th}>Module</th>
                  <th className={th}>Rows</th><th className={th}>TT</th><th className={th}></th>
                </tr>
              </thead>
              <tbody>
                {datasets.map((d) => (
                  <tr key={d.id}>
                    <td className={td}>
                      <div>{d.code}</div>
                      <div className="text-xs text-[var(--muted)]">{d.name}</div>
                    </td>
                    <td className={td}>{d.moduleCode}</td>
                    <td className={td}>{d.rowCountEstimate.toLocaleString()}</td>
                    <td className={td}>
                      <span className={statusPill(d.status === "Ready" ? "success" : "warning")}>{d.status}</span>
                    </td>
                    <td className={td}>
                      {canManage && (
                        <button type="button" className={btn.ghost}
                          onClick={() => void run(() => refreshBiDataset(d.id, "Manual refresh"), "Đã refresh")}>
                          Refresh
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Dashboard & widget</h2>
          {canManage && (
            <form className="mb-3 flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(() => upsertBiDashboard({
                code: dbCode, name: dbName, dashboardType: dbType,
                moduleCode: dbType === "Module" ? dbMod : null,
              }), "Đã lưu dashboard");
            }}>
              <input className={field} placeholder="Mã DB" value={dbCode} onChange={(e) => setDbCode(e.target.value)} />
              <input className={field} placeholder="Tên" value={dbName} onChange={(e) => setDbName(e.target.value)} />
              <select className={field} value={dbType} onChange={(e) => setDbType(e.target.value)}>
                <option value="Executive">Executive</option>
                <option value="Module">Module</option>
              </select>
              {dbType === "Module" && (
                <input className={field} placeholder="Module" value={dbMod} onChange={(e) => setDbMod(e.target.value)} />
              )}
              <button className={btn.primary} type="submit">Thêm DB</button>
            </form>
          )}
          <div className="mb-2 flex flex-wrap gap-2">
            {dashboards.map((d) => (
              <button key={d.id} type="button"
                className={selectedDashId === d.id ? btn.primary : btn.ghost}
                onClick={() => setSelectedDashId(d.id)}>
                {d.code} ({d.widgetCount})
              </button>
            ))}
          </div>
          {dashDetail && (
            <div className="space-y-2 text-sm">
              <div>
                <b>{dashDetail.dashboard.name}</b> · {dashDetail.dashboard.dashboardType}
                {dashDetail.dashboard.moduleCode && ` · ${dashDetail.dashboard.moduleCode}`}
              </div>
              <div className="grid gap-2 sm:grid-cols-2">
                {dashDetail.widgets.map((w) => (
                  <div key={w.id} className="rounded-md border border-black/10 p-3">
                    <div className="text-xs text-[var(--muted)]">{w.metricKey} · {w.widgetType}</div>
                    <div className="font-medium">{w.title}</div>
                    <div className="text-lg tracking-tight">
                      {w.stubValue.toLocaleString()} <span className="text-xs">{w.unit}</span>
                    </div>
                  </div>
                ))}
              </div>
              {canManage && (
                <form className="flex flex-wrap gap-2 border-t border-black/10 pt-3" onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  void run(() => upsertBiWidget({
                    dashboardId: dashDetail.dashboard.id,
                    code: wCode, title: wTitle, widgetType: "Kpi",
                    metricKey: wMetric, stubValue: Number(wValue) || 0, unit: "VND",
                  }), "Đã thêm widget");
                }}>
                  <input className={field} placeholder="Mã" value={wCode} onChange={(e) => setWCode(e.target.value)} />
                  <input className={field} placeholder="Tiêu đề" value={wTitle} onChange={(e) => setWTitle(e.target.value)} />
                  <select className={field} value={wMetric} onChange={(e) => setWMetric(e.target.value)}>
                    <option value="Revenue">Revenue</option>
                    <option value="Profit">Profit</option>
                    <option value="Custom">Custom</option>
                  </select>
                  <input className={field} placeholder="Giá trị stub" value={wValue} onChange={(e) => setWValue(e.target.value)} />
                  <button className={btn.primary} type="submit">Thêm widget</button>
                </form>
              )}
            </div>
          )}
        </section>
      </div>
    </div>
  );
}
