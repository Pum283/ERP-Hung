"use client";

import React, { useEffect, useState } from "react";
import {
  fetchUserSessions,
  revokeUserSession,
  fetchTrustedDevices,
  registerTrustedDevice,
  revokeTrustedDevice,
  type UserSessionDto,
  type TrustedDeviceDto,
} from "@/shared/api/sys-api";
import { Laptop, Plus, RefreshCw, ShieldCheck, Trash2 } from "lucide-react";
import { btn } from "@/shared/ui/btn";
import { field, panel } from "@/shared/ui/field";

export default function TrustedDevicesPage() {
  const [sessions, setSessions] = useState<UserSessionDto[]>([]);
  const [trustedDevices, setTrustedDevices] = useState<TrustedDeviceDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [actionMsg, setActionMsg] = useState<string | null>(null);

  const [newDeviceName, setNewDeviceName] = useState("");

  async function load() {
    try {
      setLoading(true);
      setError(null);
      const [sessionList, deviceList] = await Promise.all([
        fetchUserSessions().catch(() => [] as UserSessionDto[]),
        fetchTrustedDevices().catch(() => [] as TrustedDeviceDto[]),
      ]);
      setSessions(sessionList);
      setTrustedDevices(deviceList);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function handleRevokeSession(id: string) {
    try {
      setError(null);
      await revokeUserSession(id);
      setActionMsg("Đã thu hồi phiên đăng nhập thành công.");
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function handleRegisterDevice(e: React.FormEvent) {
    e.preventDefault();
    if (!newDeviceName.trim()) return;
    try {
      setError(null);
      const fingerprint = "fp-" + Math.random().toString(36).substring(2, 10);
      await registerTrustedDevice({ deviceFingerprint: fingerprint, deviceName: newDeviceName.trim() });
      setNewDeviceName("");
      setActionMsg("Đã lưu thiết bị tin cậy thành công (bỏ qua 2FA trong 30 ngày).");
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function handleRevokeDevice(id: string) {
    try {
      setError(null);
      await revokeTrustedDevice(id);
      setActionMsg("Đã thu hồi thiết bị tin cậy thành công.");
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  const activeSessions = sessions.filter(s => !s.isRevoked && new Date(s.expiresAt) > new Date());
  const activeDevices = trustedDevices.filter(d => d.isActive && new Date(d.expiresAt) > new Date());

  return (
    <div className="p-6 max-w-4xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
            <ShieldCheck className="w-6 h-6 text-indigo-600" /> Quản lý Phiên & Thiết bị Tin cậy (UC_SYS_010–012)
          </h1>
          <p className="text-slate-500 text-sm mt-1">Danh sách phiên làm việc active (tối đa 5 phiên) và danh sách trình duyệt/thiết bị tin cậy (bỏ qua 2FA trong 30 ngày).</p>
        </div>
        <button type="button" className={btn.soft} onClick={() => void load()}>
          <RefreshCw className="w-4 h-4 mr-1 inline" /> Làm mới
        </button>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {actionMsg && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{actionMsg}</div>}

      {/* UC_SYS_010 & UC_SYS_011: Sessions */}
      <section className="space-y-3">
        <div className={`${panel} flex items-center justify-between`}>
          <span className="text-sm font-semibold text-slate-700 dark:text-slate-300">
            Phiên đăng nhập active: <span className="text-indigo-600 font-bold">{activeSessions.length} / 5</span>
          </span>
          <span className="text-xs text-slate-500">Giới hạn tối đa 5 phiên đồng thời (UC_SYS_011)</span>
        </div>

        <div className="bg-white dark:bg-slate-900 shadow rounded-xl border border-slate-200 dark:border-slate-800 divide-y divide-slate-100 dark:divide-slate-800">
          {activeSessions.map((session, idx) => (
            <div key={session.id} className="p-4 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="p-2.5 bg-indigo-50 dark:bg-indigo-950/40 text-indigo-600 rounded-lg">
                  <Laptop className="w-5 h-5" />
                </div>
                <div>
                  <div className="font-semibold text-slate-800 dark:text-slate-200 flex items-center gap-2">
                    {session.userAgent ? (session.userAgent.length > 50 ? session.userAgent.slice(0, 50) + "…" : session.userAgent) : "Trình duyệt Web"}
                    {idx === 0 && (
                      <span className="text-[10px] bg-emerald-100 text-emerald-700 px-2 py-0.5 rounded-full font-medium">Gần nhất / Hiện tại</span>
                    )}
                  </div>
                  <div className="text-xs text-slate-400 mt-0.5">
                    IP: {session.ipAddress || "127.0.0.1"} • Hoạt động cuối: {new Date(session.lastSeenAt).toLocaleString("vi-VN")} • Hết hạn: {new Date(session.expiresAt).toLocaleString("vi-VN")}
                  </div>
                </div>
              </div>
              <button
                onClick={() => void handleRevokeSession(session.id)}
                className="text-xs text-rose-600 hover:bg-rose-50 p-2 rounded-lg transition-colors flex items-center gap-1 font-medium"
              >
                <Trash2 className="w-4 h-4" /> Thu hồi phiên
              </button>
            </div>
          ))}

          {!loading && activeSessions.length === 0 && (
            <div className="p-6 text-center text-slate-500 text-sm">Không có phiên làm việc active nào.</div>
          )}
        </div>
      </section>

      {/* UC_SYS_012: Trusted Devices */}
      <section className="space-y-3 pt-4">
        <div className="flex items-center justify-between">
          <h2 className="text-base font-semibold text-slate-800 dark:text-slate-200">
            Thiết bị tin cậy (UC_SYS_012)
          </h2>
          <span className="text-xs text-slate-500">Đã lưu: {activeDevices.length} thiết bị</span>
        </div>

        <form onSubmit={handleRegisterDevice} className={`${panel} flex flex-wrap gap-2 items-center`}>
          <input
            className={`${field} flex-1 min-w-[200px]`}
            placeholder="Tên thiết bị (ví dụ: Laptop Làm Việc, iPhone Ca Nhân)"
            value={newDeviceName}
            onChange={(e) => setNewDeviceName(e.target.value)}
          />
          <button type="submit" className={btn.primary}>
            <Plus className="w-4 h-4 mr-1 inline" /> Thêm thiết bị tin cậy
          </button>
        </form>

        <div className="bg-white dark:bg-slate-900 shadow rounded-xl border border-slate-200 dark:border-slate-800 divide-y divide-slate-100 dark:divide-slate-800">
          {activeDevices.map((device) => (
            <div key={device.id} className="p-4 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="p-2.5 bg-emerald-50 dark:bg-emerald-950/40 text-emerald-600 rounded-lg">
                  <ShieldCheck className="w-5 h-5" />
                </div>
                <div>
                  <div className="font-semibold text-slate-800 dark:text-slate-200">
                    {device.deviceName}
                  </div>
                  <div className="text-xs text-slate-400 mt-0.5">
                    IP: {device.ipAddress} • Fingerprint: {device.deviceFingerprint} • Hết hạn tin cậy: {new Date(device.expiresAt).toLocaleString("vi-VN")}
                  </div>
                </div>
              </div>
              <button
                onClick={() => void handleRevokeDevice(device.id)}
                className="text-xs text-rose-600 hover:bg-rose-50 p-2 rounded-lg transition-colors flex items-center gap-1 font-medium"
              >
                <Trash2 className="w-4 h-4" /> Thu hồi tin cậy
              </button>
            </div>
          ))}

          {!loading && activeDevices.length === 0 && (
            <div className="p-6 text-center text-slate-500 text-sm">Chưa có thiết bị tin cậy nào được lưu.</div>
          )}
        </div>
      </section>
    </div>
  );
}
