"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { Minus, X } from "lucide-react";
import {
  applyReactionToggle,
  editMessage,
  fetchMessages,
  markConversationRead,
  recallMessage,
  sendMessage,
  toggleReaction,
  type ChatMessageDto,
} from "@/shared/api/msg-api";
import { MsgBubble } from "@/components/msg/MsgBubble";
import { useAuthStore } from "@/shared/auth/auth-store";
import {
  joinConversation,
  leaveConversation,
  sendTypingStatus,
  subscribeMsgEdited,
  subscribeMsgReceived,
  subscribeReactionToggled,
  subscribeTyping,
} from "@/shared/realtime/msg-hub";
import type { DockChat } from "@/shared/msg/messenger-store";
import { useMessengerStore } from "@/shared/msg/messenger-store";

function upsert(prev: ChatMessageDto[], msg: ChatMessageDto) {
  const i = prev.findIndex((m) => m.id === msg.id);
  if (i < 0) return [...prev, msg];
  const next = [...prev];
  next[i] = msg;
  return next;
}

export function FloatingChatWindow({ chat }: { chat: DockChat }) {
  const myId = useAuthStore((s) => s.userId);
  const minimizeChat = useMessengerStore((s) => s.minimizeChat);
  const closeChat = useMessengerStore((s) => s.closeChat);

  const [messages, setMessages] = useState<ChatMessageDto[]>([]);
  const [draft, setDraft] = useState("");
  const [typing, setTyping] = useState<string | null>(null);
  const [sending, setSending] = useState(false);
  const [replyTo, setReplyTo] = useState<ChatMessageDto | null>(null);
  const [editing, setEditing] = useState<ChatMessageDto | null>(null);
  const bottomRef = useRef<HTMLDivElement | null>(null);
  const typingTimer = useRef<ReturnType<typeof setTimeout> | null>(null);

  const load = useCallback(async () => {
    const rows = await fetchMessages(chat.conversationId);
    setMessages(rows);
    await markConversationRead(chat.conversationId);
  }, [chat.conversationId]);

  useEffect(() => {
    void joinConversation(chat.conversationId);
    void load().catch(() => {});
    return () => {
      void leaveConversation(chat.conversationId);
    };
  }, [chat.conversationId, load]);

  useEffect(() => {
    const u1 = subscribeMsgReceived((p) => {
      if (p.conversationId !== chat.conversationId) return;
      setMessages((prev) => upsert(prev, p));
      void markConversationRead(chat.conversationId);
    });
    const u2 = subscribeMsgEdited((p) => {
      if (p.conversationId !== chat.conversationId) return;
      setMessages((prev) => upsert(prev, p));
    });
    const u3 = subscribeTyping((p) => {
      if (p.conversationId !== chat.conversationId || p.userId === myId) return;
      setTyping(p.isTyping ? p.fullName : null);
    });
    const u4 = subscribeReactionToggled((ev) => {
      if (ev.conversationId !== chat.conversationId) return;
      setMessages((prev) => applyReactionToggle(prev, ev));
    });
    return () => {
      u1();
      u2();
      u3();
      u4();
    };
  }, [chat.conversationId, myId]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, typing]);

  async function onSend() {
    const text = draft.trim();
    if ((!text && !editing) || sending) return;
    setSending(true);
    try {
      if (editing) {
        const updated = await editMessage(chat.conversationId, editing.id, text);
        setMessages((prev) => upsert(prev, updated));
        setEditing(null);
      } else {
        const msg = await sendMessage(chat.conversationId, text, {
          parentMessageId: replyTo?.id,
        });
        setMessages((prev) => upsert(prev, msg));
        setReplyTo(null);
      }
      setDraft("");
      void sendTypingStatus(chat.conversationId, false);
    } finally {
      setSending(false);
    }
  }

  return (
    <div className="flex h-[420px] w-[340px] flex-col overflow-hidden rounded-t-xl border border-border bg-surface shadow-xl">
      <header className="flex h-12 shrink-0 items-center gap-2 border-b border-border bg-brand px-3 text-brand-foreground">
        <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-white/20 text-meta font-bold">
          {chat.initials}
        </span>
        <div className="min-w-0 flex-1">
          <p className="truncate text-[13px] font-bold">{chat.title}</p>
          <p className="truncate text-[10px] text-white/80">
            {typing ? `${typing} đang nhập…` : chat.kind === "Group" ? "Nhóm" : "Chat 1-1"}
          </p>
        </div>
        <button
          type="button"
          className="rounded p-1 hover:bg-white/15"
          title="Thu nhỏ"
          onClick={() => minimizeChat(chat.conversationId)}
        >
          <Minus className="h-4 w-4" />
        </button>
        <button
          type="button"
          className="rounded p-1 hover:bg-white/15"
          title="Đóng"
          onClick={() => closeChat(chat.conversationId)}
        >
          <X className="h-4 w-4" />
        </button>
      </header>

      <div className="min-h-0 flex-1 space-y-2 overflow-x-hidden overflow-y-auto overscroll-contain bg-background px-2.5 py-2">
        {messages.map((m) => (
          <MsgBubble
            key={m.id}
            m={m}
            mine={m.senderUserId === myId}
            myId={myId}
            canAct
            compact
            onReply={() => {
              setReplyTo(m);
              setEditing(null);
            }}
            onEdit={() => {
              setEditing(m);
              setDraft(m.body);
              setReplyTo(null);
            }}
            onRecall={() => {
              void recallMessage(chat.conversationId, m.id).then((updated) =>
                setMessages((prev) => upsert(prev, updated)),
              );
            }}
            onReact={(emoji) => {
              void toggleReaction(chat.conversationId, m.id, emoji).then((ev) =>
                setMessages((prev) => applyReactionToggle(prev, ev)),
              );
            }}
          />
        ))}
        <div ref={bottomRef} />
      </div>

      {(replyTo || editing) && (
        <div className="flex min-w-0 shrink-0 items-start gap-2 border-t border-border bg-muted/40 px-2.5 py-1.5">
          <div className="min-w-0 flex-1">
            <p className="truncate text-[11px] font-semibold text-brand-strong">
              {editing
                ? "Đang sửa tin nhắn"
                : replyTo?.senderUserId === myId
                  ? "Đang trả lời chính mình"
                  : `Đang trả lời ${replyTo?.senderDisplayName ?? ""}`}
            </p>
            <p className="truncate text-[11px] text-muted-foreground">
              {editing ? editing.body : replyTo?.body}
            </p>
          </div>
          <button
            type="button"
            className="shrink-0 rounded p-0.5 text-muted-foreground hover:bg-muted hover:text-foreground"
            aria-label="Hủy"
            onClick={() => {
              setReplyTo(null);
              setEditing(null);
              if (editing) setDraft("");
            }}
          >
            <X className="h-3.5 w-3.5" />
          </button>
        </div>
      )}

      <footer className="flex min-w-0 shrink-0 items-center gap-1.5 overflow-hidden border-t border-border bg-surface p-2">
        <input
          value={draft}
          onChange={(e) => {
            setDraft(e.target.value);
            void sendTypingStatus(chat.conversationId, true);
            if (typingTimer.current) clearTimeout(typingTimer.current);
            typingTimer.current = setTimeout(() => {
              void sendTypingStatus(chat.conversationId, false);
            }, 2000);
          }}
          onKeyDown={(e) => {
            if (e.key === "Enter" && !e.shiftKey) {
              e.preventDefault();
              void onSend();
            }
          }}
          placeholder={editing ? "Sửa tin nhắn…" : "Aa"}
          className="h-9 min-w-0 flex-1 rounded-full border border-border bg-muted/50 px-3 text-[13px] outline-none focus:border-brand"
        />
        <button
          type="button"
          disabled={sending || (!draft.trim() && !editing)}
          onClick={() => void onSend()}
          className="shrink-0 rounded-full bg-brand px-3 py-1.5 text-[12px] font-semibold text-brand-foreground disabled:opacity-50"
        >
          {editing ? "Lưu" : "Gửi"}
        </button>
      </footer>
    </div>
  );
}
