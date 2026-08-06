"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchInvWarehouseDetail,
  fetchInvWarehouseTypes,
  fetchInvWarehouses,
  upsertInvKeeper,
  upsertInvWarehouse,
  upsertInvWarehouseType,
  type InvWarehouseDetailDto,
  type InvWarehouseDto,
  type InvWarehouseTypeDto,
} from "@/shared/api/inv-api";
import { fetchMsgDirectory, type MsgDirectoryUserDto } from "@/shared/api/msg-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function InvWarehousesPage() {
  const { can } = usePermissions();
  const canRead = can("inv.warehouse.read");
  const canManage = can("inv.warehouse.manage");

  const [types, setTypes] = useState<InvWarehouseTypeDto[]>([]);
  const [warehouses, setWarehouses] = useState<InvWarehouseDto[]>([]);
  const [users, setUsers] = useState<MsgDirectoryUserDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<InvWarehouseDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [typeCode, setTypeCode] = useState("MAIN");
  const [typeName, setTypeName] = useState("Kho chính");
  const [code, setCode] = useState("KHO-01");
  const [name, setName] = useState("");
  const [typeId, setTypeId] = useState("");
  const [address, setAddress] = useState("");
  const [keeperUserId, setKeeperUserId] = useState("");
  const [keeperRole, setKeeperRole] = useState("Keeper");

  const load = useCallback(async () => {
    const [t, w, u] = await Promise.all([
      fetchInvWarehouseTypes(),
      fetchInvWarehouses(),
      fetchMsgDirectory().catch(() => [] as MsgDirectoryUserDto[]),
    ]);
    setTypes(t);
    setWarehouses(w);
    setUsers(u);
    if (!selectedId && w[0]) setSelectedId(w[0].id);
    if (!typeId && t[0]) setTypeId(t[0].id);
    if (!keeperUserId && u[0]) setKeeperUserId(u[0].id);
  }, [selectedId, typeId, keeperUserId]);

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
    fetchInvWarehouseDetail(selectedId)
      .then(setDetail)
      .catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function onType(e: FormEvent) {
    e.preventDefault();
    try {
      const saved = await upsertInvWarehouseType({ code: typeCode, name: typeName });
      setTypeId(saved.id);
      await load();
      flash("Đã lưu loại kho.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onWarehouse(e: FormEvent) {
    e.preventDefault();
    try {
      const saved = await upsertInvWarehouse({
        code,
        name,
        warehouseTypeId: typeId || null,
        address,
        status: "Active",
      });
      setName("");
      setAddress("");
      await load();
      setSelectedId(saved.id);
      flash("Đã lưu kho.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onKeeper(e: FormEvent) {
    e.preventDefault();
    if (!selectedId) return;
    try {
      await upsertInvKeeper(selectedId, { userId: keeperUserId, role: keeperRole });
      setDetail(await fetchInvWarehouseDetail(selectedId));
      await load();
      flash("Đã gán thủ kho.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem kho.</div>;
  }

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Kho / thủ kho</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Loại kho · danh mục kho · gán thủ kho / giám sát (UC_INV_011, 012, 014)
        </p>
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-[1fr_1.2fr]">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách kho</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Tên</th>
                  <th className={th}>Loại</th>
                  <th className={th}>Thủ kho</th>
                </tr>
              </thead>
              <tbody>
                {warehouses.map((w) => (
                  <tr
                    key={w.id}
                    className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedId === w.id ? "bg-[var(--surface-2)]" : ""}`}
                    onClick={() => setSelectedId(w.id)}
                  >
                    <td className={td}>{w.code}</td>
                    <td className={td}>
                      <div>{w.name}</div>
                      <span className={statusPill(w.status === "Active" ? "success" : "muted")}>{w.status}</span>
                    </td>
                    <td className={td}>{w.warehouseTypeName || "—"}</td>
                    <td className={td}>{w.keeperCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <div className="space-y-4">
          {canManage && (
            <>
              <section className={panel}>
                <h2 className="mb-3 text-sm font-semibold">Loại kho</h2>
                <form onSubmit={onType} className="grid gap-2 sm:grid-cols-2">
                  <input className={field} value={typeCode} onChange={(e) => setTypeCode(e.target.value)} placeholder="Mã" required />
                  <input className={field} value={typeName} onChange={(e) => setTypeName(e.target.value)} placeholder="Tên" required />
                  <button type="submit" className={`${btn.ghost} sm:col-span-2`}>Lưu loại kho</button>
                </form>
                <ul className="mt-2 space-y-1 text-sm text-[var(--muted)]">
                  {types.map((t) => <li key={t.id}>{t.code} · {t.name}</li>)}
                </ul>
              </section>

              <section className={panel}>
                <h2 className="mb-3 text-sm font-semibold">Tạo kho</h2>
                <form onSubmit={onWarehouse} className="grid gap-2 sm:grid-cols-2">
                  <input className={field} value={code} onChange={(e) => setCode(e.target.value)} placeholder="Mã" required />
                  <input className={field} value={name} onChange={(e) => setName(e.target.value)} placeholder="Tên" required />
                  <select className={field} value={typeId} onChange={(e) => setTypeId(e.target.value)}>
                    <option value="">— Loại kho —</option>
                    {types.map((t) => <option key={t.id} value={t.id}>{t.code}</option>)}
                  </select>
                  <input className={field} value={address} onChange={(e) => setAddress(e.target.value)} placeholder="Địa chỉ" />
                  <button type="submit" className={`${btn.primary} sm:col-span-2`}>Lưu kho</button>
                </form>
              </section>
            </>
          )}

          {detail && (
            <section className={panel}>
              <h2 className="mb-1 text-sm font-semibold">{detail.warehouse.name}</h2>
              <p className="mb-4 text-xs text-[var(--muted)]">
                {detail.warehouse.code} · {detail.warehouse.warehouseTypeName || "Chưa loại"} ·{" "}
                {detail.warehouse.address || "Chưa địa chỉ"}
              </p>

              <h3 className="mb-2 text-xs font-semibold uppercase text-[var(--muted)]">Thủ kho</h3>
              <ul className="mb-3 space-y-1 text-sm">
                {detail.keepers.map((k) => (
                  <li key={k.id}>
                    {k.userName} · {k.role}{" "}
                    <span className={statusPill(k.isActive ? "success" : "muted")}>
                      {k.isActive ? "Active" : "Off"}
                    </span>
                  </li>
                ))}
                {detail.keepers.length === 0 && <li className="text-[var(--muted)]">Chưa gán</li>}
              </ul>

              {canManage && (
                <form onSubmit={onKeeper} className="space-y-2">
                  <select className={field} value={keeperUserId} onChange={(e) => setKeeperUserId(e.target.value)}>
                    {users.map((u) => (
                      <option key={u.id} value={u.id}>{u.displayName || u.username}</option>
                    ))}
                  </select>
                  <select className={field} value={keeperRole} onChange={(e) => setKeeperRole(e.target.value)}>
                    <option value="Keeper">Keeper</option>
                    <option value="Supervisor">Supervisor</option>
                  </select>
                  <button type="submit" className={btn.ghost}>Gán thủ kho</button>
                </form>
              )}
            </section>
          )}
        </div>
      </div>
    </div>
  );
}
