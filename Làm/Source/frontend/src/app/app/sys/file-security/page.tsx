"use client";

import React, { useEffect, useState } from "react";
import {
  fetchSysFiles,
  scanSysFile,
  fetchFileScanStatus,
  type SysFileScanStatusDto,
} from "@/shared/api/sys-api";
import { canDownloadFile, formatScanStatusLabel } from "@/shared/api/sys-step154-helpers";
import { RefreshCw, ShieldAlert } from "lucide-react";
import { btn } from "@/shared/ui/btn";
import { panel } from "@/shared/ui/field";

type FileRow = { id: string; fileName: string; sizeBytes: number; storageKey: string };

export default function FileSecurityPage() {
  const [files, setFiles] = useState<FileRow[]>([]);
  const [statusMap, setStatusMap] = useState<Record<string, SysFileScanStatusDto>>({});
  const [error, setError] = useState<string | null>(null);
  const [msg, setMsg] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  async function load() {
    try {
      setLoading(true);
      setError(null);
      const list = await fetchSysFiles();
      setFiles(list as FileRow[]);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function onScan(id: string, fileName: string) {
    try {
      setError(null);
      const hint = fileName.toLowerCase().includes("eicar") ? "EICAR-STANDARD-ANTIVIRUS-TEST-FILE" : undefined;
      const st = await scanSysFile(id, hint);
      setStatusMap((m) => ({ ...m, [id]: st }));
      setMsg(`Quét xong: ${formatScanStatusLabel(st.scanStatus)}`);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onRefreshStatus(id: string) {
    try {
      const st = await fetchFileScanStatus(id);
      setStatusMap((m) => ({ ...m, [id]: st }));
    } catch (err) {
      setError((err as Error).message);
    }
  }

  return (
    <div className="p-6 max-w-5xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white flex items-center gap-2">
            <ShieldAlert className="w-6 h-6 text-indigo-600" /> Bảo mật file (UC_SYS_071)
          </h1>
          <p className="text-slate-500 text-sm mt-1">
            Stub scanner (EICAR). File Infected bị chặn tải xuống.
          </p>
        </div>
        <button type="button" className={btn.soft} onClick={() => void load()}>
          <RefreshCw className="w-4 h-4 mr-1 inline" /> Làm mới
        </button>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {msg && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{msg}</div>}

      <div className={`${panel} overflow-hidden p-0`}>
        {loading ? (
          <div className="p-4 text-sm text-slate-500">Đang tải…</div>
        ) : files.length === 0 ? (
          <div className="p-4 text-sm text-slate-500">Chưa có file.</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-slate-50 text-left">
              <tr>
                <th className="px-4 py-2">File</th>
                <th className="px-4 py-2">Size</th>
                <th className="px-4 py-2">Scan</th>
                <th className="px-4 py-2">Tải?</th>
                <th className="px-4 py-2" />
              </tr>
            </thead>
            <tbody className="divide-y">
              {files.map((f) => {
                const st = statusMap[f.id];
                const download = canDownloadFile(st?.scanStatus ?? "Pending");
                return (
                  <tr key={f.id}>
                    <td className="px-4 py-2 font-mono text-xs">{f.fileName}</td>
                    <td className="px-4 py-2">{f.sizeBytes}</td>
                    <td className="px-4 py-2">
                      {st ? formatScanStatusLabel(st.scanStatus) : "—"}
                      {st?.threatName ? ` (${st.threatName})` : ""}
                    </td>
                    <td className="px-4 py-2">{download.canDownload ? "OK" : download.reason}</td>
                    <td className="px-4 py-2 text-right space-x-2">
                      <button type="button" className={btn.soft} onClick={() => void onScan(f.id, f.fileName)}>Quét</button>
                      <button type="button" className={btn.soft} onClick={() => void onRefreshStatus(f.id)}>Status</button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
