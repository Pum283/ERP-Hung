"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchPosStoreDetail,
  fetchPosStores,
  upsertPosCashier,
  upsertPosPrinter,
  upsertPosStore,
  upsertPosTerminal,
  type PosStoreDetailDto,
  type PosStoreDto,
} from "@/shared/api/pos-api";
import { fetchMsgDirectory, type MsgDirectoryUserDto } from "@/shared/api/msg-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PosStoresPage() {
  const { can } = usePermissions();
  const canRead = can("pos.store.read");
  const canManage = can("pos.store.manage");

  const [stores, setStores] = useState<PosStoreDto[]>([]);
  const [users, setUsers] = useState<MsgDirectoryUserDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [detail, setDetail] = useState<PosStoreDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [code, setCode] = useState("CH-001");
  const [name, setName] = useState("");
  const [address, setAddress] = useState("");

  const [termCode, setTermCode] = useState("Q01");
  const [termName, setTermName] = useState("");
  const [printCode, setPrintCode] = useState("PRN-01");
  const [printName, setPrintName] = useState("");
  const [printType, setPrintType] = useState("Receipt");
  const [printConn, setPrintConn] = useState("");
  const [cashierUserId, setCashierUserId] = useState("");
  const [cashierRole, setCashierRole] = useState("Cashier");

  const load = useCallback(async () => {
    const [s, u] = await Promise.all([
      fetchPosStores(),
      fetchMsgDirectory().catch(() => [] as MsgDirectoryUserDto[]),
    ]);
    setStores(s);
    setUsers(u);
    if (!selectedId && s[0]) setSelectedId(s[0].id);
    if (!cashierUserId && u[0]) setCashierUserId(u[0].id);
  }, [selectedId, cashierUserId]);

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
    fetchPosStoreDetail(selectedId)
      .then(setDetail)
      .catch((e: Error) => setError(e.message));
  }, [selectedId, canRead]);

  function flash(msg: string) {
    setOk(msg);
    setError(null);
    setTimeout(() => setOk(null), 2500);
  }

  async function onSaveStore(e: FormEvent) {
    e.preventDefault();
    try {
      const saved = await upsertPosStore({ code, name, address });
      setName("");
      setAddress("");
      await load();
      setSelectedId(saved.id);
      flash("Đã lưu điểm bán.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function refreshDetail() {
    if (!selectedId) return;
    setDetail(await fetchPosStoreDetail(selectedId));
    await load();
  }

  async function onAddTerminal(e: FormEvent) {
    e.preventDefault();
    if (!selectedId) return;
    try {
      await upsertPosTerminal(selectedId, { code: termCode, name: termName });
      setTermName("");
      await refreshDetail();
      flash("Đã thêm quầy.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onAddPrinter(e: FormEvent) {
    e.preventDefault();
    if (!selectedId) return;
    try {
      await upsertPosPrinter(selectedId, {
        code: printCode,
        name: printName,
        printerType: printType,
        connectionInfo: printConn || undefined,
      });
      setPrintName("");
      setPrintConn("");
      await refreshDetail();
      flash("Đã thêm máy in.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onAddCashier(e: FormEvent) {
    e.preventDefault();
    if (!selectedId || !cashierUserId) return;
    try {
      await upsertPosCashier(selectedId, { userId: cashierUserId, role: cashierRole, isActive: true });
      await refreshDetail();
      flash("Đã gán thu ngân.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  if (!canRead) {
    return <div className="p-6 text-sm text-[var(--muted)]">Không có quyền xem điểm bán POS.</div>;
  }

  return (
    <div className="space-y-4 p-4 md:p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Điểm bán POS</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Điểm bán · quầy · máy in hóa đơn · phân quyền thu ngân (UC_POS_001–003, 007)
        </p>
      </div>

      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-[1fr_1.2fr]">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách điểm bán</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead>
                <tr>
                  <th className={th}>Mã</th>
                  <th className={th}>Tên</th>
                  <th className={th}>Quầy</th>
                  <th className={th}>In</th>
                  <th className={th}>TN</th>
                </tr>
              </thead>
              <tbody>
                {stores.map((s) => (
                  <tr
                    key={s.id}
                    className={`cursor-pointer hover:bg-[var(--surface-2)] ${selectedId === s.id ? "bg-[var(--surface-2)]" : ""}`}
                    onClick={() => setSelectedId(s.id)}
                  >
                    <td className={td}>{s.code}</td>
                    <td className={td}>
                      <div>{s.name}</div>
                      <span className={statusPill(s.status === "Active" ? "success" : "muted")}>{s.status}</span>
                    </td>
                    <td className={td}>{s.terminalCount}</td>
                    <td className={td}>{s.printerCount}</td>
                    <td className={td}>{s.cashierCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <div className="space-y-4">
          {canManage && (
            <section className={panel}>
              <h2 className="mb-3 text-sm font-semibold">Tạo điểm bán</h2>
              <form onSubmit={onSaveStore} className="grid gap-2 sm:grid-cols-2">
                <input className={field} placeholder="Mã" value={code} onChange={(e) => setCode(e.target.value)} required />
                <input className={field} placeholder="Tên" value={name} onChange={(e) => setName(e.target.value)} required />
                <input className={`${field} sm:col-span-2`} placeholder="Địa chỉ" value={address} onChange={(e) => setAddress(e.target.value)} />
                <button type="submit" className={`${btn.primary} sm:col-span-2`}>Lưu điểm bán</button>
              </form>
            </section>
          )}

          {detail && (
            <section className={panel}>
              <h2 className="mb-1 text-sm font-semibold">{detail.store.name}</h2>
              <p className="mb-4 text-xs text-[var(--muted)]">
                {detail.store.code} · {detail.store.address || "Chưa có địa chỉ"}
              </p>

              <div className="mb-4 grid gap-4 md:grid-cols-3">
                <div>
                  <h3 className="mb-2 text-xs font-semibold uppercase text-[var(--muted)]">Quầy / máy</h3>
                  <ul className="mb-2 space-y-1 text-sm">
                    {detail.terminals.map((t) => (
                      <li key={t.id}>{t.code} · {t.name}</li>
                    ))}
                    {detail.terminals.length === 0 && <li className="text-[var(--muted)]">Chưa có</li>}
                  </ul>
                  {canManage && (
                    <form onSubmit={onAddTerminal} className="space-y-1">
                      <input className={field} placeholder="Mã quầy" value={termCode} onChange={(e) => setTermCode(e.target.value)} required />
                      <input className={field} placeholder="Tên quầy" value={termName} onChange={(e) => setTermName(e.target.value)} required />
                      <button type="submit" className={btn.ghost}>Thêm quầy</button>
                    </form>
                  )}
                </div>

                <div>
                  <h3 className="mb-2 text-xs font-semibold uppercase text-[var(--muted)]">Máy in HĐ</h3>
                  <ul className="mb-2 space-y-1 text-sm">
                    {detail.printers.map((p) => (
                      <li key={p.id}>{p.code} · {p.name} ({p.printerType})</li>
                    ))}
                    {detail.printers.length === 0 && <li className="text-[var(--muted)]">Chưa có</li>}
                  </ul>
                  {canManage && (
                    <form onSubmit={onAddPrinter} className="space-y-1">
                      <input className={field} placeholder="Mã" value={printCode} onChange={(e) => setPrintCode(e.target.value)} required />
                      <input className={field} placeholder="Tên" value={printName} onChange={(e) => setPrintName(e.target.value)} required />
                      <select className={field} value={printType} onChange={(e) => setPrintType(e.target.value)}>
                        <option value="Receipt">Receipt</option>
                        <option value="Kitchen">Kitchen</option>
                      </select>
                      <input className={field} placeholder="IP / cổng" value={printConn} onChange={(e) => setPrintConn(e.target.value)} />
                      <button type="submit" className={btn.ghost}>Thêm máy in</button>
                    </form>
                  )}
                </div>

                <div>
                  <h3 className="mb-2 text-xs font-semibold uppercase text-[var(--muted)]">Thu ngân</h3>
                  <ul className="mb-2 space-y-1 text-sm">
                    {detail.cashiers.map((c) => (
                      <li key={c.id}>
                        {c.userName} · {c.role}{" "}
                        <span className={statusPill(c.isActive ? "success" : "muted")}>
                          {c.isActive ? "Active" : "Off"}
                        </span>
                      </li>
                    ))}
                    {detail.cashiers.length === 0 && <li className="text-[var(--muted)]">Chưa có</li>}
                  </ul>
                  {canManage && (
                    <form onSubmit={onAddCashier} className="space-y-1">
                      <select className={field} value={cashierUserId} onChange={(e) => setCashierUserId(e.target.value)}>
                        {users.map((u) => (
                          <option key={u.id} value={u.id}>{u.displayName || u.username}</option>
                        ))}
                      </select>
                      <select className={field} value={cashierRole} onChange={(e) => setCashierRole(e.target.value)}>
                        <option value="Cashier">Cashier</option>
                        <option value="Supervisor">Supervisor</option>
                      </select>
                      <button type="submit" className={btn.ghost}>Gán quyền</button>
                    </form>
                  )}
                </div>
              </div>
            </section>
          )}
        </div>
      </div>
    </div>
  );
}
