"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  createCrmQuote,
  fetchCrmOpportunities,
  fetchCrmOpportunityDetail,
  setCrmOpportunityStage,
  upsertCrmOpportunity,
  upsertCrmOpportunityLine,
  type CrmOpportunityDetailDto,
  type CrmOpportunityDto,
  type CrmQuoteDto,
} from "@/shared/api/crm-lead-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function CrmOpportunitiesPage() {
  const { can } = usePermissions();
  const canRead = can("crm.opportunity.read");
  const canManage = can("crm.opportunity.manage");

  const [list, setList] = useState<CrmOpportunityDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<CrmOpportunityDetailDto | null>(null);
  const [lastQuote, setLastQuote] = useState<CrmQuoteDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [name, setName] = useState("Cơ hội mới");
  const [itemCode, setItemCode] = useState("SKU-01");
  const [itemName, setItemName] = useState("Gói dịch vụ");
  const [qty, setQty] = useState("1");
  const [price, setPrice] = useState("50000000");
  const [lostReason, setLostReason] = useState("Giá cao");

  const load = useCallback(async () => {
    const o = await fetchCrmOpportunities();
    setList(o);
    if (!selectedId && o[0]) setSelectedId(o[0].id);
  }, [selectedId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchCrmOpportunityDetail(selectedId).then(setDetail).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) setDetail(await fetchCrmOpportunityDetail(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem cơ hội.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Cơ hội bán hàng</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Tạo từ lead/KH · pipeline · SP/giá trị · báo giá stub · thắng/thua (UC_CRM_062–063, 065, 067–068)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Pipeline</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Cơ hội</th><th className={th}>GT</th><th className={th}>Stage</th></tr></thead>
              <tbody>
                {list.map((o) => (
                  <tr key={o.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(o.id)}>
                    <td className={td}>
                      <div className="font-medium">{o.code}</div>
                      <div className="text-xs text-[var(--muted)]">{o.name} · {o.leadCode ?? "—"}</div>
                    </td>
                    <td className={td}>{o.estimatedValue.toLocaleString()}</td>
                    <td className={td}>
                      <span className={statusPill(
                        o.stage === "Won" ? "success" : o.stage === "Lost" ? "danger" : "brand",
                      )}>{o.stage}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {canManage && (
            <form className="mt-3 flex flex-wrap gap-2 border-t border-black/10 pt-3" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(() => upsertCrmOpportunity({ name }), "Đã tạo cơ hội");
            }}>
              <input className={field} value={name} onChange={(e) => setName(e.target.value)} />
              <button className={btn.primary} type="submit">Tạo cơ hội</button>
            </form>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chi tiết</h2>
          {detail ? (
            <div className="space-y-3 text-sm">
              <div>
                <b>{detail.opportunity.code}</b> — {detail.opportunity.name}
                <div className="text-xs text-[var(--muted)]">
                  KH: {detail.opportunity.customerName ?? "—"} · Quote: {detail.opportunity.quoteCode ?? "—"}
                </div>
              </div>
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead><tr><th className={th}>SP</th><th className={th}>SL</th><th className={th}>Thành tiền</th></tr></thead>
                  <tbody>
                    {detail.lines.map((l) => (
                      <tr key={l.id}>
                        <td className={td}>{l.itemCode} {l.itemName}</td>
                        <td className={td}>{l.quantity}</td>
                        <td className={td}>{l.lineAmount.toLocaleString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {canManage && detail.opportunity.stage !== "Won" && detail.opportunity.stage !== "Lost" && (
                <>
                  <form className="flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
                    e.preventDefault();
                    void run(() => upsertCrmOpportunityLine(detail.opportunity.id, {
                      itemCode, itemName, quantity: Number(qty) || 1, unitPrice: Number(price) || 0,
                    }), "Đã gắn SP");
                  }}>
                    <input className={field} value={itemCode} onChange={(e) => setItemCode(e.target.value)} />
                    <input className={field} value={itemName} onChange={(e) => setItemName(e.target.value)} />
                    <input className={field} value={qty} onChange={(e) => setQty(e.target.value)} />
                    <input className={field} value={price} onChange={(e) => setPrice(e.target.value)} />
                    <button className={btn.ghost} type="submit">Thêm SP</button>
                  </form>
                  <div className="flex flex-wrap gap-2">
                    {["Proposal", "Negotiation"].map((st) => (
                      <button key={st} type="button" className={btn.ghost} onClick={() => void run(
                        () => setCrmOpportunityStage(detail.opportunity.id, st), `→ ${st}`,
                      )}>{st}</button>
                    ))}
                    <button type="button" className={btn.primary} onClick={() => void run(async () => {
                      const q = await createCrmQuote(detail.opportunity.id);
                      setLastQuote(q);
                    }, "Đã tạo báo giá")}>
                      Tạo báo giá
                    </button>
                    <button type="button" className={btn.ghost} onClick={() => void run(
                      () => setCrmOpportunityStage(detail.opportunity.id, "Won"), "Won",
                    )}>
                      Won
                    </button>
                    <input className={field} value={lostReason} onChange={(e) => setLostReason(e.target.value)} />
                    <button type="button" className={btn.ghost} onClick={() => void run(
                      () => setCrmOpportunityStage(detail.opportunity.id, "Lost", lostReason), "Lost",
                    )}>
                      Lost
                    </button>
                  </div>
                </>
              )}
              {lastQuote && (
                <div className="text-xs rounded-md border border-black/10 p-2">
                  Quote {lastQuote.code} · {lastQuote.totalAmount.toLocaleString()} · {lastQuote.status}
                </div>
              )}
            </div>
          ) : (
            <p className="text-sm text-[var(--muted)]">Chọn một cơ hội (hoặc convert từ Lead).</p>
          )}
        </section>
      </div>
    </div>
  );
}
