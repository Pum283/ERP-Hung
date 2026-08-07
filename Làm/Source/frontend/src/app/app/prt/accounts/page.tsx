"use client";

import { FormEvent, useCallback, useEffect, useState } from "react";
import {
  fetchPrtAccounts,
  forgotPrtPassword,
  linkPrtCustomer,
  loginPrtAccount,
  registerPrtAccount,
  resetPrtPassword,
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
  const [resetToken, setResetToken] = useState("");
  const [newPassword, setNewPassword] = useState("");
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
        <h1 className="text-xl font-semibold tracking-tight">Tài khoản Portal Khách hàng</h1>
        <p className="mt-1 text-sm text-[var(--muted)]">
          Đăng ký · Quên / Đặt lại mật khẩu (OTP/Token) · Liên kết mã KH (UC_PRT_001–003)
        </p>
      </div>
      {error && <div className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {ok && <div className="rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{ok}</div>}
      {loading && <div className="text-sm text-[var(--muted)]">Đang tải…</div>}

      <div className="grid gap-4 xl:grid-cols-2">
        <section className={panel}>
          <h2 className="mb-3 text-sm font-semibold">Danh sách Tài khoản</h2>
          <div className={tableWrap}>
            <table className="w-full text-sm">
              <thead><tr><th className={th}>Email</th><th className={th}>KH</th><th className={th}>TT</th></tr></thead>
              <tbody>
                {list.map((a) => (
                  <tr key={a.id} className="cursor-pointer hover:bg-black/5" onClick={() => { setSelectedId(a.id); setEmail(a.email); }}>
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
          <h2 className="mb-3 text-sm font-semibold">Xác thực & Khôi phục Mật khẩu</h2>
          {canManage && (
            <div className="space-y-4 text-sm">
              <form className="space-y-2 border-b border-black/10 pb-3" onSubmit={(e: FormEvent) => {
                e.preventDefault();
                void run(() => registerPrtAccount({
                  email, displayName: name, password, customerCode: custCode || undefined,
                }), "Đã đăng ký tài khoản");
              }}>
                <div className="text-xs font-medium text-[var(--muted)]">Tạo / Đăng ký tài khoản</div>
                <input className={field} placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
                <input className={field} placeholder="Họ tên" value={name} onChange={(e) => setName(e.target.value)} />
                <input className={field} type="password" placeholder="Mật khẩu" value={password}
                  onChange={(e) => setPassword(e.target.value)} />
                <div className="flex gap-2 pt-1">
                  <button className={btn.primary} type="submit">Đăng ký</button>
                  <button type="button" className={btn.ghost} onClick={() => void run(
                    async () => {
                      const res = await loginPrtAccount({ email, password });
                      flash(`Đăng nhập thành công (${res.account.displayName})`);
                    }, "Đã xác thực",
                  )}>
                    Đăng nhập Portal
                  </button>
                </div>
              </form>

              <div className="space-y-2 border-b border-black/10 pb-3">
                <div className="text-xs font-medium text-[var(--muted)]">Quên / Đặt lại MK (Token)</div>
                <div className="flex gap-2">
                  <input className={field} placeholder="Email nhận token" value={email} onChange={(e) => setEmail(e.target.value)} />
                  <button type="button" className={btn.ghost} onClick={() => void run(
                    () => forgotPrtPassword(email), "Đã gửi Token reset qua email (Integration Logged)",
                  )}>
                    Gửi OTP/Token
                  </button>
                </div>
                <div className="flex flex-wrap gap-2 pt-1">
                  <input className={field} placeholder="Mã Token OTP" value={resetToken} onChange={(e) => setResetToken(e.target.value)} />
                  <input className={field} type="password" placeholder="MK mới" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} />
                  <button type="button" className={btn.primary} onClick={() => void run(
                    () => resetPrtPassword({ email, resetToken, newPassword }), "Đặt lại mật khẩu thành công!",
                  )}>
                    Đổi MK
                  </button>
                </div>
              </div>

              {selected && (
                <form className="flex flex-wrap gap-2 pt-1" onSubmit={(e: FormEvent) => {
                  e.preventDefault();
                  void run(() => linkPrtCustomer({
                    accountId: selected.id, customerCode: custCode, customerName: custName,
                  }), "Đã liên kết mã KH");
                }}>
                  <div className="w-full text-xs font-medium text-[var(--muted)]">Liên kết mã KH: {selected.email}</div>
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

