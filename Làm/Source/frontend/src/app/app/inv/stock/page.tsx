"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { fetchInvSkus, fetchInvWarehouses, upsertInvWarehouse, type InvSkuDto, type InvWarehouseDto } from "@/shared/api/inv-api";
import {
  activateInvReservation,
  createInvDoc,
  createInvReservation,
  fetchInvBalances,
  fetchInvDocDetail,
  fetchInvDocs,
  fetchInvReservations,
  postInvDoc,
  releaseInvReservation,
  upsertInvDocLine,
  type InvBalanceDto,
  type InvReservationDto,
  type InvStockDocDetailDto,
  type InvStockDocDto,
} from "@/shared/api/inv-stock-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function InvStockPage() {
  const { can } = usePermissions();
  const canRead = can("inv.stock.read");
  const canManage = can("inv.stock.manage");
  const canWh = can("inv.warehouse.manage");

  const [warehouses, setWarehouses] = useState<InvWarehouseDto[]>([]);
  const [skus, setSkus] = useState<InvSkuDto[]>([]);
  const [balances, setBalances] = useState<InvBalanceDto[]>([]);
  const [docs, setDocs] = useState<InvStockDocDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<InvStockDocDetailDto | null>(null);
  const [warehouseId, setWarehouseId] = useState("");
  const [skuId, setSkuId] = useState("");
  const [docType, setDocType] = useState("Receipt");
  const [sourceType, setSourceType] = useState("Adjustment");
  const [qty, setQty] = useState("1");
  const [lot, setLot] = useState("");
  const [expiry, setExpiry] = useState("");
  const [refCode, setRefCode] = useState("SO-DEMO");
  const [reserveQty, setReserveQty] = useState("1");
  const [reservations, setReservations] = useState<InvReservationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const load = useCallback(async () => {
    const [w, s, b, d, r] = await Promise.all([
      fetchInvWarehouses(), fetchInvSkus(), fetchInvBalances(), fetchInvDocs(),
      fetchInvReservations().catch(() => [] as InvReservationDto[]),
    ]);
    setWarehouses(w);
    setSkus(s.filter((x) => x.status === "Active"));
    setBalances(b);
    setDocs(d);
    setReservations(r);
    if (!warehouseId && w[0]) setWarehouseId(w[0].id);
    if (!skuId && s[0]) setSkuId(s.find((x) => x.status === "Active")?.id ?? "");
    if (!selectedId && d[0]) setSelectedId(d[0].id);
  }, [warehouseId, skuId, selectedId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchInvDocDetail(selectedId).then(setDetail).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) setDetail(await fetchInvDocDetail(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem tồn kho.</div>;
  }

  const wh = warehouses.find((w) => w.id === warehouseId);

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Tồn & phiếu kho</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          FIFO/FEFO · tồn âm · tồn thực tế · nhập/xuất điều chỉnh · lô (UC_INV_015–017, 019, 022, 026, 030, 039)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      {canWh && wh && (
        <section className={`${panel} flex flex-wrap items-center gap-2 text-sm`}>
          <span className="font-medium">{wh.code}</span>
          <select className={field} value={wh.pickPolicy} onChange={(e) => void run(() => upsertInvWarehouse({
            id: wh.id, code: wh.code, name: wh.name, warehouseTypeId: wh.warehouseTypeId,
            address: wh.address, status: wh.status, pickPolicy: e.target.value,
            allowNegativeStock: wh.allowNegativeStock,
          }), "Đã cập nhật PickPolicy")}>
            <option value="Fifo">Fifo</option>
            <option value="Fefo">Fefo</option>
          </select>
          <label className="flex items-center gap-1 text-xs">
            <input type="checkbox" checked={wh.allowNegativeStock} onChange={(e) => void run(() => upsertInvWarehouse({
              id: wh.id, code: wh.code, name: wh.name, warehouseTypeId: wh.warehouseTypeId,
              address: wh.address, status: wh.status, pickPolicy: wh.pickPolicy,
              allowNegativeStock: e.target.checked,
            }), "Đã cập nhật tồn âm")} />
            Cho tồn âm
          </label>
          <select className={field} value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)}>
            {warehouses.map((w) => <option key={w.id} value={w.id}>{w.code}</option>)}
          </select>
        </section>
      )}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Tồn theo lô (UC_INV_043)</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>SKU / Lô</th><th className={th}>HSD</th>
                  <th className={th}>OnHand</th><th className={th}>Reserved</th><th className={th}>Avail</th>
                </tr>
              </thead>
              <tbody>
                {balances.map((b) => (
                  <tr key={b.id}>
                    <td className={td}>
                      <div>{b.skuCode}</div>
                      <div className="text-xs text-[var(--muted)]">{b.warehouseName}{b.lotCode ? ` · ${b.lotCode}` : ""}</div>
                    </td>
                    <td className={td}>{b.expiryDate ?? "—"}</td>
                    <td className={td}>{b.qtyOnHand}</td>
                    <td className={td}>{b.qtyReserved}</td>
                    <td className={td}>{b.qtyAvailable}</td>
                  </tr>
                ))}
                {balances.length === 0 && (
                  <tr><td className={td} colSpan={5}>Chưa có tồn — nhập kho hoặc post GRN PUR</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Phiếu nhập / xuất</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Phiếu</th><th className={th}>Loại</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {docs.map((d) => (
                  <tr key={d.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(d.id)}>
                    <td className={td}>
                      <div className="font-medium">{d.code}</div>
                      <div className="text-xs text-[var(--muted)]">{d.refCode ?? d.sourceType}</div>
                    </td>
                    <td className={td}>{d.docType}</td>
                    <td className={td}>
                      <span className={statusPill(d.status === "Posted" ? "success" : "brand")}>{d.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {canManage && (
            <form className="mt-3 flex flex-wrap gap-2 border-t border-black/10 pt-3" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(async () => {
                const d = await createInvDoc({
                  docType, sourceType: docType === "Receipt" ? (sourceType === "Internal" ? "Adjustment" : sourceType) : (sourceType === "Purchase" ? "Internal" : sourceType),
                  warehouseId,
                });
                setSelectedId(d.id);
              }, "Đã tạo phiếu");
            }}>
              <select className={field} value={docType} onChange={(e) => {
                setDocType(e.target.value);
                setSourceType(e.target.value === "Receipt" ? "Adjustment" : "Internal");
              }}>
                <option value="Receipt">Receipt</option>
                <option value="Issue">Issue</option>
              </select>
              <select className={field} value={sourceType} onChange={(e) => setSourceType(e.target.value)}>
                {docType === "Receipt" ? (
                  <>
                    <option value="Adjustment">Adjustment</option>
                    <option value="Purchase">Purchase</option>
                  </>
                ) : (
                  <>
                    <option value="Internal">Internal</option>
                    <option value="Adjustment">Adjustment</option>
                  </>
                )}
              </select>
              <button className={btn.primary} type="submit">Tạo phiếu</button>
            </form>
          )}

          {detail && (
            <div className="mt-3 space-y-2 border-t border-black/10 pt-3 text-sm">
              <div><b>{detail.header.code}</b> · {detail.header.sourceType}</div>
              <ul className="space-y-1">
                {detail.lines.map((l) => (
                  <li key={l.id}>{l.skuCode} · {l.qty}{l.lotCode ? ` · ${l.lotCode}` : ""}</li>
                ))}
              </ul>
              {canManage && detail.header.status === "Draft" && (
                <form className="flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  void run(() => upsertInvDocLine(detail.header.id, {
                    skuId, qty: Number(qty) || 1,
                    lotCode: lot || undefined,
                    expiryDate: expiry || undefined,
                  }), "Đã thêm dòng");
                }}>
                  <select className={field} value={skuId} onChange={(e) => setSkuId(e.target.value)}>
                    {skus.map((s) => <option key={s.id} value={s.id}>{s.code}</option>)}
                  </select>
                  <input className={field} value={qty} onChange={(e) => setQty(e.target.value)} />
                  <input className={field} value={lot} onChange={(e) => setLot(e.target.value)} placeholder={detail.header.docType === "Issue" ? "Lot trống = FEFO" : "Lot"} />
                  <input className={field} type="date" value={expiry} onChange={(e) => setExpiry(e.target.value)} title="HSD" />
                  <button className={btn.ghost} type="submit">Thêm dòng</button>
                  <button type="button" className={btn.primary} onClick={() => void run(
                    () => postInvDoc(detail.header.id), "Đã post phiếu (FEFO/chặn HSD)",
                  )}>Post</button>
                </form>
              )}
            </div>
          )}
        </section>
      </div>

      <section className={panel}>
        <h2 className="mb-3 text-sm font-semibold">Giữ hàng (UC_INV_037 · 038 · 042)</h2>
        {canManage && (
          <form
            className="mb-3 grid gap-2 sm:grid-cols-5"
            onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(
                () => createInvReservation({
                  warehouseId,
                  refCode,
                  activate: true,
                  lines: [{ skuId, qty: Number(reserveQty) || 1, lotCode: lot || undefined }],
                }),
                "Đã giữ hàng (Active).",
              );
            }}
          >
            <select className={field} value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)}>
              {warehouses.map((w) => <option key={w.id} value={w.id}>{w.code}</option>)}
            </select>
            <select className={field} value={skuId} onChange={(e) => setSkuId(e.target.value)}>
              {skus.map((s) => <option key={s.id} value={s.id}>{s.code}</option>)}
            </select>
            <input className={field} value={reserveQty} onChange={(e) => setReserveQty(e.target.value)} placeholder="SL giữ" />
            <input className={field} value={refCode} onChange={(e) => setRefCode(e.target.value)} placeholder="Ref đơn" />
            <button type="submit" className={btn.primary} disabled={!warehouseId || !skuId}>Giữ hàng</button>
          </form>
        )}
        <div className={tableWrap}>
          <table className="w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Mã</th><th className={th}>Kho</th><th className={th}>Ref</th>
                <th className={th}>TT</th><th className={th}></th>
              </tr>
            </thead>
            <tbody>
              {reservations.map((r) => (
                <tr key={r.id}>
                  <td className={td}>{r.code}</td>
                  <td className={td}>{r.warehouseName}</td>
                  <td className={td}>{r.refCode ?? "—"}</td>
                  <td className={td}><span className={statusPill(r.status === "Active" ? "warning" : r.status === "Released" ? "muted" : "success")}>{r.status}</span></td>
                  <td className={td}>
                    {canManage && r.status === "Draft" && (
                      <button type="button" className={btn.ghost} onClick={() => void run(() => activateInvReservation(r.id), "Đã Activate.")}>Activate</button>
                    )}
                    {canManage && r.status === "Active" && (
                      <button type="button" className={btn.ghost} onClick={() => void run(() => releaseInvReservation(r.id), "Đã Release.")}>Release</button>
                    )}
                  </td>
                </tr>
              ))}
              {reservations.length === 0 && <tr><td className={td} colSpan={5}>Chưa có phiếu giữ hàng.</td></tr>}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}
