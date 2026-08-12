"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  approveMfgWorkOrder,
  calculateMfgCost,
  cancelMfgPlan,
  cancelMfgWorkOrder,
  closeMfgWorkOrder,
  confirmMfgPlan,
  downloadMfgWorkOrderCsv,
  fetchMfgItems,
  fetchMfgPlanDetail,
  fetchMfgPlans,
  fetchMfgWorkOrderDetail,
  fetchMfgWorkOrders,
  fetchMfgWorkshops,
  issueMfgMaterials,
  pauseMfgWorkOrder,
  printMfgWorkOrder,
  pushMfgCost,
  receiveMfgFg,
  recordMfgScrap,
  releaseMfgWorkOrder,
  resumeMfgWorkOrder,
  upsertMfgPlan,
  upsertMfgPlanLine,
  upsertMfgWorkOrder,
  type MfgItemDto,
  type MfgPlanDetailDto,
  type MfgPlanDto,
  type MfgWorkOrderDetailDto,
  type MfgWorkOrderDto,
  type MfgWorkshopDto,
} from "@/shared/api/mfg-api";
import {
  canApproveWorkOrder,
  canCancelPlan,
  canConfirmPlan,
  canPrintWorkOrder,
  canReleaseWorkOrder,
  validatePlanSourceOrder,
  validateWorkOrderCreate,
} from "@/shared/api/mfg-step111-helpers";
import { fetchFinAccounts, fetchFinPeriods, type FinAccountDto, type FinPeriodDto } from "@/shared/api/fin-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

function money(n: number) {
  return n.toLocaleString("vi-VN", { maximumFractionDigits: 2 });
}

export default function MfgOrdersPage() {
  const { can } = usePermissions();
  const canPlan = can("mfg.plan.read");
  const canPlanManage = can("mfg.plan.manage");
  const canWo = can("mfg.wo.read");
  const canWoManage = can("mfg.wo.manage");
  const canRead = canPlan || canWo;

  const [plans, setPlans] = useState<MfgPlanDto[]>([]);
  const [wos, setWos] = useState<MfgWorkOrderDto[]>([]);
  const [items, setItems] = useState<MfgItemDto[]>([]);
  const [workshops, setWorkshops] = useState<MfgWorkshopDto[]>([]);
  const [selectedPlanId, setSelectedPlanId] = useState("");
  const [selectedWoId, setSelectedWoId] = useState("");
  const [planDetail, setPlanDetail] = useState<MfgPlanDetailDto | null>(null);
  const [woDetail, setWoDetail] = useState<MfgWorkOrderDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [so, setSo] = useState("SO-001");
  const [planItemId, setPlanItemId] = useState("");
  const [planQty, setPlanQty] = useState("10");
  const [planWsId, setPlanWsId] = useState("");
  const [woItemId, setWoItemId] = useState("");
  const [woQty, setWoQty] = useState("10");
  const [woWsId, setWoWsId] = useState("");
  const [issueItemId, setIssueItemId] = useState("");
  const [issueQty, setIssueQty] = useState("1");
  const [fgQty, setFgQty] = useState("1");
  const [scrapQty, setScrapQty] = useState("1");
  const [scrapType, setScrapType] = useState("Scrap");
  const [cancelReason, setCancelReason] = useState("Hủy theo yêu cầu SX");
  const [slipPreview, setSlipPreview] = useState<string | null>(null);
  const [periods, setPeriods] = useState<FinPeriodDto[]>([]);
  const [accounts, setAccounts] = useState<FinAccountDto[]>([]);
  const [periodId, setPeriodId] = useState("");
  const [wipAccId, setWipAccId] = useState("");
  const [fgAccId, setFgAccId] = useState("");

  const load = useCallback(async () => {
    const [p, w, i, ws, per, acc] = await Promise.all([
      canPlan ? fetchMfgPlans() : Promise.resolve([] as MfgPlanDto[]),
      canWo ? fetchMfgWorkOrders() : Promise.resolve([] as MfgWorkOrderDto[]),
      fetchMfgItems().catch(() => [] as MfgItemDto[]),
      fetchMfgWorkshops().catch(() => [] as MfgWorkshopDto[]),
      fetchFinPeriods().catch(() => [] as FinPeriodDto[]),
      fetchFinAccounts().catch(() => [] as FinAccountDto[]),
    ]);
    setPlans(p); setWos(w); setItems(i); setWorkshops(ws);
    setPeriods(per.filter((x) => x.status !== "Locked"));
    setAccounts(acc.filter((x) => x.isPostable && x.status === "Active"));
    if (!periodId && per[0]) setPeriodId(per.find((x) => x.status !== "Locked")?.id ?? "");
    if (!wipAccId && acc[0]) setWipAccId(acc.find((x) => x.isPostable)?.id ?? "");
    if (!fgAccId && acc[1]) setFgAccId(acc.filter((x) => x.isPostable)[1]?.id ?? acc[0]?.id ?? "");
    const fg = i.filter((x) => x.itemType === "FG" || x.itemType === "SFG");
    if (!planItemId && fg[0]) setPlanItemId(fg[0].id);
    if (!woItemId && fg[0]) setWoItemId(fg[0].id);
    if (!planWsId && ws[0]) setPlanWsId(ws[0].id);
    if (!woWsId && ws[0]) setWoWsId(ws[0].id);
    if (!selectedPlanId && p[0]) setSelectedPlanId(p[0].id);
    if (!selectedWoId && w[0]) setSelectedWoId(w[0].id);
  }, [canPlan, canWo, planItemId, woItemId, planWsId, woWsId, selectedPlanId, selectedWoId, periodId, wipAccId, fgAccId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedPlanId || !canPlan) return;
    fetchMfgPlanDetail(selectedPlanId).then(setPlanDetail).catch((e: Error) => setError(e.message));
  }, [selectedPlanId, canPlan]);

  useEffect(() => {
    if (!selectedWoId || !canWo) return;
    fetchMfgWorkOrderDetail(selectedWoId)
      .then((d) => {
        setWoDetail(d);
        if (!issueItemId && d.requiredMaterials[0]) setIssueItemId(d.requiredMaterials[0].componentItemId);
      })
      .catch((e: Error) => setError(e.message));
  }, [selectedWoId, canWo, issueItemId]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedPlanId && canPlan) setPlanDetail(await fetchMfgPlanDetail(selectedPlanId));
      if (selectedWoId && canWo) setWoDetail(await fetchMfgWorkOrderDetail(selectedWoId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem KH / lệnh SX.</div>;
  }

  const fgItems = items.filter((x) => x.itemType === "FG" || x.itemType === "SFG");

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">KH / lệnh sản xuất</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          KH · lệnh · NVL/TP · phế · WIP · giá thành (UC_MFG_013, 017–020, 022–025, 027/029/031)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        {canPlan && (
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Kế hoạch SX</h2>
            {canPlanManage && (
              <form
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  const v = validatePlanSourceOrder(so);
                  if (!v.isValid) { setError(v.error ?? "SO không hợp lệ."); return; }
                  run(async () => {
                    const p = await upsertMfgPlan({ sourceOrderCode: so });
                    setSelectedPlanId(p.id);
                  }, "Đã tạo KH.");
                }}
                className="mb-3 flex gap-2"
              >
                <input className={field} value={so} onChange={(e) => setSo(e.target.value)} placeholder="Mã SO" required />
                <button type="submit" className={btn.primary}>Tạo KH</button>
              </form>
            )}
            <div className={`${tableWrap} mb-3`}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>SO</th>
                    <th className={th}>TT</th>
                  </tr>
                </thead>
                <tbody>
                  {plans.map((p) => (
                    <tr
                      key={p.id}
                      className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedPlanId === p.id ? "bg-[var(--surface-2)]" : ""}`}
                      onClick={() => setSelectedPlanId(p.id)}
                    >
                      <td className={td}>{p.code}</td>
                      <td className={td}>{p.sourceOrderCode}</td>
                      <td className={td}><span className={statusPill(p.status === "Confirmed" ? "success" : "muted")}>{p.status}</span></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            {planDetail && (
              <div>
                <p className="mb-2 text-xs text-[var(--muted)]">{planDetail.plan.code} · {planDetail.plan.lineCount} dòng</p>
                <ul className="mb-2 space-y-1 text-sm">
                  {planDetail.lines.map((l) => (
                    <li key={l.id}>{l.itemCode} × {l.qty} · {l.workshopName || "—"}</li>
                  ))}
                </ul>
                {canPlanManage && planDetail.plan.status === "Draft" && (
                  <>
                    <form
                      onSubmit={(e: FormEvent) => {
                        e.preventDefault();
                        run(
                          () => upsertMfgPlanLine(planDetail.plan.id, {
                            itemId: planItemId,
                            qty: Number(planQty) || 1,
                            workshopId: planWsId || null,
                          }),
                          "Đã thêm dòng KH.",
                        );
                      }}
                      className="mb-2 grid gap-2 sm:grid-cols-4"
                    >
                      <select className={field} value={planItemId} onChange={(e) => setPlanItemId(e.target.value)}>
                        {fgItems.map((i) => <option key={i.id} value={i.id}>{i.code}</option>)}
                      </select>
                      <input className={field} value={planQty} onChange={(e) => setPlanQty(e.target.value)} />
                      <select className={field} value={planWsId} onChange={(e) => setPlanWsId(e.target.value)}>
                        {workshops.map((w) => <option key={w.id} value={w.id}>{w.code}</option>)}
                      </select>
                      <button type="submit" className={btn.ghost}>Thêm</button>
                    </form>
                    <div className="flex flex-wrap gap-2">
                      <button
                        type="button"
                        className={btn.ghost}
                        disabled={!canConfirmPlan(planDetail.plan.status, planDetail.lines.length).canConfirm}
                        onClick={() => {
                          const c = canConfirmPlan(planDetail.plan.status, planDetail.lines.length);
                          if (!c.canConfirm) { setError(c.reason ?? "Không xác nhận được."); return; }
                          run(() => confirmMfgPlan(planDetail.plan.id), "Đã xác nhận KH.");
                        }}
                      >
                        Xác nhận KH
                      </button>
                      <button
                        type="button"
                        className={btn.ghost}
                        disabled={!canCancelPlan(planDetail.plan.status, 0).canCancel}
                        onClick={() => run(() => cancelMfgPlan(planDetail.plan.id), "Đã hủy KH.")}
                      >
                        Hủy KH
                      </button>
                    </div>
                  </>
                )}
                {canPlanManage && planDetail.plan.status === "Confirmed" && (
                  <button
                    type="button"
                    className={btn.ghost}
                    disabled={!canCancelPlan(
                      planDetail.plan.status,
                      wos.filter((w) => w.planId === planDetail.plan.id && w.status !== "Cancelled").length,
                    ).canCancel}
                    onClick={() => {
                      const linked = wos.filter((w) => w.planId === planDetail.plan.id && w.status !== "Cancelled").length;
                      const c = canCancelPlan(planDetail.plan.status, linked);
                      if (!c.canCancel) { setError(c.reason ?? "Không hủy được."); return; }
                      run(() => cancelMfgPlan(planDetail.plan.id), "Đã hủy KH.");
                    }}
                  >
                    Hủy KH
                  </button>
                )}
              </div>
            )}
          </section>
        )}

        {canWo && (
          <section className={panel}>
            <h2 className="mb-3 text-sm font-semibold">Lệnh sản xuất</h2>
            {canWoManage && (
              <form
                onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  const planStatus = plans.find((p) => p.id === selectedPlanId)?.status ?? null;
                  const v = validateWorkOrderCreate(
                    woItemId,
                    Number(woQty) || 0,
                    selectedPlanId || null,
                    selectedPlanId ? planStatus : null,
                  );
                  if (!v.isValid) { setError(v.error ?? "LSX không hợp lệ."); return; }
                  run(async () => {
                    const wo = await upsertMfgWorkOrder({
                      itemId: woItemId,
                      qty: Number(woQty) || 1,
                      workshopId: woWsId || null,
                      planId: selectedPlanId || null,
                    });
                    setSelectedWoId(wo.id);
                  }, "Đã tạo lệnh SX.");
                }}
                className="mb-3 grid gap-2 sm:grid-cols-4"
              >
                <select className={field} value={woItemId} onChange={(e) => setWoItemId(e.target.value)}>
                  {fgItems.map((i) => <option key={i.id} value={i.id}>{i.code}</option>)}
                </select>
                <input className={field} value={woQty} onChange={(e) => setWoQty(e.target.value)} placeholder="SL" />
                <select className={field} value={woWsId} onChange={(e) => setWoWsId(e.target.value)}>
                  {workshops.map((w) => <option key={w.id} value={w.id}>{w.code}</option>)}
                </select>
                <button type="submit" className={btn.primary}>Tạo LSX</button>
              </form>
            )}
            <div className={`${tableWrap} mb-3`}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>SP</th>
                    <th className={th}>SL</th>
                    <th className={th}>TT</th>
                  </tr>
                </thead>
                <tbody>
                  {wos.map((w) => (
                    <tr
                      key={w.id}
                      className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedWoId === w.id ? "bg-[var(--surface-2)]" : ""}`}
                      onClick={() => setSelectedWoId(w.id)}
                    >
                      <td className={td}>{w.code}</td>
                      <td className={td}>{w.itemCode}</td>
                      <td className={td}>
                        {w.qtyFgReceived}/{w.qty}
                        {w.qtyScrap > 0 ? ` · phế ${w.qtyScrap}` : ""}
                      </td>
                      <td className={td}>
                        <span className={statusPill(
                          w.status === "Closed" || w.status === "Completed" ? "success"
                            : w.status === "Cancelled" ? "danger"
                              : w.status === "Paused" ? "warning" : "muted",
                        )}>{w.status}</span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {woDetail && (
              <div>
                <p className="mb-2 text-xs text-[var(--muted)]">
                  {woDetail.order.code} · BOM {woDetail.order.bomCode || "—"} · NVL xuất {woDetail.order.qtyIssuedMaterial}
                  · phế {woDetail.order.qtyScrap}
                  {woDetail.order.printedAt ? " · đã in phiếu" : ""}
                  {woDetail.order.cancelReason ? ` · hủy: ${woDetail.order.cancelReason}` : ""}
                </p>
                <h3 className="mb-1 text-xs font-semibold uppercase text-[var(--muted)]">Định mức BOM</h3>
                <ul className="mb-2 space-y-1 text-sm">
                  {woDetail.requiredMaterials.map((m) => (
                    <li key={m.id}>{m.componentCode} × {m.qty * woDetail.order.qty} (chuẩn {m.qty}/1)</li>
                  ))}
                </ul>
                {canWoManage && (
                  <div className="mb-3 flex flex-wrap gap-2">
                    {canApproveWorkOrder(woDetail.order.status).canApprove && (
                      <button type="button" className={btn.ghost} onClick={() => run(() => approveMfgWorkOrder(woDetail.order.id), "Đã duyệt.")}>
                        Duyệt
                      </button>
                    )}
                    {canReleaseWorkOrder(woDetail.order.status).canRelease && (
                      <button type="button" className={btn.ghost} onClick={() => run(() => releaseMfgWorkOrder(woDetail.order.id), "Đã phát hành lệnh.")}>
                        Phát hành
                      </button>
                    )}
                    {canPrintWorkOrder(woDetail.order.status).canPrint && (
                      <>
                        <button
                          type="button"
                          className={btn.ghost}
                          onClick={() => run(async () => {
                            const printed = await printMfgWorkOrder(woDetail.order.id);
                            setSlipPreview(printed.slipText);
                          }, "Đã in phiếu LSX.")}
                        >
                          In phiếu
                        </button>
                        <button
                          type="button"
                          className={btn.ghost}
                          onClick={() => run(async () => {
                            const blob = await downloadMfgWorkOrderCsv(woDetail.order.id);
                            const url = URL.createObjectURL(blob);
                            const a = document.createElement("a");
                            a.href = url;
                            a.download = `LSX_${woDetail.order.code}.csv`;
                            a.click();
                            URL.revokeObjectURL(url);
                          }, "Đã xuất CSV phiếu LSX.")}
                        >
                          Xuất CSV
                        </button>
                      </>
                    )}
                  </div>
                )}
                {slipPreview && (
                  <pre className="mb-3 max-h-48 overflow-auto rounded-md bg-[var(--surface-2)] p-3 text-xs whitespace-pre-wrap">
                    {slipPreview}
                  </pre>
                )}
                {canWoManage && (woDetail.order.status === "Released" || woDetail.order.status === "MaterialsIssued") && (
                  <form
                    onSubmit={(e: FormEvent) => {
                      e.preventDefault();
                      run(
                        () => issueMfgMaterials(woDetail.order.id, {
                          itemId: issueItemId,
                          qty: Number(issueQty) || 1,
                        }),
                        "Đã xuất NVL.",
                      );
                    }}
                    className="mb-2 grid gap-2 sm:grid-cols-3"
                  >
                    <select className={field} value={issueItemId} onChange={(e) => setIssueItemId(e.target.value)}>
                      {(woDetail.requiredMaterials.length
                        ? woDetail.requiredMaterials.map((m) => (
                            <option key={m.id} value={m.componentItemId}>{m.componentCode}</option>
                          ))
                        : items.filter((i) => i.itemType === "RM" || i.itemType === "SFG").map((i) => (
                            <option key={i.id} value={i.id}>{i.code}</option>
                          )))}
                    </select>
                    <input className={field} value={issueQty} onChange={(e) => setIssueQty(e.target.value)} />
                    <button type="submit" className={btn.ghost}>Xuất NVL</button>
                  </form>
                )}
                {canWoManage && (woDetail.order.status === "Released" || woDetail.order.status === "MaterialsIssued") && (
                  <form
                    onSubmit={(e: FormEvent) => {
                      e.preventDefault();
                      run(
                        () => receiveMfgFg(woDetail.order.id, { qty: Number(fgQty) || 1 }),
                        "Đã nhập TP.",
                      );
                    }}
                    className="mb-2 flex gap-2"
                  >
                    <input className={field} value={fgQty} onChange={(e) => setFgQty(e.target.value)} placeholder="SL TP" />
                    <button type="submit" className={btn.primary}>Nhập TP</button>
                  </form>
                )}
                {canWoManage && (woDetail.order.status === "Released" || woDetail.order.status === "MaterialsIssued" || woDetail.order.status === "Completed") && (
                  <form
                    onSubmit={(e: FormEvent) => {
                      e.preventDefault();
                      run(
                        () => recordMfgScrap(woDetail.order.id, {
                          itemId: issueItemId || null,
                          qty: Number(scrapQty) || 1,
                          scrapType,
                        }),
                        "Đã ghi phế / hao hụt.",
                      );
                    }}
                    className="mb-2 grid gap-2 sm:grid-cols-3"
                  >
                    <select className={field} value={scrapType} onChange={(e) => setScrapType(e.target.value)}>
                      <option value="Scrap">Scrap</option>
                      <option value="Loss">Loss</option>
                    </select>
                    <input className={field} value={scrapQty} onChange={(e) => setScrapQty(e.target.value)} placeholder="SL phế" />
                    <button type="submit" className={btn.ghost}>Ghi phế</button>
                  </form>
                )}
                {canWoManage && (
                  <div className="mb-2 flex flex-wrap gap-2">
                    {(woDetail.order.status === "Released" || woDetail.order.status === "MaterialsIssued") && (
                      <button type="button" className={btn.ghost} onClick={() => run(() => pauseMfgWorkOrder(woDetail.order.id), "Đã tạm dừng.")}>
                        Tạm dừng
                      </button>
                    )}
                    {woDetail.order.status === "Paused" && (
                      <button type="button" className={btn.primary} onClick={() => run(() => resumeMfgWorkOrder(woDetail.order.id), "Đã tiếp tục.")}>
                        Tiếp tục
                      </button>
                    )}
                    {(woDetail.order.status === "Completed" || woDetail.order.status === "MaterialsIssued" || woDetail.order.status === "Released" || woDetail.order.status === "Paused") && (
                      <button type="button" className={btn.primary} onClick={() => run(() => closeMfgWorkOrder(woDetail.order.id), "Đã đóng lệnh.")}>
                        Đóng lệnh
                      </button>
                    )}
                    {woDetail.order.status !== "Closed" && woDetail.order.status !== "Cancelled" && woDetail.order.status !== "Completed" && (
                      <>
                        <input className={`${field} min-w-[160px]`} value={cancelReason} onChange={(e) => setCancelReason(e.target.value)} placeholder="Lý do hủy" />
                        <button type="button" className={btn.ghost} onClick={() => run(() => cancelMfgWorkOrder(woDetail.order.id, cancelReason), "Đã hủy lệnh.")}>
                          Hủy lệnh
                        </button>
                      </>
                    )}
                  </div>
                )}
                <ul className="mt-3 max-h-28 space-y-1 overflow-auto text-xs text-[var(--muted)]">
                  {woDetail.issues.map((i) => (
                    <li key={i.id}>Xuất {i.itemCode} × {i.qty} · ĐG {money(i.unitCost)} · {money(i.amount)}</li>
                  ))}
                  {woDetail.receipts.map((r) => (
                    <li key={r.id}>Nhập TP × {r.qty} · {new Date(r.receivedAt).toLocaleString()}</li>
                  ))}
                  {woDetail.scraps.map((s) => (
                    <li key={s.id}>{s.scrapType} {s.itemCode || "—"} × {s.qty} · {new Date(s.recordedAt).toLocaleString()}</li>
                  ))}
                </ul>

                {(woDetail.order.qtyFgReceived > 0 || woDetail.costSheet) && (
                  <div className="mt-4 rounded-md border border-black/10 p-3">
                    <h3 className="mb-2 text-xs font-semibold uppercase text-[var(--muted)]">
                      Giá thành (UC_MFG_027 · 029 · 031)
                    </h3>
                    {woDetail.costSheet ? (
                      <div className="mb-2 space-y-1 text-sm">
                        <div>
                          {woDetail.costSheet.code} ·{" "}
                          <span className={statusPill(
                            woDetail.costSheet.status === "Pushed" ? "success" : "brand",
                          )}>{woDetail.costSheet.status}</span>
                        </div>
                        <div>NVL: <b>{money(woDetail.costSheet.materialCost)}</b> · TP tốt: {woDetail.costSheet.goodQty}</div>
                        <div>Đơn giá TP: <b>{money(woDetail.costSheet.unitCost)}</b></div>
                        {woDetail.costSheet.invSkuCode && (
                          <div className="text-xs text-[var(--muted)]">INV {woDetail.costSheet.invSkuCode}</div>
                        )}
                        {woDetail.costSheet.finJournalCode && (
                          <div className="text-xs text-[var(--muted)]">BT {woDetail.costSheet.finJournalCode}</div>
                        )}
                        <ul className="max-h-24 space-y-1 overflow-auto text-xs text-[var(--muted)]">
                          {woDetail.costSheet.lines.map((l) => (
                            <li key={l.id}>{l.itemCode} × {l.qty} · {money(l.amount)}</li>
                          ))}
                        </ul>
                      </div>
                    ) : (
                      <p className="mb-2 text-xs text-[var(--muted)]">Chưa tính giá thành — cần xuất NVL + nhập TP.</p>
                    )}
                    {canWoManage && (
                      <div className="grid gap-2">
                        {woDetail.costSheet?.status !== "Pushed" && (
                          <button
                            type="button"
                            className={btn.primary}
                            onClick={() => run(() => calculateMfgCost(woDetail.order.id), "Đã tính giá thành.")}
                          >
                            Tính giá thành NVL
                          </button>
                        )}
                        {woDetail.costSheet?.status === "Calculated" && (
                          <>
                            <select className={field} value={periodId} onChange={(e) => setPeriodId(e.target.value)}>
                              <option value="">— Kỳ KT (auto tháng hiện tại) —</option>
                              {periods.map((p) => <option key={p.id} value={p.id}>{p.code}</option>)}
                            </select>
                            <select className={field} value={wipAccId} onChange={(e) => setWipAccId(e.target.value)}>
                              <option value="">— TK WIP (auto 154*) —</option>
                              {accounts.map((a) => <option key={a.id} value={a.id}>{a.code} · {a.name}</option>)}
                            </select>
                            <select className={field} value={fgAccId} onChange={(e) => setFgAccId(e.target.value)}>
                              <option value="">— TK TP (auto 155*) —</option>
                              {accounts.map((a) => <option key={a.id} value={a.id}>{a.code} · {a.name}</option>)}
                            </select>
                            <button
                              type="button"
                              className={btn.ghost}
                              onClick={() => run(async () => {
                                const sheet = await pushMfgCost(woDetail.order.id, {
                                  periodId: periodId || null,
                                  wipAccountId: wipAccId || null,
                                  fgAccountId: fgAccId || null,
                                });
                                return sheet;
                              }, "Đã đẩy giá thành INV + JE WIP→TP (auto TK/kỳ nếu trống).")}
                            >
                              Đẩy INV + FIN JE
                            </button>
                          </>
                        )}
                      </div>
                    )}
                  </div>
                )}
              </div>
            )}
          </section>
        )}
      </div>
    </div>
  );
}
