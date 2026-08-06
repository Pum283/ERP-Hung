"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  acceptFsmTicket,
  addFsmAssetHistory,
  assignFsmTicket,
  checkoutFsmTicket,
  closeFsmTicket,
  consumeFsmTicketPart,
  escalateFsmTicket,
  fetchFsmAssetDetail,
  fetchFsmAssets,
  fetchFsmFaultCodes,
  fetchFsmParts,
  fetchFsmServiceTypes,
  fetchFsmTicketParts,
  fetchFsmTickets,
  setFsmAppointment,
  setFsmTicketStatus,
  upsertFsmAsset,
  upsertFsmTicket,
  workLogFsmTicket,
  type FsmAssetDetailDto,
  type FsmAssetDto,
  type FsmFaultCodeDto,
  type FsmPartDto,
  type FsmServiceTypeDto,
  type FsmTicketDto,
  type FsmTicketPartLineDto,
} from "@/shared/api/fsm-api";
import { fetchMsgDirectory, type MsgDirectoryUserDto } from "@/shared/api/msg-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function FsmTicketsPage() {
  const { can } = usePermissions();
  const canTicket = can("fsm.ticket.read");
  const canTicketManage = can("fsm.ticket.manage");
  const canAsset = can("fsm.asset.read");
  const canAssetManage = can("fsm.asset.manage");
  const canRead = canTicket || canAsset;

  const [tickets, setTickets] = useState<FsmTicketDto[]>([]);
  const [assets, setAssets] = useState<FsmAssetDto[]>([]);
  const [types, setTypes] = useState<FsmServiceTypeDto[]>([]);
  const [faults, setFaults] = useState<FsmFaultCodeDto[]>([]);
  const [parts, setParts] = useState<FsmPartDto[]>([]);
  const [ticketParts, setTicketParts] = useState<FsmTicketPartLineDto[]>([]);
  const [users, setUsers] = useState<MsgDirectoryUserDto[]>([]);
  const [selectedTicketId, setSelectedTicketId] = useState("");
  const [selectedAssetId, setSelectedAssetId] = useState("");
  const [assetDetail, setAssetDetail] = useState<FsmAssetDetailDto | null>(null);
  const [consumePartId, setConsumePartId] = useState("");
  const [consumeQty, setConsumeQty] = useState("1");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [cust, setCust] = useState("");
  const [serial, setSerial] = useState("");
  const [model, setModel] = useState("");
  const [channel, setChannel] = useState("Phone");
  const [subject, setSubject] = useState("");
  const [priority, setPriority] = useState("Normal");
  const [serviceTypeId, setServiceTypeId] = useState("");
  const [faultCodeId, setFaultCodeId] = useState("");
  const [ticketAssetId, setTicketAssetId] = useState("");
  const [techId, setTechId] = useState("");
  const [escReason, setEscReason] = useState("");
  const [histSummary, setHistSummary] = useState("");
  const [apptAt, setApptAt] = useState("");
  const [rootCause, setRootCause] = useState("");
  const [resolution, setResolution] = useState("");
  const [signerName, setSignerName] = useState("");

  const selectedTicket = tickets.find((t) => t.id === selectedTicketId) ?? null;

  const load = useCallback(async () => {
    const [t, a, st, fc, u, p] = await Promise.all([
      canTicket ? fetchFsmTickets() : Promise.resolve([] as FsmTicketDto[]),
      canAsset ? fetchFsmAssets() : Promise.resolve([] as FsmAssetDto[]),
      fetchFsmServiceTypes().catch(() => [] as FsmServiceTypeDto[]),
      fetchFsmFaultCodes().catch(() => [] as FsmFaultCodeDto[]),
      fetchMsgDirectory().catch(() => [] as MsgDirectoryUserDto[]),
      fetchFsmParts().catch(() => [] as FsmPartDto[]),
    ]);
    setTickets(t); setAssets(a); setTypes(st); setFaults(fc); setUsers(u); setParts(p);
    if (!selectedTicketId && t[0]) setSelectedTicketId(t[0].id);
    if (!selectedAssetId && a[0]) setSelectedAssetId(a[0].id);
    if (!serviceTypeId && st[0]) setServiceTypeId(st[0].id);
    if (!faultCodeId && fc[0]) setFaultCodeId(fc[0].id);
    if (!ticketAssetId && a[0]) setTicketAssetId(a[0].id);
    if (!techId && u[0]) setTechId(u[0].id);
    if (!consumePartId && p[0]) setConsumePartId(p[0].id);
  }, [canTicket, canAsset, selectedTicketId, selectedAssetId, serviceTypeId, faultCodeId, ticketAssetId, techId, consumePartId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedAssetId || !canAsset) return;
    fetchFsmAssetDetail(selectedAssetId).then(setAssetDetail).catch((e: Error) => setError(e.message));
  }, [selectedAssetId, canAsset]);

  useEffect(() => {
    if (!selectedTicketId || !canTicket) { setTicketParts([]); return; }
    fetchFsmTicketParts(selectedTicketId).then(setTicketParts).catch((e: Error) => setError(e.message));
  }, [selectedTicketId, canTicket]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedAssetId && canAsset) setAssetDetail(await fetchFsmAssetDetail(selectedAssetId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem FSM.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Ticket / thiết bị</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Install base · serial/BH · lịch sử · ticket kênh · ưu tiên · phân công · escalate (UC_FSM_008–010, 013–015, 017)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        {canAsset && (
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Thiết bị tại khách</h2>
            {canAssetManage && (
              <form
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  run(async () => {
                    const a = await upsertFsmAsset({
                      customerName: cust,
                      serialNo: serial,
                      model,
                      activatedAt: new Date().toISOString(),
                      warrantyEndAt: new Date(Date.now() + 365 * 86400000).toISOString(),
                    });
                    setSelectedAssetId(a.id);
                    setCust(""); setSerial(""); setModel("");
                  }, "Đã lưu thiết bị.");
                }}
                className="mb-3 grid gap-2 sm:grid-cols-2"
              >
                <input className={field} value={cust} onChange={(e) => setCust(e.target.value)} placeholder="Khách hàng" required />
                <input className={field} value={serial} onChange={(e) => setSerial(e.target.value)} placeholder="Serial" required />
                <input className={`${field} sm:col-span-2`} value={model} onChange={(e) => setModel(e.target.value)} placeholder="Model" />
                <button type="submit" className={`${btn.primary} sm:col-span-2`}>Tạo thiết bị</button>
              </form>
            )}
            <div className={`${tableWrap} mb-3`}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>Serial</th>
                    <th className={th}>KH</th>
                    <th className={th}>BH</th>
                  </tr>
                </thead>
                <tbody>
                  {assets.map((a) => (
                    <tr
                      key={a.id}
                      className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedAssetId === a.id ? "bg-[var(--surface-2)]" : ""}`}
                      onClick={() => setSelectedAssetId(a.id)}
                    >
                      <td className={td}>{a.code}</td>
                      <td className={td}>{a.serialNo}</td>
                      <td className={td}>{a.customerName}</td>
                      <td className={td}>
                        {a.warrantyExpiringSoon
                          ? <span className={statusPill("warning")}>Sắp hết</span>
                          : <span className={statusPill("muted")}>{a.status}</span>}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            {assetDetail && (
              <div>
                <p className="mb-2 text-xs text-[var(--muted)]">
                  {assetDetail.asset.model || "—"} · BH đến{" "}
                  {assetDetail.asset.warrantyEndAt
                    ? new Date(assetDetail.asset.warrantyEndAt).toLocaleDateString()
                    : "—"}
                </p>
                <h3 className="mb-1 text-xs font-semibold uppercase text-[var(--muted)]">Lịch sử BH / sửa</h3>
                <ul className="mb-2 max-h-32 space-y-1 overflow-auto text-xs text-[var(--muted)]">
                  {assetDetail.history.map((h) => (
                    <li key={h.id}>
                      {new Date(h.occurredAt).toLocaleString()} · {h.eventType}: {h.summary}
                    </li>
                  ))}
                </ul>
                {canAssetManage && (
                  <form
                    onSubmit={(e: FormEvent) => {
                      e.preventDefault();
                      run(
                        () => addFsmAssetHistory(assetDetail.asset.id, {
                          eventType: "Repair",
                          summary: histSummary || "Ghi chú sửa chữa",
                        }),
                        "Đã ghi lịch sử.",
                      );
                      setHistSummary("");
                    }}
                    className="flex gap-2"
                  >
                    <input className={field} value={histSummary} onChange={(e) => setHistSummary(e.target.value)} placeholder="Ghi lịch sử" />
                    <button type="submit" className={btn.ghost}>Ghi</button>
                  </form>
                )}
              </div>
            )}
          </section>
        )}

        {canTicket && (
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Ticket</h2>
            {canTicketManage && (
              <form
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  run(async () => {
                    const t = await upsertFsmTicket({
                      channel,
                      subject,
                      customerName: cust || assets.find((a) => a.id === ticketAssetId)?.customerName || "Khách",
                      serviceTypeId: serviceTypeId || null,
                      faultCodeId: faultCodeId || null,
                      assetId: ticketAssetId || null,
                      priority,
                    });
                    setSelectedTicketId(t.id);
                    setSubject("");
                  }, "Đã tạo ticket.");
                }}
                className="mb-3 grid gap-2 sm:grid-cols-2"
              >
                <select className={field} value={channel} onChange={(e) => setChannel(e.target.value)}>
                  <option value="Phone">Phone</option>
                  <option value="Email">Email</option>
                  <option value="Portal">Portal</option>
                  <option value="WalkIn">WalkIn</option>
                  <option value="Other">Other</option>
                </select>
                <select className={field} value={priority} onChange={(e) => setPriority(e.target.value)}>
                  <option value="Low">Low</option>
                  <option value="Normal">Normal</option>
                  <option value="High">High</option>
                  <option value="Critical">Critical</option>
                </select>
                <input className={`${field} sm:col-span-2`} value={subject} onChange={(e) => setSubject(e.target.value)} placeholder="Tiêu đề" required />
                <select className={field} value={serviceTypeId} onChange={(e) => setServiceTypeId(e.target.value)}>
                  <option value="">— Loại DV —</option>
                  {types.map((t) => <option key={t.id} value={t.id}>{t.code}</option>)}
                </select>
                <select className={field} value={faultCodeId} onChange={(e) => setFaultCodeId(e.target.value)}>
                  <option value="">— Mã lỗi —</option>
                  {faults.map((f) => <option key={f.id} value={f.id}>{f.code}</option>)}
                </select>
                <select className={`${field} sm:col-span-2`} value={ticketAssetId} onChange={(e) => setTicketAssetId(e.target.value)}>
                  <option value="">— Thiết bị —</option>
                  {assets.map((a) => <option key={a.id} value={a.id}>{a.code} · {a.serialNo}</option>)}
                </select>
                <button type="submit" className={`${btn.primary} sm:col-span-2`}>Tạo ticket</button>
              </form>
            )}
            <div className={`${tableWrap} mb-3`}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>Tiêu đề</th>
                    <th className={th}>Ưu tiên</th>
                    <th className={th}>TT</th>
                  </tr>
                </thead>
                <tbody>
                  {tickets.map((t) => (
                    <tr
                      key={t.id}
                      className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedTicketId === t.id ? "bg-[var(--surface-2)]" : ""}`}
                      onClick={() => setSelectedTicketId(t.id)}
                    >
                      <td className={td}>{t.code}</td>
                      <td className={td}>
                        <div>{t.subject}</div>
                        <div className="text-xs text-[var(--muted)]">{t.channel} · {t.customerName}</div>
                      </td>
                      <td className={td}>{t.priority}</td>
                      <td className={td}>
                        <span className={statusPill(t.status === "Resolved" || t.status === "Closed" ? "success" : t.status === "Escalated" ? "warning" : "muted")}>
                          {t.status}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {selectedTicket && (
              <div>
                <p className="mb-2 text-xs text-[var(--muted)]">
                  KTV: {selectedTicket.assignedTechName || "—"} · SLA: {selectedTicket.slaPolicyName || "—"}
                  {selectedTicket.dueResolveAt
                    ? ` · hạn XL ${new Date(selectedTicket.dueResolveAt).toLocaleString()}`
                    : ""}
                  {selectedTicket.appointmentAt
                    ? ` · lịch ${new Date(selectedTicket.appointmentAt).toLocaleString()}`
                    : ""}
                  {selectedTicket.escalateReason ? ` · Escalate: ${selectedTicket.escalateReason}` : ""}
                  {selectedTicket.slaResolveMet != null
                    ? ` · SLA resolve: ${selectedTicket.slaResolveMet ? "đạt" : "trễ"}`
                    : ""}
                  {selectedTicket.acceptanceSignerName
                    ? ` · NT: ${selectedTicket.acceptanceSignerName}`
                    : ""}
                </p>
                {canTicketManage && (
                  <>
                    <div className="mb-2 flex flex-wrap gap-2">
                      <select className={`${field} w-44`} value={techId} onChange={(e) => setTechId(e.target.value)}>
                        {users.map((u) => (
                          <option key={u.id} value={u.id}>{u.displayName || u.username}</option>
                        ))}
                      </select>
                      <button
                        type="button"
                        className={btn.ghost}
                        onClick={() => run(() => assignFsmTicket(selectedTicket.id, { techUserId: techId }), "Đã phân công.")}
                      >
                        Phân công
                      </button>
                      <button
                        type="button"
                        className={btn.ghost}
                        onClick={() => run(() => setFsmTicketStatus(selectedTicket.id, "InProgress"), "Đang xử lý.")}
                      >
                        Đang XL
                      </button>
                    </div>
                    <div className="mb-2 flex flex-wrap gap-2">
                      <input
                        className={`${field} w-48`}
                        type="datetime-local"
                        value={apptAt}
                        onChange={(e) => setApptAt(e.target.value)}
                      />
                      <button
                        type="button"
                        className={btn.soft}
                        disabled={!apptAt}
                        onClick={() =>
                          run(
                            () => setFsmAppointment(selectedTicket.id, {
                              appointmentAt: new Date(apptAt).toISOString(),
                            }),
                            "Đã đặt lịch hẹn.",
                          )
                        }
                      >
                        Lịch hẹn
                      </button>
                    </div>
                    <div className="mb-2 grid gap-2 sm:grid-cols-2">
                      <input className={field} value={rootCause} onChange={(e) => setRootCause(e.target.value)} placeholder="Nguyên nhân" />
                      <input className={field} value={resolution} onChange={(e) => setResolution(e.target.value)} placeholder="Cách xử lý" />
                      <button
                        type="button"
                        className={`${btn.soft} sm:col-span-2`}
                        disabled={!rootCause || !resolution}
                        onClick={() =>
                          run(
                            () => workLogFsmTicket(selectedTicket.id, {
                              rootCause,
                              resolutionNote: resolution,
                            }),
                            "Đã ghi nguyên nhân / xử lý.",
                          )
                        }
                      >
                        Ghi xử lý
                      </button>
                    </div>
                    <div className="mb-2 flex flex-wrap gap-2">
                      <button
                        type="button"
                        className={btn.ghost}
                        onClick={() => run(() => checkoutFsmTicket(selectedTicket.id), "Check-out / Resolved.")}
                      >
                        Check-out
                      </button>
                      <input
                        className={`${field} w-40`}
                        value={signerName}
                        onChange={(e) => setSignerName(e.target.value)}
                        placeholder="Khách ký NT"
                      />
                      <button
                        type="button"
                        className={btn.ghost}
                        disabled={!signerName}
                        onClick={() =>
                          run(
                            () => acceptFsmTicket(selectedTicket.id, { signerName }),
                            "Đã nghiệm thu.",
                          )
                        }
                      >
                        Nghiệm thu
                      </button>
                      <button
                        type="button"
                        className={btn.primary}
                        onClick={() => run(() => closeFsmTicket(selectedTicket.id), "Đã đóng ticket.")}
                      >
                        Đóng (SLA)
                      </button>
                    </div>
                    <div className="flex gap-2">
                      <input
                        className={field}
                        value={escReason}
                        onChange={(e) => setEscReason(e.target.value)}
                        placeholder="Lý do escalate"
                      />
                      <button
                        type="button"
                        className={btn.ghost}
                        onClick={() =>
                          run(
                            () => escalateFsmTicket(selectedTicket.id, {
                              newTechUserId: techId,
                              reason: escReason || "Escalate lên KTV khác",
                            }),
                            "Đã escalate.",
                          )
                        }
                      >
                        Escalate
                      </button>
                    </div>
                    <div className="mt-3 border-t border-[var(--border)] pt-3">
                      <p className="mb-2 text-xs font-medium text-[var(--muted)]">Xuất linh kiện theo ticket (UC_FSM_024)</p>
                      <div className="mb-2 flex flex-wrap gap-2">
                        <select className={`${field} w-44`} value={consumePartId} onChange={(e) => setConsumePartId(e.target.value)}>
                          {parts.map((p) => <option key={p.id} value={p.id}>{p.code}</option>)}
                        </select>
                        <input className={`${field} w-24`} type="number" min={0.001} step="any" value={consumeQty} onChange={(e) => setConsumeQty(e.target.value)} />
                        <button
                          type="button"
                          className={btn.ghost}
                          disabled={!consumePartId}
                          onClick={() =>
                            run(async () => {
                              await consumeFsmTicketPart(selectedTicket.id, {
                                partId: consumePartId,
                                qty: Number(consumeQty),
                                source: "Tech",
                              });
                              setTicketParts(await fetchFsmTicketParts(selectedTicket.id));
                            }, "Đã xuất linh kiện.")
                          }
                        >
                          Xuất từ túi KTV
                        </button>
                      </div>
                    </div>
                  </>
                )}
                {ticketParts.length > 0 && (
                  <ul className="mt-2 space-y-1 text-xs text-[var(--muted)]">
                    {ticketParts.map((l) => (
                      <li key={l.id}>{l.partCode} × {l.qty} · {l.amount} ({l.source})</li>
                    ))}
                  </ul>
                )}
              </div>
            )}
          </section>
        )}
      </div>
    </div>
  );
}
