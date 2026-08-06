"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  createFsmPartIssue,
  createFsmPartReconcile,
  fetchFsmPartIssues,
  fetchFsmPartReconciles,
  fetchFsmPartStock,
  fetchFsmParts,
  receiptFsmPartStock,
  type FsmPartDto,
  type FsmPartIssueDocDto,
  type FsmPartReconcileDocDto,
  type FsmPartStockDto,
} from "@/shared/api/fsm-api";
import { fetchMsgDirectory, type MsgDirectoryUserDto } from "@/shared/api/msg-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

type Tab = "warehouse" | "tech" | "issue" | "reconcile";

export default function FsmPartsStockPage() {
  const { can } = usePermissions();
  const canRead = can("fsm.master.read");
  const canManage = can("fsm.master.manage");

  const [tab, setTab] = useState<Tab>("warehouse");
  const [parts, setParts] = useState<FsmPartDto[]>([]);
  const [users, setUsers] = useState<MsgDirectoryUserDto[]>([]);
  const [stock, setStock] = useState<FsmPartStockDto[]>([]);
  const [issues, setIssues] = useState<FsmPartIssueDocDto[]>([]);
  const [reconciles, setReconciles] = useState<FsmPartReconcileDocDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [partId, setPartId] = useState("");
  const [qty, setQty] = useState("1");
  const [unitCost, setUnitCost] = useState("0");
  const [techId, setTechId] = useState("");
  const [countedQty, setCountedQty] = useState("0");
  const [scope, setScope] = useState("Warehouse");

  const load = useCallback(async () => {
    const [p, u] = await Promise.all([
      fetchFsmParts(),
      fetchMsgDirectory().catch(() => [] as MsgDirectoryUserDto[]),
    ]);
    setParts(p); setUsers(u);
    if (!partId && p[0]) setPartId(p[0].id);
    if (!techId && u[0]) setTechId(u[0].id);

    if (tab === "warehouse") setStock(await fetchFsmPartStock("Warehouse"));
    else if (tab === "tech") setStock(await fetchFsmPartStock("Tech"));
    else if (tab === "issue") setIssues(await fetchFsmPartIssues());
    else setReconciles(await fetchFsmPartReconciles());
  }, [tab, partId, techId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem kho linh kiện FSM.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Kho linh kiện KT</h1>
          <p className="text-sm text-[var(--muted)]">UC_FSM_037 · 038 · 039 · tồn kho · cấp KTV · đối soát.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {([
            ["warehouse", "Tồn kho KT"],
            ["tech", "Tồn túi KTV"],
            ["issue", "Cấp linh kiện"],
            ["reconcile", "Đối soát"],
          ] as [Tab, string][]).map(([k, label]) => (
            <button key={k} type="button" className={tab === k ? btn.primary : btn.ghost} onClick={() => setTab(k)}>
              {label}
            </button>
          ))}
        </div>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-800">{ok}</div>}
      {loading && <p className="text-sm text-[var(--muted)]">Đang tải…</p>}

      {tab === "warehouse" && (
        <section className={panel}>
          {canManage && (
            <form
              className="mb-3 grid gap-2 sm:grid-cols-4"
              onSubmit={(e: FormEvent) => {
                e.preventDefault();
                run(
                  () => receiptFsmPartStock({
                    partId,
                    qty: Number(qty),
                    unitCost: Number(unitCost) || 0,
                  }),
                  "Đã nhập kho KT.",
                );
              }}
            >
              <select className={field} value={partId} onChange={(e) => setPartId(e.target.value)}>
                {parts.map((p) => <option key={p.id} value={p.id}>{p.code} · {p.name}</option>)}
              </select>
              <input className={field} type="number" min={0.001} step="any" value={qty} onChange={(e) => setQty(e.target.value)} placeholder="SL" />
              <input className={field} type="number" min={0} step="any" value={unitCost} onChange={(e) => setUnitCost(e.target.value)} placeholder="Đơn giá" />
              <button type="submit" className={btn.primary}>Nhập kho KT</button>
            </form>
          )}
          <StockTable rows={stock} showTech={false} />
        </section>
      )}

      {tab === "tech" && (
        <section className={panel}>
          <StockTable rows={stock} showTech />
        </section>
      )}

      {tab === "issue" && (
        <section className={panel}>
          {canManage && (
            <form
              className="mb-3 grid gap-2 sm:grid-cols-4"
              onSubmit={(e: FormEvent) => {
                e.preventDefault();
                run(
                  () => createFsmPartIssue({
                    techUserId: techId,
                    lines: [{ partId, qty: Number(qty), unitCost: Number(unitCost) || undefined }],
                  }),
                  "Đã cấp linh kiện cho KTV.",
                );
              }}
            >
              <select className={field} value={techId} onChange={(e) => setTechId(e.target.value)}>
                {users.map((u) => <option key={u.id} value={u.id}>{u.displayName || u.username}</option>)}
              </select>
              <select className={field} value={partId} onChange={(e) => setPartId(e.target.value)}>
                {parts.map((p) => <option key={p.id} value={p.id}>{p.code}</option>)}
              </select>
              <input className={field} type="number" min={0.001} step="any" value={qty} onChange={(e) => setQty(e.target.value)} />
              <button type="submit" className={btn.primary}>Cấp → KTV</button>
            </form>
          )}
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>KTV</th>
                  <th className={th}>TT</th>
                  <th className={th}>Dòng</th>
                </tr>
              </thead>
              <tbody>
                {issues.map((d) => (
                  <tr key={d.id}>
                    <td className={td}>{d.code}</td>
                    <td className={td}>{d.techName}</td>
                    <td className={td}><span className={statusPill("success")}>{d.status}</span></td>
                    <td className={td}>
                      {d.lines.map((l) => `${l.partCode}×${l.qty}`).join(", ")}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}

      {tab === "reconcile" && (
        <section className={panel}>
          {canManage && (
            <form
              className="mb-3 grid gap-2 sm:grid-cols-5"
              onSubmit={(e: FormEvent) => {
                e.preventDefault();
                run(
                  () => createFsmPartReconcile({
                    scope,
                    techUserId: scope === "Tech" ? techId : null,
                    lines: [{ partId, countedQty: Number(countedQty) }],
                  }),
                  "Đã đối soát & điều chỉnh tồn.",
                );
              }}
            >
              <select className={field} value={scope} onChange={(e) => setScope(e.target.value)}>
                <option value="Warehouse">Kho KT</option>
                <option value="Tech">Túi KTV</option>
              </select>
              <select className={field} value={techId} onChange={(e) => setTechId(e.target.value)} disabled={scope !== "Tech"}>
                {users.map((u) => <option key={u.id} value={u.id}>{u.displayName || u.username}</option>)}
              </select>
              <select className={field} value={partId} onChange={(e) => setPartId(e.target.value)}>
                {parts.map((p) => <option key={p.id} value={p.id}>{p.code}</option>)}
              </select>
              <input className={field} type="number" min={0} step="any" value={countedQty} onChange={(e) => setCountedQty(e.target.value)} placeholder="SL đếm" />
              <button type="submit" className={btn.primary}>Đối soát</button>
            </form>
          )}
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Phạm vi</th>
                  <th className={th}>Chênh</th>
                  <th className={th}>Chi tiết</th>
                </tr>
              </thead>
              <tbody>
                {reconciles.map((d) => (
                  <tr key={d.id}>
                    <td className={td}>{d.code}</td>
                    <td className={td}>{d.scope}{d.techName ? ` · ${d.techName}` : ""}</td>
                    <td className={td}>{d.lines.reduce((s, l) => s + l.diffQty, 0)}</td>
                    <td className={td}>
                      {d.lines.map((l) => `${l.partCode}: ${l.systemQty}→${l.countedQty}`).join("; ")}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}
    </div>
  );
}

function StockTable({ rows, showTech }: { rows: FsmPartStockDto[]; showTech: boolean }) {
  return (
    <div className={tableWrap}>
      <table className="w-full text-sm">
        <thead>
          <tr>
            <th className={th}>Mã</th>
            <th className={th}>Tên</th>
            {showTech && <th className={th}>KTV</th>}
            <th className={th}>Tồn</th>
            <th className={th}>Đơn giá</th>
            <th className={th}>Giá trị</th>
          </tr>
        </thead>
        <tbody>
          {rows.length === 0 && (
            <tr><td className={td} colSpan={showTech ? 6 : 5}>Chưa có tồn.</td></tr>
          )}
          {rows.map((r) => (
            <tr key={r.id}>
              <td className={td}>{r.partCode}</td>
              <td className={td}>{r.partName}</td>
              {showTech && <td className={td}>{r.techName || "—"}</td>}
              <td className={td}>{r.qtyOnHand} {r.unit}</td>
              <td className={td}>{r.unitCost}</td>
              <td className={td}>{r.amount}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
