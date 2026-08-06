"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  approvePurPo,
  approvePurPr,
  createPoFromPr,
  fetchPurPoDetail,
  fetchPurPos,
  fetchPurPrDetail,
  fetchPurPrs,
  fetchPurVendors,
  rejectPurPr,
  returnPurPr,
  cancelPurPo,
  closePurPo,
  printPurPo,
  revisePurPo,
  sendPurPo,
  submitPurPo,
  submitPurPr,
  upsertPurPo,
  upsertPurPoLine,
  upsertPurPr,
  upsertPurPrLine,
  type PurPoDetailDto,
  type PurPrDetailDto,
  type PurPurchaseOrderDto,
  type PurPurchaseRequestDto,
  type PurVendorDto,
} from "@/shared/api/pur-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

const PR_TONE: Record<string, "muted" | "brand" | "success" | "warning" | "danger"> = {
  Draft: "muted",
  Submitted: "brand",
  Approved: "success",
  Rejected: "danger",
  Returned: "warning",
};

const PO_TONE: Record<string, "muted" | "brand" | "success" | "warning" | "danger"> = {
  Draft: "muted",
  PendingApproval: "warning",
  Approved: "brand",
  Sent: "success",
  Closed: "success",
  Cancelled: "danger",
};

export default function PurOrdersPage() {
  const { can } = usePermissions();
  const canPrRead = can("pur.pr.read");
  const canPrManage = can("pur.pr.manage");
  const canPrApprove = can("pur.pr.approve");
  const canPoRead = can("pur.po.read");
  const canPoManage = can("pur.po.manage");
  const canPoApprove = can("pur.po.approve");

  const [tab, setTab] = useState<"pr" | "po">("pr");
  const [prs, setPrs] = useState<PurPurchaseRequestDto[]>([]);
  const [pos, setPos] = useState<PurPurchaseOrderDto[]>([]);
  const [vendors, setVendors] = useState<PurVendorDto[]>([]);
  const [prId, setPrId] = useState("");
  const [poId, setPoId] = useState("");
  const [prDetail, setPrDetail] = useState<PurPrDetailDto | null>(null);
  const [poDetail, setPoDetail] = useState<PurPoDetailDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [prCode, setPrCode] = useState("PR-001");
  const [prUnit, setPrUnit] = useState("Kinh doanh");
  const [lineCode, setLineCode] = useState("SP-001");
  const [lineName, setLineName] = useState("");
  const [lineQty, setLineQty] = useState("1");
  const [decisionNote, setDecisionNote] = useState("");
  const [poCode, setPoCode] = useState("PO-001");
  const [poVendorId, setPoVendorId] = useState("");
  const [poLinePrice, setPoLinePrice] = useState("0");

  const load = useCallback(async () => {
    const jobs: Promise<void>[] = [];
    if (canPrRead) {
      jobs.push(
        fetchPurPrs().then((rows) => {
          setPrs(rows);
          if (!prId && rows[0]) setPrId(rows[0].id);
        }),
      );
    }
    if (canPoRead) {
      jobs.push(
        fetchPurPos().then((rows) => {
          setPos(rows);
          if (!poId && rows[0]) setPoId(rows[0].id);
        }),
      );
    }
    jobs.push(
      fetchPurVendors()
        .then((v) => {
          setVendors(v);
          if (!poVendorId && v[0]) setPoVendorId(v[0].id);
        })
        .catch(() => undefined),
    );
    await Promise.all(jobs);
  }, [canPrRead, canPoRead, prId, poId, poVendorId]);

  useEffect(() => {
    load().catch((e: Error) => setError(e.message));
  }, [load]);

  useEffect(() => {
    if (!prId || !canPrRead) return;
    fetchPurPrDetail(prId)
      .then(setPrDetail)
      .catch((e: Error) => setError(e.message));
  }, [prId, canPrRead]);

  useEffect(() => {
    if (!poId || !canPoRead) return;
    fetchPurPoDetail(poId)
      .then(setPoDetail)
      .catch((e: Error) => setError(e.message));
  }, [poId, canPoRead]);

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function refreshPr() {
    await load();
    if (prId) setPrDetail(await fetchPurPrDetail(prId));
  }

  async function refreshPo() {
    await load();
    if (poId) setPoDetail(await fetchPurPoDetail(poId));
  }

  if (!canPrRead && !canPoRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền xem PR/PO.</div>;
  }

  return (
    <div className="space-y-4 p-4 md:p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">PR / PO</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Tạo PR · duyệt/từ chối/trả · tạo PO từ PR · duyệt hạn mức · gửi NCC (UC_PUR_014, 017–019, 026–028)
        </p>
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}

      <div className="flex gap-2">
        {canPrRead && (
          <button type="button" className={tab === "pr" ? btn.primary : btn.ghost} onClick={() => setTab("pr")}>
            Yêu cầu mua (PR)
          </button>
        )}
        {canPoRead && (
          <button type="button" className={tab === "po" ? btn.primary : btn.ghost} onClick={() => setTab("po")}>
            Đơn mua (PO)
          </button>
        )}
      </div>

      {tab === "pr" && canPrRead && (
        <div className="grid gap-4 xl:grid-cols-[1fr_1.2fr]">
          <section className={panel}>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>Đơn vị</th>
                    <th className={th}>TT</th>
                    <th className={th}>Dòng</th>
                  </tr>
                </thead>
                <tbody>
                  {prs.map((p) => (
                    <tr
                      key={p.id}
                      className={`cursor-pointer hover:bg-[var(--surface-2)] ${prId === p.id ? "bg-[var(--surface-2)]" : ""}`}
                      onClick={() => setPrId(p.id)}
                    >
                      <td className={td}>{p.code}</td>
                      <td className={td}>{p.requestingUnit ?? "—"}</td>
                      <td className={td}>
                        <span className={statusPill(PR_TONE[p.status] ?? "muted")}>{p.status}</span>
                      </td>
                      <td className={td}>{p.lineCount}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>

          <div className="space-y-4">
            {canPrManage && (
              <section className={panel}>
                <h2 className="mb-3 text-sm font-semibold">Tạo PR</h2>
                <form
                  className="grid gap-2 sm:grid-cols-2"
                  onSubmit={async (e: FormEvent) => {
                    e.preventDefault();
                    try {
                      const saved = await upsertPurPr({ code: prCode, requestingUnit: prUnit });
                      await load();
                      setPrId(saved.id);
                      flash("Đã tạo PR.");
                    } catch (err) {
                      setError((err as Error).message);
                    }
                  }}
                >
                  <input className={field} value={prCode} onChange={(e) => setPrCode(e.target.value)} required />
                  <input className={field} value={prUnit} onChange={(e) => setPrUnit(e.target.value)} />
                  <button type="submit" className={`${btn.primary} sm:col-span-2`}>Tạo PR</button>
                </form>
              </section>
            )}

            {prDetail && (
              <section className={panel}>
                <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
                  <div>
                    <h2 className="text-sm font-semibold">{prDetail.header.code}</h2>
                    <p className="text-xs text-[var(--muted)]">
                      {prDetail.header.requestingUnit} · {prDetail.header.requestedByName}
                    </p>
                  </div>
                  <span className={statusPill(PR_TONE[prDetail.header.status] ?? "muted")}>
                    {prDetail.header.status}
                  </span>
                </div>

                <ul className="mb-3 space-y-1 text-sm">
                  {prDetail.lines.map((l) => (
                    <li key={l.id}>{l.productCode} · {l.productName} · {l.qty} {l.unit}</li>
                  ))}
                  {prDetail.lines.length === 0 && <li className="text-[var(--muted)]">Chưa có dòng.</li>}
                </ul>

                {canPrManage && (prDetail.header.status === "Draft" || prDetail.header.status === "Returned") && (
                  <form
                    className="mb-3 grid gap-2 sm:grid-cols-3"
                    onSubmit={async (e) => {
                      e.preventDefault();
                      try {
                        await upsertPurPrLine(prId, {
                          productCode: lineCode,
                          productName: lineName,
                          qty: Number(lineQty) || 1,
                        });
                        setLineName("");
                        await refreshPr();
                        flash("Đã thêm dòng PR.");
                      } catch (err) {
                        setError((err as Error).message);
                      }
                    }}
                  >
                    <input className={field} value={lineCode} onChange={(e) => setLineCode(e.target.value)} required />
                    <input className={field} placeholder="Tên SP" value={lineName} onChange={(e) => setLineName(e.target.value)} required />
                    <input className={field} type="number" min={0.001} step="any" value={lineQty} onChange={(e) => setLineQty(e.target.value)} />
                    <button type="submit" className={`${btn.ghost} sm:col-span-2`}>Thêm dòng</button>
                    <button
                      type="button"
                      className={btn.primary}
                      onClick={async () => {
                        try {
                          await submitPurPr(prId);
                          await refreshPr();
                          flash("Đã gửi duyệt PR.");
                        } catch (err) {
                          setError((err as Error).message);
                        }
                      }}
                    >
                      Gửi duyệt
                    </button>
                  </form>
                )}

                {canPrApprove && prDetail.header.status === "Submitted" && (
                  <div className="space-y-2 border-t border-[var(--border)] pt-3">
                    <input className={field} placeholder="Ghi chú quyết định" value={decisionNote} onChange={(e) => setDecisionNote(e.target.value)} />
                    <div className="flex flex-wrap gap-2">
                      <button type="button" className={btn.primary} onClick={async () => { try { await approvePurPr(prId, decisionNote); await refreshPr(); flash("Đã duyệt PR."); } catch (err) { setError((err as Error).message); } }}>Duyệt</button>
                      <button type="button" className={btn.ghost} onClick={async () => { try { await returnPurPr(prId, decisionNote); await refreshPr(); flash("Đã trả PR."); } catch (err) { setError((err as Error).message); } }}>Trả lại</button>
                      <button type="button" className={btn.ghost} onClick={async () => { try { await rejectPurPr(prId, decisionNote); await refreshPr(); flash("Đã từ chối PR."); } catch (err) { setError((err as Error).message); } }}>Từ chối</button>
                    </div>
                  </div>
                )}

                {canPoManage && prDetail.header.status === "Approved" && (
                  <div className="mt-3 flex flex-wrap gap-2 border-t border-[var(--border)] pt-3">
                    <select className={`${field} min-w-[180px]`} value={poVendorId} onChange={(e) => setPoVendorId(e.target.value)}>
                      {vendors.map((v) => (
                        <option key={v.id} value={v.id}>{v.code} · {v.name}</option>
                      ))}
                    </select>
                    <input className={field} value={poCode} onChange={(e) => setPoCode(e.target.value)} />
                    <button
                      type="button"
                      className={btn.primary}
                      onClick={async () => {
                        try {
                          const po = await createPoFromPr(prId, { code: poCode, vendorId: poVendorId });
                          setTab("po");
                          setPoId(po.id);
                          await load();
                          flash("Đã tạo PO từ PR.");
                        } catch (err) {
                          setError((err as Error).message);
                        }
                      }}
                    >
                      Tạo PO từ PR
                    </button>
                  </div>
                )}
              </section>
            )}
          </div>
        </div>
      )}

      {tab === "po" && canPoRead && (
        <div className="grid gap-4 xl:grid-cols-[1fr_1.2fr]">
          <section className={panel}>
            <div className={tableWrap}>
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th className={th}>Mã</th>
                    <th className={th}>NCC</th>
                    <th className={th}>TT</th>
                    <th className={th}>Tổng</th>
                  </tr>
                </thead>
                <tbody>
                  {pos.map((p) => (
                    <tr
                      key={p.id}
                      className={`cursor-pointer hover:bg-[var(--surface-2)] ${poId === p.id ? "bg-[var(--surface-2)]" : ""}`}
                      onClick={() => setPoId(p.id)}
                    >
                      <td className={td}>
                        <div>{p.code}</div>
                        <div className="text-xs text-[var(--muted)]">v{p.version} · nhận {p.receivedPct}%</div>
                      </td>
                      <td className={td}>{p.vendorName ?? "—"}</td>
                      <td className={td}>
                        <span className={statusPill(PO_TONE[p.status] ?? "muted")}>{p.status}</span>
                      </td>
                      <td className={td}>{p.totalAmount.toLocaleString("vi-VN")}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>

          <div className="space-y-4">
            {canPoManage && (
              <section className={panel}>
                <h2 className="mb-3 text-sm font-semibold">Tạo PO thủ công</h2>
                <form
                  className="grid gap-2 sm:grid-cols-2"
                  onSubmit={async (e) => {
                    e.preventDefault();
                    try {
                      const saved = await upsertPurPo({ code: poCode, vendorId: poVendorId });
                      await load();
                      setPoId(saved.id);
                      flash("Đã tạo PO.");
                    } catch (err) {
                      setError((err as Error).message);
                    }
                  }}
                >
                  <input className={field} value={poCode} onChange={(e) => setPoCode(e.target.value)} required />
                  <select className={field} value={poVendorId} onChange={(e) => setPoVendorId(e.target.value)}>
                    {vendors.map((v) => (
                      <option key={v.id} value={v.id}>{v.code} · {v.name}</option>
                    ))}
                  </select>
                  <button type="submit" className={`${btn.primary} sm:col-span-2`}>Tạo PO</button>
                </form>
              </section>
            )}

            {poDetail && (
              <section className={panel}>
                <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
                  <div>
                    <h2 className="text-sm font-semibold">{poDetail.header.code}</h2>
                    <p className="text-xs text-[var(--muted)]">
                      {poDetail.header.vendorName}
                      {poDetail.header.sourcePrCode ? ` · từ ${poDetail.header.sourcePrCode}` : ""}
                      {" · "}
                      {poDetail.header.totalAmount.toLocaleString("vi-VN")} {poDetail.header.currency}
                    </p>
                  </div>
                  <span className={statusPill(PO_TONE[poDetail.header.status] ?? "muted")}>
                    {poDetail.header.status}
                  </span>
                </div>

                <ul className="mb-3 space-y-1 text-sm">
                  {poDetail.lines.map((l) => (
                    <li key={l.id}>
                      {l.productCode} · {l.qty} × {l.unitPrice.toLocaleString("vi-VN")} ={" "}
                      {(l.qty * l.unitPrice).toLocaleString("vi-VN")}
                      <span className="text-xs text-[var(--muted)]"> · nhận {l.receivedQty}/{l.qty} · HĐ {l.invoicedQty}</span>
                    </li>
                  ))}
                </ul>

                {canPoManage && poDetail.header.status === "Draft" && (
                  <form
                    className="mb-3 grid gap-2 sm:grid-cols-2"
                    onSubmit={async (e) => {
                      e.preventDefault();
                      try {
                        await upsertPurPoLine(poId, {
                          productCode: lineCode,
                          productName: lineName || lineCode,
                          qty: Number(lineQty) || 1,
                          unitPrice: Number(poLinePrice) || 0,
                        });
                        await refreshPo();
                        flash("Đã thêm dòng PO.");
                      } catch (err) {
                        setError((err as Error).message);
                      }
                    }}
                  >
                    <input className={field} value={lineCode} onChange={(e) => setLineCode(e.target.value)} required />
                    <input className={field} placeholder="Tên SP" value={lineName} onChange={(e) => setLineName(e.target.value)} />
                    <input className={field} type="number" value={lineQty} onChange={(e) => setLineQty(e.target.value)} />
                    <input className={field} type="number" value={poLinePrice} onChange={(e) => setPoLinePrice(e.target.value)} />
                    <button type="submit" className={btn.ghost}>Thêm dòng</button>
                    <button
                      type="button"
                      className={btn.primary}
                      onClick={async () => {
                        try {
                          await submitPurPo(poId);
                          await refreshPo();
                          flash("Đã gửi / tự duyệt PO (≤ 10tr).");
                        } catch (err) {
                          setError((err as Error).message);
                        }
                      }}
                    >
                      Gửi duyệt
                    </button>
                  </form>
                )}

                <div className="flex flex-wrap gap-2">
                  {canPoApprove && poDetail.header.status === "PendingApproval" && (
                    <button type="button" className={btn.primary} onClick={async () => { try { await approvePurPo(poId); await refreshPo(); flash("Đã duyệt PO."); } catch (err) { setError((err as Error).message); } }}>
                      Duyệt PO
                    </button>
                  )}
                  {canPoManage && poDetail.header.status === "Approved" && (
                    <button type="button" className={btn.primary} onClick={async () => { try { await sendPurPo(poId); await refreshPo(); flash("Đã gửi PO cho NCC."); } catch (err) { setError((err as Error).message); } }}>
                      Gửi NCC
                    </button>
                  )}
                  {canPoManage && (poDetail.header.status === "Approved" || poDetail.header.status === "Sent") && (
                    <button type="button" className={btn.ghost} onClick={async () => { try { await revisePurPo(poId); await refreshPo(); flash("Đã tạo phiên bản PO mới (Draft)."); } catch (err) { setError((err as Error).message); } }}>
                      Sửa phiên bản
                    </button>
                  )}
                  {canPoManage && poDetail.header.status !== "Draft" && poDetail.header.status !== "Cancelled" && (
                    <button type="button" className={btn.ghost} onClick={async () => { try { await printPurPo(poId); await refreshPo(); flash("Đã in/xuất PO (stub)."); } catch (err) { setError((err as Error).message); } }}>
                      In PO
                    </button>
                  )}
                  {canPoManage && (poDetail.header.status === "Sent" || poDetail.header.status === "Approved") && (
                    <button type="button" className={btn.ghost} onClick={async () => { try { await closePurPo(poId); await refreshPo(); flash("Đã đóng PO."); } catch (err) { setError((err as Error).message); } }}>
                      Đóng PO
                    </button>
                  )}
                  {canPoManage && poDetail.header.status !== "Closed" && poDetail.header.status !== "Cancelled" && (
                    <button type="button" className={btn.ghost} onClick={async () => { try { await cancelPurPo(poId, "Hủy có kiểm soát"); await refreshPo(); flash("Đã hủy PO."); } catch (err) { setError((err as Error).message); } }}>
                      Hủy PO
                    </button>
                  )}
                </div>
              </section>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
