"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchBiDatasets,
  fetchBiPermissions,
  fetchBiReports,
  fetchBiRuns,
  runBiReport,
  upsertBiPermission,
  upsertBiReport,
  type BiDatasetDto,
  type BiReportDto,
  type BiReportPermissionDto,
  type BiReportRunDto,
} from "@/shared/api/bi-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function BiReportsPage() {
  const { can } = usePermissions();
  const canRead = can("bi.report.read");
  const canRun = can("bi.report.run");
  const canManage = can("bi.catalog.manage");

  const [reports, setReports] = useState<BiReportDto[]>([]);
  const [datasets, setDatasets] = useState<BiDatasetDto[]>([]);
  const [runs, setRuns] = useState<BiReportRunDto[]>([]);
  const [perms, setPerms] = useState<BiReportPermissionDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [lastRun, setLastRun] = useState<BiReportRunDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [code, setCode] = useState("RPT-SALES");
  const [name, setName] = useState("");
  const [mod, setMod] = useState("CRM");
  const [datasetId, setDatasetId] = useState("");
  const [filterFrom, setFilterFrom] = useState("");
  const [filterTo, setFilterTo] = useState("");
  const [principal, setPrincipal] = useState("ADMIN");
  const [access, setAccess] = useState("View");

  const load = useCallback(async () => {
    const [r, d, runsList] = await Promise.all([
      fetchBiReports(),
      fetchBiDatasets().catch(() => [] as BiDatasetDto[]),
      fetchBiRuns(),
    ]);
    setReports(r); setDatasets(d); setRuns(runsList);
    if (!selectedId && r[0]) setSelectedId(r[0].id);
    if (!datasetId && d[0]) setDatasetId(d[0].id);
  }, [selectedId, datasetId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) { setPerms([]); return; }
    fetchBiPermissions(selectedId).then(setPerms).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) setPerms(await fetchBiPermissions(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem thư viện báo cáo.</div>;
  }

  const selected = reports.find((r) => r.id === selectedId) ?? null;

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Thư viện báo cáo</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Danh mục BC · quyền xem · chạy lọc · xuất Excel/PDF stub (UC_BI_003, 013–014, 016)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh mục báo cáo</h2>
          {canManage && (
            <form className="mb-3 flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(() => upsertBiReport({
                code, name: name || code, moduleCode: mod, datasetId: datasetId || null,
              }), "Đã lưu BC");
            }}>
              <input className={field} placeholder="Mã" value={code} onChange={(e) => setCode(e.target.value)} />
              <input className={field} placeholder="Tên" value={name} onChange={(e) => setName(e.target.value)} />
              <input className={field} placeholder="Module" value={mod} onChange={(e) => setMod(e.target.value)} />
              <select className={field} value={datasetId} onChange={(e) => setDatasetId(e.target.value)}>
                <option value="">— Dataset —</option>
                {datasets.map((d) => <option key={d.id} value={d.id}>{d.code}</option>)}
              </select>
              <button className={btn.primary} type="submit">Thêm BC</button>
            </form>
          )}
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Mã</th><th className={th}>Module</th><th className={th}>Quyền</th></tr></thead>
              <tbody>
                {reports.map((r) => (
                  <tr key={r.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(r.id)}>
                    <td className={td}>
                      <div className="font-medium">{r.code}</div>
                      <div className="text-xs text-[var(--muted)]">{r.name}</div>
                    </td>
                    <td className={td}>{r.moduleCode}</td>
                    <td className={td}>{r.permissionCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chạy / quyền / xuất</h2>
          {selected ? (
            <div className="space-y-3 text-sm">
              <div>
                <b>{selected.code}</b> — {selected.name}
                <div className="text-xs text-[var(--muted)]">
                  Dataset: {selected.datasetName ?? "—"} ·{" "}
                  <span className={statusPill(selected.status === "Active" ? "success" : "muted")}>{selected.status}</span>
                </div>
              </div>

              {canRun && (
                <div className="flex flex-wrap gap-2">
                  <input className={field} type="date" value={filterFrom} onChange={(e) => setFilterFrom(e.target.value)} />
                  <input className={field} type="date" value={filterTo} onChange={(e) => setFilterTo(e.target.value)} />
                  <button type="button" className={btn.primary} onClick={() => {
                    const filterJson = JSON.stringify({ from: filterFrom || null, to: filterTo || null });
                    void run(async () => {
                      const res = await runBiReport(selected.id, { filterJson, exportFormat: "None" });
                      setLastRun(res);
                    }, "Đã chạy BC");
                  }}>
                    Chạy
                  </button>
                  <button type="button" className={btn.ghost} onClick={() => {
                    const filterJson = JSON.stringify({ from: filterFrom || null, to: filterTo || null });
                    void run(async () => {
                      const res = await runBiReport(selected.id, { filterJson, exportFormat: "Excel" });
                      setLastRun(res);
                    }, "Đã xuất Excel stub");
                  }}>
                    Excel
                  </button>
                  <button type="button" className={btn.ghost} onClick={() => {
                    const filterJson = JSON.stringify({ from: filterFrom || null, to: filterTo || null });
                    void run(async () => {
                      const res = await runBiReport(selected.id, { filterJson, exportFormat: "Pdf" });
                      setLastRun(res);
                    }, "Đã xuất PDF stub");
                  }}>
                    PDF
                  </button>
                </div>
              )}

              {lastRun && lastRun.reportId === selected.id && (
                <div className="rounded-md border border-black/10 p-3 text-xs">
                  <div>Run: {new Date(lastRun.runAt).toLocaleString()} · {lastRun.rowCount} dòng · {lastRun.exportFormat}</div>
                  {lastRun.exportFileName && <div>File: {lastRun.exportFileName}</div>}
                  {lastRun.resultPreviewJson && <pre className="mt-1 overflow-auto">{lastRun.resultPreviewJson}</pre>}
                </div>
              )}

              {canManage && (
                <form className="flex flex-wrap gap-2 border-t border-black/10 pt-3" onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  void run(() => upsertBiPermission({
                    reportId: selected.id, principalType: "Role",
                    principalCode: principal, accessLevel: access,
                  }), "Đã gán quyền");
                }}>
                  <input className={field} placeholder="Role code" value={principal}
                    onChange={(e) => setPrincipal(e.target.value)} />
                  <select className={field} value={access} onChange={(e) => setAccess(e.target.value)}>
                    <option value="View">View</option>
                    <option value="Run">Run</option>
                    <option value="Export">Export</option>
                  </select>
                  <button className={btn.primary} type="submit">Gán quyền</button>
                </form>
              )}
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead><tr><th className={th}>Principal</th><th className={th}>Level</th></tr></thead>
                  <tbody>
                    {perms.map((p) => (
                      <tr key={p.id}>
                        <td className={td}>{p.principalType}:{p.principalCode}</td>
                        <td className={td}>{p.accessLevel}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          ) : (
            <p className="text-sm text-[var(--muted)]">Chọn một báo cáo.</p>
          )}
        </section>

        <section className={`${panel} xl:col-span-2`}>
          <h2 className="mb-3 text-sm font-semibold">Lịch sử chạy</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Thời điểm</th><th className={th}>BC</th>
                  <th className={th}>Dòng</th><th className={th}>Xuất</th><th className={th}>TT</th>
                </tr>
              </thead>
              <tbody>
                {runs.map((r) => (
                  <tr key={r.id}>
                    <td className={td}>{new Date(r.runAt).toLocaleString()}</td>
                    <td className={td}>{r.reportCode}</td>
                    <td className={td}>{r.rowCount}</td>
                    <td className={td}>{r.exportFormat}{r.exportFileName ? ` · ${r.exportFileName}` : ""}</td>
                    <td className={td}>
                      <span className={statusPill(r.status === "Succeeded" ? "success" : "danger")}>{r.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      </div>
    </div>
  );
}
