"use client";

import React, { useEffect, useState } from "react";
import {
  fetchSsoProviders,
  upsertSsoProvider,
  type SysSsoProviderDto,
} from "@/shared/api/sys-api";
import { validateSsoProviderForm } from "@/shared/api/sys-step153-helpers";
import { KeyRound, Plus, RefreshCw } from "lucide-react";
import { btn } from "@/shared/ui/btn";
import { field, panel } from "@/shared/ui/field";

export default function SsoProvidersPage() {
  const [rows, setRows] = useState<SysSsoProviderDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [msg, setMsg] = useState<string | null>(null);

  const [code, setCode] = useState("AZURE_AD");
  const [displayName, setDisplayName] = useState("Azure AD");
  const [clientId, setClientId] = useState("");
  const [clientSecret, setClientSecret] = useState("");
  const [authorityUrl, setAuthorityUrl] = useState("");
  const [redirectUri, setRedirectUri] = useState("http://localhost:3000/login?sso=callback");
  const [scopes, setScopes] = useState("openid profile email");
  const [jit, setJit] = useState(true);
  const [active, setActive] = useState(true);

  async function load() {
    try {
      setLoading(true);
      setError(null);
      setRows(await fetchSsoProviders());
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function onSave(e: React.FormEvent) {
    e.preventDefault();
    const v = validateSsoProviderForm({ code, displayName, clientId, redirectUri });
    if (!v.isValid) {
      setError(v.error ?? "Form không hợp lệ.");
      return;
    }
    try {
      setError(null);
      await upsertSsoProvider({
        code,
        displayName,
        clientId,
        clientSecret: clientSecret || null,
        authorityUrl: authorityUrl || null,
        redirectUri,
        scopes,
        jitProvisioning: jit,
        isActive: active,
      });
      setMsg("Đã lưu IdP SSO.");
      setClientSecret("");
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  return (
    <div className="p-6 max-w-5xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
            <KeyRound className="w-6 h-6 text-indigo-600" /> SSO / OAuth (UC_SYS_009)
          </h1>
          <p className="text-slate-500 text-sm mt-1">
            Cấu hình IdP. Đăng nhập Day-1 dùng mã <code>dev:email|subject</code> trên trang Login.
          </p>
        </div>
        <button type="button" className={btn.soft} onClick={() => void load()}>
          <RefreshCw className="w-4 h-4 mr-1 inline" /> Làm mới
        </button>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {msg && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{msg}</div>}

      <form onSubmit={(e) => void onSave(e)} className={`${panel} space-y-3`}>
        <div className="text-sm font-semibold text-slate-700">Thêm / cập nhật IdP</div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <label className="block space-y-1">
            <span className="text-xs text-slate-500">Mã</span>
            <input className={field} value={code} onChange={(e) => setCode(e.target.value)} />
          </label>
          <label className="block space-y-1">
            <span className="text-xs text-slate-500">Tên hiển thị</span>
            <input className={field} value={displayName} onChange={(e) => setDisplayName(e.target.value)} />
          </label>
          <label className="block space-y-1">
            <span className="text-xs text-slate-500">Client ID</span>
            <input className={field} value={clientId} onChange={(e) => setClientId(e.target.value)} />
          </label>
          <label className="block space-y-1">
            <span className="text-xs text-slate-500">Client Secret (tuỳ chọn)</span>
            <input className={field} type="password" value={clientSecret} onChange={(e) => setClientSecret(e.target.value)} />
          </label>
          <label className="block space-y-1 md:col-span-2">
            <span className="text-xs text-slate-500">Authority URL</span>
            <input className={field} value={authorityUrl} onChange={(e) => setAuthorityUrl(e.target.value)} />
          </label>
          <label className="block space-y-1 md:col-span-2">
            <span className="text-xs text-slate-500">Redirect URI</span>
            <input className={field} value={redirectUri} onChange={(e) => setRedirectUri(e.target.value)} />
          </label>
          <label className="block space-y-1">
            <span className="text-xs text-slate-500">Scopes</span>
            <input className={field} value={scopes} onChange={(e) => setScopes(e.target.value)} />
          </label>
          <div className="flex items-end gap-4 pb-1">
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={jit} onChange={(e) => setJit(e.target.checked)} /> JIT provisioning
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={active} onChange={(e) => setActive(e.target.checked)} /> Active
            </label>
          </div>
        </div>
        <button type="submit" className={btn.primary}>
          <Plus className="w-4 h-4 mr-1 inline" /> Lưu IdP
        </button>
      </form>

      <div className="bg-white dark:bg-slate-900 shadow rounded-xl border border-slate-200 dark:border-slate-800 overflow-hidden">
        {loading ? (
          <div className="p-4 text-sm text-slate-500">Đang tải…</div>
        ) : rows.length === 0 ? (
          <div className="p-4 text-sm text-slate-500">Chưa có IdP.</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-slate-50 dark:bg-slate-800/50 text-left">
              <tr>
                <th className="px-4 py-2">Mã</th>
                <th className="px-4 py-2">Tên</th>
                <th className="px-4 py-2">Client</th>
                <th className="px-4 py-2">JIT</th>
                <th className="px-4 py-2">TT</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
              {rows.map((r) => (
                <tr key={r.id}>
                  <td className="px-4 py-2 font-mono">{r.code}</td>
                  <td className="px-4 py-2">{r.displayName}</td>
                  <td className="px-4 py-2 font-mono text-xs">{r.clientId}</td>
                  <td className="px-4 py-2">{r.jitProvisioning ? "Có" : "Không"}</td>
                  <td className="px-4 py-2">{r.isActive ? "Active" : "Off"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
