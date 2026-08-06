import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type ConversationDto = {
  id: string;
  kind: string;
  title?: string | null;
  peerDisplayName?: string | null;
  peerUserId?: string | null;
  lastMessagePreview?: string | null;
  lastMessageAt?: string | null;
  unreadCount: number;
  muted?: boolean;
};

export type MessageReactionDto = {
  id: string;
  messageId: string;
  userId: string;
  displayName: string;
  reactionType: string;
};

export type ReactionToggledDto = {
  messageId: string;
  conversationId: string;
  userId: string;
  displayName: string;
  reactionType: string;
  removed: boolean;
};

export type ChatMessageDto = {
  id: string;
  conversationId: string;
  senderUserId: string;
  senderDisplayName: string;
  body: string;
  attachmentFileId?: string | null;
  attachmentStorageKey?: string | null;
  sentAt: string;
  recalled: boolean;
  parentMessageId?: string | null;
  parentPreview?: string | null;
  isEdited?: boolean;
  reactions?: MessageReactionDto[];
};

export type MsgDirectoryUserDto = {
  id: string;
  username: string;
  displayName: string;
};

export type ConversationMemberDto = {
  userId: string;
  displayName: string;
  username: string;
  isSelf: boolean;
};

export async function fetchConversations() {
  const { data } = await api.get<Envelope<ConversationDto[]>>("/api/sys/msg/conversations");
  return data.data;
}

export async function createDirectConversation(peerUserId: string) {
  const { data } = await api.post<Envelope<ConversationDto>>("/api/sys/msg/conversations", {
    peerUserId,
  });
  return data.data;
}

export async function createGroupConversation(title: string, memberIds: string[]) {
  const { data } = await api.post<Envelope<ConversationDto>>("/api/sys/msg/conversations", {
    title,
    memberIds,
  });
  return data.data;
}

export async function recallMessage(conversationId: string, messageId: string) {
  const { data } = await api.post<Envelope<ChatMessageDto>>(
    `/api/sys/msg/conversations/${conversationId}/messages/${messageId}/recall`,
  );
  return data.data;
}

export async function toggleReaction(conversationId: string, messageId: string, reactionType: string) {
  const { data } = await api.post<Envelope<ReactionToggledDto>>(
    `/api/sys/msg/conversations/${conversationId}/messages/${messageId}/reactions`,
    { reactionType },
  );
  return data.data;
}

/** Áp dụng event reaction realtime / response toggle lên danh sách tin. */
export function applyReactionToggle(prev: ChatMessageDto[], ev: ReactionToggledDto): ChatMessageDto[] {
  return prev.map((m) => {
    if (m.id !== ev.messageId) return m;
    const reactions = [...(m.reactions ?? [])];
    if (ev.removed) {
      return {
        ...m,
        reactions: reactions.filter(
          (r) => !(r.userId === ev.userId && r.reactionType === ev.reactionType),
        ),
      };
    }
    const idx = reactions.findIndex(
      (r) => r.userId === ev.userId && r.reactionType === ev.reactionType,
    );
    const next: MessageReactionDto = {
      id: idx >= 0 ? reactions[idx].id : `tmp-${ev.userId}-${ev.reactionType}`,
      messageId: ev.messageId,
      userId: ev.userId,
      displayName: ev.displayName,
      reactionType: ev.reactionType,
    };
    if (idx >= 0) reactions[idx] = next;
    else reactions.push(next);
    return { ...m, reactions };
  });
}

export async function editMessage(conversationId: string, messageId: string, body: string) {
  const { data } = await api.put<Envelope<ChatMessageDto>>(
    `/api/sys/msg/conversations/${conversationId}/messages/${messageId}`,
    { body },
  );
  return data.data;
}

export async function fetchMessages(conversationId: string, before?: string, take = 50) {
  const { data } = await api.get<Envelope<ChatMessageDto[]>>(
    `/api/sys/msg/conversations/${conversationId}/messages`,
    { params: { before, take } },
  );
  return data.data;
}

export async function sendMessage(
  conversationId: string,
  body: string,
  opts?: { attachmentStorageKey?: string; parentMessageId?: string },
) {
  const { data } = await api.post<Envelope<ChatMessageDto>>(
    `/api/sys/msg/conversations/${conversationId}/messages`,
    {
      body,
      attachmentStorageKey: opts?.attachmentStorageKey ?? null,
      parentMessageId: opts?.parentMessageId ?? null,
    },
  );
  return data.data;
}

export async function markConversationRead(conversationId: string) {
  await api.post(`/api/sys/msg/conversations/${conversationId}/read`);
}

export async function muteConversation(conversationId: string, muted: boolean) {
  await api.post(`/api/sys/msg/conversations/${conversationId}/mute`, { muted });
}

export async function fetchUnreadCount() {
  const { data } = await api.get<Envelope<{ count: number }>>("/api/sys/msg/unread-count");
  return data.data.count;
}

export async function fetchMsgDirectory() {
  const { data } = await api.get<Envelope<MsgDirectoryUserDto[]>>("/api/sys/msg/directory");
  return data.data;
}

export async function fetchMembers(conversationId: string) {
  const { data } = await api.get<Envelope<ConversationMemberDto[]>>(
    `/api/sys/msg/conversations/${conversationId}/members`,
  );
  return data.data;
}

export async function addMembers(conversationId: string, memberIds: string[]) {
  await api.post(`/api/sys/msg/conversations/${conversationId}/members`, { memberIds });
}

export async function removeMember(conversationId: string, memberUserId: string) {
  await api.delete(`/api/sys/msg/conversations/${conversationId}/members/${memberUserId}`);
}

export async function uploadMsgFile(file: File) {
  const fd = new FormData();
  fd.append("file", file);
  const { data } = await api.post<Envelope<{ storageKey: string; fileName?: string }>>(
    "/api/sys/files/upload",
    fd,
    { headers: { "Content-Type": "multipart/form-data" } },
  );
  return data.data;
}
