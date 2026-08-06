"use client";

import { useCallback, useEffect, useState } from "react";
import { fetchMfgWorkshops, type MfgWorkshopDto } from "@/shared/api/mfg-api";
import {
  downloadMfgReportCsv,
  fetchMfgDashboard,
  fetchMfgMaterialVariance,
  fetchMfgOutput,
  fetchMfgWoProgress,
  type MfgDashboardDto,
  type MfgMaterialVarianceRowDto,
  type MfgOutputRowDto,
  type MfgWoProgressRowDto,
} from "@/shared/api/mfg-report-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, tableWrap, td, th } from "@/shared/ui/field";

function qty(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 2 });
}
function isoDay(d: Date) {
  return d.toISOString().slice(0, 10);
}

type Tab = "dashboard" | "progress" | "output" | "variance";

export default function MfgReportsPage() {
  const { can } = usePermissions();
  const canRead = can("mfg.wo.read");

  const [tab, setTab] = useState<Tab>("dashboard");
  const [workshops, setWorkshops] = useState<MfgWorkshopDto[]>([]);
  const [workshopId, setWorkshopId] = useState("");
  const [status, setStatus] = useState("");
  const [from, setFrom] = useState(() => {
    const d = new Date(); d.setDate(d.getDate() - 30); return isoDay(d);
  });
  const [to, setTo] = useState(() => isoDay(new Date()));
  const [dashboard, setDashboard] = useState<MfgDashboardDto | null>(null);
  const [progress, setProgress] = useState<MfgWoProgressRowDto[]>([]);
  const [output, setOutput] = useState<MfgOutputRowDto[]>([]);
  const [variance, setVariance] = useState<MfgMaterialVarianceRowDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const range = useCallback(() => ({
    from: new Date(from + "T00:00:00").toISOString(),
    to: new Date(to + "T23:59:59").toISOString(),
  }), [from, to]);

  const load = useCallback(async () => {
    const r = range();
    const wh = workshopId ? { workshopId } : {};
    if (tab === "dashboard") setDashboard(await fetchMfgDashboard(r));
    else if (tab === "progress") setProgress(await fetchMfgWoProgress({ ...wh, ...(status ? { status } : {}) }));
    else if (tab === "output") setOutput(await fetchMfgOutput({ ...r, ...wh }));
    else setVariance(await fetchMfgMaterialVariance());
  }, [tab, range, workshopId, status]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    Promise.all([fetchMfgWorkshops().catch(() => [] as MfgWorkshopDto[]), load()])
      .then(([w]) => setWorkshops(w))
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [canRead, load]);

  async function exportCsv() {
    try {
      setError(null);
      const r = range();
      const report =
        tab === "progress" ? "wo-progress"
          : tab === "output" ? "output"
            : tab === "variance" ? "material-variance" : "dashboard";
      await downloadMfgReportCsv({
        report,
        ...(workshopId ? { workshopId } : {}),
        ...(status && tab === "progress" ? { status } : {}),
        ...(tab === "output" || tab === "dashboard" ? r : {}),
      });
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem báo cáo sản xuất.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Báo cáo sản xuất</h1>
          <p className="text-sm text-[var(--muted)]">UC_MFG_041–043 · 045–046 · tiến độ lệnh · sản lượng · variance NVL · dashboard · CSV. (Ca SX = Cap-1 Should sau)</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {([
            ["dashboard", "Dashboard"],
            ["progress", "Tiến độ lệnh"],
            ["output", "Sản lượng"],
            ["variance", "Variance NVL"],
          ] as [Tab, string][]).map(([k, label]) => (
            <button key={k} type="button" className={tab === k ? btn.primary : btn.ghost} onClick={() => setTab(k)}>
              {label}
            </button>
          ))}
          <button type="button" className={btn.soft} onClick={() => void exportCsv()}>Xuất CSV</button>
        </div>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}

      <div className={`${panel} flex flex-wrap gap-3`}>
        {(tab === "dashboard" || tab === "output") && (
          <>
            <label className={field.label}>Từ<input className={field.input} type="date" value={from} onChange={(e) => setFrom(e.target.value)} /></label>
            <label className={field.label}>Đến<input className={field.input} type="date" value={to} onChange={(e) => setTo(e.target.value)} /></label>
          </>
        )}
        {(tab === "progress" || tab === "output") && (
          <label className={field.label}>
            Xưởng
            <select className={field.input} value={workshopId} onChange={(e) => setWorkshopId(e.target.value)}>
              <option value="">Tất cả</option>
              {workshops.map((w) => <option key={w.id} value={w.id}>{w.code} · {w.name}</option>)}
            </select>
          </label>
        )}
        {tab === "progress" && (
          <label className={field.label}>
            Trạng thái
            <select className={field.input} value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="">Tất cả</option>
              {["Draft", "Approved", "Released", "MaterialsIssued", "Paused", "Completed", "Closed"].map((s) => (
                <option key={s} value={s}>{s}</option>
              ))}
            </select>
          </label>
        )}
        <button type="button" className={btn.primary} disabled={loading} onClick={() => void load().catch((e: Error) => setError(e.message))}>
          {loading ? "Đang tải…" : "Làm mới"}
        </button>
      </div>

      {tab === "dashboard" && dashboard && (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {[
            ["Draft", String(dashboard.draftCount)],
            ["Released/Approved", String(dashboard.releasedCount)],
            ["Đang SX", String(dashboard.inProgressCount)],
            ["Paused", String(dashboard.pausedCount)],
            ["Completed", String(dashboard.completedCount)],
            ["Closed", String(dashboard.closedCount)],
            ["SL kế hoạch mở", qty(dashboard.qtyPlannedOpen)],
            ["TP kỳ", qty(dashboard.qtyFgPeriod)],
            ["Phế kỳ", qty(dashboard.qtyScrapPeriod)],
            ["Lệnh mở", String(dashboard.openWoCount)],
            ["Dòng NVL vượt ĐM", String(dashboard.varianceOverCount)],
          ].map(([label, val]) => (
            <div key={label} className={panel}>
              <div className="text-xs text-[var(--muted)]">{label}</div>
              <div className="mt-1 text-lg font-semibold">{val}</div>
            </div>
          ))}
        </div>
      )}

      <div className={tableWrap}>
        {tab === "progress" && (
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Lệnh</th><th className={th}>SP</th><th className={th}>Xưởng</th>
                <th className={th}>TT</th><th className={th}>KH</th><th className={th}>TP</th>
                <th className={th}>Phế</th><th className={th}>%</th>
              </tr>
            </thead>
            <tbody>
              {progress.map((r) => (
                <tr key={r.workOrderId}>
                  <td className={td}>{r.code}</td>
                  <td className={td}>{r.itemCode} · {r.itemName}</td>
                  <td className={td}>{r.workshopCode ?? "—"}</td>
                  <td className={td}>{r.status}</td>
                  <td className={td}>{qty(r.qtyPlanned)}</td>
                  <td className={td}>{qty(r.qtyFgReceived)}</td>
                  <td className={td}>{qty(r.qtyScrap)}</td>
                  <td className={td}>{r.progressPercent}%</td>
                </tr>
              ))}
              {!loading && progress.length === 0 && <tr><td className={td} colSpan={8}>Không có lệnh.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "output" && (
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Ngày</th><th className={th}>Ca</th><th className={th}>Xưởng</th>
                <th className={th}>SL TP</th><th className={th}>Phiếu</th><th className={th}>Lệnh</th>
              </tr>
            </thead>
            <tbody>
              {output.map((r, i) => (
                <tr key={`${r.day}-${r.workshopId}-${i}`}>
                  <td className={td}>{r.day}</td>
                  <td className={td}>{r.shiftLabel}</td>
                  <td className={td}>{r.workshopCode ? `${r.workshopCode} · ${r.workshopName}` : "—"}</td>
                  <td className={td}>{qty(r.qtyFg)}</td>
                  <td className={td}>{r.receiptCount}</td>
                  <td className={td}>{r.workOrderCount}</td>
                </tr>
              ))}
              {!loading && output.length === 0 && <tr><td className={td} colSpan={6}>Không có nhập TP trong kỳ.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "variance" && (
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Lệnh</th><th className={th}>TT</th><th className={th}>NVL</th>
                <th className={th}>Định mức</th><th className={th}>Thực xuất</th>
                <th className={th}>Chênh</th><th className={th}>%</th>
              </tr>
            </thead>
            <tbody>
              {variance.map((r, i) => (
                <tr key={`${r.workOrderId}-${r.itemId}-${i}`}>
                  <td className={td}>{r.workOrderCode}</td>
                  <td className={td}>{r.status}</td>
                  <td className={td}>{r.itemCode} · {r.itemName}</td>
                  <td className={td}>{qty(r.qtyPlanned)}</td>
                  <td className={td}>{qty(r.qtyActual)}</td>
                  <td className={td}>{qty(r.qtyVariance)}</td>
                  <td className={td}>{r.variancePercent}%</td>
                </tr>
              ))}
              {!loading && variance.length === 0 && <tr><td className={td} colSpan={7}>Không có variance.</td></tr>}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
