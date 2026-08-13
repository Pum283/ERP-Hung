"use client";

import React, { useEffect, useState } from "react";
import {
  fetchRoles,
  fetchRoleHomes,
  upsertRoleHome,
  deleteRoleHome,
  type RoleDto,
  type SysRoleHomeDto,
} from "@/shared/api/sys-api";
import { validateLandingPath } from "@/shared/api/sys-theme-role-home-msg-helpers";
import { Home, RefreshCw } from "lucide-react";
import { btn } from "@/shared/ui/btn";
import { field, panel } from "@/shared/ui/field";

export default function RoleHomesPage() {
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [rows, setRows] = useState<SysRoleHomeDto[]>([]);
  const [roleId, setRoleId] = useState("");
  const [path, setPath] = useState("/app/hrm");
  const [priority, setPriority] = useState(10);
  const [error, setError] = useState<string | null>(null);
  const [msg, setMsg] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  async function load() {
    try {
      setLoading(true);
      setError(null);
      const [r, h] = await Promise.all([fetchRoles(), fetchRoleHomes()]);
      setRoles(r);
      setRows(h);
      if (!roleId && r[0]) setRoleId(r[0].id);
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
    const v = validateLandingPath(path);
    if (!v.isValid) {
      setError(v.error ?? "Path không hợp lệ.");
      return;
    }
    if (!roleId) {
      setError("Chọn vai trò.");
      return;
    }
    try {
      setError(null);
      await upsertRoleHome({ roleId, landingPath: path.trim(), priority, isActive: true });
      setMsg("Đã lưu trang chủ theo vai trò.");
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onDelete(id: string) {
    try {
      await deleteRoleHome(id);
      setMsg("Đã xóa cấu hình.");
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  return (
    <div className="p-6 max-w-4xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
            <Home className="w-6 h-6 text-indigo-600" /> Trang chủ theo vai trò (UC_SYS_094)
          </h1>
          <p className="text-slate-500 text-sm mt-1">
            Priority nhỏ hơn = ưu tiên cao hơn khi user có nhiều role.
          </p>
        </div>
        <button type="button" className={btn.soft} onClick={() => void load()}>
          <RefreshCw className="w-4 h-4 mr-1 inline" /> Làm mới
        </button>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {msg && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{msg}</div>}

      <form onSubmit={(e) => void onSave(e)} className={`${panel} space-y-3`}>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <select className={field} value={roleId} onChange={(e) => setRoleId(e.target.value)}>
            {roles.map((r) => (
              <option key={r.id} value={r.id}>{r.code} — {r.name}</option>
            ))}
          </select>
          <input className={field} value={path} onChange={(e) => setPath(e.target.value)} placeholder="/app/..." />
          <input
            className={field}
            type="number"
            value={priority}
            onChange={(e) => setPriority(Number(e.target.value))}
          />
        </div>
        <button type="submit" className={btn.primary}>Lưu</button>
      </form>

      <div className="bg-white dark:bg-slate-900 shadow rounded-xl border overflow-hidden">
        {loading ? (
          <div className="p-4 text-sm text-slate-500">Đang tải…</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-slate-50 text-left">
              <tr>
                <th className="px-4 py-2">Role</th>
                <th className="px-4 py-2">Path</th>
                <th className="px-4 py-2">Priority</th>
                <th className="px-4 py-2" />
              </tr>
            </thead>
            <tbody className="divide-y">
              {rows.map((r) => (
                <tr key={r.id}>
                  <td className="px-4 py-2 font-mono">{r.roleCode}</td>
                  <td className="px-4 py-2">{r.landingPath}</td>
                  <td className="px-4 py-2">{r.priority}</td>
                  <td className="px-4 py-2 text-right">
                    <button type="button" className={btn.soft} onClick={() => void onDelete(r.id)}>Xóa</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
