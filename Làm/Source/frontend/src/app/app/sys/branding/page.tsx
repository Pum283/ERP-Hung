"use client";

import React, { useEffect, useState } from "react";
import { fetchTheme, upsertTheme, type SysThemeDto } from "@/shared/api/sys-api";
import { applyThemeCssVars, validateThemeForm } from "@/shared/api/sys-step155-helpers";
import { Palette, RefreshCw } from "lucide-react";
import { btn } from "@/shared/ui/btn";
import { field, panel } from "@/shared/ui/field";

export default function BrandingPage() {
  const [theme, setTheme] = useState<SysThemeDto | null>(null);
  const [primary, setPrimary] = useState("#0EA5E9");
  const [accent, setAccent] = useState("#F59E0B");
  const [favicon, setFavicon] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [msg, setMsg] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  async function load() {
    try {
      setLoading(true);
      setError(null);
      const t = await fetchTheme();
      setTheme(t);
      setPrimary(t.primaryColor || "#0EA5E9");
      setAccent(t.accentColor || "#F59E0B");
      setFavicon(t.faviconUrl || "");
      const vars = applyThemeCssVars(t.primaryColor, t.accentColor);
      Object.entries(vars).forEach(([k, v]) => document.documentElement.style.setProperty(k, v));
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
    const v = validateThemeForm({ primaryColor: primary, accentColor: accent });
    if (!v.isValid) {
      setError(v.error ?? "Form không hợp lệ.");
      return;
    }
    try {
      setError(null);
      const saved = await upsertTheme({
        primaryColor: primary || null,
        accentColor: accent || null,
        faviconUrl: favicon || null,
      });
      setTheme(saved);
      setMsg("Đã lưu theme.");
      const vars = applyThemeCssVars(saved.primaryColor, saved.accentColor);
      Object.entries(vars).forEach(([k, v]) => document.documentElement.style.setProperty(k, v));
    } catch (err) {
      setError((err as Error).message);
    }
  }

  return (
    <div className="p-6 max-w-3xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
            <Palette className="w-6 h-6 text-indigo-600" /> Theme / Branding (UC_SYS_093)
          </h1>
          <p className="text-slate-500 text-sm mt-1">
            Màu brand + favicon. Logo upload vẫn ở trang Tenant.
          </p>
        </div>
        <button type="button" className={btn.soft} onClick={() => void load()}>
          <RefreshCw className="w-4 h-4 mr-1 inline" /> Làm mới
        </button>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {msg && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{msg}</div>}

      {loading ? (
        <div className="text-sm text-slate-500">Đang tải…</div>
      ) : (
        <form onSubmit={(e) => void onSave(e)} className={`${panel} space-y-4`}>
          <div className="text-sm text-slate-600">
            Tenant: <strong>{theme?.tenantName}</strong>
            {theme?.logoUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img src={theme.logoUrl} alt="logo" className="mt-2 h-12 object-contain" />
            ) : null}
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            <label className="block space-y-1">
              <span className="text-xs text-slate-500">Primary (#RRGGBB)</span>
              <input className={field} value={primary} onChange={(e) => setPrimary(e.target.value)} />
            </label>
            <label className="block space-y-1">
              <span className="text-xs text-slate-500">Accent (#RRGGBB)</span>
              <input className={field} value={accent} onChange={(e) => setAccent(e.target.value)} />
            </label>
          </div>
          <label className="block space-y-1">
            <span className="text-xs text-slate-500">Favicon URL</span>
            <input className={field} value={favicon} onChange={(e) => setFavicon(e.target.value)} />
          </label>
          <div className="flex gap-3 items-center">
            <span className="h-8 w-8 rounded" style={{ background: primary }} />
            <span className="h-8 w-8 rounded" style={{ background: accent }} />
            <span className="text-xs text-slate-500">Preview</span>
          </div>
          <button type="submit" className={btn.primary}>Lưu theme</button>
        </form>
      )}
    </div>
  );
}
