"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { CheckCheck, X } from "lucide-react";
import {
  fetchNotifications,
  markAllNotificationsRead,
  markNotificationRead,
  type AppNotificationDto,
} from "@/shared/api/notify-api";
import { cn } from "@/shared/lib/cn";

function timeAgo(iso: string) {
  const ms = Date.now() - new Date(iso).getTime();
  const m = Math.floor(ms / 60000);
  if (m < 1) return "vừa xong";
  if (m < 60) return `${m} phút`;
  const h = Math.floor(m / 60);
  if (h < 24) return `${h} giờ`;
  return `${Math.floor(h / 24)} ngày`;
}

type Filter = "all" | "unread";

export function NotificationDropdown({
  onClose,
  onUnreadChange,
}: {
  onClose: () => void;
  onUnreadChange?: (count: number) => void;
}) {
  const router = useRouter();
  const [rows, setRows] = useState<AppNotificationDto[]>([]);
  const [filter, setFilter] = useState<Filter>("all");
  const [loading, setLoading] = useState(true);

  async function load() {
    setLoading(true);
    try {
      const list = await fetchNotifications();
      setRows(list);
      onUnreadChange?.(list.filter((n) => !n.isRead).length);
    } catch {
      setRows([]);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const visible = useMemo(() => {
    if (filter === "unread") return rows.filter((n) => !n.isRead);
    return rows;
  }, [rows, filter]);

  async function openItem(n: AppNotificationDto) {
    if (!n.isRead) {
      await markNotificationRead(n.id).catch(() => {});
      setRows((prev) => prev.map((x) => (x.id === n.id ? { ...x, isRead: true } : x)));
      onUnreadChange?.(Math.max(0, rows.filter((x) => !x.isRead && x.id !== n.id).length));
    }
    onClose();
    if (n.link) router.push(n.link);
  }

  return (
    <div className="absolute right-0 top-full z-50 mt-2 flex max-h-[min(72vh,520px)] w-[360px] flex-col overflow-hidden rounded-xl border border-border bg-surface shadow-2xl">
      <div className="flex items-center justify-between gap-2 px-4 pt-3 pb-2">
        <h2 className="text-[17px] font-bold text-foreground">Thông báo</h2>
        <div className="flex items-center gap-1">
          <button
            type="button"
            className="rounded-full p-2 text-muted-foreground hover:bg-muted"
            title="Đánh dấu tất cả đã đọc"
            onClick={() => {
              void markAllNotificationsRead().then(() => {
                setRows((prev) => prev.map((n) => ({ ...n, isRead: true })));
                onUnreadChange?.(0);
              });
            }}
          >
            <CheckCheck className="h-4 w-4" />
          </button>
          <button
            type="button"
            className="rounded-full p-2 text-muted-foreground hover:bg-muted"
            onClick={onClose}
          >
            <X className="h-4 w-4" />
          </button>
        </div>
      </div>

      <div className="flex gap-1.5 px-3 pb-2">
        {(
          [
            ["all", "Tất cả"],
            ["unread", "Chưa đọc"],
          ] as const
        ).map(([id, label]) => (
          <button
            key={id}
            type="button"
            onClick={() => setFilter(id)}
            className={cn(
              "rounded-full px-3 py-1 text-[12px] font-semibold transition",
              filter === id
                ? "bg-brand text-brand-foreground"
                : "bg-muted text-muted-foreground hover:bg-muted/80",
            )}
          >
            {label}
          </button>
        ))}
      </div>

      <ul className="min-h-0 flex-1 overflow-y-auto px-1.5 pb-2">
        {loading ? (
          <p className="px-3 py-8 text-center text-[13px] text-muted-foreground">Đang tải…</p>
        ) : visible.length === 0 ? (
          <p className="px-3 py-8 text-center text-[13px] text-muted-foreground">
            Không có thông báo.
          </p>
        ) : (
          visible.map((n) => (
            <li key={n.id}>
              <button
                type="button"
                onClick={() => void openItem(n)}
                className={cn(
                  "flex w-full gap-3 rounded-lg px-2 py-2.5 text-left hover:bg-muted",
                  !n.isRead && "bg-brand-muted/30",
                )}
              >
                <span className="mt-0.5 flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-brand-muted text-meta font-bold text-brand-strong">
                  {(n.eventType ?? "TB").slice(0, 2).toUpperCase()}
                </span>
                <span className="min-w-0 flex-1">
                  <span
                    className={cn(
                      "block text-[13px] text-foreground",
                      !n.isRead ? "font-bold" : "font-semibold",
                    )}
                  >
                    {n.title}
                  </span>
                  <span className="mt-0.5 line-clamp-2 block text-[12px] text-muted-foreground">
                    {n.body}
                  </span>
                  <span className="mt-1 block text-[11px] font-medium text-brand">
                    {timeAgo(n.createdAt)}
                  </span>
                </span>
                {!n.isRead && <span className="mt-2 h-2.5 w-2.5 shrink-0 rounded-full bg-brand" />}
              </button>
            </li>
          ))
        )}
      </ul>
    </div>
  );
}
