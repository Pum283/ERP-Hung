"use client";

import React, { useState } from "react";
import {
  fetchConfigVersions,
  upsertSettingVersioned,
  rollbackConfigVersion,
  type SysConfigVersionDto,
} from "@/shared/api/sys-api";
import { canRollbackVersion, validateConfigKey } from "@/shared/api/sys-sso-field-config-push-helpers";
import { History, RefreshCw } from "lucide-react";
import { btn } from "@/shared/ui/btn";
import { field, panel } from "@/shared/ui/field";

export default function ConfigVersionsPage() {
  const [key, setKey] = useState("password.policy");
  const [valueJson, setValueJson] = useState('{"MinLength":8}');
  const [commitNote, setCommitNote] = useState("");
  const [versions, setVersions] = useState<SysConfigVersionDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [msg, setMsg] = useState<string | null>(null);

  async function load() {
    const v = validateConfigKey(key);
    if (!v.isValid) {
      setError(v.error ?? "Key không hợp lệ.");
      return;
    }
    try {
      setLoading(true);
      setError(null);
      setVersions(await fetchConfigVersions(key.trim()));
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  }

  async function onSave(e: React.FormEvent) {
    e.preventDefault();
    const v = validateConfigKey(key);
    if (!v.isValid) {
      setError(v.error ?? "Key không hợp lệ.");
      return;
    }
    try {
      setError(null);
      await upsertSettingVersioned(key.trim(), valueJson, commitNote || undefined);
      setMsg(`Đã lưu phiên bản mới cho ${key.trim()}.`);
      setCommitNote("");
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onRollback(versionNumber: number) {
    const check = canRollbackVersion(
      versions.map((x) => ({ versionNumber: x.versionNumber, isCurrent: x.isCurrent })),
      versionNumber,
    );
    if (!check.canRollback) {
      setError(check.reason ?? "Không rollback được.");
      return;
    }
    try {
      setError(null);
      await rollbackConfigVersion(key.trim(), versionNumber);
      setMsg(`Đã rollback về v${versionNumber}.`);
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  return (
    <div className="max-w-5xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground flex items-center gap-2">
            <History className="w-6 h-6 text-brand" /> Phiên bản cấu hình (UC_SYS_058)
          </h1>
          <p className="text-body text-muted-foreground mt-1">
            Mỗi lần upsert setting tạo version mới; rollback tạo version kế tiếp từ bản cũ.
          </p>
        </div>
        <button type="button" className={btn.soft} onClick={() => void load()}>
          <RefreshCw className="w-4 h-4 mr-1 inline" /> Tải lịch sử
        </button>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {msg && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{msg}</div>}

      <form onSubmit={(e) => void onSave(e)} className={`${panel} space-y-3`}>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <label className="block space-y-1">
            <span className="text-xs text-muted-foreground">Config key</span>
            <input className={field} value={key} onChange={(e) => setKey(e.target.value)} />
          </label>
          <label className="block space-y-1">
            <span className="text-xs text-muted-foreground">Commit note</span>
            <input className={field} value={commitNote} onChange={(e) => setCommitNote(e.target.value)} />
          </label>
        </div>
        <label className="block space-y-1">
          <span className="text-xs text-muted-foreground">Value JSON</span>
          <textarea
            className={`${field} min-h-[120px] font-mono text-xs`}
            value={valueJson}
            onChange={(e) => setValueJson(e.target.value)}
          />
        </label>
        <div className="flex gap-2">
          <button type="submit" className={btn.primary}>Lưu phiên bản</button>
          <button type="button" className={btn.soft} onClick={() => void load()}>Xem lịch sử</button>
        </div>
      </form>

      <div className="bg-surface shadow rounded-xl border border-border overflow-hidden">
        {loading ? (
          <div className="p-4 text-sm text-muted-foreground">Đang tải…</div>
        ) : versions.length === 0 ? (
          <div className="p-4 text-sm text-muted-foreground">Chưa có version — bấm Lưu hoặc Tải lịch sử.</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-slate-50 dark:bg-slate-800/50 text-left">
              <tr>
                <th className="px-4 py-2">Ver</th>
                <th className="px-4 py-2">Current</th>
                <th className="px-4 py-2">Note</th>
                <th className="px-4 py-2">Value</th>
                <th className="px-4 py-2" />
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
              {versions.map((v) => (
                <tr key={v.id}>
                  <td className="px-4 py-2 font-mono">v{v.versionNumber}</td>
                  <td className="px-4 py-2">{v.isCurrent ? "●" : ""}</td>
                  <td className="px-4 py-2">{v.commitNote || "—"}</td>
                  <td className="px-4 py-2 font-mono text-xs max-w-xs truncate">{v.configValue}</td>
                  <td className="px-4 py-2 text-right">
                    {!v.isCurrent && (
                      <button type="button" className={btn.soft} onClick={() => void onRollback(v.versionNumber)}>
                        Rollback
                      </button>
                    )}
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
