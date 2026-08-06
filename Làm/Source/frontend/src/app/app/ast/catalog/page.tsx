"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchAstGroups,
  fetchAstLocations,
  fetchAstMethods,
  upsertAstGroup,
  upsertAstLocation,
  upsertAstMethod,
  type AstAssetGroupDto,
  type AstDepreciationMethodDto,
  type AstLocationDto,
} from "@/shared/api/ast-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function AstCatalogPage() {
  const { can } = usePermissions();
  const canRead = can("ast.master.read");
  const canManage = can("ast.master.manage");

  const [groups, setGroups] = useState<AstAssetGroupDto[]>([]);
  const [locs, setLocs] = useState<AstLocationDto[]>([]);
  const [methods, setMethods] = useState<AstDepreciationMethodDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [gCode, setGCode] = useState("MAYMOC");
  const [gName, setGName] = useState("");
  const [gLife, setGLife] = useState("60");
  const [lCode, setLCode] = useState("HQ");
  const [lName, setLName] = useState("");
  const [lBranch, setLBranch] = useState("Trụ sở");
  const [mCode, setMCode] = useState("SL");
  const [mName, setMName] = useState("Đường thẳng");
  const [mType, setMType] = useState("StraightLine");
  const [mLife, setMLife] = useState("36");
  const [mRate, setMRate] = useState("33.33");

  const load = useCallback(async () => {
    const [g, l, m] = await Promise.all([fetchAstGroups(), fetchAstLocations(), fetchAstMethods()]);
    setGroups(g); setLocs(l); setMethods(m);
  }, []);

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
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem danh mục TSCĐ.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Danh mục TSCĐ</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Nhóm TS · vị trí/CN · phương pháp & tỷ lệ KH (UC_AST_001, 004, 008–009)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 lg:grid-cols-3">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Nhóm TSCĐ</h2>
          {canManage && (
            <form className="mb-3 space-y-2" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(() => upsertAstGroup({
                code: gCode, name: gName, defaultUsefulLifeMonths: Number(gLife) || 36,
              }), "Đã lưu nhóm");
            }}>
              <input className={field} placeholder="Mã" value={gCode} onChange={(e) => setGCode(e.target.value)} />
              <input className={field} placeholder="Tên" value={gName} onChange={(e) => setGName(e.target.value)} />
              <input className={field} placeholder="Tháng KH mặc định" value={gLife} onChange={(e) => setGLife(e.target.value)} />
              <button className={btn.primary} type="submit">Thêm nhóm</button>
            </form>
          )}
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Mã</th><th className={th}>Tên</th><th className={th}>TS</th></tr></thead>
              <tbody>
                {groups.map((g) => (
                  <tr key={g.id}>
                    <td className={td}>{g.code}</td><td className={td}>{g.name}</td><td className={td}>{g.assetCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Vị trí / chi nhánh</h2>
          {canManage && (
            <form className="mb-3 space-y-2" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(() => upsertAstLocation({
                code: lCode, name: lName, branchName: lBranch,
              }), "Đã lưu vị trí");
            }}>
              <input className={field} placeholder="Mã" value={lCode} onChange={(e) => setLCode(e.target.value)} />
              <input className={field} placeholder="Tên" value={lName} onChange={(e) => setLName(e.target.value)} />
              <input className={field} placeholder="Chi nhánh" value={lBranch} onChange={(e) => setLBranch(e.target.value)} />
              <button className={btn.primary} type="submit">Thêm vị trí</button>
            </form>
          )}
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Mã</th><th className={th}>Tên</th><th className={th}>CN</th></tr></thead>
              <tbody>
                {locs.map((l) => (
                  <tr key={l.id}>
                    <td className={td}>{l.code}</td><td className={td}>{l.name}</td><td className={td}>{l.branchName ?? "—"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Phương pháp KH</h2>
          {canManage && (
            <form className="mb-3 space-y-2" onSubmit={(e: FormEvent) => {
              e.preventDefault();
              void run(() => upsertAstMethod({
                code: mCode, name: mName, methodType: mType,
                defaultUsefulLifeMonths: Number(mLife) || 36,
                defaultRatePercent: Number(mRate) || 0,
              }), "Đã lưu PP KH");
            }}>
              <input className={field} placeholder="Mã" value={mCode} onChange={(e) => setMCode(e.target.value)} />
              <input className={field} placeholder="Tên" value={mName} onChange={(e) => setMName(e.target.value)} />
              <select className={field} value={mType} onChange={(e) => setMType(e.target.value)}>
                <option value="StraightLine">StraightLine</option>
                <option value="DecliningBalance">DecliningBalance</option>
              </select>
              <input className={field} placeholder="Tháng" value={mLife} onChange={(e) => setMLife(e.target.value)} />
              <input className={field} placeholder="% năm" value={mRate} onChange={(e) => setMRate(e.target.value)} />
              <button className={btn.primary} type="submit">Thêm PP</button>
            </form>
          )}
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Mã</th><th className={th}>Loại</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {methods.map((m) => (
                  <tr key={m.id}>
                    <td className={td}>{m.code}</td>
                    <td className={td}>{m.methodType}</td>
                    <td className={td}><span className={statusPill(m.status === "Active" ? "success" : "muted")}>{m.status}</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      </div>
    </div>
  );
}
