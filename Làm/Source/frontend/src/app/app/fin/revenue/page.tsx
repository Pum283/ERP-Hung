"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { fetchFinAccounts, fetchFinPeriods, type FinAccountDto, type FinPeriodDto } from "@/shared/api/fin-api";
import {
  fetchFinRevenueDocuments,
  fetchFinRevenueSummary,
  recognizeFinCogs,
  recognizeFinRevenueFromAr,
  recognizeFinRevenueFromOrder,
  recognizeFinRevenueFromPos,
  voidFinRevenueDocument,
  type FinRevenueDocumentDto,
  type FinRevenueSummaryDto,
} from "@/shared/api/fin-revenue-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 0 });
}

type Tab = "list" | "recognize" | "summary";

export default function FinRevenuePage() {
  const { can } = usePermissions();
  const canRead = can("fin.revenue.read");
  const canManage = can("fin.revenue.manage");

  const [tab, setTab] = useState<Tab>("list");
  const [docs, setDocs] = useState<FinRevenueDocumentDto[]>([]);
  const [summary, setSummary] = useState<FinRevenueSummaryDto | null>(null);
  const [periods, setPeriods] = useState<FinPeriodDto[]>([]);
  const [accounts, setAccounts] = useState<FinAccountDto[]>([]);
  const [periodId, setPeriodId] = useState("");
  const [kindFilter, setKindFilter] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [sourceKind, setSourceKind] = useState<"pos" | "order" | "ar" | "cogs">("pos");
  const [sourceId, setSourceId] = useState("");
  const [debitId, setDebitId] = useState("");
  const [creditId, setCreditId] = useState("");

  const load = useCallback(async () => {
    const [d, s, p, a] = await Promise.all([
      fetchFinRevenueDocuments({
        ...(periodId ? { periodId } : {}),
        ...(kindFilter ? { kind: kindFilter } : {}),
      }),
      fetchFinRevenueSummary(periodId ? { periodId } : undefined),
      fetchFinPeriods().catch(() => [] as FinPeriodDto[]),
      fetchFinAccounts().catch(() => [] as FinAccountDto[]),
    ]);
    setDocs(d);
    setSummary(s);
    setPeriods(p.filter((x) => x.status !== "Locked"));
    setAccounts(a.filter((x) => x.status === "Active"));
    if (!periodId && p[0]) setPeriodId(p.find((x) => x.status !== "Locked")?.id ?? "");
  }, [periodId, kindFilter]);

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

  function onRecognize(e: FormEvent) {
    e.preventDefault();
    if (!sourceId.trim()) { setError("Nhập Id nguồn."); return; }
    const body = {
      periodId: periodId || null,
      debitAccountId: debitId || null,
      creditAccountId: creditId || null,
    };
    const id = sourceId.trim();
    void run(async () => {
      if (sourceKind === "pos") await recognizeFinRevenueFromPos(id, body);
      else if (sourceKind === "order") await recognizeFinRevenueFromOrder(id, body);
      else if (sourceKind === "ar") await recognizeFinRevenueFromAr(id, body);
      else await recognizeFinCogs(id, body);
    }, "Đã ghi nhận");
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem doanh thu / giá vốn.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Doanh thu &amp; giá vốn</h1>
          <p className="text-sm text-[var(--muted)]">Ghi nhận từ POS · đơn bán · AR · xuất kho bán (COGS).</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {(["list", "recognize", "summary"] as Tab[]).map((t) => (
            <button key={t} type="button" className={tab === t ? btn.primary : btn.ghost} onClick={() => setTab(t)}>
              {t === "list" ? "Chứng từ" : t === "recognize" ? "Ghi nhận" : "Tổng hợp"}
            </button>
          ))}
        </div>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}

      <div className="flex flex-wrap gap-3">
        <label className={field.label}>
          Kỳ
          <select className={field.input} value={periodId} onChange={(e) => setPeriodId(e.target.value)}>
            <option value="">— Tất cả / không gắn —</option>
            {periods.map((p) => <option key={p.id} value={p.id}>{p.code}</option>)}
          </select>
        </label>
        {tab === "list" && (
          <label className={field.label}>
            Loại
            <select className={field.input} value={kindFilter} onChange={(e) => setKindFilter(e.target.value)}>
              <option value="">Tất cả</option>
              <option value="PosRevenue">POS</option>
              <option value="OrderRevenue">Đơn bán</option>
              <option value="ArRevenue">AR</option>
              <option value="Cogs">COGS</option>
            </select>
          </label>
        )}
      </div>

      {loading ? (
        <p className="text-sm text-[var(--muted)]">Đang tải…</p>
      ) : tab === "summary" && summary ? (
        <div className={`${panel} grid gap-3 sm:grid-cols-2 lg:grid-cols-3`}>
          <Stat label="DT POS" value={summary.posRevenue} hint={`${summary.posCount} chứng từ`} />
          <Stat label="DT đơn" value={summary.orderRevenue} hint={`${summary.orderCount} chứng từ`} />
          <Stat label="DT AR" value={summary.arRevenue} hint={`${summary.arCount} chứng từ`} />
          <Stat label="Giá vốn" value={summary.cogsAmount} hint={`${summary.cogsCount} chứng từ`} />
          <Stat label="Biên gộp" value={summary.grossMargin} hint="DT − COGS" />
        </div>
      ) : tab === "recognize" ? (
        <form className={`${panel} grid max-w-xl gap-3`} onSubmit={onRecognize}>
          <label className={field.label}>
            Nguồn
            <select className={field.input} value={sourceKind} onChange={(e) => setSourceKind(e.target.value as typeof sourceKind)}>
              <option value="pos">Doanh thu POS (saleId)</option>
              <option value="order">Doanh thu đơn CRM (orderId)</option>
              <option value="ar">Doanh thu AR (arInvoiceId)</option>
              <option value="cogs">Giá vốn — phiếu xuất INV (docId)</option>
            </select>
          </label>
          <label className={field.label}>
            Id nguồn
            <input className={field.input} value={sourceId} onChange={(e) => setSourceId(e.target.value)} placeholder="GUID" />
          </label>
          <label className={field.label}>
            TK Nợ (tuỳ chọn — có thì đẩy BT)
            <select className={field.input} value={debitId} onChange={(e) => setDebitId(e.target.value)}>
              <option value="">— Không đẩy BT —</option>
              {accounts.map((a) => <option key={a.id} value={a.id}>{a.code} · {a.name}</option>)}
            </select>
          </label>
          <label className={field.label}>
            TK Có
            <select className={field.input} value={creditId} onChange={(e) => setCreditId(e.target.value)}>
              <option value="">— Không đẩy BT —</option>
              {accounts.map((a) => <option key={a.id} value={a.id}>{a.code} · {a.name}</option>)}
            </select>
          </label>
          {canManage && (
            <button type="submit" className={btn.primary}>Ghi nhận</button>
          )}
        </form>
      ) : (
        <div className={tableWrap}>
          <table className="min-w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Mã</th>
                <th className={th}>Loại</th>
                <th className={th}>Nguồn</th>
                <th className={th}>Ngày</th>
                <th className={th}>DT</th>
                <th className={th}>COGS</th>
                <th className={th}>BT</th>
                <th className={th}>TT</th>
                <th className={th} />
              </tr>
            </thead>
            <tbody>
              {docs.length === 0 ? (
                <tr><td className={td} colSpan={9}>Chưa có chứng từ.</td></tr>
              ) : docs.map((d) => (
                <tr key={d.id}>
                  <td className={td}>{d.code}</td>
                  <td className={td}>{d.kind}</td>
                  <td className={td}>{d.sourceModule} · {d.sourceCode ?? "—"}</td>
                  <td className={td}>{new Date(d.docDate).toLocaleDateString("vi-VN")}</td>
                  <td className={td}>{money(d.revenueAmount)}</td>
                  <td className={td}>{money(d.cogsAmount)}</td>
                  <td className={td}>{d.finJournalCode ?? "—"}</td>
                  <td className={td}>
                    <span className={statusPill(d.status === "Posted" ? "success" : d.status === "Void" ? "danger" : "brand")}>
                      {d.status}
                    </span>
                  </td>
                  <td className={td}>
                    {canManage && d.status === "Posted" && !d.finJournalId && (
                      <button type="button" className={btn.ghost}
                        onClick={() => void run(() => voidFinRevenueDocument(d.id), "Đã hủy")}>
                        Hủy
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function Stat({ label, value, hint }: { label: string; value: number; hint: string }) {
  return (
    <div>
      <div className="text-xs uppercase tracking-wide text-[var(--muted)]">{label}</div>
      <div className="text-lg font-semibold">{money(value)}</div>
      <div className="text-xs text-[var(--muted)]">{hint}</div>
    </div>
  );
}
