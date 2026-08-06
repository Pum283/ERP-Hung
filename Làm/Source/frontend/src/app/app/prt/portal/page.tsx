"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchPrtAccounts,
  fetchPrtArSummary,
  fetchPrtInvoices,
  fetchPrtOrderDetail,
  fetchPrtOrders,
  fetchPrtPayments,
  fetchPrtTickets,
  upsertPrtInvoice,
  upsertPrtOrder,
  upsertPrtPayment,
  upsertPrtTicket,
  type PrtAccountDto,
  type PrtArSummaryDto,
  type PrtInvoiceDto,
  type PrtOrderDetailDto,
  type PrtOrderDto,
  type PrtPaymentDto,
  type PrtTicketDto,
} from "@/shared/api/prt-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PrtPortalPage() {
  const { can } = usePermissions();
  const canRead = can("prt.portal.read");
  const canManage = can("prt.portal.manage");

  const [accounts, setAccounts] = useState<PrtAccountDto[]>([]);
  const [accountId, setAccountId] = useState("");
  const [orders, setOrders] = useState<PrtOrderDto[]>([]);
  const [orderDetail, setOrderDetail] = useState<PrtOrderDetailDto | null>(null);
  const [ar, setAr] = useState<PrtArSummaryDto | null>(null);
  const [invoices, setInvoices] = useState<PrtInvoiceDto[]>([]);
  const [payments, setPayments] = useState<PrtPaymentDto[]>([]);
  const [tickets, setTickets] = useState<PrtTicketDto[]>([]);
  const [selectedOrderId, setSelectedOrderId] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [itemCode, setItemCode] = useState("SKU-01");
  const [itemName, setItemName] = useState("Hàng demo");
  const [qty, setQty] = useState("2");
  const [price, setPrice] = useState("1500000");
  const [invAmount, setInvAmount] = useState("5000000");
  const [payAmount, setPayAmount] = useState("1000000");
  const [ticketSubject, setTicketSubject] = useState("Hỗ trợ giao hàng");

  const load = useCallback(async () => {
    const accs = await fetchPrtAccounts().catch(() => [] as PrtAccountDto[]);
    setAccounts(accs);
    const aid = accountId || accs[0]?.id || "";
    if (!accountId && aid) setAccountId(aid);
    if (!aid) { setOrders([]); setAr(null); setInvoices([]); setPayments([]); setTickets([]); return; }
    const [o, summary, inv, pay, t] = await Promise.all([
      fetchPrtOrders(aid),
      fetchPrtArSummary(aid),
      fetchPrtInvoices(aid, true),
      fetchPrtPayments(aid),
      fetchPrtTickets(aid),
    ]);
    setOrders(o); setAr(summary); setInvoices(inv); setPayments(pay); setTickets(t);
    if (!selectedOrderId && o[0]) setSelectedOrderId(o[0].id);
  }, [accountId, selectedOrderId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedOrderId || !canRead) return;
    fetchPrtOrderDetail(selectedOrderId).then(setOrderDetail).catch((e: Error) => setError(e.message));
  }, [selectedOrderId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedOrderId) setOrderDetail(await fetchPrtOrderDetail(selectedOrderId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem portal KH.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Đơn / công nợ / ticket</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Đơn hàng · AR · hóa đơn mở · thanh toán · ticket (UC_PRT_007–008, 014–016, 019–020)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="flex flex-wrap gap-2">
        <select className={field} value={accountId} onChange={(e) => setAccountId(e.target.value)}>
          <option value="">— Tài khoản portal —</option>
          {accounts.map((a) => <option key={a.id} value={a.id}>{a.email} ({a.customerCode ?? "—"})</option>)}
        </select>
        {ar && (
          <div className="text-sm self-center">
            Công nợ mở: <b>{ar.openAmount.toLocaleString()}</b> · {ar.openInvoiceCount} HĐ ·
            Đã TT YTD: {ar.paidYtd.toLocaleString()}
          </div>
        )}
      </div>

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Đơn hàng</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Mã</th><th className={th}>Tiền</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {orders.map((o) => (
                  <tr key={o.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedOrderId(o.id)}>
                    <td className={td}>{o.code}</td>
                    <td className={td}>{o.totalAmount.toLocaleString()}</td>
                    <td className={td}><span className={statusPill("brand")}>{o.status}</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {orderDetail && (
            <div className="mt-2 text-xs">
              Chi tiết {orderDetail.order.code}: {orderDetail.lines.map((l) => `${l.itemCode}×${l.quantity}`).join(", ")}
            </div>
          )}
          {canManage && accountId && (
            <form className="mt-3 flex flex-wrap gap-2 border-t border-black/10 pt-3" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(() => upsertPrtOrder({
                accountId,
                status: "Confirmed",
                lines: [{
                  itemCode, itemName, quantity: Number(qty) || 1, unitPrice: Number(price) || 0,
                }],
              }), "Đã tạo đơn");
            }}>
              <input className={field} value={itemCode} onChange={(e) => setItemCode(e.target.value)} />
              <input className={field} value={itemName} onChange={(e) => setItemName(e.target.value)} />
              <input className={field} value={qty} onChange={(e) => setQty(e.target.value)} />
              <input className={field} value={price} onChange={(e) => setPrice(e.target.value)} />
              <button className={btn.primary} type="submit">Tạo đơn</button>
            </form>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Công nợ & thanh toán</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>HĐ mở</th><th className={th}>Còn lại</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {invoices.map((i) => (
                  <tr key={i.id}>
                    <td className={td}>{i.code}</td>
                    <td className={td}>{i.openAmount.toLocaleString()}</td>
                    <td className={td}>{i.status}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="mt-2 text-xs text-[var(--muted)]">Lịch sử TT: {payments.length} dòng</div>
          <ul className="mt-1 text-xs">
            {payments.slice(0, 5).map((p) => (
              <li key={p.id}>{p.code} · {p.amount.toLocaleString()} · {p.method}</li>
            ))}
          </ul>
          {canManage && accountId && (
            <div className="mt-3 flex flex-wrap gap-2 border-t border-black/10 pt-3">
              <input className={field} value={invAmount} onChange={(e) => setInvAmount(e.target.value)} />
              <button type="button" className={btn.ghost} onClick={() => void run(
                () => upsertPrtInvoice({ accountId, amount: Number(invAmount) || 0 }),
                "Đã tạo HĐ",
              )}>
                Tạo HĐ
              </button>
              <input className={field} value={payAmount} onChange={(e) => setPayAmount(e.target.value)} />
              <button type="button" className={btn.primary} onClick={() => void run(
                () => upsertPrtPayment({
                  accountId,
                  invoiceId: invoices[0]?.id ?? null,
                  amount: Number(payAmount) || 0,
                }),
                "Đã ghi TT",
              )}>
                Ghi TT
              </button>
            </div>
          )}
        </section>

        <section className={`${panel} xl:col-span-2`}>
          <h2 className="mb-3 text-sm font-semibold">Ticket hỗ trợ</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Mã</th><th className={th}>Tiêu đề</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {tickets.map((t) => (
                  <tr key={t.id}>
                    <td className={td}>{t.code}</td>
                    <td className={td}>{t.subject}</td>
                    <td className={td}>
                      <span className={statusPill(t.status === "Open" ? "warning" : "success")}>{t.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {canManage && accountId && (
            <form className="mt-3 flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(() => upsertPrtTicket({
                accountId, subject: ticketSubject, description: "Ticket từ portal Cap-1",
              }), "Đã tạo ticket");
            }}>
              <input className={field} value={ticketSubject} onChange={(e) => setTicketSubject(e.target.value)} />
              <button className={btn.primary} type="submit">Tạo ticket</button>
            </form>
          )}
        </section>
      </div>
    </div>
  );
}
