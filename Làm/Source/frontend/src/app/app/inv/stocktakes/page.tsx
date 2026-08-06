"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { fetchInvWarehouses, type InvWarehouseDto } from "@/shared/api/inv-api";
import {
  countInvStocktakeLine,
  createInvStocktake,
  fetchInvStocktakeDetail,
  fetchInvStocktakes,
  postInvStocktake,
  reviewInvStocktake,
  type InvStocktakeDetailDto,
  type InvStocktakeDto,
} from "@/shared/api/inv-stock-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function InvStocktakesPage() {
  const { can } = usePermissions();
  const canRead = can("inv.stocktake.read");
  const canManage = can("inv.stocktake.manage");

  const [warehouses, setWarehouses] = useState<InvWarehouseDto[]>([]);
  const [list, setList] = useState<InvStocktakeDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<InvStocktakeDetailDto | null>(null);
  const [warehouseId, setWarehouseId] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const load = useCallback(async () => {
    const [w, s] = await Promise.all([fetchInvWarehouses(), fetchInvStocktakes()]);
    setWarehouses(w);
    setList(s);
    if (!warehouseId && w[0]) setWarehouseId(w[0].id);
    if (!selectedId && s[0]) setSelectedId(s[0].id);
  }, [warehouseId, selectedId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchInvStocktakeDetail(selectedId).then(setDetail).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) setDetail(await fetchInvStocktakeDetail(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem kiểm kê.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Kiểm kê</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Tạo phiếu · nhập đếm · đối chiếu lệch · duyệt điều chỉnh (UC_INV_049–050, 052–053)
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
              <thead><tr><th className={th}>Mã</th><th className={th}>Kho</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {list.map((s) => (
                  <tr key={s.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(s.id)}>
                    <td className={td}>
                      <div className="font-medium">{s.code}</div>
                      <div className="text-xs text-[var(--muted)]">{s.lineCount} dòng · lệch {s.varianceCount}</div>
                    </td>
                    <td className={td}>{s.warehouseName}</td>
                    <td className={td}>
                      <span className={statusPill(s.status === "Posted" ? "success" : "brand")}>{s.status}</span>
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
                const s = await createInvStocktake({ warehouseId });
                setSelectedId(s.id);
              }, "Đã tạo KK (snapshot tồn)");
            }}>
              <select className={field} value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)}>
                {warehouses.map((w) => <option key={w.id} value={w.id}>{w.code}</option>)}
              </select>
              <button className={btn.primary} type="submit">Tạo KK</button>
            </form>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chi tiết</h2>
          {detail ? (
            <div className="space-y-3 text-sm">
              <div>
                <b>{detail.header.code}</b> · {detail.header.warehouseName}
              </div>
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead><tr><th className={th}>SKU</th><th className={th}>Hệ thống</th><th className={th}>Đếm</th><th className={th}>Lệch</th><th className={th}></th></tr></thead>
                  <tbody>
                    {detail.lines.map((l) => (
                      <tr key={l.id}>
                        <td className={td}>{l.skuCode}</td>
                        <td className={td}>{l.systemQty}</td>
                        <td className={td}>{l.countedQty ?? "—"}</td>
                        <td className={td}>{l.varianceQty}</td>
                        <td className={td}>
                          {canManage && (detail.header.status === "Counting" || detail.header.status === "Draft") && (
                            <button type="button" className={btn.ghost} onClick={() => void run(
                              () => countInvStocktakeLine(detail.header.id, l.id, Math.max(0, l.systemQty - 1)),
                              "Đã đếm (system−1)",
                            )}>Đếm −1</button>
                          )}
                        </td>
                      </tr>
                    ))}
                    {detail.lines.length === 0 && (
                      <tr><td className={td} colSpan={5}>Kho chưa có tồn để snapshot</td></tr>
                    )}
                  </tbody>
                </table>
              </div>
              {canManage && detail.header.status === "Counting" && (
                <button type="button" className={btn.primary} onClick={() => void run(
                  () => reviewInvStocktake(detail.header.id), "Đã Reviewed",
                )}>Duyệt đối chiếu</button>
              )}
              {canManage && detail.header.status === "Reviewed" && (
                <button type="button" className={btn.primary} onClick={() => void run(
                  () => postInvStocktake(detail.header.id), "Đã post điều chỉnh",
                )}>Post điều chỉnh</button>
              )}
            </div>
          ) : (
            <p className="text-sm text-[var(--muted)]">Nhập tồn trước (GRN/phiếu nhập) rồi tạo KK.</p>
          )}
        </section>
      </div>
    </div>
  );
}
