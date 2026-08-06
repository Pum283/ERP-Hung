"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { fetchPurPos, type PurPurchaseOrderDto } from "@/shared/api/pur-api";
import {
  createPurGrn,
  fetchPurGrnDetail,
  fetchPurGrns,
  postPurGrn,
  pushPurGrnInventory,
  updatePurGrnLine,
  type PurGrnDetailDto,
  type PurGrnDto,
} from "@/shared/api/pur-receiving-api";
import { parseInvPushError, pushStatusTone } from "@/shared/api/pur-push-helpers";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PurReceiptsPage() {
  const { can } = usePermissions();
  const canRead = can("pur.grn.read");
  const canManage = can("pur.grn.manage");

  const [pos, setPos] = useState<PurPurchaseOrderDto[]>([]);
  const [list, setList] = useState<PurGrnDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<PurGrnDetailDto | null>(null);
  const [poId, setPoId] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const load = useCallback(async () => {
    const [p, g] = await Promise.all([fetchPurPos(), fetchPurGrns()]);
    const sent = p.filter((x) => x.status === "Sent");
    setPos(sent);
    setList(g);
    if (!poId && sent[0]) setPoId(sent[0].id);
    if (!selectedId && g[0]) setSelectedId(g[0].id);
  }, [poId, selectedId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchPurGrnDetail(selectedId).then(setDetail).catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) setDetail(await fetchPurGrnDetail(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem nhận hàng.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Nhận hàng (GRN)</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Tạo theo PO · lệch SL/CL · post · đẩy phiếu nhập INV thật (UC_PUR_034–035, 037)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Phiếu nhận</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>GRN</th><th className={th}>PO</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {list.map((g) => (
                  <tr key={g.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(g.id)}>
                    <td className={td}>
                      <div className="font-medium">{g.code}</div>
                      <div className="text-xs text-[var(--muted)]">
                        Acc {g.totalAcceptedQty} · Rej {g.totalRejectedQty}
                      </div>
                    </td>
                    <td className={td}>{g.poCode ?? "—"}</td>
                    <td className={td}>
                      <span className={statusPill(g.status === "Posted" ? "success" : "brand")}>{g.status}</span>
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
                const g = await createPurGrn({ poId });
                setSelectedId(g.id);
              }, "Đã tạo GRN từ PO");
            }}>
              <select className={field} value={poId} onChange={(e) => setPoId(e.target.value)}>
                {pos.map((p) => (
                  <option key={p.id} value={p.id}>{p.code} · nhận {p.receivedPct}%</option>
                ))}
              </select>
              <button className={btn.primary} type="submit" disabled={!poId}>Tạo GRN</button>
            </form>
          )}
          {!pos.length && (
            <p className="mt-2 text-xs text-[var(--muted)]">Cần PO status Sent (gửi NCC) trước.</p>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chi tiết</h2>
          {detail ? (
            <div className="space-y-3 text-sm">
              <div>
                <b>{detail.header.code}</b> · {detail.header.vendorName}
                <div className="text-xs text-[var(--muted)]">
                  INV push:{" "}
                  <span className={statusPill(pushStatusTone(detail.header.inventoryPushStatus))}>
                    {detail.header.inventoryPushStatus}
                  </span>
                  {detail.header.qualityNote ? ` · ${detail.header.qualityNote}` : ""}
                </div>
                {parseInvPushError(detail.header.note) && (
                  <div className="mt-1 text-xs text-red-600">
                    Lỗi đẩy INV: {parseInvPushError(detail.header.note)}
                  </div>
                )}
              </div>
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead>
                    <tr>
                      <th className={th}>SP</th>
                      <th className={th}>Nhận</th>
                      <th className={th}>Đạt</th>
                      <th className={th}>Loại</th>
                      <th className={th}></th>
                    </tr>
                  </thead>
                  <tbody>
                    {detail.lines.map((l) => (
                      <tr key={l.id}>
                        <td className={td}>{l.productCode}</td>
                        <td className={td}>{l.receivedQty}</td>
                        <td className={td}>{l.acceptedQty}</td>
                        <td className={td}>{l.rejectedQty}</td>
                        <td className={td}>
                          {canManage && detail.header.status === "Draft" && (
                            <button type="button" className={btn.ghost} onClick={() => {
                              const half = Math.max(0, l.receivedQty - 1);
                              void run(() => updatePurGrnLine(detail.header.id, {
                                lineId: l.id,
                                receivedQty: l.receivedQty,
                                acceptedQty: half,
                                rejectedQty: l.receivedQty - half,
                              }), "Đã ghi lệch/CL");
                            }}>Lệch −1</button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {canManage && detail.header.status === "Draft" && (
                <button type="button" className={btn.primary} onClick={() => void run(
                  () => postPurGrn(detail.header.id), "Đã post GRN + đẩy INV",
                )}>Post GRN</button>
              )}
              {canManage && detail.header.status === "Posted" && (
                <button type="button" className={btn.ghost} onClick={() => void run(
                  () => pushPurGrnInventory(detail.header.id), "Đã đẩy phiếu nhập INV (Receipt Purchase).",
                )}>Đẩy INV lại</button>
              )}
            </div>
          ) : (
            <p className="text-sm text-[var(--muted)]">Chọn hoặc tạo phiếu nhận.</p>
          )}
        </section>
      </div>
    </div>
  );
}
