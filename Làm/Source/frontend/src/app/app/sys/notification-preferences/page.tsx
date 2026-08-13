"use client";

import React, { useEffect, useState } from "react";
import {
  fetchMyNotificationPreferences,
  upsertMyNotificationPreferences,
  type SysNotificationPreferenceDto,
} from "@/shared/api/sys-api";
import { validateQuietHours } from "@/shared/api/sys-notif-scan-export-ip-helpers";
import { Bell, RefreshCw } from "lucide-react";
import { btn } from "@/shared/ui/btn";
import { field, panel } from "@/shared/ui/field";

export default function NotificationPreferencesPage() {
  const [prefs, setPrefs] = useState<SysNotificationPreferenceDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [msg, setMsg] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  async function load() {
    try {
      setLoading(true);
      setError(null);
      setPrefs(await fetchMyNotificationPreferences());
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
    if (!prefs) return;
    const v = validateQuietHours(prefs.quietHoursStart, prefs.quietHoursEnd);
    if (!v.isValid) {
      setError(v.error ?? "Quiet hours không hợp lệ.");
      return;
    }
    try {
      setError(null);
      const saved = await upsertMyNotificationPreferences({
        channelInApp: prefs.channelInApp,
        channelEmail: prefs.channelEmail,
        channelSms: prefs.channelSms,
        channelPush: prefs.channelPush,
        muteAll: prefs.muteAll,
        quietHoursStart: prefs.quietHoursStart || null,
        quietHoursEnd: prefs.quietHoursEnd || null,
      });
      setPrefs(saved);
      setMsg("Đã lưu tùy chọn thông báo.");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  return (
    <div className="p-6 max-w-3xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
            <Bell className="w-6 h-6 text-indigo-600" /> Tùy chọn thông báo (UC_SYS_064)
          </h1>
          <p className="text-slate-500 text-sm mt-1">
            Kênh In-app / Email / SMS / Push · Mute all · Quiet hours. Sự kiện bảo mật luôn gửi.
          </p>
        </div>
        <button type="button" className={btn.soft} onClick={() => void load()}>
          <RefreshCw className="w-4 h-4 mr-1 inline" /> Làm mới
        </button>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {msg && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{msg}</div>}

      {loading || !prefs ? (
        <div className="text-sm text-slate-500">Đang tải…</div>
      ) : (
        <form onSubmit={(e) => void onSave(e)} className={`${panel} space-y-4`}>
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" checked={prefs.muteAll} onChange={(e) => setPrefs({ ...prefs, muteAll: e.target.checked })} />
            Mute all (trừ sự kiện bảo mật)
          </label>
          <div className="grid grid-cols-2 gap-3 text-sm">
            <label className="flex items-center gap-2">
              <input type="checkbox" checked={prefs.channelInApp} onChange={(e) => setPrefs({ ...prefs, channelInApp: e.target.checked })} />
              In-app
            </label>
            <label className="flex items-center gap-2">
              <input type="checkbox" checked={prefs.channelEmail} onChange={(e) => setPrefs({ ...prefs, channelEmail: e.target.checked })} />
              Email
            </label>
            <label className="flex items-center gap-2">
              <input type="checkbox" checked={prefs.channelSms} onChange={(e) => setPrefs({ ...prefs, channelSms: e.target.checked })} />
              SMS
            </label>
            <label className="flex items-center gap-2">
              <input type="checkbox" checked={prefs.channelPush} onChange={(e) => setPrefs({ ...prefs, channelPush: e.target.checked })} />
              Push
            </label>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <label className="block space-y-1">
              <span className="text-xs text-slate-500">Quiet start (HH:mm UTC)</span>
              <input className={field} placeholder="22:00" value={prefs.quietHoursStart ?? ""} onChange={(e) => setPrefs({ ...prefs, quietHoursStart: e.target.value })} />
            </label>
            <label className="block space-y-1">
              <span className="text-xs text-slate-500">Quiet end (HH:mm UTC)</span>
              <input className={field} placeholder="06:00" value={prefs.quietHoursEnd ?? ""} onChange={(e) => setPrefs({ ...prefs, quietHoursEnd: e.target.value })} />
            </label>
          </div>
          <button type="submit" className={btn.primary}>Lưu tùy chọn</button>
        </form>
      )}
    </div>
  );
}
