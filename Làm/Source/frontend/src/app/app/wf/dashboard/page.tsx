"use client";

import { useEffect, useState } from "react";
import { fetchWfDashboard, type WfDashboardDto } from "@/shared/api/wf-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { btn } from "@/shared/ui/btn";

export default function WfDashboardPage() {
  const { can } = usePermissions();
  const canRead = can("wf.task.read");
  const [data, setData] = useState<WfDashboardDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      setData(await fetchWfDashboard());
    } catch {
      setError("Không tải được dashboard WF.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (!canRead) return;
    void load();
  }, [canRead]);

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền wf.task.read</p>;
  }

  const cards = data
    ? [
        ["Pending", data.pendingTasks],
        ["Quá hạn", data.overdueTasks],
        ["Duyệt hôm nay", data.completedToday],
        ["Từ chối hôm nay", data.rejectedToday],
        ["Instance chạy", data.runningInstances],
        ["Instance xong", data.completedInstances],
        ["Instance reject", data.rejectedInstances],
      ]
    : [];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground">Dashboard workflow</h1>
          <p className="mt-1 text-body text-muted-foreground">
            Khối lượng phê duyệt · theo module · 7 ngày · tải người duyệt
          </p>
        </div>
        <button type="button" className={btn.ghost} onClick={() => void load()}>
          Làm mới
        </button>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {loading && <p className="text-body text-muted-foreground">Đang tải…</p>}

      {data && (
        <>
          <section className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {cards.map(([label, value]) => (
              <div
                key={String(label)}
                className="rounded-xl border border-border bg-surface px-4 py-3 shadow-sm"
              >
                <div className="text-meta text-muted-foreground">{label}</div>
                <div className="mt-1 font-display text-title font-bold">{value}</div>
              </div>
            ))}
          </section>

          <section className="grid gap-4 lg:grid-cols-2">
            <div className="rounded-xl border border-border bg-surface p-4 shadow-sm">
              <h2 className="text-lead font-bold">Theo module</h2>
              {data.byModule.length === 0 ? (
                <p className="mt-2 text-body text-muted-foreground">Chưa có dữ liệu.</p>
              ) : (
                <table className="mt-3 w-full text-left text-body">
                  <thead>
                    <tr className="border-b border-border text-muted-foreground">
                      <th className="py-2 pr-2">Module</th>
                      <th className="py-2 pr-2">Pending</th>
                      <th className="py-2 pr-2">Approved</th>
                      <th className="py-2">Rejected</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.byModule.map((m) => (
                      <tr key={m.moduleCode} className="border-b border-border/60">
                        <td className="py-2 pr-2">{m.moduleCode}</td>
                        <td className="py-2 pr-2">{m.pending}</td>
                        <td className="py-2 pr-2">{m.completed}</td>
                        <td className="py-2">{m.rejected}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>

            <div className="rounded-xl border border-border bg-surface p-4 shadow-sm">
              <h2 className="text-lead font-bold">7 ngày gần nhất</h2>
              <table className="mt-3 w-full text-left text-body">
                <thead>
                  <tr className="border-b border-border text-muted-foreground">
                    <th className="py-2 pr-2">Ngày</th>
                    <th className="py-2 pr-2">Duyệt</th>
                    <th className="py-2">Từ chối</th>
                  </tr>
                </thead>
                <tbody>
                  {data.last7Days.map((d) => (
                    <tr key={d.date} className="border-b border-border/60">
                      <td className="py-2 pr-2">{d.date.slice(0, 10)}</td>
                      <td className="py-2 pr-2">{d.completed}</td>
                      <td className="py-2">{d.rejected}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>

          <section className="rounded-xl border border-border bg-surface p-4 shadow-sm">
            <h2 className="text-lead font-bold">Top người duyệt (pending)</h2>
            {data.topAssignees.length === 0 ? (
              <p className="mt-2 text-body text-muted-foreground">Không có task pending.</p>
            ) : (
              <ul className="mt-2 space-y-1 text-body">
                {data.topAssignees.map((a) => (
                  <li key={a.userId}>
                    {a.userName}: <strong>{a.pendingCount}</strong>
                  </li>
                ))}
              </ul>
            )}
          </section>
        </>
      )}
    </div>
  );
}
