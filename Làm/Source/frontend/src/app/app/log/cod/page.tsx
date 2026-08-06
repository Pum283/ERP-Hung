"use client";

import { FormEvent, useCallback, useEffect, useMemo, useState } from "react";
import {
  createLogCodHandover,
  fetchLogCodDeliveries,
  fetchLogCodHandoverDetail,
  fetchLogCodHandovers,
  fetchLogCodOverdue,
  fetchLogCodReport,
  reconcileLogCodHandover,
  resolveLogCodVariance,
  submitLogCodHandover,
  type LogCodHandoverDetailDto,
  type LogCodHandoverDto,
  type LogCodReportDto,
} from "@/shared/api/log-cod-api";
import type { LogDeliveryOrderDto } from "@/shared/api/log-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

export default function LogCodPage() {
  const { can } = usePermissions();
  const canRead = can("log.cod.read");
  const canManage = can("log.cod.manage");

  const [report, setReport] = useState<LogCodReportDto | null>(null);
  const [codList, setCodList] = useState<LogDeliveryOrderDto[]>([]);
  const [overdue, setOverdue] = useState<LogDeliveryOrderDto[]>([]);
  const [handovers, setHandovers] = useState<LogCodHandoverDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<LogCodHandoverDetailDto | null>(null);
  const [picked, setPicked] = useState<Record<string, boolean>>({});
  const [remit, setRemit] = useState("");
  const [varNote, setVarNote] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const load = useCallback(async () => {
    const [r, c, o, h] = await Promise.all([
      fetchLogCodReport(),
      fetchLogCodDeliveries(),
      fetchLogCodOverdue(),
      fetchLogCodHandovers(),
    ]);
    setReport(r);
    setCodList(c);
    setOverdue(o);
    setHandovers(h);
    if (!selectedId && h[0]) setSelectedId(h[0].id);
  }, [selectedId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchLogCodHandoverDetail(selectedId)
      .then((d) => {
        setDetail(d);
        setRemit(String(d.header.expectedAmount));
      })
      .catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  const collected = useMemo(
    () => codList.filter((x) => x.codStatus === "Collected" && !x.codHandoverId),
    [codList],
  );

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) setDetail(await fetchLogCodHandoverDetail(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem COD.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">COD</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Thu · bàn giao · đối soát 3 chiều · cảnh báo quá hạn (UC_LOG_020–026, 038)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      {report && (
        <section className="grid gap-3 sm:grid-cols-3 xl:grid-cols-6">
          {[
            ["Pending", report.pendingCount, report.pendingAmount],
            ["Collected", report.collectedCount, report.collectedAmount],
            ["Remitted", report.remittedCount, report.remittedAmount],
            ["Reconciled", report.reconciledCount, report.reconciledAmount],
            ["Overdue", report.overdueCount, report.overdueAmount],
            ["Variance", report.varianceCount, report.varianceAmount],
          ].map(([label, cnt, amt]) => (
            <div key={String(label)} className={panel}>
              <div className="text-xs uppercase text-[var(--muted)]">{label}</div>
              <div className="mt-1 text-lg font-semibold">{money(Number(amt))}</div>
              <div className="text-xs text-[var(--muted)]">{cnt} lệnh</div>
            </div>
          ))}
        </section>
      )}

      {overdue.length > 0 && (
        <section className={`${panel} border-amber-200 bg-amber-50/40`}>
          <h2 className="mb-2 text-sm font-semibold text-amber-900">
            Cảnh báo COD quá hạn ({overdue.length})
          </h2>
          <ul className="space-y-1 text-sm">
            {overdue.map((d) => (
              <li key={d.id}>
                {d.code} · {d.customerName} · {money(d.codAmount)} · hạn{" "}
                {d.codDueAt ? new Date(d.codDueAt).toLocaleDateString("vi-VN") : "—"}
              </li>
            ))}
          </ul>
        </section>
      )}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">COD đã thu — tạo bàn giao</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th} />
                  <th className={th}>Lệnh</th>
                  <th className={th}>Tiền</th>
                  <th className={th}>TT</th>
                </tr>
              </thead>
              <tbody>
                {collected.map((d) => (
                  <tr key={d.id}>
                    <td className={td}>
                      <input
                        type="checkbox"
                        checked={!!picked[d.id]}
                        onChange={(e) => setPicked((p) => ({ ...p, [d.id]: e.target.checked }))}
                        disabled={!canManage}
                      />
                    </td>
                    <td className={td}>
                      <div className="font-medium">{d.code}</div>
                      <div className="text-xs text-[var(--muted)]">{d.customerName}</div>
                    </td>
                    <td className={td}>{money(d.codAmount)}</td>
                    <td className={td}>
                      <span className={statusPill(d.codOverdue ? "danger" : "brand")}>{d.codStatus}</span>
                    </td>
                  </tr>
                ))}
                {collected.length === 0 && (
                  <tr><td className={td} colSpan={4}>Chưa có COD Collected (đánh dấu/thu ở Lệnh giao).</td></tr>
                )}
              </tbody>
            </table>
          </div>
          {canManage && (
            <button
              type="button"
              className={`${btn.primary} mt-3`}
              onClick={() => {
                const ids = Object.entries(picked).filter(([, v]) => v).map(([k]) => k);
                void run(async () => {
                  const d = await createLogCodHandover({ deliveryOrderIds: ids });
                  setSelectedId(d.header.id);
                  setPicked({});
                }, "Đã tạo bàn giao COD");
              }}
            >
              Tạo bàn giao
            </button>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Bàn giao / đối soát</h2>
          <div className={`${tableWrap} mb-3`}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>TT</th>
                  <th className={th}>Kỳ vọng</th>
                </tr>
              </thead>
              <tbody>
                {handovers.map((h) => (
                  <tr
                    key={h.id}
                    className="cursor-pointer hover:bg-black/5"
                    onClick={() => setSelectedId(h.id)}
                  >
                    <td className={td}>
                      <div className="font-medium">{h.code}</div>
                      <div className="text-xs text-[var(--muted)]">{h.lineCount} dòng</div>
                    </td>
                    <td className={td}>
                      <span className={statusPill(h.status === "Reconciled" ? "success" : h.status === "Variance" ? "danger" : "brand")}>
                        {h.status}
                      </span>
                    </td>
                    <td className={td}>{money(h.expectedAmount)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {detail && (
            <div className="space-y-3 border-t border-black/10 pt-3">
              <div>
                <div className="font-medium">{detail.header.code}</div>
                <p className="text-xs text-[var(--muted)]">
                  Thu {money(detail.header.collectedAmount)} · Nộp {money(detail.header.remittedAmount)}
                  {detail.header.varianceAmount !== 0
                    ? ` · Lệch ${money(detail.header.varianceAmount)}`
                    : ""}
                </p>
              </div>
              <ul className="space-y-1 text-sm">
                {detail.lines.map((l) => (
                  <li key={l.id}>
                    {l.deliveryCode} · {l.customerName} — {money(l.codAmount)}
                  </li>
                ))}
              </ul>

              {canManage && detail.header.status === "Draft" && (
                <button
                  type="button"
                  className={btn.primary}
                  onClick={() => run(() => submitLogCodHandover(detail.header.id), "Đã nộp bàn giao")}
                >
                  Nộp bàn giao
                </button>
              )}

              {canManage && detail.header.status === "Submitted" && (
                <form
                  className="flex flex-wrap gap-2"
                  onSubmit={(e: FormEvent) => {
                    e.preventDefault();
                    void run(
                      () => reconcileLogCodHandover(detail.header.id, Number(remit) || 0, varNote || undefined),
                      "Đã đối soát 3 chiều",
                    );
                  }}
                >
                  <input
                    className={field}
                    value={remit}
                    onChange={(e) => setRemit(e.target.value)}
                    placeholder="Số tiền thực nộp"
                  />
                  <input
                    className={field}
                    value={varNote}
                    onChange={(e) => setVarNote(e.target.value)}
                    placeholder="Ghi chú"
                  />
                  <button type="submit" className={btn.primary}>Đối soát</button>
                </form>
              )}

              {canManage && detail.header.status === "Variance" && (
                <form
                  className="flex flex-wrap gap-2"
                  onSubmit={(e: FormEvent) => {
                    e.preventDefault();
                    void run(
                      () => resolveLogCodVariance(
                        detail.header.id,
                        varNote || "Đã xử lý lệch",
                        remit ? Number(remit) : undefined,
                      ),
                      "Đã xử lý lệch COD",
                    );
                  }}
                >
                  <input
                    className={field}
                    value={remit}
                    onChange={(e) => setRemit(e.target.value)}
                    placeholder="Số nộp điều chỉnh (tuỳ chọn)"
                  />
                  <input
                    className={`${field} min-w-[200px]`}
                    value={varNote}
                    onChange={(e) => setVarNote(e.target.value)}
                    placeholder="Ghi chú xử lý lệch"
                    required
                  />
                  <button type="submit" className={btn.primary}>Xử lý lệch</button>
                </form>
              )}
            </div>
          )}
        </section>
      </div>

      <section className={panel}>
        <h2 className="mb-3 text-sm font-semibold">Tất cả COD</h2>
        <div className={tableWrap}>
          <table className="w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Lệnh</th>
                <th className={th}>Khách</th>
                <th className={th}>Tiền</th>
                <th className={th}>COD</th>
                <th className={th}>Hạn</th>
              </tr>
            </thead>
            <tbody>
              {codList.map((d) => (
                <tr key={d.id}>
                  <td className={td}>{d.code}</td>
                  <td className={td}>{d.customerName}</td>
                  <td className={td}>{money(d.codAmount)}</td>
                  <td className={td}>
                    <span className={statusPill(d.codOverdue ? "danger" : "muted")}>{d.codStatus}</span>
                  </td>
                  <td className={td}>
                    {d.codDueAt ? new Date(d.codDueAt).toLocaleDateString("vi-VN") : "—"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}
