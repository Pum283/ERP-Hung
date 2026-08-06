"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  calculateAstDepreciation,
  fetchAstAssets,
  fetchAstGroups,
  fetchAstLocations,
  fetchAstMethods,
  fetchAstRunDetail,
  fetchAstRuns,
  pushAstDepreciationToFin,
  upsertAstAsset,
  type AstAssetDto,
  type AstAssetGroupDto,
  type AstDepreciationMethodDto,
  type AstDepreciationRunDetailDto,
  type AstDepreciationRunDto,
  type AstLocationDto,
} from "@/shared/api/ast-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function AstAssetsPage() {
  const { can } = usePermissions();
  const canRead = can("ast.asset.read");
  const canManage = can("ast.asset.manage");

  const [assets, setAssets] = useState<AstAssetDto[]>([]);
  const [groups, setGroups] = useState<AstAssetGroupDto[]>([]);
  const [locs, setLocs] = useState<AstLocationDto[]>([]);
  const [methods, setMethods] = useState<AstDepreciationMethodDto[]>([]);
  const [runs, setRuns] = useState<AstDepreciationRunDto[]>([]);
  const [selectedRunId, setSelectedRunId] = useState("");
  const [runDetail, setRunDetail] = useState<AstDepreciationRunDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [name, setName] = useState("");
  const [cost, setCost] = useState("50000000");
  const [life, setLife] = useState("36");
  const [groupId, setGroupId] = useState("");
  const [locId, setLocId] = useState("");
  const [methodId, setMethodId] = useState("");
  const [purchaseRef, setPurchaseRef] = useState("");
  const [year, setYear] = useState(String(new Date().getFullYear()));
  const [month, setMonth] = useState(String(new Date().getMonth() + 1));

  const load = useCallback(async () => {
    const [a, g, l, m, r] = await Promise.all([
      fetchAstAssets(),
      fetchAstGroups().catch(() => [] as AstAssetGroupDto[]),
      fetchAstLocations().catch(() => [] as AstLocationDto[]),
      fetchAstMethods().catch(() => [] as AstDepreciationMethodDto[]),
      fetchAstRuns(),
    ]);
    setAssets(a); setGroups(g); setLocs(l); setMethods(m); setRuns(r);
    if (!groupId && g[0]) setGroupId(g[0].id);
    if (!locId && l[0]) setLocId(l[0].id);
    if (!methodId && m[0]) setMethodId(m[0].id);
    if (!selectedRunId && r[0]) setSelectedRunId(r[0].id);
  }, [groupId, locId, methodId, selectedRunId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedRunId || !canRead) return;
    fetchAstRunDetail(selectedRunId).then(setRunDetail).catch((e: Error) => setError(e.message));
  }, [selectedRunId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function run(action: () => Promise<unknown>, msg: string) {
    try {
      await action();
      await load();
      if (selectedRunId) setRunDetail(await fetchAstRunDetail(selectedRunId));
      flash(msg);
    } catch (err) { setError((err as Error).message); }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem tài sản.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Thẻ TS / khấu hao</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Thẻ · nguyên giá · ghi tăng mua sắm · tính KH · sổ · đẩy FIN stub (UC_AST_002–003, 010–012, 014)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách thẻ TS</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th><th className={th}>Tên</th>
                  <th className={th}>NG</th><th className={th}>GTCL</th><th className={th}>TT</th>
                </tr>
              </thead>
              <tbody>
                {assets.map((a) => (
                  <tr key={a.id}>
                    <td className={td}>{a.code}</td>
                    <td className={td}>
                      <div>{a.name}</div>
                      <div className="text-xs text-[var(--muted)]">
                        {a.locationName ?? "—"} · {a.groupName ?? "—"}
                      </div>
                    </td>
                    <td className={td}>{a.originalCost.toLocaleString()}</td>
                    <td className={td}>{a.bookValue.toLocaleString()}</td>
                    <td className={td}>
                      <span className={statusPill(a.status === "Active" ? "success" : "muted")}>{a.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {canManage && (
            <form className="mt-3 space-y-2 border-t border-black/10 pt-3" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(() => upsertAstAsset({
                name: name || "Tài sản mới",
                originalCost: Number(cost) || 0,
                usefulLifeMonths: Number(life) || 36,
                groupId: groupId || null,
                locationId: locId || null,
                depreciationMethodId: methodId || null,
                purchaseRef: purchaseRef || null,
                capitalizeDate: new Date().toISOString(),
                status: "Active",
              }), "Đã tạo thẻ TS");
            }}>
              <div className="text-sm font-medium">Tạo thẻ / ghi tăng</div>
              <input className={field} placeholder="Tên TS" value={name} onChange={(e) => setName(e.target.value)} />
              <input className={field} placeholder="Nguyên giá" value={cost} onChange={(e) => setCost(e.target.value)} />
              <input className={field} placeholder="Tháng KH" value={life} onChange={(e) => setLife(e.target.value)} />
              <select className={field} value={groupId} onChange={(e) => setGroupId(e.target.value)}>
                <option value="">— Nhóm —</option>
                {groups.map((g) => <option key={g.id} value={g.id}>{g.code}</option>)}
              </select>
              <select className={field} value={locId} onChange={(e) => setLocId(e.target.value)}>
                <option value="">— Vị trí —</option>
                {locs.map((l) => <option key={l.id} value={l.id}>{l.code}</option>)}
              </select>
              <select className={field} value={methodId} onChange={(e) => setMethodId(e.target.value)}>
                <option value="">— PP KH —</option>
                {methods.map((m) => <option key={m.id} value={m.id}>{m.code}</option>)}
              </select>
              <input className={field} placeholder="Mã PO/mua sắm" value={purchaseRef}
                onChange={(e) => setPurchaseRef(e.target.value)} />
              <div className="flex flex-wrap gap-2">
                <button className={btn.primary} type="submit">Tạo Active</button>
                <button type="button" className={btn.ghost} onClick={() => {
                  if (!purchaseRef.trim()) { setError("Cần mã mua sắm để ghi tăng."); return; }
                  void run(() => upsertAstAsset({
                    name: name || `TS từ ${purchaseRef}`,
                    originalCost: Number(cost) || 0,
                    usefulLifeMonths: Number(life) || 36,
                    groupId: groupId || null,
                    locationId: locId || null,
                    depreciationMethodId: methodId || null,
                    purchaseRef,
                    capitalizeFromPurchase: true,
                  }), "Đã ghi tăng từ mua sắm");
                }}>
                  Ghi tăng từ mua sắm
                </button>
              </div>
            </form>
          )}
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Khấu hao định kỳ / sổ</h2>
          {canManage && (
            <div className="mb-3 flex flex-wrap gap-2">
              <input className={field} style={{ width: 90 }} value={year} onChange={(e) => setYear(e.target.value)} />
              <input className={field} style={{ width: 70 }} value={month} onChange={(e) => setMonth(e.target.value)} />
              <button type="button" className={btn.primary} onClick={() => void run(
                () => calculateAstDepreciation(Number(year), Number(month)),
                "Đã tính KH kỳ",
              )}>
                Tính KH
              </button>
            </div>
          )}
          <div className="mb-2 flex flex-wrap gap-2">
            {runs.map((r) => (
              <button key={r.id} type="button"
                className={selectedRunId === r.id ? btn.primary : btn.ghost}
                onClick={() => setSelectedRunId(r.id)}>
                {r.code} · {r.totalAmount.toLocaleString()}
              </button>
            ))}
          </div>
          {runDetail ? (
            <div className="space-y-2 text-sm">
              <div>
                <b>{runDetail.run.code}</b>{" "}
                <span className={statusPill(runDetail.run.status === "Pushed" ? "success" : "warning")}>
                  {runDetail.run.status}
                </span>
                {runDetail.run.finJournalId && (
                  <span className="ml-2 text-xs text-[var(--muted)]">FIN JE đã tạo</span>
                )}
              </div>
              <div className={tableWrap}>
                <table className="w-full text-sm">
                  <thead><tr><th className={th}>TS</th><th className={th}>KH</th><th className={th}>GTCL sau</th></tr></thead>
                  <tbody>
                    {runDetail.lines.map((l) => (
                      <tr key={l.id}>
                        <td className={td}>{l.assetCode}</td>
                        <td className={td}>{l.amount.toLocaleString()}</td>
                        <td className={td}>{l.bookValueAfter.toLocaleString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {canManage && runDetail.run.status !== "Pushed" && (
                <button type="button" className={btn.ghost} onClick={() => void run(
                  () => pushAstDepreciationToFin(runDetail.run.id),
                  "Đã đẩy FIN stub",
                )}>
                  Đẩy BT KH sang FIN (stub)
                </button>
              )}
            </div>
          ) : (
            <p className="text-sm text-[var(--muted)]">Chưa có kỳ KH — tính kỳ đầu tiên.</p>
          )}
        </section>
      </div>
    </div>
  );
}
