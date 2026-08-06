"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { fetchInvSkus, fetchInvWarehouses, type InvSkuDto, type InvWarehouseDto } from "@/shared/api/inv-api";
import {
  createInvTransfer,
  fetchInvTransferDetail,
  fetchInvTransfers,
  receiveInvTransfer,
  shipInvTransfer,
  upsertInvTransferLine,
  type InvTransferDetailDto,
  type InvTransferDto,
} from "@/shared/api/inv-stock-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function InvTransfersPage() {
  const { can } = usePermissions();
  const canRead = can("inv.stock.read");
  const canManage = can("inv.stock.manage");

  const [warehouses, setWarehouses] = useState<InvWarehouseDto[]>([]);
  const [skus, setSkus] = useState<InvSkuDto[]>([]);
  const [list, setList] = useState<InvTransferDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<InvTransferDetailDto | null>(null);
  const [fromId, setFromId] = useState("");
  const [toId, setToId] = useState("");
  const [skuId, setSkuId] = useState("");
  const [qty, setQty] = useState("1");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const load = useCallback(async () => {
    const [w, s, t] = await Promise.all([fetchInvWarehouses(), fetchInvSkus(), fetchInvTransfers()]);
    setWarehouses(w);
    setSkus(s.filter((x) => x.status === "Active"));
    setList(t);
    if (!fromId && w[0]) setFromId(w[0].id);
    if (!toId && w[1]) setToId(w[1].id);
    else if (!toId && w[0]) setToId(w[0].id);
    if (!skuId && s[0]) setSkuId(s.find((x) => x.status === "Active")?.id ?? "");
    if (!selectedId && t[0]) setSelectedId(t[0].id);
  }, [fromId, toId, skuId, selectedId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchInvTransferDetail(selectedId).then(setDetail).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) setDetail(await fetchInvTransferDetail(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem chuyển kho.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Chuyển kho</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Tạo phiếu · xuất kho gửi · theo dõi InTransit · nhập kho nhận (UC_INV_031, 033, 035)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Mã</th><th className={th}>Tuyến</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {list.map((t) => (
                  <tr key={t.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(t.id)}>
                    <td className={td}>{t.code}</td>
                    <td className={td}>{t.fromWarehouseName} → {t.toWarehouseName}</td>
                    <td className={td}>
                      <span className={statusPill(
                        t.status === "Completed" ? "success" : t.status === "InTransit" ? "warning" : "brand",
                      )}>{t.status}</span>
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
                const t = await createInvTransfer({ fromWarehouseId: fromId, toWarehouseId: toId });
                setSelectedId(t.id);
              }, "Đã tạo phiếu chuyển");
            }}>
              <select className={field} value={fromId} onChange={(e) => setFromId(e.target.value)}>
                {warehouses.map((w) => <option key={w.id} value={w.id}>Từ {w.code}</option>)}
              </select>
              <select className={field} value={toId} onChange={(e) => setToId(e.target.value)}>
                {warehouses.map((w) => <option key={w.id} value={w.id}>Đến {w.code}</option>)}
              </select>
              <button className={btn.primary} type="submit">Tạo</button>
            </form>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chi tiết</h2>
          {detail ? (
            <div className="space-y-3 text-sm">
              <div>
                <b>{detail.header.code}</b>
                <div className="text-xs text-[var(--muted)]">
                  {detail.header.fromWarehouseName} → {detail.header.toWarehouseName}
                </div>
              </div>
              <ul className="space-y-1">
                {detail.lines.map((l) => (
                  <li key={l.id}>{l.skuCode} · {l.qty}</li>
                ))}
              </ul>
              {canManage && detail.header.status === "Draft" && (
                <form className="flex flex-wrap gap-2" onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  void run(() => upsertInvTransferLine(detail.header.id, {
                    skuId, qty: Number(qty) || 1,
                  }), "Đã thêm dòng");
                }}>
                  <select className={field} value={skuId} onChange={(e) => setSkuId(e.target.value)}>
                    {skus.map((s) => <option key={s.id} value={s.id}>{s.code}</option>)}
                  </select>
                  <input className={field} value={qty} onChange={(e) => setQty(e.target.value)} />
                  <button className={btn.ghost} type="submit">Thêm</button>
                  <button type="button" className={btn.primary} onClick={() => void run(
                    () => shipInvTransfer(detail.header.id), "Đã ship → InTransit",
                  )}>Ship</button>
                </form>
              )}
              {canManage && detail.header.status === "InTransit" && (
                <button type="button" className={btn.primary} onClick={() => void run(
                  () => receiveInvTransfer(detail.header.id), "Đã nhận → Completed",
                )}>Nhận kho đích</button>
              )}
            </div>
          ) : (
            <p className="text-sm text-[var(--muted)]">Cần ≥ 2 kho để chuyển (hoặc cùng tenant nhiều kho).</p>
          )}
        </section>
      </div>
    </div>
  );
}
