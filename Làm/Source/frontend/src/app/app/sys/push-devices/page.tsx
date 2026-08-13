"use client";

import React, { useEffect, useState } from "react";
import {
  fetchMyPushDevices,
  registerPushDevice,
  revokePushDevice,
  sendTestPush,
  type SysPushDeviceDto,
} from "@/shared/api/sys-api";
import {
  formatPushPlatformLabel,
  validatePushDevice,
} from "@/shared/api/sys-sso-field-config-push-helpers";
import { Bell, Plus, RefreshCw, Trash2 } from "lucide-react";
import { btn } from "@/shared/ui/btn";
import { field, panel } from "@/shared/ui/field";

export default function PushDevicesPage() {
  const [devices, setDevices] = useState<SysPushDeviceDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [msg, setMsg] = useState<string | null>(null);
  const [platform, setPlatform] = useState("Fcm");
  const [token, setToken] = useState("");
  const [appVersion, setAppVersion] = useState("1.0.0");
  const [title, setTitle] = useState("ERP test push");
  const [body, setBody] = useState("Xin chào từ SYS_062");

  async function load() {
    try {
      setLoading(true);
      setError(null);
      setDevices(await fetchMyPushDevices());
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function onRegister(e: React.FormEvent) {
    e.preventDefault();
    const v = validatePushDevice(platform, token);
    if (!v.isValid) {
      setError(v.error ?? "Token không hợp lệ.");
      return;
    }
    try {
      setError(null);
      await registerPushDevice({ platform, deviceToken: token.trim(), appVersion });
      setMsg("Đã đăng ký device push.");
      setToken("");
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onRevoke(id: string) {
    try {
      setError(null);
      await revokePushDevice(id);
      setMsg("Đã thu hồi device.");
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onTest() {
    if (!title.trim() || !body.trim()) {
      setError("Title và Body bắt buộc.");
      return;
    }
    try {
      setError(null);
      const r = await sendTestPush({ title: title.trim(), body: body.trim() });
      setMsg(`Stub gửi ${r.deliveredStub}/${r.targetedDevices} device.`);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  return (
    <div className="p-6 max-w-4xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
            <Bell className="w-6 h-6 text-indigo-600" /> Push devices (UC_SYS_062)
          </h1>
          <p className="text-slate-500 text-sm mt-1">
            Đăng ký FCM/APNs/Web token. Gửi thử ghi IntegrationCallLog (stub, chưa gọi FCM thật).
          </p>
        </div>
        <button type="button" className={btn.soft} onClick={() => void load()}>
          <RefreshCw className="w-4 h-4 mr-1 inline" /> Làm mới
        </button>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {msg && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{msg}</div>}

      <form onSubmit={(e) => void onRegister(e)} className={`${panel} space-y-3`}>
        <div className="text-sm font-semibold">Đăng ký device</div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <select className={field} value={platform} onChange={(e) => setPlatform(e.target.value)}>
            <option value="Fcm">Fcm</option>
            <option value="Apns">Apns</option>
            <option value="Web">Web</option>
          </select>
          <input className={field} placeholder="Device token" value={token} onChange={(e) => setToken(e.target.value)} />
          <input className={field} placeholder="App version" value={appVersion} onChange={(e) => setAppVersion(e.target.value)} />
        </div>
        <button type="submit" className={btn.primary}>
          <Plus className="w-4 h-4 mr-1 inline" /> Đăng ký
        </button>
      </form>

      <div className={`${panel} space-y-3`}>
        <div className="text-sm font-semibold">Gửi thử (cần sys.push.manage)</div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <input className={field} value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Title" />
          <input className={field} value={body} onChange={(e) => setBody(e.target.value)} placeholder="Body" />
        </div>
        <button type="button" className={btn.soft} onClick={() => void onTest()}>Gửi stub push</button>
      </div>

      <div className="bg-white dark:bg-slate-900 shadow rounded-xl border border-slate-200 dark:border-slate-800 divide-y divide-slate-100 dark:divide-slate-800">
        {loading ? (
          <div className="p-4 text-sm text-slate-500">Đang tải…</div>
        ) : devices.length === 0 ? (
          <div className="p-4 text-sm text-slate-500">Chưa có device.</div>
        ) : (
          devices.map((d) => (
            <div key={d.id} className="p-4 flex items-center justify-between gap-3">
              <div>
                <div className="font-medium text-slate-800 dark:text-slate-100">
                  {formatPushPlatformLabel(d.platform)}
                  {d.appVersion ? ` · v${d.appVersion}` : ""}
                </div>
                <div className="text-xs font-mono text-slate-500 break-all">{d.deviceToken}</div>
                <div className="text-xs text-slate-400 mt-1">
                  Last seen: {new Date(d.lastSeenAt).toLocaleString("vi-VN")}
                </div>
              </div>
              <button type="button" className={btn.soft} onClick={() => void onRevoke(d.id)}>
                <Trash2 className="w-4 h-4" />
              </button>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
