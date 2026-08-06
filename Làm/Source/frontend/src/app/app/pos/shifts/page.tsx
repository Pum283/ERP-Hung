"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { fetchPosStores, type PosStoreDto } from "@/shared/api/pos-api";
import {
  closePosShift,
  fetchPosShiftDetail,
  fetchPosShifts,
  openPosShift,
  printPosShiftReport,
  type PosShiftDetailDto,
  type PosShiftDto,
} from "@/shared/api/pos-sales-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PosShiftsPage() {
  const { can } = usePermissions();
  const canRead = can("pos.shift.read");
  const canManage = can("pos.shift.manage");

  const [stores, setStores] = useState<PosStoreDto[]>([]);
  const [list, setList] = useState<PosShiftDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<PosShiftDetailDto | null>(null);
  const [storeId, setStoreId] = useState("");
  const [openingCash, setOpeningCash] = useState("500000");
  const [closingCash, setClosingCash] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const load = useCallback(async () => {
    const [s, sh] = await Promise.all([fetchPosStores(), fetchPosShifts()]);
    setStores(s);
    setList(sh);
    if (!storeId && s[0]) setStoreId(s[0].id);
    if (!selectedId && sh[0]) setSelectedId(sh[0].id);
  }, [selectedId, storeId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchPosShiftDetail(selectedId).then((d) => {
      setDetail(d);
      if (d.shift.expectedCash != null) setClosingCash(String(d.shift.expectedCash));
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
      if (selectedId) setDetail(await fetchPosShiftDetail(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem ca thu ngân.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Ca thu ngân</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Mở ca · tiền đầu · doanh thu · đóng ca / lệch quỹ · in BC (UC_POS_042–043, 045–048)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách ca</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Ca</th><th className={th}>DT</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {list.map((s) => (
                  <tr key={s.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(s.id)}>
                    <td className={td}>
                      <div className="font-medium">{s.code}</div>
                      <div className="text-xs text-[var(--muted)]">{s.storeName} · {s.cashierName ?? "—"}</div>
                    </td>
                    <td className={td}>{s.salesTotal.toLocaleString()}</td>
                    <td className={td}>
                      <span className={statusPill(s.status === "Open" ? "brand" : "success")}>{s.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {canManage && (
            <form className="mt-3 flex flex-wrap gap-2 border-t border-black/10 pt-3" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(() => openPosShift({
                storeId, openingCash: Number(openingCash) || 0,
              }), "Đã mở ca");
            }}>
              <select className={field} value={storeId} onChange={(e) => setStoreId(e.target.value)}>
                {stores.map((s) => <option key={s.id} value={s.id}>{s.code} — {s.name}</option>)}
              </select>
              <input className={field} value={openingCash} onChange={(e) => setOpeningCash(e.target.value)} placeholder="Tiền đầu ca" />
              <button className={btn.primary} type="submit">Mở ca</button>
            </form>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chi tiết ca</h2>
          {detail ? (
            <div className="space-y-3 text-sm">
              <div>
                <b>{detail.shift.code}</b> — {detail.shift.storeName}
                <div className="text-xs text-[var(--muted)]">
                  Đầu ca: {detail.shift.openingCash.toLocaleString()} · Cash bán: {detail.shift.cashSalesTotal.toLocaleString()}
                </div>
                {detail.shift.status === "Closed" && (
                  <div className="mt-1 text-xs">
                    Kỳ vọng: {detail.shift.expectedCash?.toLocaleString()} · Đếm: {detail.shift.closingCashCounted?.toLocaleString()}
                    · Lệch: <b>{detail.shift.variance?.toLocaleString()}</b>
                  </div>
                )}
              </div>
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead><tr><th className={th}>Đơn</th><th className={th}>Tổng</th><th className={th}>TT</th></tr></thead>
                  <tbody>
                    {detail.sales.map((o) => (
                      <tr key={o.id}>
                        <td className={td}>{o.code}</td>
                        <td className={td}>{o.totalAmount.toLocaleString()}</td>
                        <td className={td}>{o.status}</td>
                      </tr>
                    ))}
                    {detail.sales.length === 0 && (
                      <tr><td className={td} colSpan={3}>Chưa có đơn trong ca</td></tr>
                    )}
                  </tbody>
                </table>
              </div>
              {canManage && (
                <div className="flex flex-wrap gap-2">
                  {detail.shift.status === "Open" && (
                    <>
                      <input className={field} value={closingCash} onChange={(e) => setClosingCash(e.target.value)} placeholder="Tiền đếm" />
                      <button type="button" className={btn.primary} onClick={() => void run(
                        () => closePosShift(detail.shift.id, Number(closingCash) || 0), "Đã đóng ca",
                      )}>Đóng ca</button>
                    </>
                  )}
                  <button type="button" className={btn.ghost} onClick={() => void run(
                    () => printPosShiftReport(detail.shift.id), "Đã in BC ca (stub)",
                  )}>In BC ca</button>
                </div>
              )}
            </div>
          ) : (
            <p className="text-sm text-[var(--muted)]">Chọn hoặc mở một ca.</p>
          )}
        </section>
      </div>
    </div>
  );
}
