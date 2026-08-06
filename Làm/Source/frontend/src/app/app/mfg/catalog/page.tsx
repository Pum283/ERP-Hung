"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  activateMfgBom,
  fetchMfgBomDetail,
  fetchMfgBoms,
  fetchMfgItems,
  fetchMfgWorkshops,
  upsertMfgBom,
  upsertMfgBomLine,
  upsertMfgItem,
  upsertMfgWorkshop,
  type MfgBomDetailDto,
  type MfgBomDto,
  type MfgItemDto,
  type MfgWorkshopDto,
} from "@/shared/api/mfg-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function MfgCatalogPage() {
  const { can } = usePermissions();
  const canRead = can("mfg.master.read");
  const canManage = can("mfg.master.manage");

  const [items, setItems] = useState<MfgItemDto[]>([]);
  const [workshops, setWorkshops] = useState<MfgWorkshopDto[]>([]);
  const [boms, setBoms] = useState<MfgBomDto[]>([]);
  const [selectedBomId, setSelectedBomId] = useState("");
  const [bomDetail, setBomDetail] = useState<MfgBomDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [itemCode, setItemCode] = useState("TP-001");
  const [itemName, setItemName] = useState("");
  const [itemType, setItemType] = useState("FG");
  const [itemCost, setItemCost] = useState("10000");
  const [wsCode, setWsCode] = useState("XUONG-01");
  const [wsName, setWsName] = useState("");
  const [wsType, setWsType] = useState("Workshop");
  const [bomParentId, setBomParentId] = useState("");
  const [bomVersion, setBomVersion] = useState("1.0");
  const [compId, setCompId] = useState("");
  const [compQty, setCompQty] = useState("1");
  const [compLevel, setCompLevel] = useState("1");

  const load = useCallback(async () => {
    const [i, w, b] = await Promise.all([fetchMfgItems(), fetchMfgWorkshops(), fetchMfgBoms()]);
    setItems(i);
    setWorkshops(w);
    setBoms(b);
    const parents = i.filter((x) => x.itemType === "FG" || x.itemType === "SFG");
    if (!bomParentId && parents[0]) setBomParentId(parents[0].id);
    const comps = i.filter((x) => x.itemType === "RM" || x.itemType === "SFG");
    if (!compId && comps[0]) setCompId(comps[0].id);
    if (!selectedBomId && b[0]) setSelectedBomId(b[0].id);
  }, [bomParentId, compId, selectedBomId]);

  useEffect(() => {
    if (!canRead) { setLoading(false); return; }
    setLoading(true);
    load().catch((e: Error) => setError(e.message)).finally(() => setLoading(false));
  }, [canRead, load]);

  useEffect(() => {
    if (!selectedBomId || !canRead) return;
    fetchMfgBomDetail(selectedBomId).then(setBomDetail).catch((e: Error) => setError(e.message));
  }, [selectedBomId, canRead]);

  function flash(msg: string) {
    setOk(msg); setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function refreshBom() {
    if (!selectedBomId) return;
    setBomDetail(await fetchMfgBomDetail(selectedBomId));
    await load();
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem danh mục SX.</div>;
  }

  const parents = items.filter((x) => x.itemType === "FG" || x.itemType === "SFG");
  const comps = items.filter((x) => x.itemType === "RM" || x.itemType === "SFG");

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Danh mục / BOM</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          TP/BTP/NVL · xưởng · BOM nhiều cấp · phiên bản · định mức (UC_MFG_001–003, 006–008)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-3">
        {canManage && (
          <>
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Sản phẩm SX</h2>
              <form
                onSubmit={async (e: FormEvent) => {
                  e.preventDefault();
                  try {
                    await upsertMfgItem({
                      code: itemCode, name: itemName, itemType,
                      standardCost: Number(itemCost) || 0,
                    });
                    setItemName("");
                    await load();
                    flash("Đã lưu SP.");
                  } catch (err) { setError((err as Error).message); }
                }}
                className="space-y-2"
              >
                <input className={field} value={itemCode} onChange={(e) => setItemCode(e.target.value)} placeholder="Mã" required />
                <input className={field} value={itemName} onChange={(e) => setItemName(e.target.value)} placeholder="Tên" required />
                <select className={field} value={itemType} onChange={(e) => setItemType(e.target.value)}>
                  <option value="FG">FG — Thành phẩm</option>
                  <option value="SFG">SFG — Bán thành phẩm</option>
                  <option value="RM">RM — NVL</option>
                </select>
                <input className={field} value={itemCost} onChange={(e) => setItemCost(e.target.value)} placeholder="Giá chuẩn" />
                <button type="submit" className={btn.primary}>Lưu SP</button>
              </form>
              <ul className="mt-3 max-h-40 space-y-1 overflow-auto text-sm">
                {items.map((i) => (
                  <li key={i.id}>
                    {i.code} · {i.name}{" "}
                    <span className="text-[var(--muted)]">({i.itemType} · {i.standardCost?.toLocaleString("vi-VN") || 0})</span>
                  </li>
                ))}
              </ul>
            </section>

            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Xưởng / dây chuyền</h2>
              <form
                onSubmit={async (e: FormEvent) => {
                  e.preventDefault();
                  try {
                    await upsertMfgWorkshop({ code: wsCode, name: wsName, workshopType: wsType });
                    setWsName("");
                    await load();
                    flash("Đã lưu xưởng.");
                  } catch (err) { setError((err as Error).message); }
                }}
                className="space-y-2"
              >
                <input className={field} value={wsCode} onChange={(e) => setWsCode(e.target.value)} required />
                <input className={field} value={wsName} onChange={(e) => setWsName(e.target.value)} placeholder="Tên" required />
                <select className={field} value={wsType} onChange={(e) => setWsType(e.target.value)}>
                  <option value="Workshop">Workshop</option>
                  <option value="Line">Line</option>
                </select>
                <button type="submit" className={btn.primary}>Lưu xưởng</button>
              </form>
              <ul className="mt-3 space-y-1 text-sm">
                {workshops.map((w) => (
                  <li key={w.id}>{w.code} · {w.name} ({w.workshopType})</li>
                ))}
              </ul>
            </section>

            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Tạo BOM</h2>
              <form
                onSubmit={async (e: FormEvent) => {
                  e.preventDefault();
                  try {
                    const saved = await upsertMfgBom({ parentItemId: bomParentId, version: bomVersion });
                    await load();
                    setSelectedBomId(saved.id);
                    flash("Đã tạo BOM.");
                  } catch (err) { setError((err as Error).message); }
                }}
                className="space-y-2"
              >
                <select className={field} value={bomParentId} onChange={(e) => setBomParentId(e.target.value)}>
                  {parents.map((p) => <option key={p.id} value={p.id}>{p.code} · {p.name}</option>)}
                </select>
                <input className={field} value={bomVersion} onChange={(e) => setBomVersion(e.target.value)} placeholder="Version" required />
                <button type="submit" className={btn.primary}>Tạo BOM</button>
              </form>
            </section>
          </>
        )}
      </div>

      <div className="grid gap-4 xl:grid-cols-[1fr_1.3fr]">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách BOM</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>SP</th>
                  <th className={th}>Ver</th>
                  <th className={th}>TT</th>
                </tr>
              </thead>
              <tbody>
                {boms.map((b) => (
                  <tr
                    key={b.id}
                    className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedBomId === b.id ? "bg-[var(--surface-2)]" : ""}`}
                    onClick={() => setSelectedBomId(b.id)}
                  >
                    <td className={td}>{b.code}</td>
                    <td className={td}>{b.parentItemCode}</td>
                    <td className={td}>{b.version}</td>
                    <td className={td}>
                      <span className={statusPill(b.status === "Active" ? "success" : "muted")}>{b.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        {bomDetail && (
          <section className={panel}>
            <h2 className="mb-1 text-sm font-semibold">{bomDetail.bom.code}</h2>
            <p className="mb-3 text-xs text-[var(--muted)]">
              {bomDetail.bom.parentItemCode} · v{bomDetail.bom.version} · {bomDetail.bom.lineCount} dòng
            </p>
            <ul className="mb-3 space-y-1 text-sm">
              {bomDetail.lines.map((l) => (
                <li key={l.id}>L{l.level} · {l.componentCode} ({l.componentType}) × {l.qty} {l.unit}</li>
              ))}
              {bomDetail.lines.length === 0 && <li className="text-[var(--muted)]">Chưa có định mức</li>}
            </ul>
            {canManage && bomDetail.bom.status !== "Obsolete" && (
              <form
                onSubmit={async (e: FormEvent) => {
                  e.preventDefault();
                  try {
                    await upsertMfgBomLine(bomDetail.bom.id, {
                      componentItemId: compId,
                      qty: Number(compQty) || 1,
                      level: Number(compLevel) || 1,
                    });
                    await refreshBom();
                    flash("Đã thêm dòng BOM.");
                  } catch (err) { setError((err as Error).message); }
                }}
                className="mb-3 grid gap-2 sm:grid-cols-4"
              >
                <select className={field} value={compId} onChange={(e) => setCompId(e.target.value)}>
                  {comps.map((c) => <option key={c.id} value={c.id}>{c.code} ({c.itemType})</option>)}
                </select>
                <input className={field} value={compQty} onChange={(e) => setCompQty(e.target.value)} placeholder="SL" />
                <input className={field} value={compLevel} onChange={(e) => setCompLevel(e.target.value)} placeholder="Cấp" />
                <button type="submit" className={btn.ghost}>Thêm dòng</button>
              </form>
            )}
            {canManage && bomDetail.bom.status === "Draft" && (
              <button
                type="button"
                className={btn.primary}
                onClick={() =>
                  activateMfgBom(bomDetail.bom.id)
                    .then(() => refreshBom())
                    .then(() => flash("Đã kích hoạt BOM."))
                    .catch((e: Error) => setError(e.message))
                }
              >
                Kích hoạt phiên bản
              </button>
            )}
          </section>
        )}
      </div>
    </div>
  );
}
