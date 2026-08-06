"use client";

import { useCallback, useEffect, useState } from "react";
import { actWfTask, fetchMyWfTasks, type WfTaskDto } from "@/shared/api/wf-api";
import { usePermissions } from "@/shared/hooks/use-permissions";
import { subscribeWfInbox } from "@/shared/realtime/wf-hub";

export default function WfTasksPage() {
  const { can } = usePermissions();
  const canRead = can("wf.task.read");
  const canAct = can("wf.task.act");
  const [rows, setRows] = useState<WfTaskDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [live, setLive] = useState(false);

  const load = useCallback(async (opts?: { silent?: boolean }) => {
    if (!opts?.silent) setLoading(true);
    setError(null);
    try {
      setRows(await fetchMyWfTasks());
    } catch {
      setError("Không tải được inbox phê duyệt.");
    } finally {
      if (!opts?.silent) setLoading(false);
    }
  }, []);

  // Load 1 lần khi vào trang — không poll
  useEffect(() => {
    if (!canRead) return;
    void load();
  }, [canRead, load]);

  // Realtime: SignalR đẩy inboxChanged → refresh 1 lần
  useEffect(() => {
    if (!canRead) return;
    const unsub = subscribeWfInbox(() => {
      setLive(true);
      void load({ silent: true });
    });
    setLive(true);
    return unsub;
  }, [canRead, load]);

  async function onAct(taskId: string, action: "Approve" | "Reject") {
    setBusyId(taskId);
    setError(null);
    setOk(null);
    try {
      await actWfTask(taskId, action);
      setOk(action === "Approve" ? "Đã duyệt." : "Đã từ chối.");
      // Optimistic: bỏ task khỏi list; SignalR sẽ sync lại nếu cần
      setRows((prev) => prev.filter((t) => t.id !== taskId));
    } catch {
      setError("Không xử lý được task (thiếu quyền hoặc đã xử lý).");
    } finally {
      setBusyId(null);
    }
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền wf.task.read</p>;
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-title font-bold text-foreground">Phê duyệt của tôi</h1>
          <p className="mt-1 text-body text-muted-foreground">
            Inbox WF · Approve / Reject · realtime SignalR
          </p>
        </div>
        <span className="text-meta text-muted-foreground">
          {live ? "Live · /hubs/wf" : "Đang kết nối…"}
        </span>
      </div>

      {error && <p className="text-body text-destructive">{error}</p>}
      {ok && <p className="text-body text-brand-strong">{ok}</p>}

      {loading ? (
        <p className="text-body text-muted-foreground">Đang tải…</p>
      ) : (
        <div className="overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
          <table className="w-full text-body">
            <thead className="border-b border-border bg-muted text-left text-muted-foreground">
              <tr>
                <th className="px-4 py-2.5 font-semibold">Nội dung</th>
                <th className="px-4 py-2.5 font-semibold">Bước</th>
                <th className="px-4 py-2.5 font-semibold">Hạn</th>
                <th className="px-4 py-2.5 font-semibold" />
              </tr>
            </thead>
            <tbody>
              {rows.map((t) => (
                <tr key={t.id} className="border-t border-border">
                  <td className="px-4 py-2.5">
                    <div className="font-medium text-foreground">{t.docSummary ?? "—"}</div>
                    <div className="text-meta text-muted-foreground">
                      {t.sourceModule}/{t.sourceDocType}
                      {t.viaDelegation
                        ? ` · ủy quyền từ ${t.assigneeName ?? "người khác"}`
                        : ""}
                    </div>
                  </td>
                  <td className="px-4 py-2.5">{t.nodeName ?? "—"}</td>
                  <td className="px-4 py-2.5 text-meta">
                    {t.dueAt ? new Date(t.dueAt).toLocaleString("vi-VN") : "—"}
                  </td>
                  <td className="px-4 py-2.5 text-right">
                    {canAct && (
                      <div className="inline-flex gap-2">
                        <button
                          type="button"
                          disabled={busyId === t.id}
                          onClick={() => void onAct(t.id, "Approve")}
                          className="h-8 rounded-md bg-brand px-2.5 text-meta font-semibold text-brand-foreground hover:bg-brand-hover"
                        >
                          Duyệt
                        </button>
                        <button
                          type="button"
                          disabled={busyId === t.id}
                          onClick={() => void onAct(t.id, "Reject")}
                          className="h-8 rounded-md border border-border px-2.5 text-meta font-medium hover:bg-muted"
                        >
                          Từ chối
                        </button>
                      </div>
                    )}
                  </td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-4 py-8 text-center text-muted-foreground">
                    Không có task chờ duyệt.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
