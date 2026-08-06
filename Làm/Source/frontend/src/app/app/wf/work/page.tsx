"use client";

import { useEffect, useState } from "react";
import { api } from "@/shared/api/client";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";
import { field, panel, statusPill, tableWrap, td, th } from "@/shared/ui/field";
import { cn } from "@/shared/lib/cn";

type Item = {
  id: string;
  kind: string;
  title: string;
  status: string;
  priority: string;
  dueAt?: string | null;
};

function statusTone(status: string) {
  const s = status.toLowerCase();
  if (s.includes("done") || s.includes("closed")) return "success" as const;
  if (s.includes("overdue") || s.includes("cancel")) return "danger" as const;
  if (s.includes("open") || s.includes("progress")) return "brand" as const;
  return "muted" as const;
}

function priorityTone(priority: string) {
  const p = priority.toLowerCase();
  if (p.includes("high") || p.includes("urgent")) return "danger" as const;
  if (p.includes("low")) return "muted" as const;
  return "warning" as const;
}

export default function WfWorkPage() {
  const { can } = usePermissions();
  const [rows, setRows] = useState<Item[]>([]);
  const [title, setTitle] = useState("");
  const [workload, setWorkload] = useState<{ open: number; overdue: number } | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setError(null);
    try {
      const [items, wl] = await Promise.all([
        api.get<{ data: Item[] }>("/api/wf/work-items"),
        api.get<{ data: { open: number; overdue: number } }>("/api/wf/workload"),
      ]);
      setRows(items.data.data);
      setWorkload(wl.data.data);
    } catch {
      setError("Không tải được công việc.");
    }
  }

  useEffect(() => {
    if (!can("wf.task.read")) return;
    void load();
  }, [can]);

  if (!can("wf.task.read")) return <p className="text-body text-destructive">Không có quyền wf.task.read</p>;

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground">Công việc / ticket</h1>
          <p className="mt-1 text-body text-muted-foreground">Task nội bộ · theo dõi khối lượng & quá hạn</p>
        </div>
        <button type="button" className={btn.ghost} onClick={() => void load()}>
          Làm mới
        </button>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}

      {workload && (
        <div className="grid gap-3 sm:grid-cols-2">
          <div className="relative overflow-hidden rounded-xl border border-border bg-surface px-4 py-3">
            <div className="pointer-events-none absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-brand/80 via-accent/60 to-transparent" aria-hidden />
            <div className="text-meta text-muted-foreground">Đang mở</div>
            <div className="mt-1 font-display text-title font-bold tabular-nums text-brand-strong">{workload.open}</div>
          </div>
          <div className="relative overflow-hidden rounded-xl border border-border bg-surface px-4 py-3">
            <div className="pointer-events-none absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-destructive/70 to-transparent" aria-hidden />
            <div className="text-meta text-muted-foreground">Quá hạn</div>
            <div className="mt-1 font-display text-title font-bold tabular-nums text-destructive">{workload.overdue}</div>
          </div>
        </div>
      )}

      {can("wf.task.act") && (
        <section className={panel}>
          <h2 className="mb-3 font-display text-lead font-bold">Tạo task / ticket</h2>
          <div className="flex flex-wrap gap-2">
            <input
              className={cn(field.input, "min-w-[220px] flex-1")}
              placeholder="Tiêu đề task / ticket"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
            />
            <button
              type="button"
              className={btn.primary}
              onClick={() => {
                if (!title.trim()) return;
                void api
                  .post("/api/wf/work-items", {
                    kind: "Task",
                    title,
                    status: "Open",
                    priority: "Normal",
                  })
                  .then(() => {
                    setTitle("");
                    return load();
                  })
                  .catch(() => setError("Tạo task thất bại."));
              }}
            >
              Tạo
            </button>
          </div>
        </section>
      )}

      <div className={tableWrap}>
        <table className="w-full text-body">
          <thead className="border-b border-border bg-muted">
            <tr>
              <th className={th}>Loại</th>
              <th className={th}>Tiêu đề</th>
              <th className={th}>TT</th>
              <th className={th}>Ưu tiên</th>
              <th className={th}>Hạn</th>
            </tr>
          </thead>
          <tbody>
            {rows.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-3 py-8 text-center text-muted-foreground">
                  Chưa có công việc.
                </td>
              </tr>
            ) : (
              rows.map((r) => (
                <tr key={r.id} className="border-t border-border">
                  <td className={td}>
                    <span className={statusPill("muted")}>{r.kind}</span>
                  </td>
                  <td className={cn(td, "font-medium")}>{r.title}</td>
                  <td className={td}>
                    <span className={statusPill(statusTone(r.status))}>{r.status}</span>
                  </td>
                  <td className={td}>
                    <span className={statusPill(priorityTone(r.priority))}>{r.priority}</span>
                  </td>
                  <td className={cn(td, "tabular-nums text-muted-foreground")}>
                    {r.dueAt ? new Date(r.dueAt).toLocaleString("vi-VN") : "—"}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
