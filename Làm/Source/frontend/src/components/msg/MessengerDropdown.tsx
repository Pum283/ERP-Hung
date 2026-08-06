"use client";

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { Expand, PenSquare, Search, X } from "lucide-react";
import {
  createDirectConversation,
  fetchConversations,
  fetchMsgDirectory,
  type ConversationDto,
  type MsgDirectoryUserDto,
} from "@/shared/api/msg-api";
import { chatInitials, useMessengerStore } from "@/shared/msg/messenger-store";
import { cn } from "@/shared/lib/cn";

function titleOf(c: ConversationDto) {
  return c.kind === "Direct" ? c.peerDisplayName ?? "Chat 1-1" : c.title ?? "Nhóm";
}

function timeAgo(iso?: string | null) {
  if (!iso) return "";
  const ms = Date.now() - new Date(iso).getTime();
  const m = Math.floor(ms / 60000);
  if (m < 1) return "vừa xong";
  if (m < 60) return `${m} phút`;
  const h = Math.floor(m / 60);
  if (h < 24) return `${h} giờ`;
  const d = Math.floor(h / 24);
  return `${d} ngày`;
}

type Filter = "all" | "unread" | "group";

export function MessengerDropdown({ onClose }: { onClose: () => void }) {
  const openChat = useMessengerStore((s) => s.openChat);
  const [convs, setConvs] = useState<ConversationDto[]>([]);
  const [q, setQ] = useState("");
  const [filter, setFilter] = useState<Filter>("all");
  const [compose, setCompose] = useState(false);
  const [directory, setDirectory] = useState<MsgDirectoryUserDto[]>([]);
  const [peerQ, setPeerQ] = useState("");

  useEffect(() => {
    void fetchConversations().then(setConvs).catch(() => setConvs([]));
  }, []);

  const rows = useMemo(() => {
    let list = convs;
    if (filter === "unread") list = list.filter((c) => c.unreadCount > 0);
    if (filter === "group") list = list.filter((c) => c.kind === "Group");
    const s = q.trim().toLowerCase();
    if (s) {
      list = list.filter((c) =>
        `${titleOf(c)} ${c.lastMessagePreview ?? ""}`.toLowerCase().includes(s),
      );
    }
    return list;
  }, [convs, filter, q]);

  function open(c: ConversationDto) {
    openChat({
      conversationId: c.id,
      title: titleOf(c),
      kind: c.kind,
      peerUserId: c.peerUserId,
      initials: chatInitials(titleOf(c)),
    });
    onClose();
  }

  async function startCompose() {
    setCompose(true);
    setDirectory(await fetchMsgDirectory().catch(() => []));
  }

  return (
    <div className="absolute right-0 top-full z-50 mt-2 flex max-h-[min(72vh,560px)] w-[380px] flex-col overflow-hidden rounded-xl border border-border bg-surface shadow-2xl">
      <div className="flex items-center justify-between gap-2 px-4 pt-3 pb-2">
        <h2 className="text-[17px] font-bold text-foreground">Đoạn chat</h2>
        <div className="flex items-center gap-1">
          <button
            type="button"
            className="rounded-full p-2 text-muted-foreground hover:bg-muted"
            title="Tin nhắn mới"
            onClick={() => void startCompose()}
          >
            <PenSquare className="h-4 w-4" />
          </button>
          <Link
            href="/app/sys/messages"
            className="rounded-full p-2 text-muted-foreground hover:bg-muted"
            title="Mở trang đầy đủ"
            onClick={onClose}
          >
            <Expand className="h-4 w-4" />
          </Link>
          <button
            type="button"
            className="rounded-full p-2 text-muted-foreground hover:bg-muted"
            onClick={onClose}
          >
            <X className="h-4 w-4" />
          </button>
        </div>
      </div>

      {compose ? (
        <div className="flex min-h-0 flex-1 flex-col px-3 pb-3">
          <button
            type="button"
            className="mb-2 text-left text-meta font-semibold text-brand"
            onClick={() => setCompose(false)}
          >
            ← Quay lại danh sách
          </button>
          <input
            value={peerQ}
            onChange={(e) => setPeerQ(e.target.value)}
            placeholder="Tìm người để chat…"
            className="mb-2 h-9 rounded-full border-0 bg-muted px-3 text-[13px] outline-none"
          />
          <ul className="min-h-0 flex-1 space-y-0.5 overflow-y-auto">
            {directory
              .filter((u) => {
                const s = peerQ.trim().toLowerCase();
                if (!s) return true;
                return `${u.displayName} ${u.username}`.toLowerCase().includes(s);
              })
              .map((u) => (
                <li key={u.id}>
                  <button
                    type="button"
                    className="flex w-full items-center gap-3 rounded-lg px-2 py-2 hover:bg-muted"
                    onClick={() => {
                      void createDirectConversation(u.id).then((c) => open(c));
                    }}
                  >
                    <span className="flex h-10 w-10 items-center justify-center rounded-full bg-brand-muted text-meta font-bold text-brand-strong">
                      {chatInitials(u.displayName)}
                    </span>
                    <span className="text-left">
                      <span className="block text-[13px] font-semibold">{u.displayName}</span>
                      <span className="block text-meta text-muted-foreground">@{u.username}</span>
                    </span>
                  </button>
                </li>
              ))}
          </ul>
        </div>
      ) : (
        <>
          <div className="px-3 pb-2">
            <div className="flex h-9 items-center gap-2 rounded-full bg-muted px-3">
              <Search className="h-3.5 w-3.5 text-muted-foreground" />
              <input
                value={q}
                onChange={(e) => setQ(e.target.value)}
                placeholder="Tìm kiếm trên Messenger"
                className="w-full border-0 bg-transparent text-[13px] outline-none"
              />
            </div>
            <div className="mt-2 flex flex-wrap gap-1.5">
              {(
                [
                  ["all", "Tất cả"],
                  ["unread", "Chưa đọc"],
                  ["group", "Nhóm"],
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
          </div>

          <ul className="min-h-0 flex-1 overflow-y-auto px-1.5 pb-1">
            {rows.map((c) => {
              const title = titleOf(c);
              const unread = c.unreadCount > 0;
              return (
                <li key={c.id}>
                  <button
                    type="button"
                    onClick={() => open(c)}
                    className="flex w-full items-center gap-3 rounded-lg px-2 py-2 text-left hover:bg-muted"
                  >
                    <span className="relative flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-brand-muted text-[13px] font-bold text-brand-strong">
                      {chatInitials(title)}
                      {c.kind === "Group" && (
                        <span className="absolute -bottom-0.5 -right-0.5 flex h-4 w-4 items-center justify-center rounded-full border-2 border-surface bg-muted text-[8px]">
                          G
                        </span>
                      )}
                    </span>
                    <span className="min-w-0 flex-1">
                      <span
                        className={cn(
                          "block truncate text-[13px]",
                          unread ? "font-bold text-foreground" : "font-semibold text-foreground",
                        )}
                      >
                        {title}
                      </span>
                      <span
                        className={cn(
                          "block truncate text-[12px]",
                          unread ? "font-semibold text-foreground" : "text-muted-foreground",
                        )}
                      >
                        {c.lastMessagePreview || "—"}
                        {c.lastMessageAt ? ` · ${timeAgo(c.lastMessageAt)}` : ""}
                      </span>
                    </span>
                    {unread && (
                      <span className="h-2.5 w-2.5 shrink-0 rounded-full bg-brand" />
                    )}
                  </button>
                </li>
              );
            })}
            {rows.length === 0 && (
              <p className="px-3 py-8 text-center text-[13px] text-muted-foreground">
                Không có đoạn chat.
              </p>
            )}
          </ul>

          <Link
            href="/app/sys/messages"
            onClick={onClose}
            className="block border-t border-border py-3 text-center text-[13px] font-semibold text-brand hover:underline"
          >
            Xem tất cả trong Messenger
          </Link>
        </>
      )}
    </div>
  );
}
