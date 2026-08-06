"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchPrtAccounts,
  forgotPrtPassword,
  linkPrtCustomer,
  loginPrtStub,
  registerPrtAccount,
  type PrtAccountDto,
} from "@/shared/api/prt-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";

export default function PrtAccountsPage() {
  const { can } = usePermissions();
  const canRead = can("prt.account.read");
  const canManage = can("prt.account.manage");

  const [list, setList] = useState<PrtAccountDto[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);

  const [email, setEmail] = useState("khach@demo.local");
  const [name, setName] = useState("Khách demo");
  const [password, setPassword] = useState("!Abc123");
  const [custCode, setCustCode] = useState("CUS001");
  const [custName, setCustName] = useState("KH Demo");

  const load = useCallback(async () => {
    const a = await fetchPrtAccounts();
    setList(a);
    if (!selectedId && a[0]) setSelectedId(a[0].id);
  }, [selectedId]);

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
    return <div className="p-6 text-sm text-[var(--muted)]">Bạn không có quyền xem tài khoản portal.</div>;
  }

  const selected = list.find((x) => x.id === selectedId) ?? null;

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Tài khoản portal</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Đăng ký · login/quên MK stub · liên kết mã KH (UC_PRT_001–003)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Email</th><th className={th}>KH</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {list.map((a) => (
                  <tr key={a.id} className="cursor-pointer hover:bg-black/5" onClick={() => setSelectedId(a.id)}>
                    <td className={td}>
                      <div>{a.email}</div>
                      <div className="text-xs text-[var(--muted)]">{a.displayName} · {a.code}</div>
                    </td>
                    <td className={td}>{a.customerCode ?? "—"}</td>
                    <td className={td}>
                      <span className={statusPill(a.status === "Active" ? "success" : "warning")}>{a.status}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Thao tác stub</h2>
          {canManage && (
            <div className="space-y-3 text-sm">
              <form className="space-y-2" onSubmit={(e: FormEvent) => {
                e.preventDefault();
                void run(() => registerPrtAccount({
                  email, displayName: name, password, customerCode: custCode || undefined,
                }), "Đã đăng ký");
              }}>
                <input className={field} placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
                <input className={field} placeholder="Tên" value={name} onChange={(e) => setName(e.target.value)} />
                <input className={field} type="password" placeholder="MK" value={password}
                  onChange={(e) => setPassword(e.target.value)} />
                <button className={btn.primary} type="submit">Đăng ký</button>
              </form>
              <div className="flex flex-wrap gap-2">
                <button type="button" className={btn.ghost} onClick={() => void run(
                  () => loginPrtStub({ email, password }), "Login stub OK",
                )}>
                  Login stub
                </button>
                <button type="button" className={btn.ghost} onClick={() => void run(
                  () => forgotPrtPassword(email), "Đã tạo reset token stub",
                )}>
                  Quên MK stub
                </button>
              </div>
              {selected && (
                <form className="flex flex-wrap gap-2 border-t border-black/10 pt-3" onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  void run(() => linkPrtCustomer({
                    accountId: selected.id, customerCode: custCode, customerName: custName,
                  }), "Đã liên kết mã KH");
                }}>
                  <div className="w-full text-xs text-[var(--muted)]">Liên kết: {selected.email}</div>
                  <input className={field} placeholder="Mã KH" value={custCode} onChange={(e) => setCustCode(e.target.value)} />
                  <input className={field} placeholder="Tên KH" value={custName} onChange={(e) => setCustName(e.target.value)} />
                  <button className={btn.primary} type="submit">Liên kết KH</button>
                </form>
              )}
            </div>
          )}
        </section>
      </div>
    </div>
  );
}
