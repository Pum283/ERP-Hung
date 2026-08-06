"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  assignLogDelivery,
  cancelLogDelivery,
  confirmLogDelivery,
  confirmLogPick,
  dispatchLogDelivery,
  failLogDelivery,
  fetchLogCarriers,
  fetchLogDeliveries,
  fetchLogDeliveryDetail,
  printLogWaybill,
  returnLogDelivery,
  splitLogDelivery,
  startLogPick,
  updateLogStatus,
  upsertLogDelivery,
  upsertLogDeliveryLine,
  type LogCarrierDto,
  type LogDeliveryDetailDto,
  type LogDeliveryOrderDto,
} from "@/shared/api/log-api";
import { collectLogCod, markLogCod, setLogCodAmount } from "@/shared/api/log-cod-api";
import { fetchMsgDirectory, type MsgDirectoryUserDto } from "@/shared/api/msg-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function LogDeliveriesPage() {
  const { can } = usePermissions();
  const canRead = can("log.delivery.read");
  const canManage = can("log.delivery.manage");
  const canCod = can("log.cod.manage");

  const [list, setList] = useState<LogDeliveryOrderDto[]>([]);
  const [carriers, setCarriers] = useState<LogCarrierDto[]>([]);
  const [users, setUsers] = useState<MsgDirectoryUserDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<LogDeliveryDetailDto | null>(null);
  const [q, setQ] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [so, setSo] = useState("SO-001");
  const [customer, setCustomer] = useState("");
  const [address, setAddress] = useState("");
  const [phone, setPhone] = useState("");
  const [pCode, setPCode] = useState("SKU-001");
  const [pName, setPName] = useState("");
  const [pQty, setPQty] = useState("1");
  const [carrierId, setCarrierId] = useState("");
  const [driverId, setDriverId] = useState("");
  const [failReason, setFailReason] = useState("");
  const [splitLineId, setSplitLineId] = useState("");
  const [splitQty, setSplitQty] = useState("1");
  const [codAmount, setCodAmount] = useState("500000");
  const [promisedAt, setPromisedAt] = useState("");

  const load = useCallback(async () => {
    const [d, c, u] = await Promise.all([
      fetchLogDeliveries(q || undefined),
      fetchLogCarriers().catch(() => [] as LogCarrierDto[]),
      fetchMsgDirectory().catch(() => [] as MsgDirectoryUserDto[]),
    ]);
    setList(d);
    setCarriers(c);
    setUsers(u);
    if (!selectedId && d[0]) setSelectedId(d[0].id);
    if (!carrierId && c[0]) setCarrierId(c[0].id);
    if (!driverId && u[0]) setDriverId(u[0].id);
  }, [q, selectedId, carrierId, driverId]);

  useEffect(() => {
    if (!canRead) {
      setLoading(false);
      return;
    }
    setLoading(true);
    load()
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedId || !canRead) return;
    fetchLogDeliveryDetail(selectedId)
      .then((d) => {
        setDetail(d);
        if (!splitLineId && d.lines[0]) setSplitLineId(d.lines[0].id);
      })
      .catch((e: Error) => setError(e.message));
  }, [selectedId, canRead, splitLineId]);

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function refresh() {
    if (!selectedId) return;
    setDetail(await fetchLogDeliveryDetail(selectedId));
    await load();
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await refresh();
      flash(msg);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onCreate(e: FormEvent) {
    e.preventDefault();
    try {
      const saved = await upsertLogDelivery({
        sourceOrderCode: so,
        customerName: customer,
        shipAddress: address,
        phone,
        promisedAt: promisedAt ? new Date(promisedAt).toISOString() : null,
      });
      setCustomer("");
      setAddress("");
      await load();
      setSelectedId(saved.id);
      flash("Đã tạo lệnh giao.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onLine(e: FormEvent) {
    e.preventDefault();
    if (!selectedId) return;
    await run(
      () => upsertLogDeliveryLine(selectedId, {
        productCode: pCode,
        productName: pName,
        qty: Number(pQty) || 1,
      }),
      "Đã thêm dòng hàng.",
    );
    setPName("");
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem lệnh giao.</div>;
  }

  const o = detail?.order;

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Lệnh giao hàng</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Tạo từ SO · tách đợt · pick · xuất · vận đơn · phân công · TT · thất bại (UC_LOG_006–014, 017)
        </p>
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-[1fr_1.4fr]">
        <section className={panel}>
          <div className="mb-3 flex gap-2">
            <input className={`${field} w-40`} value={q} onChange={(e) => setQ(e.target.value)} placeholder="Tìm" />
            <button type="button" className={btn.ghost} onClick={() => load().catch((e: Error) => setError(e.message))}>
              Tìm
            </button>
          </div>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>SO / KH</th>
                  <th className={th}>TT</th>
                  <th className={th}>Dòng</th>
                </tr>
              </thead>
              <tbody>
                {list.map((d) => (
                  <tr
                    key={d.id}
                    className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedId === d.id ? "bg-[var(--surface-2)]" : ""}`}
                    onClick={() => setSelectedId(d.id)}
                  >
                    <td className={td}>
                      {d.code}
                      {d.batchNo > 1 && <span className="ml-1 text-xs text-[var(--muted)]">#{d.batchNo}</span>}
                    </td>
                    <td className={td}>
                      <div>{d.sourceOrderCode}</div>
                      <div className="text-xs text-[var(--muted)]">{d.customerName}</div>
                    </td>
                    <td className={td}>
                      <span className={statusPill(d.status === "Delivered" ? "success" : d.status === "Failed" ? "danger" : "muted")}>
                        {d.status}
                      </span>
                      {d.isCod && (
                        <div className="mt-0.5 text-xs text-[var(--muted)]">
                          COD {d.codStatus}{d.codOverdue ? " · quá hạn" : ""}
                        </div>
                      )}
                    </td>
                    <td className={td}>{d.lineCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <div className="space-y-4">
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Tạo lệnh từ đơn hàng</h2>
              <form onSubmit={onCreate} className="grid gap-2 sm:grid-cols-2">
                <input className={field} value={so} onChange={(e) => setSo(e.target.value)} placeholder="Mã SO" required />
                <input className={field} value={customer} onChange={(e) => setCustomer(e.target.value)} placeholder="Khách hàng" required />
                <input className={`${field} sm:col-span-2`} value={address} onChange={(e) => setAddress(e.target.value)} placeholder="Địa chỉ giao" />
                <input className={field} value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="SĐT" />
                <input className={field} type="datetime-local" value={promisedAt} onChange={(e) => setPromisedAt(e.target.value)} title="Hẹn giao" />
                <button type="submit" className={btn.primary}>Tạo lệnh</button>
              </form>
            </section>
          )}

          {detail && o && (
            <section className={panel}>
              <h2 className="mb-1 text-sm font-semibold">{o.code} · {o.customerName}</h2>
              <p className="mb-3 text-xs text-[var(--muted)]">
                SO {o.sourceOrderCode} · đợt {o.batchNo}
                {o.waybillNo ? ` · VD ${o.waybillNo}` : ""}
                {o.promisedAt ? ` · hẹn ${new Date(o.promisedAt).toLocaleString()}` : ""}
                {o.onTime != null ? ` · ${o.onTime ? "đúng hạn" : "trễ hạn"}` : ""}
                {o.failureReason ? ` · Lỗi: ${o.failureReason}` : ""}
              </p>
              <span className={statusPill(o.status === "Delivered" ? "success" : "muted")}>{o.status}</span>
              {o.isCod && (
                <span className={`${statusPill(o.codOverdue ? "danger" : "brand")} ml-2`}>
                  COD {o.codStatus} · {o.codAmount.toLocaleString("vi-VN")}
                </span>
              )}

              {canCod && o.status !== "Cancelled" && o.status !== "Returned" && (
                <div className="mt-3 flex flex-wrap gap-2 border-t border-black/10 pt-3">
                  <input
                    className={`${field} w-32`}
                    value={codAmount}
                    onChange={(e) => setCodAmount(e.target.value)}
                    placeholder="Tiền COD"
                  />
                  {!o.isCod || o.codStatus === "None" ? (
                    <button
                      type="button"
                      className={btn.ghost}
                      onClick={() => run(() => markLogCod(o.id, Number(codAmount) || 0, 3), "Đã đánh dấu COD.")}
                    >
                      Đánh dấu COD
                    </button>
                  ) : (
                    <>
                      {(o.codStatus === "Pending" || o.codStatus === "Collected" || o.codStatus === "Variance") && (
                        <button
                          type="button"
                          className={btn.ghost}
                          onClick={() => run(() => setLogCodAmount(o.id, Number(codAmount) || 0), "Đã cập nhật tiền COD.")}
                        >
                          Sửa tiền COD
                        </button>
                      )}
                      {o.codStatus === "Pending" && (o.status === "Delivered" || o.status === "InTransit" || o.status === "Dispatched") && (
                        <button
                          type="button"
                          className={btn.ghost}
                          onClick={() => run(() => collectLogCod(o.id), "Đã xác nhận thu COD.")}
                        >
                          Xác nhận thu COD
                        </button>
                      )}
                    </>
                  )}
                </div>
              )}

              <h3 className="mb-2 mt-4 text-xs font-semibold uppercase text-[var(--muted)]">Dòng hàng / pick</h3>
              <ul className="mb-3 space-y-1 text-sm">
                {detail.lines.map((l) => (
                  <li key={l.id}>
                    {l.productCode} · {l.productName} — {l.qtyPicked}/{l.qty} {l.unit}
                  </li>
                ))}
                {detail.lines.length === 0 && <li className="text-[var(--muted)]">Chưa có dòng</li>}
              </ul>

              {canManage && o.status === "Draft" && (
                <form onSubmit={onLine} className="mb-4 grid gap-2 sm:grid-cols-4">
                  <input className={field} value={pCode} onChange={(e) => setPCode(e.target.value)} placeholder="Mã SP" required />
                  <input className={field} value={pName} onChange={(e) => setPName(e.target.value)} placeholder="Tên SP" required />
                  <input className={field} value={pQty} onChange={(e) => setPQty(e.target.value)} placeholder="SL" />
                  <button type="submit" className={btn.ghost}>Thêm dòng</button>
                </form>
              )}

              {canManage && (
                <div className="mb-4 flex flex-wrap gap-2">
                  {o.status === "Draft" && (
                    <button type="button" className={btn.ghost} onClick={() => run(() => confirmLogDelivery(o.id), "Đã xác nhận.")}>
                      Xác nhận
                    </button>
                  )}
                  {(o.status === "Draft" || o.status === "Confirmed") && detail.lines[0] && (
                    <>
                      <select className={`${field} w-36`} value={splitLineId} onChange={(e) => setSplitLineId(e.target.value)}>
                        {detail.lines.map((l) => (
                          <option key={l.id} value={l.id}>{l.productCode} ({l.qty})</option>
                        ))}
                      </select>
                      <input className={`${field} w-20`} value={splitQty} onChange={(e) => setSplitQty(e.target.value)} />
                      <button
                        type="button"
                        className={btn.ghost}
                        onClick={() =>
                          run(
                            () => splitLogDelivery(o.id, {
                              lines: [{ lineId: splitLineId, qty: Number(splitQty) || 1 }],
                            }),
                            "Đã tách đợt.",
                          )
                        }
                      >
                        Tách đợt
                      </button>
                    </>
                  )}
                  {(o.status === "Draft" || o.status === "Confirmed") && (
                    <button type="button" className={btn.ghost} onClick={() => run(() => startLogPick(o.id), "Bắt đầu soạn hàng.")}>
                      Pick list
                    </button>
                  )}
                  {o.status === "Picking" && (
                    <button
                      type="button"
                      className={btn.ghost}
                      onClick={() =>
                        run(
                          () => confirmLogPick(
                            o.id,
                            detail.lines.map((l) => ({ lineId: l.id, qtyPicked: l.qty })),
                          ),
                          "Đã soạn xong.",
                        )
                      }
                    >
                      Xác nhận pick đủ
                    </button>
                  )}
                  {o.status === "Ready" && (
                    <button type="button" className={btn.ghost} onClick={() => run(() => dispatchLogDelivery(o.id), "Đã xuất hàng.")}>
                      Xuất hàng
                    </button>
                  )}
                  {o.status !== "Draft" && o.status !== "Cancelled" && (
                    <button type="button" className={btn.ghost} onClick={() => run(() => printLogWaybill(o.id), "Đã in vận đơn.")}>
                      In vận đơn
                    </button>
                  )}
                  {(o.status === "Dispatched" || o.status === "InTransit") && (
                    <>
                      <button type="button" className={btn.ghost} onClick={() => run(() => updateLogStatus(o.id, "InTransit"), "Đang giao.")}>
                        Đang giao
                      </button>
                      <button type="button" className={btn.ghost} onClick={() => run(() => updateLogStatus(o.id, "Delivered"), "Đã giao.")}>
                        Đã giao
                      </button>
                    </>
                  )}
                  {o.status !== "Cancelled" && o.status !== "Delivered" && o.status !== "Returned" && (
                    <button type="button" className={btn.ghost} onClick={() => run(() => cancelLogDelivery(o.id, "Hủy"), "Đã hủy.")}>
                      Hủy
                    </button>
                  )}
                  {(o.status === "Dispatched" || o.status === "InTransit" || o.status === "Failed" || o.status === "Delivered") && (
                    <button type="button" className={btn.ghost} onClick={() => run(() => returnLogDelivery(o.id, "Hoàn"), "Đã hoàn.")}>
                      Hoàn lệnh
                    </button>
                  )}
                </div>
              )}

              {canManage && (
                <div className="mb-4 grid gap-2 sm:grid-cols-3">
                  <select className={field} value={carrierId} onChange={(e) => setCarrierId(e.target.value)}>
                    <option value="">— ĐVVC —</option>
                    {carriers.map((c) => <option key={c.id} value={c.id}>{c.code}</option>)}
                  </select>
                  <select className={field} value={driverId} onChange={(e) => setDriverId(e.target.value)}>
                    {users.map((u) => (
                      <option key={u.id} value={u.id}>{u.displayName || u.username}</option>
                    ))}
                  </select>
                  <button
                    type="button"
                    className={btn.ghost}
                    onClick={() =>
                      run(
                        () => assignLogDelivery(o.id, {
                          carrierId: carrierId || null,
                          driverUserId: driverId || null,
                        }),
                        "Đã phân công.",
                      )
                    }
                  >
                    Phân công
                  </button>
                </div>
              )}

              {canManage && (o.status === "Dispatched" || o.status === "InTransit") && (
                <div className="mb-4 flex gap-2">
                  <input
                    className={`${field} flex-1`}
                    value={failReason}
                    onChange={(e) => setFailReason(e.target.value)}
                    placeholder="Lý do giao thất bại"
                  />
                  <button
                    type="button"
                    className={btn.ghost}
                    onClick={() => run(() => failLogDelivery(o.id, failReason || "Không liên lạc được"), "Đã ghi thất bại.")}
                  >
                    Ghi thất bại
                  </button>
                </div>
              )}

              {detail.childBatches.length > 0 && (
                <div className="mb-3 text-sm">
                  <h3 className="mb-1 text-xs font-semibold uppercase text-[var(--muted)]">Đợt tách</h3>
                  <ul className="space-y-1">
                    {detail.childBatches.map((b) => (
                      <li key={b.id}>
                        <button type="button" className="underline" onClick={() => setSelectedId(b.id)}>
                          {b.code} · #{b.batchNo} · {b.status}
                        </button>
                      </li>
                    ))}
                  </ul>
                </div>
              )}

              <h3 className="mb-2 text-xs font-semibold uppercase text-[var(--muted)]">Lịch sử</h3>
              <ul className="max-h-40 space-y-1 overflow-auto text-xs text-[var(--muted)]">
                {detail.events.map((ev) => (
                  <li key={ev.id}>
                    {new Date(ev.occurredAt).toLocaleString()} · {ev.status} · {ev.note} ({ev.actorName})
                  </li>
                ))}
              </ul>
            </section>
          )}
        </div>
      </div>
    </div>
  );
}
