"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { BellOff, Paperclip, Search } from "lucide-react";
import {
  addMembers,
  createDirectConversation,
  createGroupConversation,
  applyReactionToggle,
  editMessage,
  fetchConversations,
  fetchMembers,
  fetchMessages,
  fetchMsgDirectory,
  markConversationRead,
  muteConversation,
  recallMessage,
  removeMember,
  sendMessage,
  toggleReaction,
  uploadMsgFile,
  type ChatMessageDto,
  type ConversationDto,
  type ConversationMemberDto,
  type MsgDirectoryUserDto,
} from "@/shared/api/msg-api";
import {
  searchMessages,
  setConversationMute,
  type SysMessageSearchHitDto,
} from "@/shared/api/sys-api";
import {
  highlightSearchSnippet,
  validateMessageSearchQuery,
  validateMuteUntil,
} from "@/shared/api/sys-step155-helpers";
import { MsgBubble } from "@/components/msg/MsgBubble";
import { useAuthStore } from "@/shared/auth/auth-store";
import { usePermissions } from "@/shared/hooks/use-permissions";
import {
  joinConversation,
  leaveConversation,
  sendTypingStatus,
  subscribeConversationUpdated,
  subscribeMsgEdited,
  subscribeMsgReceived,
  subscribeReactionToggled,
  subscribeTyping,
} from "@/shared/realtime/msg-hub";
import { btn } from "@/shared/ui/btn";
import { SideSheet } from "@/shared/ui/SideSheet";

function convTitle(c: ConversationDto) {
  return c.kind === "Direct" ? c.peerDisplayName ?? "Chat 1-1" : c.title ?? "Nhóm";
}

function upsertMessage(prev: ChatMessageDto[], msg: ChatMessageDto) {
  const i = prev.findIndex((m) => m.id === msg.id);
  if (i < 0) return [...prev, msg];
  const next = [...prev];
  next[i] = msg;
  return next;
}

export default function MessagesPage() {
  const { can } = usePermissions();
  const canRead = can("sys.msg.read");
  const canSend = can("sys.msg.send");
  const myId = useAuthStore((s) => s.userId);

  const [convs, setConvs] = useState<ConversationDto[]>([]);
  const [filter, setFilter] = useState("");
  const [msgSearchQ, setMsgSearchQ] = useState("");
  const [msgHits, setMsgHits] = useState<SysMessageSearchHitDto[]>([]);
  const [activeId, setActiveId] = useState<string | null>(null);
  const [messages, setMessages] = useState<ChatMessageDto[]>([]);
  const [draft, setDraft] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);
  const [typingName, setTypingName] = useState<string | null>(null);
  const [replyTo, setReplyTo] = useState<ChatMessageDto | null>(null);
  const [editing, setEditing] = useState<ChatMessageDto | null>(null);

  const [newOpen, setNewOpen] = useState(false);
  const [directory, setDirectory] = useState<MsgDirectoryUserDto[]>([]);
  const [peerQ, setPeerQ] = useState("");
  const [groupTitle, setGroupTitle] = useState("");
  const [groupMembers, setGroupMembers] = useState<string[]>([]);

  const [membersOpen, setMembersOpen] = useState(false);
  const [members, setMembers] = useState<ConversationMemberDto[]>([]);
  const [addMemberIds, setAddMemberIds] = useState<string[]>([]);

  const typingTimer = useRef<ReturnType<typeof setTimeout> | null>(null);
  const bottomRef = useRef<HTMLDivElement | null>(null);
  const fileRef = useRef<HTMLInputElement | null>(null);
  const activeIdRef = useRef<string | null>(null);

  useEffect(() => {
    activeIdRef.current = activeId;
  }, [activeId]);

  const loadConvs = useCallback(async () => {
    try {
      setConvs(await fetchConversations());
    } catch {
      setError("Không tải được danh sách hội thoại.");
    }
  }, []);

  const openConv = useCallback(
    async (id: string) => {
      const prev = activeIdRef.current;
      if (prev && prev !== id) void leaveConversation(prev);
      setActiveId(id);
      setReplyTo(null);
      setEditing(null);
      setTypingName(null);
      setError(null);
      try {
        await joinConversation(id);
        const rows = await fetchMessages(id);
        setMessages(rows);
        await markConversationRead(id);
        await loadConvs();
      } catch {
        setError("Không tải được tin nhắn.");
      }
    },
    [loadConvs],
  );

  useEffect(() => {
    if (!canRead) return;
    setLoading(true);
    void loadConvs().finally(() => setLoading(false));
  }, [canRead, loadConvs]);

  useEffect(() => {
    if (!canRead) return;
    const unsubMsg = subscribeMsgReceived((payload) => {
      if (payload.conversationId === activeIdRef.current) {
        setMessages((prev) => upsertMessage(prev, payload));
        void markConversationRead(payload.conversationId);
      }
      void loadConvs();
    });
    const unsubEdit = subscribeMsgEdited((payload) => {
      if (payload.conversationId === activeIdRef.current) {
        setMessages((prev) => upsertMessage(prev, payload));
      }
      void loadConvs();
    });
    const unsubConv = subscribeConversationUpdated(() => {
      void loadConvs();
    });
    const unsubTyping = subscribeTyping((p) => {
      if (p.conversationId !== activeIdRef.current || p.userId === myId) return;
      setTypingName(p.isTyping ? p.fullName : null);
      if (p.isTyping) {
        window.setTimeout(() => setTypingName((n) => (n === p.fullName ? null : n)), 3000);
      }
    });
    const unsubReact = subscribeReactionToggled((ev) => {
      if (ev.conversationId === activeIdRef.current) {
        setMessages((prev) => applyReactionToggle(prev, ev));
      }
    });
    return () => {
      unsubMsg();
      unsubEdit();
      unsubConv();
      unsubTyping();
      unsubReact();
    };
  }, [canRead, loadConvs, myId]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, typingName]);

  const active = useMemo(() => convs.find((c) => c.id === activeId) ?? null, [convs, activeId]);
  const filteredConvs = useMemo(() => {
    const q = filter.trim().toLowerCase();
    if (!q) return convs;
    return convs.filter((c) => {
      const t = `${convTitle(c)} ${c.lastMessagePreview ?? ""}`.toLowerCase();
      return t.includes(q);
    });
  }, [convs, filter]);

  function onDraftChange(value: string) {
    setDraft(value);
    if (!activeId || !canSend) return;
    void sendTypingStatus(activeId, true);
    if (typingTimer.current) clearTimeout(typingTimer.current);
    typingTimer.current = setTimeout(() => {
      if (activeId) void sendTypingStatus(activeId, false);
    }, 2000);
  }

  async function onSend() {
    if (!activeId || !canSend || sending) return;
    const text = draft.trim();
    if (!text && !editing) return;
    setSending(true);
    setError(null);
    try {
      if (editing) {
        const updated = await editMessage(activeId, editing.id, text);
        setMessages((prev) => upsertMessage(prev, updated));
        setEditing(null);
      } else {
        const msg = await sendMessage(activeId, text, {
          parentMessageId: replyTo?.id,
        });
        setMessages((prev) => upsertMessage(prev, msg));
        setReplyTo(null);
      }
      setDraft("");
      void sendTypingStatus(activeId, false);
      await loadConvs();
    } catch {
      setError("Gửi / sửa tin thất bại.");
    } finally {
      setSending(false);
    }
  }

  async function onAttach(file: File) {
    if (!activeId || !canSend) return;
    setSending(true);
    try {
      const saved = await uploadMsgFile(file);
      const msg = await sendMessage(activeId, draft.trim() || file.name, {
        attachmentStorageKey: saved.storageKey,
        parentMessageId: replyTo?.id,
      });
      setMessages((prev) => upsertMessage(prev, msg));
      setDraft("");
      setReplyTo(null);
      await loadConvs();
    } catch {
      setError("Đính kèm thất bại (cần quyền upload file).");
    } finally {
      setSending(false);
    }
  }

  async function openNew() {
    setPeerQ("");
    setGroupTitle("");
    setGroupMembers([]);
    setDirectory(await fetchMsgDirectory());
    setNewOpen(true);
  }

  async function openMembers() {
    if (!activeId || active?.kind !== "Group") return;
    setMembers(await fetchMembers(activeId));
    setDirectory(await fetchMsgDirectory());
    setAddMemberIds([]);
    setMembersOpen(true);
  }

  if (!canRead) {
    return <p className="text-body text-destructive">Không có quyền sys.msg.read</p>;
  }

  return (
    <div className="flex h-[calc(100vh-7rem)] min-h-[480px] overflow-hidden rounded-xl border border-border bg-surface shadow-sm">
      {/* Sidebar — Digi style */}
      <aside className="flex w-80 shrink-0 flex-col border-r border-border">
        <div className="space-y-2 border-b border-border p-3">
          <div className="flex items-center justify-between gap-2">
            <h1 className="font-display text-title font-bold">Tin nhắn</h1>
            {canSend && (
              <button type="button" className={btn.soft} onClick={() => void openNew()}>
                Mới
              </button>
            )}
          </div>
          <div className="flex h-9 items-center gap-2 rounded-md border border-border bg-background px-2">
            <Search className="h-3.5 w-3.5 text-muted-foreground" />
            <input
              value={filter}
              onChange={(e) => setFilter(e.target.value)}
              placeholder="Tìm hội thoại…"
              className="w-full border-0 bg-transparent text-body outline-none"
            />
          </div>
          <div className="flex gap-1">
            <input
              value={msgSearchQ}
              onChange={(e) => setMsgSearchQ(e.target.value)}
              placeholder="Tìm nội dung tin (UC_103)…"
              className="h-8 flex-1 rounded-md border border-border bg-background px-2 text-meta outline-none"
              onKeyDown={(e) => {
                if (e.key !== "Enter") return;
                const v = validateMessageSearchQuery(msgSearchQ);
                if (!v.isValid) {
                  setError(v.error ?? "Query không hợp lệ.");
                  return;
                }
                void searchMessages(msgSearchQ.trim())
                  .then(setMsgHits)
                  .catch((err) => setError((err as Error).message));
              }}
            />
            <button
              type="button"
              className={btn.soft}
              onClick={() => {
                const v = validateMessageSearchQuery(msgSearchQ);
                if (!v.isValid) {
                  setError(v.error ?? "Query không hợp lệ.");
                  return;
                }
                void searchMessages(msgSearchQ.trim())
                  .then(setMsgHits)
                  .catch((err) => setError((err as Error).message));
              }}
            >
              Tìm
            </button>
          </div>
          {msgHits.length > 0 && (
            <div className="max-h-36 overflow-y-auto rounded-md border border-border text-meta">
              {msgHits.map((h) => (
                <button
                  key={h.messageId}
                  type="button"
                  className="block w-full border-b border-border/50 px-2 py-1.5 text-left hover:bg-muted"
                  onClick={() => void openConv(h.conversationId)}
                >
                  <span className="font-medium">{h.conversationTitle || "Hội thoại"}</span>
                  <span className="block truncate text-muted-foreground">
                    {highlightSearchSnippet(h.bodyPreview, msgSearchQ)}
                  </span>
                </button>
              ))}
            </div>
          )}
        </div>
        <div className="flex-1 overflow-y-auto">
          {loading ? (
            <p className="p-3 text-muted-foreground">Đang tải…</p>
          ) : filteredConvs.length === 0 ? (
            <p className="p-3 text-muted-foreground">Chưa có hội thoại.</p>
          ) : (
            filteredConvs.map((c) => (
              <button
                key={c.id}
                type="button"
                onClick={() => void openConv(c.id)}
                className={`flex w-full flex-col gap-0.5 border-b border-border/60 px-3 py-2.5 text-left hover:bg-muted/60 ${
                  activeId === c.id ? "bg-brand-muted/50" : ""
                }`}
              >
                <div className="flex items-center gap-2">
                  <span className="truncate font-semibold">{convTitle(c)}</span>
                  {c.muted && <BellOff className="h-3 w-3 shrink-0 text-muted-foreground" />}
                  {c.unreadCount > 0 && (
                    <span className="ml-auto rounded-full bg-brand px-1.5 text-meta font-bold text-brand-foreground">
                      {c.unreadCount}
                    </span>
                  )}
                </div>
                <span className="truncate text-meta text-muted-foreground">
                  {c.lastMessagePreview || "—"}
                </span>
              </button>
            ))
          )}
        </div>
      </aside>

      {/* Main */}
      <section className="flex min-w-0 flex-1 flex-col">
        {!active ? (
          <div className="flex flex-1 items-center justify-center text-muted-foreground">
            Chọn cuộc hội thoại
          </div>
        ) : (
          <>
            <header className="flex flex-wrap items-center gap-2 border-b border-border px-4 py-2.5">
              <div className="min-w-0 flex-1">
                <h2 className="truncate font-semibold">{convTitle(active)}</h2>
                <p className="text-meta text-muted-foreground">
                  {active.kind === "Group" ? "Nhóm" : "Chat 1-1"}
                  {typingName ? ` · ${typingName} đang nhập…` : ""}
                </p>
              </div>
              <button
                type="button"
                className={btn.secondary}
                onClick={() => {
                  void muteConversation(active.id, !active.muted).then(loadConvs);
                }}
              >
                {active.muted ? "Bỏ tắt tiếng" : "Tắt tiếng"}
              </button>
              <button
                type="button"
                className={btn.soft}
                title="Mute 1 giờ (UC_104)"
                onClick={() => {
                  const until = new Date(Date.now() + 60 * 60 * 1000).toISOString();
                  const v = validateMuteUntil(true, until);
                  if (!v.isValid) {
                    setError(v.error ?? "MuteUntil không hợp lệ.");
                    return;
                  }
                  void setConversationMute(active.id, { muted: true, muteUntil: until }).then(loadConvs);
                }}
              >
                Mute 1h
              </button>
              {active.kind === "Group" && (
                <button type="button" className={btn.soft} onClick={() => void openMembers()}>
                  Thành viên
                </button>
              )}
            </header>

            <div className="flex-1 space-y-2 overflow-y-auto px-4 py-3">
              {messages.map((m) => (
                <MsgBubble
                  key={m.id}
                  m={m}
                  mine={m.senderUserId === myId}
                  myId={myId}
                  canAct={canSend}
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
                    void recallMessage(active.id, m.id).then((updated) =>
                      setMessages((prev) => upsertMessage(prev, updated)),
                    );
                  }}
                  onReact={(emoji) => {
                    void toggleReaction(active.id, m.id, emoji).then((ev) =>
                      setMessages((prev) => applyReactionToggle(prev, ev)),
                    );
                  }}
                />
              ))}
              <div ref={bottomRef} />
            </div>

            {(replyTo || editing) && (
              <div className="flex items-center gap-2 border-t border-border bg-muted/40 px-4 py-2 text-meta">
                <span className="flex-1 truncate">
                  {editing ? `Sửa: ${editing.body}` : `Trả lời: ${replyTo?.body}`}
                </span>
                <button
                  type="button"
                  className={btn.ghost}
                  onClick={() => {
                    setReplyTo(null);
                    setEditing(null);
                    if (editing) setDraft("");
                  }}
                >
                  Hủy
                </button>
              </div>
            )}

            {error && <p className="px-4 text-meta text-destructive">{error}</p>}

            {canSend && (
              <footer className="flex items-end gap-2 border-t border-border p-3">
                <input
                  ref={fileRef}
                  type="file"
                  className="hidden"
                  onChange={(e) => {
                    const f = e.target.files?.[0];
                    if (f) void onAttach(f);
                    e.target.value = "";
                  }}
                />
                <button type="button" className={btn.secondary} title="Đính kèm" onClick={() => fileRef.current?.click()}>
                  <Paperclip className="h-4 w-4" />
                </button>
                <textarea
                  rows={2}
                  value={draft}
                  onChange={(e) => onDraftChange(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" && !e.shiftKey) {
                      e.preventDefault();
                      void onSend();
                    }
                  }}
                  placeholder="Nhập tin nhắn… (Enter gửi · Shift+Enter xuống dòng)"
                  className="min-h-[2.5rem] flex-1 resize-none rounded-md border border-border bg-background px-3 py-2 text-body outline-none focus:border-brand"
                />
                <button type="button" disabled={sending} className={btn.primary} onClick={() => void onSend()}>
                  {editing ? "Lưu" : "Gửi"}
                </button>
              </footer>
            )}
          </>
        )}
      </section>

      <SideSheet
        open={newOpen}
        onOpenChange={(o) => !o && setNewOpen(false)}
        title="Tạo hội thoại"
        description="Chat 1-1 hoặc nhóm — như Digi"
        widthClassName="max-w-md"
      >
        <div className="space-y-4">
          <div>
            <h3 className="mb-2 text-body font-semibold">Chat 1-1</h3>
            <input
              value={peerQ}
              onChange={(e) => setPeerQ(e.target.value)}
              placeholder="Lọc người dùng…"
              className="mb-2 h-9 w-full rounded-md border px-2"
            />
            <ul className="max-h-40 space-y-1 overflow-y-auto">
              {directory
                .filter((u) => {
                  const q = peerQ.trim().toLowerCase();
                  if (!q) return true;
                  return `${u.displayName} ${u.username}`.toLowerCase().includes(q);
                })
                .map((u) => (
                  <li key={u.id}>
                    <button
                      type="button"
                      className="w-full rounded-md px-2 py-1.5 text-left hover:bg-muted"
                      onClick={() => {
                        void createDirectConversation(u.id).then((c) => {
                          setNewOpen(false);
                          void openConv(c.id);
                        });
                      }}
                    >
                      {u.displayName} <span className="text-meta text-muted-foreground">@{u.username}</span>
                    </button>
                  </li>
                ))}
            </ul>
          </div>
          <div>
            <h3 className="mb-2 text-body font-semibold">Tạo nhóm</h3>
            <input
              value={groupTitle}
              onChange={(e) => setGroupTitle(e.target.value)}
              placeholder="Tên nhóm"
              className="mb-2 h-9 w-full rounded-md border px-2"
            />
            <ul className="mb-2 max-h-36 space-y-1 overflow-y-auto">
              {directory.map((u) => (
                <label key={u.id} className="flex items-center gap-2 rounded px-2 py-1 hover:bg-muted">
                  <input
                    type="checkbox"
                    checked={groupMembers.includes(u.id)}
                    onChange={() =>
                      setGroupMembers((prev) =>
                        prev.includes(u.id) ? prev.filter((x) => x !== u.id) : [...prev, u.id],
                      )
                    }
                  />
                  {u.displayName}
                </label>
              ))}
            </ul>
            <button
              type="button"
              className={btn.primary}
              disabled={!groupTitle.trim() || groupMembers.length < 1}
              onClick={() => {
                void createGroupConversation(groupTitle.trim(), groupMembers).then((c) => {
                  setNewOpen(false);
                  void openConv(c.id);
                });
              }}
            >
              Tạo nhóm
            </button>
          </div>
        </div>
      </SideSheet>

      <SideSheet
        open={membersOpen}
        onOpenChange={(o) => !o && setMembersOpen(false)}
        title="Thành viên nhóm"
        widthClassName="max-w-md"
      >
        <ul className="mb-4 space-y-1">
          {members.map((m) => (
            <li key={m.userId} className="flex items-center justify-between gap-2 rounded px-2 py-1.5 hover:bg-muted">
              <span>
                {m.displayName}{" "}
                <span className="text-meta text-muted-foreground">@{m.username}</span>
              </span>
              {canSend && activeId && (
                <button
                  type="button"
                  className={btn.danger}
                  onClick={() => {
                    void removeMember(activeId, m.userId).then(async () => {
                      if (m.isSelf) {
                        setMembersOpen(false);
                        setActiveId(null);
                        await loadConvs();
                      } else {
                        setMembers(await fetchMembers(activeId));
                      }
                    });
                  }}
                >
                  {m.isSelf ? "Rời" : "Xóa"}
                </button>
              )}
            </li>
          ))}
        </ul>
        {canSend && activeId && (
          <div className="space-y-2 border-t pt-3">
            <p className="text-body font-semibold">Thêm thành viên</p>
            {directory
              .filter((u) => !members.some((m) => m.userId === u.id))
              .map((u) => (
                <label key={u.id} className="flex items-center gap-2 text-body">
                  <input
                    type="checkbox"
                    checked={addMemberIds.includes(u.id)}
                    onChange={() =>
                      setAddMemberIds((prev) =>
                        prev.includes(u.id) ? prev.filter((x) => x !== u.id) : [...prev, u.id],
                      )
                    }
                  />
                  {u.displayName}
                </label>
              ))}
            <button
              type="button"
              className={btn.primary}
              disabled={addMemberIds.length === 0}
              onClick={() => {
                void addMembers(activeId, addMemberIds).then(async () => {
                  setMembers(await fetchMembers(activeId));
                  setAddMemberIds([]);
                });
              }}
            >
              Thêm
            </button>
          </div>
        )}
      </SideSheet>
    </div>
  );
}
