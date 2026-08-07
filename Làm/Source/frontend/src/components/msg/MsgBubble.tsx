"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { MoreHorizontal, Reply, Smile } from "lucide-react";
import type { ChatMessageDto, MessageReactionDto } from "@/shared/api/msg-api";
import { cn } from "@/shared/lib/cn";

const QUICK_REACTIONS = ["👍", "❤️", "😂", "😮", "😢", "😡"] as const;

function groupReactions(reactions: MessageReactionDto[]) {
  const map = new Map<string, MessageReactionDto[]>();
  for (const r of reactions) {
    const list = map.get(r.reactionType) ?? [];
    list.push(r);
    map.set(r.reactionType, list);
  }
  return [...map.entries()];
}

export function MsgBubble({
  m,
  mine,
  myId,
  canAct,
  compact,
  onReply,
  onEdit,
  onRecall,
  onReact,
}: {
  m: ChatMessageDto;
  mine: boolean;
  myId?: string | null;
  canAct: boolean;
  compact?: boolean;
  onReply: () => void;
  onEdit: () => void;
  onRecall: () => void;
  onReact: (emoji: string) => void;
}) {
  const [hovered, setHovered] = useState(false);
  const [pickerOpen, setPickerOpen] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement | null>(null);

  const groups = useMemo(() => groupReactions(m.reactions ?? []), [m.reactions]);
  const myTypes = useMemo(
    () => new Set((m.reactions ?? []).filter((r) => r.userId === myId).map((r) => r.reactionType)),
    [m.reactions, myId],
  );

  const apiBase = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:1111";
  const showChrome = canAct && !m.recalled && (hovered || pickerOpen || menuOpen);

  useEffect(() => {
    if (!pickerOpen && !menuOpen) return;
    function onDoc(e: MouseEvent) {
      if (!rootRef.current?.contains(e.target as Node)) {
        setPickerOpen(false);
        setMenuOpen(false);
      }
    }
    document.addEventListener("mousedown", onDoc);
    return () => document.removeEventListener("mousedown", onDoc);
  }, [pickerOpen, menuOpen]);

  const toolbar = (
    <div
      className={cn(
        "relative flex shrink-0 items-center gap-0.5 self-center rounded-full border border-border bg-surface px-0.5 py-0.5 shadow-sm transition-opacity",
        showChrome ? "opacity-100" : "pointer-events-none opacity-0",
      )}
      aria-hidden={!showChrome}
    >
      {menuOpen && mine && (
        <div className="absolute bottom-full left-1/2 z-20 mb-1 min-w-[112px] -translate-x-1/2 overflow-hidden rounded-lg border border-border bg-surface py-1 text-[12px] shadow-lg">
          <button
            type="button"
            className="block w-full px-3 py-1.5 text-left hover:bg-muted"
            onClick={() => {
              setMenuOpen(false);
              onEdit();
            }}
          >
            Sửa
          </button>
          <button
            type="button"
            className="block w-full px-3 py-1.5 text-left text-destructive hover:bg-muted"
            onClick={() => {
              setMenuOpen(false);
              onRecall();
            }}
          >
            Thu hồi
          </button>
        </div>
      )}
      {mine && (
        <button
          type="button"
          title="Thêm"
          tabIndex={showChrome ? 0 : -1}
          className="rounded-full p-1 text-muted-foreground hover:bg-muted hover:text-foreground"
          onClick={() => {
            setMenuOpen((o) => !o);
            setPickerOpen(false);
          }}
        >
          <MoreHorizontal className="h-3.5 w-3.5" />
        </button>
      )}
      <button
        type="button"
        title="Trả lời"
        tabIndex={showChrome ? 0 : -1}
        className="rounded-full p-1 text-muted-foreground hover:bg-muted hover:text-foreground"
        onClick={() => {
          setPickerOpen(false);
          setMenuOpen(false);
          onReply();
        }}
      >
        <Reply className="h-3.5 w-3.5" />
      </button>
      <button
        type="button"
        title="Cảm xúc"
        tabIndex={showChrome ? 0 : -1}
        className="rounded-full p-1 text-muted-foreground hover:bg-muted hover:text-foreground"
        onClick={() => {
          setPickerOpen((o) => !o);
          setMenuOpen(false);
        }}
      >
        <Smile className="h-3.5 w-3.5" />
      </button>
    </div>
  );

  return (
    <div
      ref={rootRef}
      className={cn(
        "relative flex w-full min-w-0 items-end gap-1",
        mine ? "justify-end" : "justify-start",
        (groups.length > 0 || pickerOpen) && "pb-3",
        pickerOpen && "pt-10",
      )}
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => {
        if (!pickerOpen && !menuOpen) setHovered(false);
      }}
    >
      {/* Picker neo theo cả hàng tin — luôn nằm trong ô chat */}
      {pickerOpen && (
        <div
          className={cn(
            "absolute top-0 z-20 flex max-w-full flex-nowrap gap-0.5 overflow-hidden rounded-full border border-border bg-surface p-1 shadow-lg",
            mine ? "right-0" : "left-8",
          )}
        >
          {QUICK_REACTIONS.map((emoji) => (
            <button
              key={emoji}
              type="button"
              className="shrink-0 rounded-full px-1 py-0.5 text-[14px] leading-none hover:bg-muted"
              onClick={() => {
                setPickerOpen(false);
                onReact(emoji);
              }}
            >
              {emoji}
            </button>
          ))}
        </div>
      )}

      {!mine && (
        <span
          className={cn(
            "mb-0.5 flex shrink-0 items-center justify-center rounded-full bg-muted font-bold text-muted-foreground",
            compact ? "h-6 w-6 text-[9px]" : "h-7 w-7 text-[10px]",
          )}
        >
          {m.senderDisplayName.slice(0, 1).toUpperCase()}
        </span>
      )}

      {mine && canAct && !m.recalled && toolbar}

      <div
        className={cn(
          "relative min-w-0",
          compact ? "max-w-[calc(100%-5.75rem)] text-[12px]" : "max-w-[calc(100%-6.5rem)] text-body",
        )}
      >
        <div
          className={cn(
            "rounded-2xl",
            compact ? "px-2.5 py-1.5 leading-snug" : "px-3 py-2",
            mine ? "bg-brand text-brand-foreground" : "bg-muted text-foreground",
            m.recalled && "italic opacity-70",
          )}
        >
          {!mine && !compact && (
            <div className="mb-0.5 text-meta font-semibold opacity-80">{m.senderDisplayName}</div>
          )}
          {m.parentPreview && (
            <div
              className={cn(
                "mb-1 truncate border-l-2 pl-2 text-meta",
                mine ? "border-white/50 text-white/80" : "border-brand/40 text-muted-foreground",
              )}
            >
              {m.parentPreview}
            </div>
          )}
          {m.recalled ? (
            <em className="opacity-80">Tin nhắn đã thu hồi</em>
          ) : (
            <>
              <p className="whitespace-pre-wrap break-words">{m.body}</p>
              {m.attachmentStorageKey && (
                <a
                  className="mt-1 block truncate text-meta underline opacity-90"
                  href={`${apiBase}/api/sys/files/${encodeURIComponent(m.attachmentStorageKey)}`}
                  target="_blank"
                  rel="noreferrer"
                >
                  Tải đính kèm
                </a>
              )}
              {m.isEdited && <span className="text-meta opacity-70"> (đã sửa)</span>}
            </>
          )}
        </div>

        {groups.length > 0 && (
          <div
            className={cn(
              "absolute -bottom-2.5 flex max-w-full flex-wrap gap-0.5",
              mine ? "right-1 justify-end" : "left-1 justify-start",
            )}
          >
            {groups.map(([emoji, list]) => (
              <button
                key={emoji}
                type="button"
                disabled={!canAct || m.recalled}
                title={list.map((r) => r.displayName).join(", ")}
                onClick={() => onReact(emoji)}
                className={cn(
                  "inline-flex items-center gap-0.5 rounded-full border bg-surface px-1.5 py-0.5 text-[11px] shadow-sm",
                  myTypes.has(emoji)
                    ? "border-brand text-brand-strong"
                    : "border-border text-foreground hover:bg-muted",
                  (!canAct || m.recalled) && "opacity-70",
                )}
              >
                <span className="leading-none">{emoji}</span>
                {list.length > 1 && <span className="font-semibold leading-none">{list.length}</span>}
              </button>
            ))}
          </div>
        )}
      </div>

      {!mine && canAct && !m.recalled && toolbar}
    </div>
  );
}
