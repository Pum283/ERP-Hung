"use client";

import React, { useState } from "react";
import { Laptop, ShieldCheck, Trash2 } from "lucide-react";

export default function TrustedDevicesPage() {
  const [devices, setDevices] = useState([
    { id: "1", name: "MacBook Pro 16 (Workstation)", ip: "192.168.1.50", lastUsed: "Hôm nay, 14:15", isCurrent: true },
    { id: "2", name: "iPhone 15 Pro Max", ip: "10.0.0.12", lastUsed: "Hôm qua, 09:30", isCurrent: false },
  ]);

  const handleRevoke = (id: string) => {
    setDevices(devices.filter(d => d.id !== id));
  };

  return (
    <div className="p-6 max-w-4xl space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
          <ShieldCheck className="w-6 h-6 text-indigo-600" /> Quản lý Thiết bị Tin cậy (UC_SYS_012)
        </h1>
        <p className="text-slate-500 text-sm mt-1">Danh sách các trình duyệt và thiết bị đã lưu để bỏ qua 2FA trong 30 ngày.</p>
      </div>

      <div className="bg-white dark:bg-slate-900 shadow rounded-xl border border-slate-200 dark:border-slate-800 divide-y divide-slate-100 dark:divide-slate-800">
        {devices.map((device) => (
          <div key={device.id} className="p-4 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="p-2.5 bg-indigo-50 dark:bg-indigo-950/40 text-indigo-600 rounded-lg">
                <Laptop className="w-5 h-5" />
              </div>
              <div>
                <div className="font-semibold text-slate-800 dark:text-slate-200 flex items-center gap-2">
                  {device.name}
                  {device.isCurrent && (
                    <span className="text-[10px] bg-emerald-100 text-emerald-700 px-2 py-0.5 rounded-full font-medium">Thiết bị này</span>
                  )}
                </div>
                <div className="text-xs text-slate-400 mt-0.5">IP: {device.ip} • Đăng nhập gần nhất: {device.lastUsed}</div>
              </div>
            </div>
            {!device.isCurrent && (
              <button
                onClick={() => handleRevoke(device.id)}
                className="text-xs text-rose-600 hover:bg-rose-50 p-2 rounded-lg transition-colors flex items-center gap-1 font-medium"
              >
                <Trash2 className="w-4 h-4" /> Thu hồi
              </button>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
