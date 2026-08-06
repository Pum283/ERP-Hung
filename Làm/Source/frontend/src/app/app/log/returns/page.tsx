"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import { fetchInvWarehouses, type InvWarehouseDto } from "@/shared/api/inv-api";
import { fetchLogDeliveries, type LogDeliveryOrderDto } from "@/shared/api/log-api";
import {
  cancelLogReturn,
  confirmLogReturnCount,
  countLogReturnLine,
  createLogReturn,
  fetchLogOpsReport,
  fetchLogReturnDetail,
  fetchLogReturns,
  postLogReturn,
  type LogOpsReportDto,
  type LogReturnDetailDto,
  type LogReturnNoteDto,
} from "@/shared/api/log-return-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function LogReturnsPage() {
  const { can } = usePermissions();
  const canRead = can("log.return.read");
  const canManage = can("log.return.manage");
  const canOps = can("log.delivery.read");

  const [list, setList] = useState<LogReturnNoteDto[]>([]);
  const [deliveries, setDeliveries] = useState<LogDeliveryOrderDto[]>([]);
  const [warehouses, setWarehouses] = useState<InvWarehouseDto[]>([]);
  const [ops, setOps] = useState<LogOpsReportDto | null>(null);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<LogReturnDetailDto | null>(null);
  const [deliveryId, setDeliveryId] = useState("");
  const [warehouseId, setWarehouseId] = useState("");
  const [reason, setReason] = useState("Khách hoàn / giao thất bại");
  const [countVals, setCountVals] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const load = useCallback(async () => {
    const [r, d, w, o] = await Promise.all([
      fetchLogReturns(),
      fetchLogDeliveries().catch(() => [] as LogDeliveryOrderDto[]),
      fetchInvWarehouses().catch(() => [] as InvWarehouseDto[]),
      canOps ? fetchLogOpsReport().catch(() => null) : Promise.resolve(null),
    ]);
    setList(r);
    const eligible = d.filter((x) =>
      ["Delivered", "Failed", "Returned", "InTransit", "Dispatched"].includes(x.status),
    );
    setDeliveries(eligible);
    setWarehouses(w.filter((x) => x.status === "Active"));
    setOps(o);
    if (!selectedId && r[0]) setSelectedId(r[0].id);
    if (!deliveryId && eligible[0]) setDeliveryId(eligible[0].id);
    if (!warehouseId && w[0]) setWarehouseId(w.find((x) => x.status === "Active")?.id ?? "");
  }, [selectedId, deliveryId, warehouseId, canOps]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchLogReturnDetail(selectedId)
      .then((d) => {
        setDetail(d);
        const map: Record<string, string> = {};
        d.lines.forEach((l) => {
          map[l.id] = String(l.qtyCounted > 0 ? l.qtyCounted : l.qtyExpected);
        });
        setCountVals(map);
      })
      .catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedId) setDetail(await fetchLogReturnDetail(selectedId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem hoàn hàng LOG.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Hoàn hàng</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Phiếu hoàn · kiểm đếm · nhập kho INV (UC_LOG_027–029) · dashboard vận hành (035, 039)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      {ops && (
        <section className="grid gap-3 sm:grid-cols-3 xl:grid-cols-8">
          {[
            ["Đã giao", ops.deliveredCount],
            ["Thất bại", ops.failedCount],
            ["Hoàn", ops.returnedCount],
            ["% hoàn", `${ops.returnRatePct}%`],
            ["% thất bại", `${ops.failRatePct}%`],
            ["COD quá hạn", ops.codOverdueCount],
            ["Đúng hạn", ops.onTimeDeliveredCount],
            ["% đúng hạn", `${ops.onTimeRatePct}%`],
          ].map(([label, val]) => (
            <div key={String(label)} className={panel}>
              <div className="text-xs uppercase text-[var(--muted)]">{label}</div>
              <div className="mt-1 text-lg font-semibold">{val}</div>
            </div>
          ))}
        </section>
      )}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Phiếu hoàn</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Lệnh</th>
                  <th className={th}>TT</th>
                </tr>
              </thead>
              <tbody>
                {list.map((n) => (
                  <tr key={n.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(n.id)}>
                    <td className={td}>
                      <div className="font-medium">{n.code}</div>
                      <div className="text-xs text-[var(--muted)]">
                        Acc {n.qtyAcceptedTotal}/{n.qtyExpectedTotal} · {n.warehouseName}
                      </div>
                    </td>
                    <td className={td}>{n.deliveryCode}</td>
                    <td className={td}>
                      <span className={statusPill(n.status === "Posted" ? "success" : n.status === "Cancelled" ? "danger" : "brand")}>
                        {n.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {canManage && (
            <form
              className="mt-3 grid gap-2 border-t border-black/10 pt-3 sm:grid-cols-2"
              onSubmit={(e: FormEvent) => {
                e.preventDefault();
                void run(async () => {
                  const d = await createLogReturn({
                    deliveryOrderId: deliveryId,
                    warehouseId,
                    reason,
                  });
                  setSelectedId(d.header.id);
                }, "Đã tạo phiếu hoàn");
              }}
            >
              <select className={field} value={deliveryId} onChange={(e) => setDeliveryId(e.target.value)} required>
                <option value="">— Lệnh giao —</option>
                {deliveries.map((d) => (
                  <option key={d.id} value={d.id}>{d.code} · {d.status}</option>
                ))}
              </select>
              <select className={field} value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)} required>
                <option value="">— Kho nhận —</option>
                {warehouses.map((w) => (
                  <option key={w.id} value={w.id}>{w.code} · {w.name}</option>
                ))}
              </select>
              <input className={`${field} sm:col-span-2`} value={reason} onChange={(e) => setReason(e.target.value)} placeholder="Lý do" />
              <button className={btn.primary} type="submit">Tạo phiếu hoàn</button>
            </form>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Chi tiết / kiểm đếm</h2>
          {detail ? (
            <div className="space-y-3 text-sm">
              <div>
                <b>{detail.header.code}</b>
                <div className="text-xs text-[var(--muted)]">
                  {detail.header.deliveryCode} · {detail.header.warehouseName}
                  {detail.header.invStockDocCode ? ` · INV ${detail.header.invStockDocCode}` : ""}
                </div>
                <span className={statusPill(detail.header.status === "Posted" ? "success" : "brand")}>
                  {detail.header.status}
                </span>
              </div>

              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead>
                    <tr>
                      <th className={th}>SP</th>
                      <th className={th}>Expect</th>
                      <th className={th}>Đếm</th>
                      <th className={th}></th>
                    </tr>
                  </thead>
                  <tbody>
                    {detail.lines.map((l) => (
                      <tr key={l.id}>
                        <td className={td}>{l.productCode} · {l.productName}</td>
                        <td className={td}>{l.qtyExpected}</td>
                        <td className={td}>
                          {detail.header.status === "Draft" && canManage ? (
                            <input
                              className={`${field} w-20`}
                              value={countVals[l.id] ?? ""}
                              onChange={(e) => setCountVals((m) => ({ ...m, [l.id]: e.target.value }))}
                            />
                          ) : (
                            <>{l.qtyCounted} / acc {l.qtyAccepted}</>
                          )}
                        </td>
                        <td className={td}>
                          {detail.header.status === "Draft" && canManage && (
                            <button
                              type="button"
                              className={btn.ghost}
                              onClick={() => void run(
                                () => countLogReturnLine(detail.header.id, {
                                  lineId: l.id,
                                  qtyCounted: Number(countVals[l.id]) || 0,
                                }),
                                "Đã ghi đếm",
                              )}
                            >
                              Lưu
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {canManage && (
                <div className="flex flex-wrap gap-2">
                  {detail.header.status === "Draft" && (
                    <>
                      <button
                        type="button"
                        className={btn.primary}
                        onClick={() => void run(() => confirmLogReturnCount(detail.header.id), "Đã xác nhận đếm")}
                      >
                        Xác nhận đếm
                      </button>
                      <button
                        type="button"
                        className={btn.ghost}
                        onClick={() => void run(() => cancelLogReturn(detail.header.id, "Hủy"), "Đã hủy phiếu")}
                      >
                        Hủy
                      </button>
                    </>
                  )}
                  {detail.header.status === "Counted" && (
                    <button
                      type="button"
                      className={btn.primary}
                      onClick={() => void run(() => postLogReturn(detail.header.id), "Đã nhập kho hoàn")}
                    >
                      Nhập kho INV
                    </button>
                  )}
                </div>
              )}
            </div>
          ) : (
            <p className="text-sm text-[var(--muted)]">Chọn phiếu hoặc tạo mới.</p>
          )}
        </section>
      </div>
    </div>
  );
}
