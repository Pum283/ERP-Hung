"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  compareBiPeriods,
  fetchBiAlertThresholds,
  fetchBiKpiTargets,
  fetchBiTargetBoard,
  upsertBiAlertThreshold,
  upsertBiKpiTarget,
  type BiAlertThresholdDto,
  type BiKpiTargetDto,
  type BiPeriodCompareDto,
  type BiTargetVsActualRowDto,
} from "@/shared/api/bi-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

function monthKey(d = new Date()) {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}`;
}

type Tab = "board" | "targets" | "thresholds" | "compare";

export default function BiKpiPage() {
  const { can } = usePermissions();
  const canRead = can("bi.catalog.read") || can("bi.report.read");
  const canManage = can("bi.catalog.manage");
  const canCompare = can("bi.report.read");

  const [tab, setTab] = useState<Tab>("board");
  const [periodKey, setPeriodKey] = useState(monthKey());
  const [targets, setTargets] = useState<BiKpiTargetDto[]>([]);
  const [thresholds, setThresholds] = useState<BiAlertThresholdDto[]>([]);
  const [board, setBoard] = useState<BiTargetVsActualRowDto[]>([]);
  const [compare, setCompare] = useState<BiPeriodCompareDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [tCode, setTCode] = useState("REV-M");
  const [tName, setTName] = useState("Doanh thu tháng");
  const [tModule, setTModule] = useState("FIN");
  const [tMetric, setTMetric] = useState("Revenue");
  const [tTarget, setTTarget] = useState("100000000");
  const [tActual, setTActual] = useState("85000000");
  const [tFrom, setTFrom] = useState(`${new Date().getFullYear()}-${String(new Date().getMonth() + 1).padStart(2, "0")}-01`);
  const [tTo, setTTo] = useState(`${new Date().getFullYear()}-${String(new Date().getMonth() + 1).padStart(2, "0")}-28`);

  const [thCode, setThCode] = useState("REV-LOW");
  const [thName, setThName] = useState("DT dưới ngưỡng");
  const [thMetric, setThMetric] = useState("Revenue");
  const [thOp, setThOp] = useState("Lt");
  const [thValue, setThValue] = useState("90000000");
  const [thSev, setThSev] = useState("Warn");

  const [cMetric, setCMetric] = useState("Revenue");
  const [cCurrent, setCCurrent] = useState(monthKey());
  const [cPrior, setCPrior] = useState(() => {
    const d = new Date(); d.setMonth(d.getMonth() - 1);
    return monthKey(d);
  });

  const load = useCallback(async () => {
    const [t, th] = await Promise.all([
      fetchBiKpiTargets(periodKey ? { periodKey } : undefined),
      fetchBiAlertThresholds(),
    ]);
    setTargets(t);
    setThresholds(th);
    if (canCompare && periodKey) {
      setBoard(await fetchBiTargetBoard(periodKey).catch(() => []));
    }
  }, [periodKey, canCompare]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  function onTarget(e: FormEvent) {
    e.preventDefault();
    void run(() => upsertBiKpiTarget({
      code: tCode, name: tName, moduleCode: tModule, metricKey: tMetric, periodKey,
      periodFrom: tFrom, periodTo: tTo,
      targetValue: Number(tTarget) || 0, actualStubValue: Number(tActual) || 0,
    }), "Đã lưu KPI");
  }

  function onThreshold(e: FormEvent) {
    e.preventDefault();
    void run(() => upsertBiAlertThreshold({
      code: thCode, name: thName, metricKey: thMetric,
      operator: thOp, thresholdValue: Number(thValue) || 0, severity: thSev,
    }), "Đã lưu ngưỡng");
  }

  async function onCompare(e: FormEvent) {
    e.preventDefault();
    try {
      setCompare(await compareBiPeriods({
        metricKey: cMetric, currentPeriodKey: cCurrent, priorPeriodKey: cPrior || null,
      }));
      flash("Đã so sánh kỳ");
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem KPI BI.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">KPI · Target vs Actual</h1>
          <p className="text-sm text-[var(--muted)]">UC_BI_018 · 019 · 021 · mục tiêu · ngưỡng · so sánh kỳ.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {([
            ["board", "Bảng theo dõi"],
            ["targets", "Mục tiêu"],
            ["thresholds", "Ngưỡng"],
            ["compare", "So sánh kỳ"],
          ] as [Tab, string][]).map(([k, label]) => (
            <button key={k} type="button" className={tab === k ? btn.primary : btn.ghost} onClick={() => setTab(k)}>
              {label}
            </button>
          ))}
        </div>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}

      <label className={`${field.label} max-w-xs`}>
        PeriodKey
        <input className={field.input} value={periodKey} onChange={(e) => setPeriodKey(e.target.value)} placeholder="2026-08" />
      </label>

      {loading ? (
        <p className="text-sm text-[var(--muted)]">Đang tải…</p>
      ) : tab === "board" ? (
        <div className={tableWrap}>
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>KPI</th>
                <th className={th}>Metric</th>
                <th className={th}>Target</th>
                <th className={th}>Actual</th>
                <th className={th}>Δ</th>
                <th className={th}>Δ%</th>
                <th className={th}>Cảnh báo</th>
              </tr>
            </thead>
            <tbody>
              {board.length === 0 ? (
                <tr><td className={td} colSpan={7}>Chưa có KPI kỳ này — chọn kỳ khác hoặc tạo ở tab Mục tiêu.</td></tr>
              ) : board.map((r) => {
                const attain = r.targetValue > 0 ? Math.round((r.actualValue / r.targetValue) * 100) : 0;
                const isPass = r.actualValue >= r.targetValue;
                return (
                  <tr key={r.targetId} className="hover:bg-slate-50/80">
                    <td className={`${td} font-medium`}>
                      <div className="font-semibold text-slate-900">{r.name}</div>
                      <div className="text-xs text-slate-500">{r.code} · Module {r.moduleCode}</div>
                    </td>
                    <td className={td}><span className="rounded bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-700">{r.metricKey}</span></td>
                    <td className={`${td} font-semibold text-slate-800`}>{money(r.targetValue)} ₫</td>
                    <td className={`${td} font-bold ${isPass ? "text-emerald-700" : "text-amber-700"}`}>{money(r.actualValue)} ₫</td>
                    <td className={`${td} ${r.variance >= 0 ? "text-emerald-600 font-medium" : "text-red-600 font-medium"}`}>
                      {r.variance >= 0 ? "+" : ""}{money(r.variance)} ₫
                    </td>
                    <td className={td}>
                      <div className="flex items-center gap-2">
                        <span className={`font-semibold ${r.variancePercent >= 0 ? "text-emerald-600" : "text-red-600"}`}>
                          {r.variancePercent >= 0 ? "+" : ""}{r.variancePercent}%
                        </span>
                        <div className="h-2 w-16 rounded-full bg-slate-200 overflow-hidden">
                          <div className={`h-full ${isPass ? "bg-emerald-500" : "bg-amber-500"}`} style={{ width: `${Math.min(attain, 100)}%` }} />
                        </div>
                      </div>
                    </td>
                    <td className={td}>
                      {r.breached ? (
                        <span className="inline-flex items-center gap-1 rounded bg-red-100 px-2 py-0.5 text-xs font-semibold text-red-700">
                          ⚠️ {r.breachSeverity} · {r.breachNote}
                        </span>
                      ) : (
                        <span className="inline-flex items-center gap-1 rounded bg-emerald-100 px-2 py-0.5 text-xs font-semibold text-emerald-700">
                          ✓ Đạt chỉ tiêu ({attain}%)
                        </span>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      ) : tab === "targets" ? (
        <div className="space-y-4">
          {canManage && (
            <form className={`${panel} grid gap-3 md:grid-cols-3`} onSubmit={onTarget}>
              <label className={field.label}>Mã<input className={field.input} value={tCode} onChange={(e) => setTCode(e.target.value)} /></label>
              <label className={field.label}>Tên<input className={field.input} value={tName} onChange={(e) => setTName(e.target.value)} /></label>
              <label className={field.label}>Module<input className={field.input} value={tModule} onChange={(e) => setTModule(e.target.value)} /></label>
              <label className={field.label}>
                Metric
                <select className={field.input} value={tMetric} onChange={(e) => setTMetric(e.target.value)}>
                  <option>Revenue</option><option>Profit</option><option>Custom</option>
                </select>
              </label>
              <label className={field.label}>Target<input className={field.input} value={tTarget} onChange={(e) => setTTarget(e.target.value)} /></label>
              <label className={field.label}>Actual (tùy chỉnh)<input className={field.input} value={tActual} onChange={(e) => setTActual(e.target.value)} /></label>
              <label className={field.label}>Từ<input className={field.input} type="date" value={tFrom} onChange={(e) => setTFrom(e.target.value)} /></label>
              <label className={field.label}>Đến<input className={field.input} type="date" value={tTo} onChange={(e) => setTTo(e.target.value)} /></label>
              <div className="flex items-end"><button type="submit" className={btn.primary}>Lưu mục tiêu</button></div>
            </form>
          )}
          <div className={tableWrap}>
            <table className="min-w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th><th className={th}>Kỳ</th><th className={th}>Metric</th>
                  <th className={th}>Target</th><th className={th}>Actual</th><th className={th}>Δ%</th>
                </tr>
              </thead>
              <tbody>
                {targets.map((t) => (
                  <tr key={t.id}>
                    <td className={td}>{t.code}</td>
                    <td className={td}>{t.periodKey}</td>
                    <td className={td}>{t.metricKey}</td>
                    <td className={td}>{money(t.targetValue)}</td>
                    <td className={td}>{money(t.actualStubValue)}</td>
                    <td className={td}>{t.variancePercent}%</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      ) : tab === "thresholds" ? (
        <div className="space-y-4">
          {canManage && (
            <form className={`${panel} grid gap-3 md:grid-cols-3`} onSubmit={onThreshold}>
              <label className={field.label}>Mã<input className={field.input} value={thCode} onChange={(e) => setThCode(e.target.value)} /></label>
              <label className={field.label}>Tên<input className={field.input} value={thName} onChange={(e) => setThName(e.target.value)} /></label>
              <label className={field.label}>
                Metric
                <select className={field.input} value={thMetric} onChange={(e) => setThMetric(e.target.value)}>
                  <option>Revenue</option><option>Profit</option><option>Custom</option>
                </select>
              </label>
              <label className={field.label}>
                Operator
                <select className={field.input} value={thOp} onChange={(e) => setThOp(e.target.value)}>
                  <option value="Lt">Lt</option><option value="Lte">Lte</option>
                  <option value="Gt">Gt</option><option value="Gte">Gte</option>
                </select>
              </label>
              <label className={field.label}>Ngưỡng<input className={field.input} value={thValue} onChange={(e) => setThValue(e.target.value)} /></label>
              <label className={field.label}>
                Severity
                <select className={field.input} value={thSev} onChange={(e) => setThSev(e.target.value)}>
                  <option>Info</option><option>Warn</option><option>Critical</option>
                </select>
              </label>
              <div className="flex items-end"><button type="submit" className={btn.primary}>Lưu ngưỡng</button></div>
            </form>
          )}
          <div className={tableWrap}>
            <table className="min-w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th><th className={th}>Metric</th><th className={th}>Op</th>
                  <th className={th}>Ngưỡng</th><th className={th}>Severity</th><th className={th}>TT</th>
                </tr>
              </thead>
              <tbody>
                {thresholds.map((t) => (
                  <tr key={t.id}>
                    <td className={td}>{t.code}</td>
                    <td className={td}>{t.metricKey}</td>
                    <td className={td}>{t.operator}</td>
                    <td className={td}>{money(t.thresholdValue)}</td>
                    <td className={td}>{t.severity}</td>
                    <td className={td}><span className={statusPill(t.status === "Active" ? "success" : "muted")}>{t.status}</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      ) : !canCompare ? (
        <p className="text-sm text-[var(--muted)]">Cần quyền bi.report.read để so sánh kỳ.</p>
      ) : (
        <div className="space-y-4">
          <form className={`${panel} grid gap-3 md:grid-cols-4`} onSubmit={onCompare}>
            <label className={field.label}>
              Metric
              <select className={field.input} value={cMetric} onChange={(e) => setCMetric(e.target.value)}>
                <option>Revenue</option><option>Profit</option><option>Custom</option>
              </select>
            </label>
            <label className={field.label}>Kỳ hiện tại<input className={field.input} value={cCurrent} onChange={(e) => setCCurrent(e.target.value)} /></label>
            <label className={field.label}>Kỳ trước<input className={field.input} value={cPrior} onChange={(e) => setCPrior(e.target.value)} /></label>
            <div className="flex items-end"><button type="submit" className={btn.primary}>So sánh</button></div>
          </form>
          {compare && (
            <div className={`${panel} grid gap-3 sm:grid-cols-2 lg:grid-cols-3`}>
              <Stat label="Actual kỳ này" value={compare.currentActual} />
              <Stat label="Actual kỳ trước" value={compare.priorActual ?? 0} hint={compare.priorPeriodKey ?? "—"} />
              <Stat label="Δ kỳ" value={compare.periodDelta ?? 0} hint={`${compare.periodDeltaPercent ?? 0}%`} />
              <Stat label="Target" value={compare.targetValue ?? 0} />
              <Stat label="Δ vs target" value={compare.vsTargetDelta ?? 0} hint={`${compare.vsTargetPercent ?? 0}%`} />
            </div>
          )}
        </div>
      )}
    </div>
  );
}

function Stat({ label, value, hint }: { label: string; value: number; hint?: string }) {
  return (
    <div>
      <div className="text-xs uppercase tracking-wide text-[var(--muted)]">{label}</div>
      <div className="text-lg font-semibold">{money(value)}</div>
      {hint && <div className="text-xs text-[var(--muted)]">{hint}</div>}
    </div>
  );
}
