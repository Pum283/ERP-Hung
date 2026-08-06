"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  downloadInvSkusCsv,
  fetchInvConversions,
  fetchInvGroups,
  fetchInvSkus,
  fetchInvUoms,
  importInvSkusCsv,
  setInvSkuStatus,
  upsertInvConversion,
  upsertInvGroup,
  upsertInvSku,
  upsertInvUom,
  type InvItemGroupDto,
  type InvSkuDto,
  type InvUnitConversionDto,
  type InvUomDto,
} from "@/shared/api/inv-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function InvItemsPage() {
  const { can } = usePermissions();
  const canRead = can("inv.item.read");
  const canManage = can("inv.item.manage");

  const [groups, setGroups] = useState<InvItemGroupDto[]>([]);
  const [uoms, setUoms] = useState<InvUomDto[]>([]);
  const [conversions, setConversions] = useState<InvUnitConversionDto[]>([]);
  const [skus, setSkus] = useState<InvSkuDto[]>([]);
  const [q, setQ] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [csvText, setCsvText] = useState("");

  const [groupCode, setGroupCode] = useState("NHOM-01");
  const [groupName, setGroupName] = useState("");
  const [uomCode, setUomCode] = useState("CAI");
  const [uomName, setUomName] = useState("Cái");
  const [fromUnitId, setFromUnitId] = useState("");
  const [toUnitId, setToUnitId] = useState("");
  const [factor, setFactor] = useState("1");

  const [skuCode, setSkuCode] = useState("SKU-001");
  const [skuName, setSkuName] = useState("");
  const [skuGroupId, setSkuGroupId] = useState("");
  const [skuUnitId, setSkuUnitId] = useState("");
  const [trackLot, setTrackLot] = useState(false);
  const [trackSerial, setTrackSerial] = useState(false);
  const [trackExpiry, setTrackExpiry] = useState(false);
  const [costing, setCosting] = useState("Average");
  const [stdCost, setStdCost] = useState("0");
  const [minQty, setMinQty] = useState("");
  const [maxQty, setMaxQty] = useState("");
  const [reorderQty, setReorderQty] = useState("");

  const load = useCallback(async () => {
    const [g, u, c, s] = await Promise.all([
      fetchInvGroups(),
      fetchInvUoms(),
      fetchInvConversions(),
      fetchInvSkus(q || undefined),
    ]);
    setGroups(g);
    setUoms(u);
    setConversions(c);
    setSkus(s);
    if (!skuGroupId && g[0]) setSkuGroupId(g[0].id);
    if (!skuUnitId && u[0]) setSkuUnitId(u[0].id);
    if (!fromUnitId && u[0]) setFromUnitId(u[0].id);
    if (!toUnitId && u[1]) setToUnitId(u[1].id);
    else if (!toUnitId && u[0]) setToUnitId(u[0].id);
  }, [q, skuGroupId, skuUnitId, fromUnitId, toUnitId]);

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

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function onGroup(e: FormEvent) {
    e.preventDefault();
    try {
      await upsertInvGroup({ code: groupCode, name: groupName });
      setGroupName("");
      await load();
      flash("Đã lưu nhóm hàng.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onUom(e: FormEvent) {
    e.preventDefault();
    try {
      await upsertInvUom({ code: uomCode, name: uomName });
      await load();
      flash("Đã lưu ĐVT.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onConv(e: FormEvent) {
    e.preventDefault();
    try {
      await upsertInvConversion({
        fromUnitId,
        toUnitId,
        factor: Number(factor) || 1,
      });
      await load();
      flash("Đã lưu quy đổi ĐVT.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onSku(e: FormEvent) {
    e.preventDefault();
    try {
      await upsertInvSku({
        code: skuCode,
        name: skuName,
        groupId: skuGroupId || null,
        baseUnitId: skuUnitId,
        trackLot,
        trackSerial,
        trackExpiry,
        costingMethod: costing,
        standardCost: Number(stdCost) || 0,
        status: "Active",
        minQty: minQty === "" ? null : Number(minQty),
        maxQty: maxQty === "" ? null : Number(maxQty),
        reorderQty: reorderQty === "" ? null : Number(reorderQty),
      });
      setSkuName("");
      await load();
      flash("Đã lưu SKU.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onImport(e: FormEvent) {
    e.preventDefault();
    try {
      const r = await importInvSkusCsv(csvText);
      await load();
      flash(`Import: ${r.success}/${r.total} OK · ${r.failed} lỗi`);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem danh mục kho.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">SKU / danh mục kho</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Nhóm · ĐVT/quy đổi · lô/serial/HSD · giá vốn · min/max · import/export (UC_INV_001–005, 007–008, 010)
        </p>
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-3">
        {canManage && (
          <>
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Nhóm hàng</h2>
              <form onSubmit={onGroup} className="space-y-2">
                <input className={field} value={groupCode} onChange={(e) => setGroupCode(e.target.value)} placeholder="Mã" required />
                <input className={field} value={groupName} onChange={(e) => setGroupName(e.target.value)} placeholder="Tên" required />
                <button type="submit" className={btn.primary}>Lưu nhóm</button>
              </form>
              <ul className="mt-3 space-y-1 text-sm">
                {groups.map((g) => (
                  <li key={g.id}>{g.code} · {g.name} ({g.skuCount})</li>
                ))}
              </ul>
            </section>

            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">ĐVT & quy đổi</h2>
              <form onSubmit={onUom} className="mb-3 grid gap-2 sm:grid-cols-2">
                <input className={field} value={uomCode} onChange={(e) => setUomCode(e.target.value)} placeholder="Mã ĐVT" required />
                <input className={field} value={uomName} onChange={(e) => setUomName(e.target.value)} placeholder="Tên ĐVT" required />
                <button type="submit" className={`${btn.ghost} sm:col-span-2`}>Lưu ĐVT</button>
              </form>
              <form onSubmit={onConv} className="space-y-2">
                <select className={field} value={fromUnitId} onChange={(e) => setFromUnitId(e.target.value)}>
                  {uoms.map((u) => <option key={u.id} value={u.id}>{u.code}</option>)}
                </select>
                <select className={field} value={toUnitId} onChange={(e) => setToUnitId(e.target.value)}>
                  {uoms.map((u) => <option key={u.id} value={u.id}>{u.code}</option>)}
                </select>
                <input className={field} value={factor} onChange={(e) => setFactor(e.target.value)} placeholder="Hệ số" />
                <button type="submit" className={btn.ghost}>Lưu quy đổi</button>
              </form>
              <ul className="mt-3 space-y-1 text-xs text-[var(--muted)]">
                {conversions.map((c) => (
                  <li key={c.id}>1 {c.fromUnitCode} = {c.factor} {c.toUnitCode}</li>
                ))}
              </ul>
            </section>

            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Tạo SKU</h2>
              <form onSubmit={onSku} className="grid gap-2">
                <input className={field} value={skuCode} onChange={(e) => setSkuCode(e.target.value)} placeholder="Mã SKU" required />
                <input className={field} value={skuName} onChange={(e) => setSkuName(e.target.value)} placeholder="Tên" required />
                <select className={field} value={skuGroupId} onChange={(e) => setSkuGroupId(e.target.value)}>
                  <option value="">— Không nhóm —</option>
                  {groups.map((g) => <option key={g.id} value={g.id}>{g.code}</option>)}
                </select>
                <select className={field} value={skuUnitId} onChange={(e) => setSkuUnitId(e.target.value)} required>
                  {uoms.map((u) => <option key={u.id} value={u.id}>{u.code}</option>)}
                </select>
                <select className={field} value={costing} onChange={(e) => setCosting(e.target.value)}>
                  <option value="Average">Average</option>
                  <option value="Fifo">Fifo</option>
                </select>
                <input className={field} value={stdCost} onChange={(e) => setStdCost(e.target.value)} placeholder="Giá vốn chuẩn" />
                <div className="flex flex-wrap gap-3 text-sm">
                  <label className="flex items-center gap-1"><input type="checkbox" checked={trackLot} onChange={(e) => setTrackLot(e.target.checked)} /> Lô</label>
                  <label className="flex items-center gap-1"><input type="checkbox" checked={trackSerial} onChange={(e) => setTrackSerial(e.target.checked)} /> Serial</label>
                  <label className="flex items-center gap-1"><input type="checkbox" checked={trackExpiry} onChange={(e) => setTrackExpiry(e.target.checked)} /> HSD</label>
                </div>
                <div className="grid grid-cols-3 gap-2">
                  <input className={field} value={minQty} onChange={(e) => setMinQty(e.target.value)} placeholder="Min" />
                  <input className={field} value={maxQty} onChange={(e) => setMaxQty(e.target.value)} placeholder="Max" />
                  <input className={field} value={reorderQty} onChange={(e) => setReorderQty(e.target.value)} placeholder="Reorder" />
                </div>
                <button type="submit" className={btn.primary}>Lưu SKU</button>
              </form>
            </section>
          </>
        )}
      </div>

      <section className={panel}>
        <div className="mb-3 flex flex-wrap items-center gap-2">
          <h2 className="mr-auto text-sm font-semibold">Danh sách SKU</h2>
          <input className={`${field} w-48`} value={q} onChange={(e) => setQ(e.target.value)} placeholder="Tìm mã/tên" />
          <button type="button" className={btn.ghost} onClick={() => load().catch((e: Error) => setError(e.message))}>Tìm</button>
          <button type="button" className={btn.ghost} onClick={() => downloadInvSkusCsv().catch((e: Error) => setError(e.message))}>Export CSV</button>
        </div>
        <div className={tableWrap}>
          <table className="w-full text-sm">
            <thead>
              <tr>
                <th className={th}>Mã</th>
                <th className={th}>Tên</th>
                <th className={th}>Nhóm</th>
                <th className={th}>ĐVT</th>
                <th className={th}>Theo dõi</th>
                <th className={th}>Giá vốn</th>
                <th className={th}>Min/Max</th>
                <th className={th}>TT</th>
                <th className={th} />
              </tr>
            </thead>
            <tbody>
              {skus.map((s) => (
                <tr key={s.id}>
                  <td className={td}>{s.code}</td>
                  <td className={td}>{s.name}</td>
                  <td className={td}>{s.groupName || "—"}</td>
                  <td className={td}>{s.baseUnitCode}</td>
                  <td className={td}>
                    {[s.trackLot && "Lô", s.trackSerial && "SN", s.trackExpiry && "HSD"].filter(Boolean).join(" · ") || "—"}
                  </td>
                  <td className={td}>{s.costingMethod} · {s.standardCost}</td>
                  <td className={td}>{s.minQty ?? "—"} / {s.maxQty ?? "—"}</td>
                  <td className={td}>
                    <span className={statusPill(s.status === "Active" ? "success" : "muted")}>{s.status}</span>
                  </td>
                  <td className={td}>
                    {canManage && (
                      <button
                        type="button"
                        className={btn.ghost}
                        onClick={() =>
                          setInvSkuStatus(s.id, s.status === "Active" ? "Inactive" : "Active")
                            .then(() => load())
                            .then(() => flash("Đã đổi trạng thái."))
                            .catch((e: Error) => setError(e.message))
                        }
                      >
                        {s.status === "Active" ? "Ngưng" : "Kích hoạt"}
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      {canManage && (
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Import CSV SKU</h2>
          <form onSubmit={onImport} className="space-y-2">
            <textarea
              className={`${field} min-h-28 font-mono text-xs`}
              value={csvText}
              onChange={(e) => setCsvText(e.target.value)}
              placeholder="Code,Name,GroupCode,BaseUnitCode,..."
            />
            <button type="submit" className={btn.primary}>Import</button>
          </form>
        </section>
      )}
    </div>
  );
}
