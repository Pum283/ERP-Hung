"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { fetchPosProducts, type PosProductDto } from "@/shared/api/pos-api";
import {
  addPosReturnLine,
  cancelPosSale,
  cancelPosSaleLine,
  completePosReturn,
  createPosReturn,
  fetchPosSaleDetail,
  fetchPosSales,
  fetchPosShifts,
  downloadPosReceipt,
  holdPosSale,
  openPosSale,
  payPosSale,
  resumePosSale,
  upsertPosSaleLine,
  fetchPosStockAlerts,
  type PosSaleDetailDto,
  type PosSaleDto,
  type PosShiftDto,
  type PosStockAlertDto,
} from "@/shared/api/pos-sales-api";
import { buildReceiptFilename, canPrintReceipt } from "@/shared/api/pos-doc-helpers";
import { summarizePosStockAlerts } from "@/shared/api/pos-stock-helpers";
import {
  applyPosPromotion,
  applyPosVoucher,
  clearPosDiscount,
  decidePosManualDiscount,
  fetchPosPromotions,
  requestPosManualDiscount,
  type PosPromotionDto,
} from "@/shared/api/pos-promo-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PosSellPage() {
  const { can } = usePermissions();
  const canRead = can("pos.sale.read");
  const canManage = can("pos.sale.manage");
  const canPromoManage = can("pos.promo.manage");

  const [shifts, setShifts] = useState<PosShiftDto[]>([]);
  const [products, setProducts] = useState<PosProductDto[]>([]);
  const [promos, setPromos] = useState<PosPromotionDto[]>([]);
  const [list, setList] = useState<PosSaleDto[]>([]);
  const [shiftId, setShiftId] = useState("");
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<PosSaleDetailDto | null>(null);
  const [productId, setProductId] = useState("");
  const [qty, setQty] = useState("1");
  const [area, setArea] = useState("Quầy 1");
  const [payMethod, setPayMethod] = useState("Cash");
  const [payAmount, setPayAmount] = useState("");
  const [returnId, setReturnId] = useState("");
  const [promoId, setPromoId] = useState("");
  const [voucherCode, setVoucherCode] = useState("SAVE10");
  const [manualType, setManualType] = useState("Percent");
  const [manualVal, setManualVal] = useState("5");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [stockAlerts, setStockAlerts] = useState<PosStockAlertDto[]>([]);

  const load = useCallback(async () => {
    const [sh, pr, sales, pm] = await Promise.all([
      fetchPosShifts({ status: "Open" }),
      fetchPosProducts(),
      fetchPosSales(shiftId ? { shiftId } : undefined),
      fetchPosPromotions().catch(() => [] as PosPromotionDto[]),
    ]);
    setShifts(sh);
    setProducts(pr.filter((p) => p.status === "Active"));
    setList(sales);
    setPromos(pm.filter((p) => p.status === "Active"));
    if (!shiftId && sh[0]) setShiftId(sh[0].id);
    if (!productId && pr[0]) setProductId(pr.find((p) => p.status === "Active")?.id ?? "");
    if (!selectedId && sales[0]) setSelectedId(sales[0].id);
    if (!promoId && pm[0]) setPromoId(pm.find((p) => p.status === "Active")?.id ?? "");
    const storeId = (shiftId ? sh.find((x) => x.id === shiftId)?.storeId : sh[0]?.storeId) ?? undefined;
    const alerts = await fetchPosStockAlerts(storeId).catch(() => [] as PosStockAlertDto[]);
    setStockAlerts(alerts);
  }, [shiftId, selectedId, productId, promoId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchPosSaleDetail(selectedId).then((d) => {
      setDetail(d);
      const remain = d.sale.totalAmount - d.sale.paidAmount;
      setPayAmount(remain > 0 ? String(remain) : "");
    }).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      const result = await action();
      await load();
      if (selectedId) {
        const d = await fetchPosSaleDetail(selectedId);
        setDetail(d);
        const remain = d.sale.totalAmount - d.sale.paidAmount;
        setPayAmount(remain > 0 ? String(remain) : "");
      }
      flash(msg);
      return result;
    } catch (err) {
      setError((err as Error).message);
      return null;
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền bán hàng POS.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Bán hàng POS</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Mở đơn · SP · TT · trừ tồn BOM khi Paid (054) · cảnh báo tồn (055) · trả hàng
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      {stockAlerts.length > 0 && (() => {
        const s = summarizePosStockAlerts(stockAlerts);
        return (
          <div className={`${panel} border-amber-200 bg-amber-50/60`}>
            <div className="mb-2 text-sm font-semibold text-amber-900">
              Cảnh báo tồn (UC_POS_055) — hết {s.outOfStock} · dưới min {s.belowMin} · gần reorder {s.nearReorder}
            </div>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>SKU</th>
                    <th className={th}>Kho</th>
                    <th className={th}>Tồn</th>
                    <th className={th}>Loại</th>
                  </tr>
                </thead>
                <tbody>
                  {stockAlerts.slice(0, 8).map((a) => (
                    <tr key={`${a.warehouseId}-${a.skuId}-${a.alertType}`}>
                      <td className={td}>{a.skuCode} · {a.skuName}</td>
                      <td className={td}>{a.warehouseName ?? "—"}</td>
                      <td className={td}>{a.qtyOnHand}</td>
                      <td className={td}>
                        <span className={statusPill(a.alertType === "OutOfStock" || a.alertType === "BelowMin" ? "danger" : "warning")}>
                          {a.alertType}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        );
      })()}

      {!shifts.length && (
        <div className="rounded-md bg-amber-50 px-3 py-2 text-sm text-amber-800">
          Chưa có ca Open — mở ca tại menu Ca thu ngân trước.
        </div>
      )}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Đơn trong ca</h2>
          <div className="mb-2 flex flex-wrap gap-2">
            <select className={field} value={shiftId} onChange={(e) => { setShiftId(e.target.value); setSelectedId(""); }}>
              {shifts.map((s) => (
                <option key={s.id} value={s.id}>{s.code} — {s.storeName}</option>
              ))}
            </select>
            {canManage && (
              <button type="button" className={btn.primary} disabled={!shiftId} onClick={() => void run(async () => {
                const s = await openPosSale({ shiftId, areaName: area });
                setSelectedId(s.id);
              }, "Đã mở đơn")}>Mở đơn</button>
            )}
            <input className={field} value={area} onChange={(e) => setArea(e.target.value)} placeholder="Khu vực" />
          </div>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Đơn</th><th className={th}>Tổng</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {list.map((o) => (
                  <tr key={o.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(o.id)}>
                    <td className={td}>
                      <div className="font-medium">{o.code}</div>
                      <div className="text-xs text-[var(--muted)]">{o.areaName ?? "—"} · {o.lineCount} dòng</div>
                    </td>
                    <td className={td}>{o.totalAmount.toLocaleString()}</td>
                    <td className={td}>
                      <span className={statusPill(
                        o.status === "Paid" ? "success" : o.status === "Cancelled" ? "danger" : "brand",
                      )}>{o.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chi tiết đơn</h2>
          {detail ? (
            <div className="space-y-3 text-sm">
              <div>
                <b>{detail.sale.code}</b>
                <div className="text-xs text-[var(--muted)]">
                  Sub {detail.sale.subTotal.toLocaleString()} + thuế {detail.sale.taxAmount.toLocaleString()}
                  {detail.sale.discountAmount > 0 && ` − KM ${detail.sale.discountAmount.toLocaleString()}`}
                  = <b>{detail.sale.totalAmount.toLocaleString()}</b>
                  · Đã TT {detail.sale.paidAmount.toLocaleString()}
                  {detail.sale.returnedAmount > 0 && ` · Đã trả ${detail.sale.returnedAmount.toLocaleString()}`}
                </div>
                {detail.sale.discountSource !== "None" && (
                  <div className="mt-1 text-xs">
                    <span className={statusPill(
                      detail.sale.discountApprovalStatus === "Pending" ? "warning"
                        : detail.sale.discountApprovalStatus === "Rejected" ? "danger" : "brand",
                    )}>
                      {detail.sale.discountSource}
                      {detail.sale.promotionCode ? ` · ${detail.sale.promotionCode}` : ""}
                      {detail.sale.appliedVoucherCode ? ` · ${detail.sale.appliedVoucherCode}` : ""}
                      {detail.sale.discountApprovalStatus !== "None"
                        ? ` · ${detail.sale.discountApprovalStatus}`
                        : ""}
                    </span>
                  </div>
                )}
              </div>
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead><tr><th className={th}>SP</th><th className={th}>SL</th><th className={th}>TT</th><th className={th}></th></tr></thead>
                  <tbody>
                    {detail.lines.map((l) => (
                      <tr key={l.id} className={l.status === "Cancelled" ? "opacity-40" : undefined}>
                        <td className={td}>{l.productCode} {l.productName}</td>
                        <td className={td}>{l.quantity}</td>
                        <td className={td}>{l.lineAmount.toLocaleString()}</td>
                        <td className={td}>
                          {canManage && detail.sale.status === "Open" && l.status === "Active" && (
                            <button type="button" className={btn.ghost} onClick={() => void run(
                              () => cancelPosSaleLine(detail.sale.id, l.id), "Đã hủy dòng",
                            )}>Hủy</button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {canManage && detail.sale.status === "Open" && (
                <form className="flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  void run(() => upsertPosSaleLine(detail.sale.id, {
                    productId: productId || undefined,
                    quantity: Number(qty) || 1,
                  }), "Đã thêm SP");
                }}>
                  <select className={field} value={productId} onChange={(e) => setProductId(e.target.value)}>
                    {products.map((p) => (
                      <option key={p.id} value={p.id}>{p.code} — {p.name}</option>
                    ))}
                  </select>
                  <input className={field} value={qty} onChange={(e) => setQty(e.target.value)} />
                  <button className={btn.ghost} type="submit">Thêm SP</button>
                </form>
              )}

              {canManage && detail.sale.status === "Open" && (
                <div className="space-y-2 rounded-md border border-black/10 p-3">
                  <div className="text-xs font-semibold uppercase text-[var(--muted)]">Khuyến mại</div>
                  <div className="flex flex-wrap gap-2">
                    <select className={field} value={promoId} onChange={(e) => setPromoId(e.target.value)}>
                      <option value="">— CTKM —</option>
                      {promos.map((p) => (
                        <option key={p.id} value={p.id}>
                          {p.code} · {p.discountType === "Percent" ? `${p.discountValue}%` : p.discountValue}
                        </option>
                      ))}
                    </select>
                    <button
                      type="button"
                      className={btn.ghost}
                      disabled={!promoId}
                      onClick={() => void run(() => applyPosPromotion(detail.sale.id, promoId), "Đã áp CTKM")}
                    >
                      Áp CTKM
                    </button>
                    <input
                      className={`${field} w-28`}
                      value={voucherCode}
                      onChange={(e) => setVoucherCode(e.target.value)}
                      placeholder="Voucher"
                    />
                    <button
                      type="button"
                      className={btn.ghost}
                      onClick={() => void run(() => applyPosVoucher(detail.sale.id, voucherCode), "Đã áp voucher")}
                    >
                      Áp voucher
                    </button>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    <select className={field} value={manualType} onChange={(e) => setManualType(e.target.value)}>
                      <option value="Percent">% tay</option>
                      <option value="Amount">Số tiền tay</option>
                    </select>
                    <input className={`${field} w-24`} value={manualVal} onChange={(e) => setManualVal(e.target.value)} />
                    <button
                      type="button"
                      className={btn.ghost}
                      onClick={() => void run(
                        () => requestPosManualDiscount(detail.sale.id, manualType, Number(manualVal) || 0, "Giảm tay quầy"),
                        "Đã gửi duyệt giảm tay",
                      )}
                    >
                      Xin giảm tay
                    </button>
                    {canPromoManage && detail.sale.discountApprovalStatus === "Pending" && (
                      <>
                        <button
                          type="button"
                          className={btn.primary}
                          onClick={() => void run(
                            () => decidePosManualDiscount(detail.sale.id, true), "Đã duyệt giảm tay",
                          )}
                        >
                          Duyệt
                        </button>
                        <button
                          type="button"
                          className={btn.ghost}
                          onClick={() => void run(
                            () => decidePosManualDiscount(detail.sale.id, false, "Từ chối"), "Đã từ chối",
                          )}
                        >
                          Từ chối
                        </button>
                      </>
                    )}
                    {detail.sale.discountSource !== "None" && (
                      <button
                        type="button"
                        className={btn.ghost}
                        onClick={() => void run(() => clearPosDiscount(detail.sale.id), "Đã gỡ KM")}
                      >
                        Gỡ KM
                      </button>
                    )}
                  </div>
                </div>
              )}

              {canManage && (
                <div className="flex flex-wrap gap-2">
                  {detail.sale.status === "Open" && (
                    <>
                      <button type="button" className={btn.ghost} onClick={() => void run(
                        () => holdPosSale(detail.sale.id), "Đã giữ đơn",
                      )}>Giữ đơn</button>
                      <button type="button" className={btn.ghost} onClick={() => void run(
                        () => cancelPosSale(detail.sale.id, "Hủy bill"), "Đã hủy bill",
                      )}>Hủy bill</button>
                      <select className={field} value={payMethod} onChange={(e) => setPayMethod(e.target.value)}>
                        <option value="Cash">Cash</option>
                        <option value="Transfer">Transfer/QR</option>
                        <option value="Card">Card</option>
                        <option value="Wallet">Wallet</option>
                      </select>
                      <input className={field} value={payAmount} onChange={(e) => setPayAmount(e.target.value)} />
                      <button
                        type="button"
                        className={btn.primary}
                        disabled={detail.sale.discountApprovalStatus === "Pending"}
                        onClick={() => void run(
                          () => payPosSale(
                            detail.sale.id,
                            payMethod,
                            Number(payAmount) || detail.sale.totalAmount - detail.sale.paidAmount,
                          ),
                          "Đã thanh toán",
                        )}
                      >
                        Thanh toán
                      </button>
                    </>
                  )}
                  {detail.sale.status === "Held" && (
                    <button type="button" className={btn.primary} onClick={() => void run(
                      () => resumePosSale(detail.sale.id), "Đã mở lại đơn",
                    )}>Mở lại</button>
                  )}
                  {canPrintReceipt(detail.sale.status) && (
                    <>
                      <button type="button" className={btn.ghost} onClick={() => void run(
                        () => downloadPosReceipt(detail.sale.id, buildReceiptFilename(detail.sale.code)),
                        "Đã tải hóa đơn bán lẻ (text 42 cột).",
                      )}>In hóa đơn</button>
                      <button type="button" className={btn.ghost} onClick={() => void run(async () => {
                        const r = await createPosReturn(detail.sale.id, "Khách trả");
                        setReturnId(r.id);
                        const active = detail.lines.find((l) => l.status === "Active");
                        if (active) await addPosReturnLine(r.id, active.id, active.quantity);
                        await completePosReturn(r.id, "Cash", "Hoàn tiền mặt");
                      }, "Đã trả hàng / hoàn tiền")}>Trả hàng</button>
                    </>
                  )}
                </div>
              )}
              {returnId && <div className="text-xs text-[var(--muted)]">Phiếu trả gần nhất: {returnId}</div>}
              {detail.payments.length > 0 && (
                <div className="text-xs text-[var(--muted)]">
                  TT: {detail.payments.map((p) => `${p.method} ${p.amount.toLocaleString()}`).join(" · ")}
                </div>
              )}
            </div>
          ) : (
            <p className="text-sm text-[var(--muted)]">Mở ca rồi tạo đơn để bán.</p>
          )}
        </section>
      </div>
    </div>
  );
}
