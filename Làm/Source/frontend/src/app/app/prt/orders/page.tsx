"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchPrtAccounts,
  fetchPrtOrderDetail,
  fetchPrtOrders,
  upsertPrtOrder,
  type PrtAccountDto,
  type PrtOrderDetailDto,
  type PrtOrderDto,
} from "@/shared/api/prt-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PrtOrdersPage() {
  const { can } = usePermissions();
  const canRead = can("prt.portal.read");
  const canManage = can("prt.portal.manage");

  const [accounts, setAccounts] = useState<PrtAccountDto[]>([]);
  const [accountId, setAccountId] = useState("");
  const [orders, setOrders] = useState<PrtOrderDto[]>([]);
  const [selectedOrderId, setSelectedOrderId] = useState("");
  const [orderDetail, setOrderDetail] = useState<PrtOrderDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  // Form states
  const [itemCode, setItemCode] = useState("SKU-PROD-01");
  const [itemName, setItemName] = useState("Sản phẩm linh kiện ERP");
  const [qty, setQty] = useState("5");
  const [price, setPrice] = useState("2500000");

  const loadData = useCallback(async () => {
    try {
      setError(null);
      const accs = await fetchPrtAccounts().catch(() => [] as PrtAccountDto[]);
      setAccounts(accs);
      const aid = accountId || accs[0]?.id || "";
      if (!accountId && aid) setAccountId(aid);
      if (!aid) {
        setOrders([]);
        return;
      }
      const orderList = await fetchPrtOrders(aid);
      setOrders(orderList);
      if (!selectedOrderId && orderList[0]) setSelectedOrderId(orderList[0].id);
    } catch (e) {
      setError((e as Error).message);
    }
  }, [accountId, selectedOrderId]);

  useEffect(() => {
    if (!canRead) {
      setLoading(false);
      return;
    }
    setLoading(true);
    loadData().finally(() => setLoading(false));
  }, [canRead, loadData]);

  useEffect(() => {
    if (!selectedOrderId || !canRead) return;
    fetchPrtOrderDetail(selectedOrderId)
      .then(setOrderDetail)
      .catch((e: Error) => setError(e.message));
  }, [selectedOrderId, canRead]);

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 3000);
  }

  async function handleCreateOrder(e: FormEvent) {
    e.preventDefault();
    if (!accountId) return;
    try {
      await upsertPrtOrder({
        accountId,
        status: "Confirmed",
        lines: [
          {
            itemCode,
            itemName,
            quantity: Number(qty) || 1,
            unitPrice: Number(price) || 0,
          },
        ],
      });
      await loadData();
      flash("Đã tạo đơn hàng thành công!");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem Đơn hàng Portal.</div>;
  }

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-200 pb-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Quản Lý Đơn Hàng Khách Hàng (UC_PRT_007–008)</h1>
          <p className="mt-1 text-sm text-slate-500">
            Xem danh sách đơn hàng đã đặt, theo dõi tiến độ giao hàng và tạo đơn hàng mới trên Portal.
          </p>
        </div>
        <select
          className={`${field} min-w-[280px] font-semibold text-slate-900`}
          value={accountId}
          onChange={(e) => setAccountId(e.target.value)}
        >
          <option value="">— Chọn tài khoản khách hàng —</option>
          {accounts.map((a) => (
            <option key={a.id} value={a.id}>
              {a.displayName || a.email} ({a.customerCode ?? "Không mã"})
            </option>
          ))}
        </select>
      </div>

      {error && <div className="rounded-lg bg-red-50 p-4 text-sm font-medium text-red-800 border border-red-200">{error}</div>}
      {ok && <div className="rounded-lg bg-emerald-50 p-4 text-sm font-medium text-emerald-800 border border-emerald-200">{ok}</div>}

      <div className="grid gap-6 xl:grid-cols-3">
        {/* Orders Master List */}
        <section className={`${panel} xl:col-span-2 space-y-4`}>
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-bold text-slate-900">Danh Sách Đơn Hàng Portal</h2>
            <span className="text-xs font-semibold text-slate-500">{orders.length} đơn hàng</span>
          </div>

          {loading ? (
            <div className="p-6 text-center text-sm text-slate-500">Đang tải đơn hàng...</div>
          ) : orders.length === 0 ? (
            <div className="p-6 text-center text-sm text-slate-500">Chưa có đơn hàng nào cho tài khoản này.</div>
          ) : (
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã Đơn</th>
                    <th className={th}>Ngày Tạo</th>
                    <th className={th}>Tổng Giá Trị</th>
                    <th className={th}>Trạng Thái</th>
                    <th className={th}>Thao Tác</th>
                  </tr>
                </thead>
                <tbody>
                  {orders.map((o) => (
                    <tr
                      key={o.id}
                      className={`cursor-pointer transition-colors ${selectedOrderId === o.id ? "bg-indigo-50/80 font-medium" : "hover:bg-slate-50"}`}
                      onClick={() => setSelectedOrderId(o.id)}
                    >
                      <td className={`${td} font-bold text-indigo-950`}>{o.code}</td>
                      <td className={td}>{o.orderDate ? new Date(o.orderDate).toLocaleDateString("vi-VN") : "—"}</td>
                      <td className={`${td} font-bold text-slate-900`}>{o.totalAmount.toLocaleString("vi-VN")} ₫</td>
                      <td className={td}>
                        <span className={statusPill(o.status === "Confirmed" ? "brand" : "success")}>{o.status}</span>
                      </td>
                      <td className={td}>
                        <button type="button" className="text-xs font-semibold text-indigo-600 hover:underline">
                          Xem chi tiết →
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        {/* Order Detail & Create Form Side Panel */}
        <div className="space-y-6">
          {/* Create Order Form */}
          {canManage && accountId && (
            <section className={panel}>
              <h2 className="text-base font-bold text-slate-900 border-b border-slate-100 pb-2 mb-3">➕ Tạo Đơn Hàng Mới</h2>
              <form onSubmit={handleCreateOrder} className="space-y-3">
                <div>
                  <label className="text-xs font-medium text-slate-700">Mã SKU linh kiện</label>
                  <input className={field} value={itemCode} onChange={(e) => setItemCode(e.target.value)} required />
                </div>
                <div>
                  <label className="text-xs font-medium text-slate-700">Tên sản phẩm</label>
                  <input className={field} value={itemName} onChange={(e) => setItemName(e.target.value)} required />
                </div>
                <div className="grid grid-cols-2 gap-2">
                  <div>
                    <label className="text-xs font-medium text-slate-700">Số lượng</label>
                    <input className={field} type="number" value={qty} onChange={(e) => setQty(e.target.value)} required />
                  </div>
                  <div>
                    <label className="text-xs font-medium text-slate-700">Đơn giá (₫)</label>
                    <input className={field} type="number" value={price} onChange={(e) => setPrice(e.target.value)} required />
                  </div>
                </div>
                <button type="submit" className={`${btn.primary} w-full mt-2 justify-center`}>
                  🚀 Gửi Đơn Hàng
                </button>
              </form>
            </section>
          )}

          {/* Selected Order Detail */}
          {orderDetail && (
            <section className={panel}>
              <h2 className="text-base font-bold text-slate-900 border-b border-slate-100 pb-2 mb-3">
                📦 Đơn Hàng: {orderDetail.order.code}
              </h2>
              <div className="space-y-2 text-xs text-slate-600 mb-4">
                <div>Tổng tiền: <b className="text-sm font-bold text-slate-900">{orderDetail.order.totalAmount.toLocaleString("vi-VN")} ₫</b></div>
                <div>Trạng thái: <span className={statusPill("brand")}>{orderDetail.order.status}</span></div>
                <div>Địa chỉ giao: {orderDetail.order.shippingAddress || "Theo hợp đồng mặc định"}</div>
              </div>
              <div className="text-xs font-semibold text-slate-700 mb-2">Chi Tiết Mặt Hàng:</div>
              <div className="space-y-1.5">
                {orderDetail.lines.map((line, idx) => (
                  <div key={idx} className="flex justify-between items-center bg-slate-50 p-2 rounded border border-slate-200 text-xs">
                    <div>
                      <div className="font-semibold text-slate-900">{line.itemName}</div>
                      <div className="text-slate-500">{line.itemCode} × {line.quantity}</div>
                    </div>
                    <div className="font-bold text-slate-800">{line.amount.toLocaleString("vi-VN")} ₫</div>
                  </div>
                ))}
              </div>
            </section>
          )}
        </div>
      </div>
    </div>
  );
}
