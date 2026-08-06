"use client";

import { useCallback, useEffect, useState } from "react";
import {
  downloadFsmReportCsv,
  fetchFsmDashboard,
  fetchFsmPartCost,
  fetchFsmSlaCompliance,
  fetchFsmTechProductivity,
  type FsmDashboardDto,
  type FsmPartCostSummaryDto,
  type FsmSlaComplianceRowDto,
  type FsmTechProductivityRowDto,
} from "@/shared/api/fsm-report-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { panel, tableWrap, td, th } from "@/shared/ui/field";

type Tab = "dashboard" | "sla" | "productivity" | "parts";

export default function FsmReportsPage() {
  const { can } = usePermissions();
  const canRead = can("fsm.ticket.read");

  const [tab, setTab] = useState<Tab>("dashboard");
  const [dashboard, setDashboard] = useState<FsmDashboardDto | null>(null);
  const [sla, setSla] = useState<FsmSlaComplianceRowDto[]>([]);
  const [prod, setProd] = useState<FsmTechProductivityRowDto[]>([]);
  const [parts, setParts] = useState<FsmPartCostSummaryDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (tab === "dashboard") setDashboard(await fetchFsmDashboard());
    else if (tab === "sla") setSla(await fetchFsmSlaCompliance());
    else if (tab === "productivity") setProd(await fetchFsmTechProductivity());
    else setParts(await fetchFsmPartCost());
  }, [tab]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  async function exportCsv() {
    try {
      setError(null);
      const report =
        tab === "sla" ? "sla"
          : tab === "productivity" ? "productivity"
            : tab === "parts" ? "parts"
              : "dashboard";
      await downloadFsmReportCsv(report);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem báo cáo FSM.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Báo cáo FSM</h1>
          <p className="text-sm text-[var(--muted)]">UC_FSM_045 · 046 · 047 · 050 · SLA · năng suất · chi phí LK · CSV.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {([
            ["dashboard", "Dashboard"],
            ["sla", "SLA"],
            ["productivity", "Năng suất KTV"],
            ["parts", "Chi phí LK"],
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
            ["Open", String(dashboard.openCount)],
            ["Assigned", String(dashboard.assignedCount)],
            ["InProgress", String(dashboard.inProgressCount)],
            ["Escalated", String(dashboard.escalatedCount)],
            ["Resolved", String(dashboard.resolvedCount)],
            ["Closed", String(dashboard.closedCount)],
            ["Quá hạn mở", String(dashboard.overdueOpenCount)],
            ["Đóng hôm nay", String(dashboard.closedTodayCount)],
            ["SLA hit %", String(dashboard.slaHitRatePercent)],
            ["Lịch hẹn hôm nay", String(dashboard.appointmentTodayCount)],
          ].map(([label, val]) => (
            <div key={label} className={panel}>
              <div className="text-xs text-[var(--muted)]">{label}</div>
              <div className="mt-1 text-lg font-semibold">{val}</div>
            </div>
          ))}
        </div>
      )}

      {tab === "parts" && parts && (
        <div className="grid gap-3 sm:grid-cols-4">
          {[
            ["Tổng SL", String(parts.totalQty)],
            ["Tổng tiền", String(parts.totalAmount)],
            ["Số dòng", String(parts.lineCount)],
            ["Số ticket", String(parts.ticketCount)],
          ].map(([label, val]) => (
            <div key={label} className={panel}>
              <div className="text-xs text-[var(--muted)]">{label}</div>
              <div className="mt-1 text-lg font-semibold">{val}</div>
            </div>
          ))}
        </div>
      )}

      <div className={tableWrap}>
        {tab === "sla" && (
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Ưu tiên</th><th className={th}>Mở</th><th className={th}>Quá hạn</th>
                <th className={th}>Đóng</th><th className={th}>SLA đạt</th><th className={th}>SLA trễ</th><th className={th}>Hit %</th>
              </tr>
            </thead>
            <tbody>
              {sla.map((r) => (
                <tr key={r.priority}>
                  <td className={td}>{r.priority}</td>
                  <td className={td}>{r.openCount}</td>
                  <td className={td}>{r.overdueOpenCount}</td>
                  <td className={td}>{r.closedCount}</td>
                  <td className={td}>{r.slaMetCount}</td>
                  <td className={td}>{r.slaMissCount}</td>
                  <td className={td}>{r.slaHitRatePercent}%</td>
                </tr>
              ))}
              {!loading && sla.length === 0 && <tr><td className={td} colSpan={7}>Không có dữ liệu.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "productivity" && (
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>KTV</th><th className={th}>Gán</th><th className={th}>Resolved</th>
                <th className={th}>Closed</th><th className={th}>On-SLA</th><th className={th}>On-SLA %</th><th className={th}>TB giờ XL</th>
              </tr>
            </thead>
            <tbody>
              {prod.map((r) => (
                <tr key={r.techUserId ?? r.techName}>
                  <td className={td}>{r.techName}</td>
                  <td className={td}>{r.assignedCount}</td>
                  <td className={td}>{r.resolvedCount}</td>
                  <td className={td}>{r.closedCount}</td>
                  <td className={td}>{r.onSlaCount}</td>
                  <td className={td}>{r.onSlaPercent}%</td>
                  <td className={td}>{r.avgResolveHours}</td>
                </tr>
              ))}
              {!loading && prod.length === 0 && <tr><td className={td} colSpan={7}>Chưa có ticket gán KTV.</td></tr>}
            </tbody>
          </table>
        )}
        {tab === "parts" && (
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Mã</th><th className={th}>Tên</th>
                <th className={th}>SL</th><th className={th}>Tiền</th><th className={th}>Ticket</th>
              </tr>
            </thead>
            <tbody>
              {(parts?.byPart ?? []).map((r) => (
                <tr key={r.partId}>
                  <td className={td}>{r.partCode}</td>
                  <td className={td}>{r.partName}</td>
                  <td className={td}>{r.qty}</td>
                  <td className={td}>{r.amount}</td>
                  <td className={td}>{r.ticketCount}</td>
                </tr>
              ))}
              {!loading && (parts?.byPart.length ?? 0) === 0 && (
                <tr><td className={td} colSpan={5}>Chưa xuất linh kiện theo ticket.</td></tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
