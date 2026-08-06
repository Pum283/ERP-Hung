"use client";

import { useEffect, useMemo, useState } from "react";
import { fetchHrmDashboard, type HrmDashboardBundleDto } from "@/shared/api/hrm-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";

function money(n: number) {
  return n.toLocaleString("vi-VN");
}

function monthStart() {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-01`;
}

function today() {
  return new Date().toISOString().slice(0, 10);
}

function Panel({
  title,
  hint,
  children,
  className = "",
}: {
  title: string;
  hint?: string;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <section className={`rounded-xl border border-border bg-surface p-4 ${className}`}>
      <div className="mb-3 flex items-baseline justify-between gap-2">
        <h2 className="font-display text-lead font-bold text-foreground">{title}</h2>
        {hint ? <span className="text-meta text-muted-foreground">{hint}</span> : null}
      </div>
      {children}
    </section>
  );
}

function Kpi({
  label,
  value,
  sub,
  tone = "default",
}: {
  label: string;
  value: string | number;
  sub?: string;
  tone?: "default" | "brand" | "success" | "warning" | "danger";
}) {
  const valueCls =
    tone === "brand"
      ? "text-brand-strong"
      : tone === "success"
        ? "text-success"
        : tone === "warning"
          ? "text-warning"
          : tone === "danger"
            ? "text-destructive"
            : "text-foreground";
  return (
    <div className="relative overflow-hidden rounded-xl border border-border bg-surface px-4 py-3">
      <div
        className="pointer-events-none absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-brand/80 via-accent/60 to-transparent"
        aria-hidden
      />
      <div className="text-meta text-muted-foreground">{label}</div>
      <div className={`mt-1 font-display text-title font-bold tabular-nums ${valueCls}`}>{value}</div>
      {sub ? <div className="mt-0.5 text-meta text-muted-foreground">{sub}</div> : null}
    </div>
  );
}

function Empty({ children }: { children: React.ReactNode }) {
  return <p className="py-6 text-center text-body text-muted-foreground">{children}</p>;
}

function BarRows({
  rows,
}: {
  rows: { key: string; label: string; value: number; max: number; hint?: string }[];
}) {
  if (rows.length === 0) return null;
  return (
    <ul className="space-y-2.5">
      {rows.map((r) => {
        const pct = r.max > 0 ? Math.max(4, Math.round((r.value / r.max) * 100)) : 0;
        return (
          <li key={r.key}>
            <div className="mb-1 flex items-center justify-between gap-2 text-body">
              <span className="truncate text-foreground">{r.label}</span>
              <span className="shrink-0 tabular-nums text-muted-foreground">
                {r.value}
                {r.hint ? ` · ${r.hint}` : ""}
              </span>
            </div>
            <div className="h-1.5 overflow-hidden rounded-sm bg-muted">
              <div className="h-full rounded-sm bg-brand" style={{ width: `${pct}%` }} />
            </div>
          </li>
        );
      })}
    </ul>
  );
}

function SimpleTable({
  headers,
  children,
}: {
  headers: string[];
  children: React.ReactNode;
}) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full text-left text-body">
        <thead>
          <tr className="border-b border-border text-meta text-muted-foreground">
            {headers.map((h) => (
              <th key={h} className="whitespace-nowrap py-2 pr-3 font-medium">
                {h}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>{children}</tbody>
      </table>
    </div>
  );
}

export default function HrmDashboardPage() {
  const { can } = usePermissions();
  const canRead = can("hrm.employee.read");

  const [data, setData] = useState<HrmDashboardBundleDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [attFrom, setAttFrom] = useState(monthStart());
  const [attTo, setAttTo] = useState(today());
  const [leaveYear, setLeaveYear] = useState(new Date().getFullYear());

  async function load() {
    setLoading(true);
    setError(null);
    try {
      setData(await fetchHrmDashboard({ attFrom, attTo, leaveYear }));
    } catch {
      setError("Không tải được dashboard HRM.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (canRead) void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canRead]);

  const hc = data?.headcount;
  const cost = data?.cost;

  const orgBars = useMemo(() => {
    const list = hc?.byOrg ?? [];
    const max = Math.max(1, ...list.map((x) => x.count));
    return list.slice(0, 8).map((x) => ({
      key: x.orgUnitId,
      label: x.orgUnitName || "—",
      value: x.count,
      max,
    }));
  }, [hc]);

  const funnelBars = useMemo(() => {
    const list = data?.recruitFunnel ?? [];
    const max = Math.max(1, ...list.map((x) => x.count));
    return list.map((x) => ({
      key: x.pipelineStatus,
      label: x.pipelineStatus,
      value: x.count,
      max,
    }));
  }, [data]);

  const leaveBars = useMemo(() => {
    const list = data?.leaveSummary ?? [];
    const max = Math.max(1, ...list.map((x) => x.entitled));
    return list.map((x) => ({
      key: x.orgUnitId,
      label: x.orgUnitName || "—",
      value: x.remaining,
      max,
      hint: `còn / ${x.entitled}`,
    }));
  }, [data]);

  const moveMax = useMemo(() => {
    const m = hc?.movements ?? [];
    return Math.max(1, ...m.flatMap((x) => [x.hired, x.resigned]));
  }, [hc]);

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền xem dashboard HRM.</p>;
  }

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground">Dashboard HRM</h1>
          <p className="mt-1 text-body text-muted-foreground">
            Headcount · công · tuyển · phép · chi phí · định biên
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-2 rounded-xl border border-border bg-surface px-3 py-2">
          <label className="flex items-center gap-1.5 text-meta text-muted-foreground">
            Công
            <input
              type="date"
              value={attFrom}
              onChange={(e) => setAttFrom(e.target.value)}
              className="h-8 rounded-md border border-border bg-background px-2 text-body text-foreground"
            />
          </label>
          <span className="text-meta text-muted-foreground">→</span>
          <input
            type="date"
            value={attTo}
            onChange={(e) => setAttTo(e.target.value)}
            className="h-8 rounded-md border border-border bg-background px-2 text-body text-foreground"
          />
          <label className="flex items-center gap-1.5 text-meta text-muted-foreground">
            Năm phép
            <input
              type="number"
              value={leaveYear}
              onChange={(e) => setLeaveYear(Number(e.target.value))}
              className="h-8 w-20 rounded-md border border-border bg-background px-2 text-body text-foreground"
            />
          </label>
          <button type="button" className={btn.primary} disabled={loading} onClick={() => void load()}>
            {loading ? "Đang tải…" : "Làm mới"}
          </button>
        </div>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}

      {!data && loading && <p className="text-body text-muted-foreground">Đang tải…</p>}

      {data && (
        <>
          <section className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            <Kpi label="Đang làm" value={hc?.totalActive ?? 0} tone="brand" sub="Active headcount" />
            <Kpi label="Thử việc" value={hc?.totalProbation ?? 0} tone="warning" />
            <Kpi label="Đã nghỉ / inactive" value={hc?.totalInactiveOrLeft ?? 0} tone="danger" />
            <Kpi
              label="Chi phí net"
              value={money(cost?.totalNet ?? 0)}
              sub={cost?.periodKey ? `Kỳ ${cost.periodKey}` : "Chưa có kỳ lương"}
            />
          </section>

          <div className="grid gap-4 xl:grid-cols-2">
            <Panel title="Theo đơn vị" hint="Top headcount">
              {orgBars.length === 0 ? <Empty>Chưa có phân bổ đơn vị.</Empty> : <BarRows rows={orgBars} />}
            </Panel>

            <Panel title="Theo trạng thái">
              {(hc?.byStatus.length ?? 0) === 0 ? (
                <Empty>Chưa có dữ liệu trạng thái.</Empty>
              ) : (
                <div className="flex flex-wrap gap-2">
                  {hc!.byStatus.map((x) => (
                    <div
                      key={x.status}
                      className="min-w-[7rem] flex-1 rounded-lg border border-border bg-muted/40 px-3 py-2"
                    >
                      <div className="text-meta text-muted-foreground">{x.status}</div>
                      <div className="font-display text-lead font-bold tabular-nums">{x.count}</div>
                    </div>
                  ))}
                </div>
              )}
            </Panel>
          </div>

          <Panel title="Biến động 6 tháng" hint="Tuyển mới · nghỉ · net">
            {(hc?.movements.length ?? 0) === 0 ? (
              <Empty>Chưa có biến động.</Empty>
            ) : (
              <div className="space-y-3">
                <div className="grid grid-cols-[4.5rem_1fr_4rem] items-center gap-2 text-meta text-muted-foreground">
                  <span>Kỳ</span>
                  <span>Tuyển / Nghỉ</span>
                  <span className="text-right">Net</span>
                </div>
                {hc!.movements.map((m) => (
                  <div key={m.periodKey} className="grid grid-cols-[4.5rem_1fr_4rem] items-center gap-2">
                    <span className="text-body tabular-nums text-foreground">{m.periodKey}</span>
                    <div className="space-y-1">
                      <div className="flex h-1.5 overflow-hidden rounded-sm bg-muted">
                        <div
                          className="h-full bg-success/80"
                          style={{ width: `${(m.hired / moveMax) * 100}%` }}
                          title={`Tuyển ${m.hired}`}
                        />
                      </div>
                      <div className="flex h-1.5 overflow-hidden rounded-sm bg-muted">
                        <div
                          className="h-full bg-destructive/70"
                          style={{ width: `${(m.resigned / moveMax) * 100}%` }}
                          title={`Nghỉ ${m.resigned}`}
                        />
                      </div>
                    </div>
                    <span
                      className={`text-right text-body font-semibold tabular-nums ${
                        m.net > 0 ? "text-success" : m.net < 0 ? "text-destructive" : "text-muted-foreground"
                      }`}
                    >
                      {m.net > 0 ? `+${m.net}` : m.net}
                    </span>
                  </div>
                ))}
                <div className="flex gap-4 text-meta text-muted-foreground">
                  <span className="inline-flex items-center gap-1.5">
                    <span className="inline-block h-1.5 w-3 rounded-sm bg-success/80" /> Tuyển mới
                  </span>
                  <span className="inline-flex items-center gap-1.5">
                    <span className="inline-block h-1.5 w-3 rounded-sm bg-destructive/70" /> Nghỉ
                  </span>
                </div>
              </div>
            )}
          </Panel>

          <div className="grid gap-4 xl:grid-cols-2">
            <Panel title="Funnel tuyển dụng">
              {funnelBars.length === 0 ? <Empty>Chưa có ứng viên.</Empty> : <BarRows rows={funnelBars} />}
            </Panel>
            <Panel title={`Quỹ phép ${leaveYear}`} hint="Ngày phép còn lại">
              {leaveBars.length === 0 ? <Empty>Chưa có quỹ phép.</Empty> : <BarRows rows={leaveBars} />}
            </Panel>
          </div>

          <Panel
            title="Công / OT / đi trễ"
            hint={`${attFrom} → ${attTo}`}
          >
            {data.attendance.length === 0 ? (
              <Empty>Chưa có dữ liệu chấm công kỳ này.</Empty>
            ) : (
              <SimpleTable headers={["Đơn vị", "Bản ghi", "Công", "OT (phút)", "Trễ (phút)", "Lần trễ"]}>
                {data.attendance.map((a) => (
                  <tr key={a.orgUnitId} className="border-b border-border/60">
                    <td className="py-2 pr-3">{a.orgUnitName || "—"}</td>
                    <td className="py-2 pr-3 tabular-nums">{a.recordCount}</td>
                    <td className="py-2 pr-3 tabular-nums">{a.workUnits}</td>
                    <td className="py-2 pr-3 tabular-nums">{a.otMinutes}</td>
                    <td className="py-2 pr-3 tabular-nums">{a.lateMinutes}</td>
                    <td className="py-2 tabular-nums">{a.lateCount}</td>
                  </tr>
                ))}
              </SimpleTable>
            )}
          </Panel>

          <div className="grid gap-4 xl:grid-cols-2">
            <Panel
              title="Chi phí nhân sự"
              hint={cost?.periodKey ? `${cost.periodKey} · ${cost.periodStatus}` : "Chưa có kỳ"}
            >
              <div className="mb-4 grid grid-cols-2 gap-2 sm:grid-cols-4">
                {[
                  ["Gross", money(cost?.totalGross ?? 0)],
                  ["Net", money(cost?.totalNet ?? 0)],
                  ["BH NV", money(cost?.totalInsurance ?? 0)],
                  ["Dòng", String(cost?.lineCount ?? 0)],
                ].map(([k, v]) => (
                  <div key={k} className="rounded-lg bg-muted/50 px-3 py-2">
                    <div className="text-meta text-muted-foreground">{k}</div>
                    <div className="font-display text-body font-bold tabular-nums">{v}</div>
                  </div>
                ))}
              </div>
              {(cost?.byOrg.length ?? 0) === 0 ? (
                <Empty>Chưa phân bổ theo đơn vị.</Empty>
              ) : (
                <SimpleTable headers={["Đơn vị", "NV", "Gross", "Net"]}>
                  {cost!.byOrg.map((o) => (
                    <tr key={o.orgUnitId} className="border-b border-border/60">
                      <td className="py-2 pr-3">{o.orgUnitName}</td>
                      <td className="py-2 pr-3 tabular-nums">{o.headcount}</td>
                      <td className="py-2 pr-3 tabular-nums">{money(o.gross)}</td>
                      <td className="py-2 tabular-nums">{money(o.net)}</td>
                    </tr>
                  ))}
                </SimpleTable>
              )}
            </Panel>

            <Panel title="Định biên vs thực tế">
              {data.headcountVsPlan.length === 0 ? (
                <Empty>Chưa có kế hoạch định biên.</Empty>
              ) : (
                <SimpleTable headers={["Phạm vi", "Đơn vị", "KH", "TT", "Gap", "Thiếu"]}>
                  {data.headcountVsPlan.map((r, i) => (
                    <tr
                      key={`${r.orgUnitId}-${r.departmentId ?? ""}-${i}`}
                      className="border-b border-border/60"
                    >
                      <td className="py-2 pr-3">{r.scopeType}</td>
                      <td className="py-2 pr-3">
                        {r.orgUnitName}
                        {r.departmentName ? ` / ${r.departmentName}` : ""}
                        {r.shiftCode ? ` · ${r.shiftCode}` : ""}
                      </td>
                      <td className="py-2 pr-3 tabular-nums">{r.planned}</td>
                      <td className="py-2 pr-3 tabular-nums">{r.actual}</td>
                      <td
                        className={`py-2 pr-3 tabular-nums font-semibold ${
                          r.gap < 0 ? "text-destructive" : r.gap > 0 ? "text-success" : ""
                        }`}
                      >
                        {r.gap}
                      </td>
                      <td className="py-2">
                        {r.shortage ? (
                          <span className="text-meta font-semibold text-destructive">Có</span>
                        ) : (
                          <span className="text-meta text-muted-foreground">—</span>
                        )}
                      </td>
                    </tr>
                  ))}
                </SimpleTable>
              )}
            </Panel>
          </div>
        </>
      )}
    </div>
  );
}
