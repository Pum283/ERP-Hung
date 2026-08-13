"use client";

import React, { useEffect, useState } from "react";
import {
  startBulkExport,
  fetchExportJobs,
  downloadExportJob,
  type SysBulkExportJobDto,
} from "@/shared/api/sys-api";
import { validateBulkExportRequest } from "@/shared/api/sys-notif-scan-export-ip-helpers";
import { getJobStatusLabel } from "@/shared/api/sys-export-helpers";
import { Download, RefreshCw } from "lucide-react";
import { btn } from "@/shared/ui/btn";
import { field, panel } from "@/shared/ui/field";

const OPTIONS = ["Users", "Files", "AuditLogs"] as const;

export default function ExportJobsPage() {
  const [selected, setSelected] = useState<string[]>(["Users"]);
  const [format, setFormat] = useState("Csv");
  const [jobs, setJobs] = useState<SysBulkExportJobDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [msg, setMsg] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  async function load() {
    try {
      setLoading(true);
      setError(null);
      setJobs(await fetchExportJobs());
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  function toggle(t: string) {
    setSelected((prev) => (prev.includes(t) ? prev.filter((x) => x !== t) : [...prev, t]));
  }

  async function onStart(e: React.FormEvent) {
    e.preventDefault();
    const v = validateBulkExportRequest(selected, format);
    if (!v.isValid) {
      setError(v.error ?? "Form không hợp lệ.");
      return;
    }
    try {
      setError(null);
      const job = await startBulkExport(selected, format);
      setMsg(`Job ${job.id.slice(0, 8)}… ${job.status} · ${job.rowCount} rows`);
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function onDownload(id: string, name?: string | null) {
    try {
      const blob = await downloadExportJob(id);
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = name || "export.bin";
      a.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      setError((err as Error).message);
    }
  }

  return (
    <div className="max-w-5xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground flex items-center gap-2">
            <Download className="w-6 h-6 text-brand" /> Xuất hàng loạt (UC_SYS_077)
          </h1>
          <p className="text-body text-muted-foreground mt-1">Chọn nhiều entity → job BulkExport → tải trong 7 ngày.</p>
        </div>
        <button type="button" className={btn.soft} onClick={() => void load()}>
          <RefreshCw className="w-4 h-4 mr-1 inline" /> Làm mới
        </button>
      </div>

      {error && <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div>}
      {msg && <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">{msg}</div>}

      <form onSubmit={(e) => void onStart(e)} className={`${panel} space-y-3`}>
        <div className="flex flex-wrap gap-4 text-sm">
          {OPTIONS.map((o) => (
            <label key={o} className="flex items-center gap-2">
              <input type="checkbox" checked={selected.includes(o)} onChange={() => toggle(o)} />
              {o}
            </label>
          ))}
        </div>
        <select className={field} value={format} onChange={(e) => setFormat(e.target.value)}>
          <option value="Csv">Csv</option>
          <option value="Pdf">Pdf</option>
        </select>
        <button type="submit" className={btn.primary}>Bắt đầu xuất</button>
      </form>

      <div className="bg-surface shadow rounded-xl border overflow-hidden">
        {loading ? (
          <div className="p-4 text-sm text-muted-foreground">Đang tải…</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-slate-50 text-left">
              <tr>
                <th className="px-4 py-2">Loại</th>
                <th className="px-4 py-2">Entities</th>
                <th className="px-4 py-2">Status</th>
                <th className="px-4 py-2">Rows</th>
                <th className="px-4 py-2" />
              </tr>
            </thead>
            <tbody className="divide-y">
              {jobs.map((j) => (
                <tr key={j.id}>
                  <td className="px-4 py-2">{j.jobType}</td>
                  <td className="px-4 py-2 font-mono text-xs">{j.entityType}</td>
                  <td className="px-4 py-2">{getJobStatusLabel(j.status)}</td>
                  <td className="px-4 py-2">{j.rowCount}</td>
                  <td className="px-4 py-2 text-right">
                    {j.status === "Completed" && j.resultFileName && (
                      <button type="button" className={btn.soft} onClick={() => void onDownload(j.id, j.resultFileName)}>
                        Tải
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
