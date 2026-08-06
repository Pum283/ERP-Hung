"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  addCrmOrderPayment,
  cancelCrmOrder,
  fetchCrmOrderDetail,
  fetchCrmOrders,
  holdCrmOrderStock,
  pushCrmOrderWarehouse,
  setCrmOrderStatus,
  type CrmSalesOrderDetailDto,
  type CrmSalesOrderDto,
} from "@/shared/api/crm-sales-api";
import {
  canHoldStock,
  canPushWarehouse,
  holdStatusTone,
  parseLogDeliveryRef,
  parseReservationRef,
  warehousePushTone,
} from "@/shared/api/crm-order-sync-helpers";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function CrmOrdersPage() {
  const { can } = usePermissions();
  const canRead = can("crm.order.read");
  const canManage = can("crm.order.manage");

  const [list, setList] = useState<CrmSalesOrderDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<CrmSalesOrderDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [payAmount, setPayAmount] = useState("");
  const [payMethod, setPayMethod] = useState("Transfer");
  const [cancelReason, setCancelReason] = useState("Khách hủy");

  const load = useCallback(async () => {
    const o = await fetchCrmOrders();
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
    fetchCrmOrderDetail(selectedId).then((d) => {
      setDetail(d);
      const remain = d.order.totalAmount - d.order.paidAmount;
      setPayAmount(remain > 0 ? String(remain) : "");
    }).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) {
        const d = await fetchCrmOrderDetail(selectedId);
        setDetail(d);
        const remain = d.order.totalAmount - d.order.paidAmount;
        setPayAmount(remain > 0 ? String(remain) : "");
      }
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem đơn hàng.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Đơn hàng bán</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Từ báo giá · trạng thái · giữ tồn INV thật · thanh toán · đẩy kho/LOG thật (UC_CRM_079, 081–082, 084, 087–088)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách đơn</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Đơn</th><th className={th}>Tổng / Đã TT</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {list.map((o) => (
                  <tr key={o.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(o.id)}>
                    <td className={td}>
                      <div className="font-medium">{o.code}</div>
                      <div className="text-xs text-[var(--muted)]">{o.quoteCode ?? "—"} · {o.customerName ?? "—"}</div>
                    </td>
                    <td className={td}>
                      {o.totalAmount.toLocaleString()}
                      <div className="text-xs text-[var(--muted)]">TT {o.paidAmount.toLocaleString()}</div>
                    </td>
                    <td className={td}>
                      <span className={statusPill(
                        o.status === "Cancelled" ? "danger" : o.status === "Delivered" ? "success" : "brand",
                      )}>{o.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chi tiết</h2>
          {detail ? (
            <div className="space-y-3 text-sm">
              <div>
                <b>{detail.order.code}</b>
                <div className="text-xs text-[var(--muted)]">
                  Giữ tồn:{" "}
                  <span className={statusPill(holdStatusTone(detail.order.stockHoldStatus))}>
                    {detail.order.stockHoldStatus}
                  </span>
                  {parseReservationRef(detail.order.note) && ` (${parseReservationRef(detail.order.note)})`}
                  {" · "}Kho:{" "}
                  <span className={statusPill(warehousePushTone(detail.order.warehousePushStatus))}>
                    {detail.order.warehousePushStatus}
                  </span>
                  {parseLogDeliveryRef(detail.order.note) && ` (lệnh giao ${parseLogDeliveryRef(detail.order.note)})`}
                </div>
                {detail.order.cancelReason && (
                  <div className="text-xs text-red-600">Hủy: {detail.order.cancelReason}</div>
                )}
              </div>
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead><tr><th className={th}>SP</th><th className={th}>SL</th><th className={th}>TT</th></tr></thead>
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
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead><tr><th className={th}>Thanh toán</th><th className={th}>Số tiền</th><th className={th}>HT</th></tr></thead>
                  <tbody>
                    {detail.payments.map((p) => (
                      <tr key={p.id}>
                        <td className={td}>{p.code}</td>
                        <td className={td}>{p.amount.toLocaleString()}</td>
                        <td className={td}>{p.method}</td>
                      </tr>
                    ))}
                    {detail.payments.length === 0 && (
                      <tr><td className={td} colSpan={3}>Chưa có thanh toán</td></tr>
                    )}
                  </tbody>
                </table>
              </div>
              {canManage && detail.order.status !== "Cancelled" && detail.order.status !== "Delivered" && (
                <>
                  <div className="flex flex-wrap gap-2">
                    {["Confirmed", "Released", "Delivered"].map((st) => (
                      <button key={st} type="button" className={btn.ghost} onClick={() => void run(
                        () => setCrmOrderStatus(detail.order.id, st), `→ ${st}`,
                      )}>{st}</button>
                    ))}
                    {canHoldStock(detail.order.status, detail.order.stockHoldStatus) && (
                      <button type="button" className={btn.ghost} onClick={() => void run(
                        () => holdCrmOrderStock(detail.order.id), "Đã giữ tồn INV (reservation Active).",
                      )}>Giữ tồn</button>
                    )}
                    {canPushWarehouse(detail.order.status, detail.order.warehousePushStatus) && (
                      <button type="button" className={btn.primary} onClick={() => void run(
                        () => pushCrmOrderWarehouse(detail.order.id), "Đã tạo lệnh giao LOG (Confirmed) + nhả giữ tồn.",
                      )}>Đẩy kho</button>
                    )}
                  </div>
                  <form className="flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
                    e.preventDefault();
                    void run(() => addCrmOrderPayment(detail.order.id, {
                      amount: Number(payAmount) || 0, method: payMethod,
                    }), "Đã ghi thanh toán");
                  }}>
                    <input className={field} value={payAmount} onChange={(e) => setPayAmount(e.target.value)} />
                    <select className={field} value={payMethod} onChange={(e) => setPayMethod(e.target.value)}>
                      <option value="Transfer">Transfer</option>
                      <option value="Cash">Cash</option>
                      <option value="Card">Card</option>
                      <option value="Other">Other</option>
                    </select>
                    <button className={btn.ghost} type="submit">Thanh toán</button>
                  </form>
                  <div className="flex flex-wrap gap-2">
                    <input className={field} value={cancelReason} onChange={(e) => setCancelReason(e.target.value)} />
                    <button type="button" className={btn.ghost} onClick={() => void run(
                      () => cancelCrmOrder(detail.order.id, cancelReason), "Đã hủy đơn",
                    )}>Hủy đơn</button>
                  </div>
                </>
              )}
            </div>
          ) : (
            <p className="text-sm text-[var(--muted)]">Chuyển đơn từ Báo giá để bắt đầu.</p>
          )}
        </section>
      </div>
    </div>
  );
}
