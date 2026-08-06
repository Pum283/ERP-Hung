"use client";

import { PenSquare } from "lucide-react";
import { FloatingChatWindow } from "@/components/msg/FloatingChatWindow";
import { useMessengerStore } from "@/shared/msg/messenger-store";
import { cn } from "@/shared/lib/cn";

/** Cửa sổ chat nổi + cột avatar mini (chat heads) — kiểu Facebook web. */
export function MessengerDock() {
  const chats = useMessengerStore((s) => s.chats);
  const toggleMinimize = useMessengerStore((s) => s.toggleMinimize);
  const setDropdownOpen = useMessengerStore((s) => s.setDropdownOpen);

  const windows = chats.filter((c) => !c.minimized).slice(0, 3);
  const heads = chats;

  if (heads.length === 0) return null;

  return (
    <div className="pointer-events-none fixed inset-x-0 bottom-0 z-[80] flex items-end justify-end gap-3 px-3 pb-0">
      {/* Cửa sổ chat nổi (trái ← phải, sát chat heads) */}
      <div className="pointer-events-auto flex items-end gap-2">
        {[...windows].reverse().map((c) => (
          <FloatingChatWindow key={c.conversationId} chat={c} />
        ))}
      </div>

      {/* Cột icon mini từng người */}
      <div className="pointer-events-auto mb-3 flex flex-col-reverse items-center gap-2">
        <button
          type="button"
          title="Tin nhắn mới"
          onClick={() => setDropdownOpen(true)}
          className="flex h-12 w-12 items-center justify-center rounded-full border border-border bg-surface text-foreground shadow-lg transition hover:scale-105 hover:bg-muted"
        >
          <PenSquare className="h-5 w-5" />
        </button>
        {heads.map((c) => (
          <button
            key={c.conversationId}
            type="button"
            title={c.title}
            onClick={() => toggleMinimize(c.conversationId)}
            className={cn(
              "relative flex h-12 w-12 items-center justify-center rounded-full text-[12px] font-bold shadow-lg transition hover:scale-105",
              c.minimized
                ? "bg-brand-muted text-brand-strong ring-2 ring-border"
                : "bg-brand text-brand-foreground ring-2 ring-brand",
            )}
          >
            {c.initials}
            {c.kind === "Group" && (
              <span className="absolute -bottom-0.5 -right-0.5 flex h-4 w-4 items-center justify-center rounded-full bg-muted text-[8px] text-foreground ring-2 ring-surface">
                G
              </span>
            )}
          </button>
        ))}
      </div>
    </div>
  );
}
