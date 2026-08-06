"use client";

import { useCallback, useEffect, useState } from "react";
import {
  downloadPjmReportCsv,
  fetchPjmDashboard,
  fetchPjmOverdue,
  fetchPjmPortfolio,
  fetchPjmProfit,
  fetchPjmProgressHealth,
  type PjmDashboardDto,
  type PjmOverdueRowDto,
  type PjmPortfolioRowDto,
  type PjmProfitRowDto,
  type PjmProgressHealthRowDto,
} from "@/shared/api/pjm-report-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

type Tab = "dashboard" | "portfolio" | "progress" | "overdue" | "profit";

function healthTone(h: string): "success" | "warning" | "danger" | "muted" {
  if (h === "OnTrack" || h === "Done") return "success";
  if (h === "AtRisk") return "warning";
  if (h === "Late") return "danger";
  return "muted";
}

export default function PjmReportsPage() {
  const { can } = usePermissions();
  const canRead = can("pjm.project.read");

  const [tab, setTab] = useState<Tab>("dashboard");
  const [dashboard, setDashboard] = useState<PjmDashboardDto | null>(null);
  const [portfolio, setPortfolio] = useState<PjmPortfolioRowDto[]>([]);
  const [progress, setProgress] = useState<PjmProgressHealthRowDto[]>([]);
  const [overdue, setOverdue] = useState<PjmOverdueRowDto[]>([]);
  const [profit, setProfit] = useState<PjmProfitRowDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (tab === "dashboard") setDashboard(await fetchPjmDashboard());
    else if (tab === "portfolio") setPortfolio(await fetchPjmPortfolio());
    else if (tab === "progress") setProgress(await fetchPjmProgressHealth());
    else if (tab === "overdue") setOverdue(await fetchPjmOverdue());
    else setProfit(await fetchPjmProfit());
  }, [tab]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  async function exportCsv() {
    try {
      setError(null);
      await downloadPjmReportCsv(tab);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem báo cáo dự án.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Báo cáo dự án</h1>
          <p className="text-sm text-[var(--muted)]">UC_PJM_017 · 023 · 038–040 · 042 · portfolio · P&amp;L · CSV.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {([
            ["dashboard", "Dashboard"],
            ["portfolio", "Portfolio"],
            ["progress", "Sức khỏe"],
            ["overdue", "Trễ hạn"],
            ["profit", "P&L / NS"],
          ] as [Tab, string][]).map(([k, label]) => (
            <button key={k} type="button" className={tab === k ? btn.primary : btn.ghost} onClick={() => setTab(k)}>
              {label}
            </button>
          ))}
          <button type="button" className={btn.soft} onClick={() => void exportCsv()}>Xuất CSV</button>
        </div>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}

      {tab === "dashboard" && dashboard && (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {[
            ["Active", String(dashboard.activeCount)],
            ["Draft", String(dashboard.draftCount)],
            ["Closed", String(dashboard.closedCount)],
            ["DA trễ", String(dashboard.overdueProjectCount)],
            ["WBS trễ", String(dashboard.overdueWbsCount)],
            ["Milestone trễ", String(dashboard.overdueMilestoneCount)],
            ["TB % Active", `${dashboard.avgActiveProgressPercent}%`],
          ].map(([label, val]) => (
            <div key={label} className={panel}>
              <div className="text-xs text-[var(--muted)]">{label}</div>
              <div className="mt-1 text-lg font-semibold">{val}</div>
            </div>
          ))}
        </div>
      )}

      <div className={tableWrap}>
        {tab === "portfolio" && (
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Dự án</th><th className={th}>TT</th><th className={th}>PM</th>
                <th className={th}>%</th><th className={th}>Health</th><th className={th}>WBS</th><th className={th}>Trễ</th>
              </tr>
            </thead>
            <tbody>
              {portfolio.map((r) => (
                <tr key={r.projectId}>
                  <td className={td}>{r.code} · {r.name}</td>
                  <td className={td}>{r.statusCode}</td>
                  <td className={td}>{r.pmName ?? "—"}</td>
                  <td className={td}>{r.progressPercent}%</td>
                  <td className={td}><span className={statusPill(healthTone(r.health))}>{r.health}</span></td>
                  <td className={td}>{r.wbsCount}</td>
                  <td className={td}>{r.overdueCount}</td>
                </tr>
              ))}
              {!loading && portfolio.length === 0 && <tr><td className={td} colSpan={7}>Không có dự án.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "progress" && (
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Dự án</th><th className={th}>%</th><th className={th}>Health</th>
                <th className={th}>Mở</th><th className={th}>Xong</th><th className={th}>Trễ</th><th className={th}>MS trễ</th>
              </tr>
            </thead>
            <tbody>
              {progress.map((r) => (
                <tr key={r.projectId}>
                  <td className={td}>{r.code} · {r.name}</td>
                  <td className={td}>{r.progressPercent}%</td>
                  <td className={td}><span className={statusPill(healthTone(r.health))}>{r.health}</span></td>
                  <td className={td}>{r.openWbs}</td>
                  <td className={td}>{r.doneWbs}</td>
                  <td className={td}>{r.overdueWbs}</td>
                  <td className={td}>{r.overdueMilestones}</td>
                </tr>
              ))}
              {!loading && progress.length === 0 && <tr><td className={td} colSpan={7}>Không có dữ liệu.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "overdue" && (
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Dự án</th><th className={th}>WBS</th><th className={th}>MS</th>
                <th className={th}>Hạn</th><th className={th}>%</th><th className={th}>Assignee</th>
              </tr>
            </thead>
            <tbody>
              {overdue.map((r) => (
                <tr key={r.wbsItemId}>
                  <td className={td}>{r.projectCode}</td>
                  <td className={td}>{r.wbsCode} · {r.wbsName}</td>
                  <td className={td}>{r.isMilestone ? "Yes" : "—"}</td>
                  <td className={td}>{r.dueDate.slice(0, 10)}</td>
                  <td className={td}>{r.percentComplete}%</td>
                  <td className={td}>{r.assigneeName ?? "—"}</td>
                </tr>
              ))}
              {!loading && overdue.length === 0 && <tr><td className={td} colSpan={6}>Không có hạng mục trễ.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "profit" && (
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Dự án</th><th className={th}>TT</th><th className={th}>NS</th>
                <th className={th}>Chi phí</th><th className={th}>DT</th><th className={th}>Margin</th>
                <th className={th}>Margin %</th><th className={th}>Chênh NS</th>
              </tr>
            </thead>
            <tbody>
              {profit.map((r) => (
                <tr key={r.projectId}>
                  <td className={td}>{r.code} · {r.name}</td>
                  <td className={td}>{r.statusCode}</td>
                  <td className={td}>{r.budget}</td>
                  <td className={td}>{r.actualCost}</td>
                  <td className={td}>{r.recognizedRevenue}</td>
                  <td className={td}>{r.margin}</td>
                  <td className={td}>{r.marginPct}%</td>
                  <td className={td}>
                    <span className={statusPill(r.overBudget ? "danger" : "success")}>{r.budgetVariance}</span>
                  </td>
                </tr>
              ))}
              {!loading && profit.length === 0 && <tr><td className={td} colSpan={8}>Không có dữ liệu P&amp;L.</td></tr>}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
