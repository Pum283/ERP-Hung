"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  applyCrmPriceList,
  convertCrmQuoteToOrder,
  decideCrmQuoteDiscount,
  fetchCrmPriceLists,
  fetchCrmQuoteDetail,
  fetchCrmQuotes,
  requestCrmQuoteDiscount,
  sendCrmQuote,
  upsertCrmPriceList,
  upsertCrmPriceListItem,
  upsertCrmQuoteLine,
  type CrmPriceListDto,
  type CrmQuoteDetailDto,
  type CrmQuoteDto,
  type CrmSalesOrderDto,
} from "@/shared/api/crm-sales-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function CrmQuotesPage() {
  const { can } = usePermissions();
  const canRead = can("crm.quote.read");
  const canManage = can("crm.quote.manage");
  const canOrder = can("crm.order.manage");

  const [list, setList] = useState<CrmQuoteDto[]>([]);
  const [priceLists, setPriceLists] = useState<CrmPriceListDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<CrmQuoteDetailDto | null>(null);
  const [lastOrder, setLastOrder] = useState<CrmSalesOrderDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [itemCode, setItemCode] = useState("SKU-01");
  const [itemName, setItemName] = useState("Gói dịch vụ");
  const [qty, setQty] = useState("1");
  const [price, setPrice] = useState("50000000");
  const [discount, setDiscount] = useState("5");
  const [plCode, setPlCode] = useState("PL-STD");
  const [plName, setPlName] = useState("Bảng giá chuẩn");
  const [plPrice, setPlPrice] = useState("45000000");
  const [selectedPl, setSelectedPl] = useState("");

  const load = useCallback(async () => {
    const [q, pl] = await Promise.all([fetchCrmQuotes(), fetchCrmPriceLists()]);
    setList(q);
    setPriceLists(pl);
    if (!selectedId && q[0]) setSelectedId(q[0].id);
    if (!selectedPl && pl[0]) setSelectedPl(pl[0].id);
  }, [selectedId, selectedPl]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchCrmQuoteDetail(selectedId).then(setDetail).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) setDetail(await fetchCrmQuoteDetail(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem báo giá.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Báo giá</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Dòng SP · bảng giá · duyệt CK · gửi Email/PDF · chuyển đơn (UC_CRM_070–074, 077)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách báo giá</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Mã</th><th className={th}>Tổng</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {list.map((q) => (
                  <tr key={q.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(q.id)}>
                    <td className={td}>
                      <div className="font-medium">{q.code}</div>
                      <div className="text-xs text-[var(--muted)]">{q.opportunityCode ?? "—"} · v{q.version}</div>
                    </td>
                    <td className={td}>{q.totalAmount.toLocaleString()}</td>
                    <td className={td}>
                      <span className={statusPill(
                        q.status === "Converted" ? "success" : q.status === "PendingDiscount" ? "danger" : "brand",
                      )}>{q.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {canManage && (
            <form className="mt-3 space-y-2 border-t border-black/10 pt-3" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(async () => {
                const pl = await upsertCrmPriceList({ code: plCode, name: plName });
                await upsertCrmPriceListItem(pl.id, {
                  itemCode, itemName, unitPrice: Number(plPrice) || 0,
                });
                setSelectedPl(pl.id);
              }, "Đã tạo/cập nhật bảng giá");
            }}>
              <div className="text-xs font-medium text-[var(--muted)]">Bảng giá (UC_072)</div>
              <div className="flex flex-wrap gap-2">
                <input className={field} value={plCode} onChange={(e) => setPlCode(e.target.value)} placeholder="Mã PL" />
                <input className={field} value={plName} onChange={(e) => setPlName(e.target.value)} placeholder="Tên" />
                <input className={field} value={plPrice} onChange={(e) => setPlPrice(e.target.value)} placeholder="Giá SKU" />
                <button className={btn.ghost} type="submit">Lưu bảng giá + dòng</button>
              </div>
              <div className="text-xs text-[var(--muted)]">
                PL hiện có: {priceLists.map((p) => `${p.code}(${p.itemCount})`).join(", ") || "—"}
              </div>
            </form>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chi tiết</h2>
          {detail ? (
            <div className="space-y-3 text-sm">
              <div>
                <b>{detail.quote.code}</b>
                <div className="text-xs text-[var(--muted)]">
                  Opp: {detail.quote.opportunityCode ?? "—"} · CK {detail.quote.discountPercent}%
                  ({detail.quote.discountApprovalStatus}) · Gửi: {detail.quote.sentChannel}
                </div>
                <div className="mt-1">
                  Sub {detail.quote.subTotal.toLocaleString()} − CK {detail.quote.discountAmount.toLocaleString()}
                  = <b>{detail.quote.totalAmount.toLocaleString()}</b>
                </div>
              </div>
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead><tr><th className={th}>SP</th><th className={th}>SL</th><th className={th}>Đơn giá</th><th className={th}>TT</th></tr></thead>
                  <tbody>
                    {detail.lines.map((l) => (
                      <tr key={l.id}>
                        <td className={td}>{l.itemCode} {l.itemName}</td>
                        <td className={td}>{l.quantity}</td>
                        <td className={td}>{l.unitPrice.toLocaleString()}</td>
                        <td className={td}>{l.lineAmount.toLocaleString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {canManage && detail.quote.status !== "Converted" && (
                <>
                  <form className="flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
                    e.preventDefault();
                    void run(() => upsertCrmQuoteLine(detail.quote.id, {
                      itemCode, itemName, quantity: Number(qty) || 1, unitPrice: Number(price) || 0,
                    }), "Đã thêm dòng");
                  }}>
                    <input className={field} value={itemCode} onChange={(e) => setItemCode(e.target.value)} />
                    <input className={field} value={itemName} onChange={(e) => setItemName(e.target.value)} />
                    <input className={field} value={qty} onChange={(e) => setQty(e.target.value)} />
                    <input className={field} value={price} onChange={(e) => setPrice(e.target.value)} />
                    <button className={btn.ghost} type="submit">Thêm dòng</button>
                  </form>
                  <div className="flex flex-wrap gap-2 items-center">
                    <select className={field} value={selectedPl} onChange={(e) => setSelectedPl(e.target.value)}>
                      <option value="">Chọn bảng giá</option>
                      {priceLists.map((p) => (
                        <option key={p.id} value={p.id}>{p.code} — {p.name}</option>
                      ))}
                    </select>
                    <button type="button" className={btn.ghost} disabled={!selectedPl} onClick={() => void run(
                      () => applyCrmPriceList(detail.quote.id, selectedPl), "Đã áp bảng giá",
                    )}>Áp giá</button>
                    <input className={field} value={discount} onChange={(e) => setDiscount(e.target.value)} />
                    <button type="button" className={btn.ghost} onClick={() => void run(
                      () => requestCrmQuoteDiscount(detail.quote.id, Number(discount) || 0), "Xin duyệt CK",
                    )}>Xin duyệt CK</button>
                    {detail.quote.discountApprovalStatus === "Pending" && (
                      <>
                        <button type="button" className={btn.primary} onClick={() => void run(
                          () => decideCrmQuoteDiscount(detail.quote.id, true), "Đã duyệt CK",
                        )}>Duyệt</button>
                        <button type="button" className={btn.ghost} onClick={() => void run(
                          () => decideCrmQuoteDiscount(detail.quote.id, false), "Từ chối CK",
                        )}>Từ chối</button>
                      </>
                    )}
                    <button type="button" className={btn.ghost} onClick={() => void run(
                      () => sendCrmQuote(detail.quote.id, "Email"), "Đã gửi Email",
                    )}>Gửi Email</button>
                    <button type="button" className={btn.ghost} onClick={() => void run(
                      () => sendCrmQuote(detail.quote.id, "Pdf"), "Đã gửi PDF",
                    )}>Gửi PDF</button>
                    {canOrder && (
                      <button type="button" className={btn.primary} onClick={() => void run(async () => {
                        const o = await convertCrmQuoteToOrder(detail.quote.id);
                        setLastOrder(o);
                      }, "Đã chuyển đơn")}>Chuyển đơn</button>
                    )}
                  </div>
                </>
              )}
              {lastOrder && (
                <div className="text-xs rounded-md border border-black/10 p-2">
                  Đơn {lastOrder.code} · {lastOrder.totalAmount.toLocaleString()} · {lastOrder.status}
                </div>
              )}
              {detail.quote.orderCode && (
                <div className="text-xs text-[var(--muted)]">Đã gắn đơn: {detail.quote.orderCode}</div>
              )}
            </div>
          ) : (
            <p className="text-sm text-[var(--muted)]">Tạo báo giá từ Cơ hội, rồi mở tại đây.</p>
          )}
        </section>
      </div>
    </div>
  );
}
