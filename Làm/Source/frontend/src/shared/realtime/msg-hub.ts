"use client";

import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import type { ChatMessageDto, ReactionToggledDto } from "@/shared/api/msg-api";

const baseURL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:1111";

export type MessageReceivedPayload = ChatMessageDto;

export type ConversationUpdatedPayload = {
  conversationId: string;
  reason: string;
};

export type TypingPayload = {
  conversationId: string;
  userId: string;
  fullName: string;
  isTyping: boolean;
};

type MessageHandler = (payload: MessageReceivedPayload) => void;
type ConvHandler = (payload: ConversationUpdatedPayload) => void;
type TypingHandler = (payload: TypingPayload) => void;
type ReactionHandler = (payload: ReactionToggledDto) => void;

let connection: HubConnection | null = null;
const messageHandlers = new Set<MessageHandler>();
const editHandlers = new Set<MessageHandler>();
const convHandlers = new Set<ConvHandler>();
const typingHandlers = new Set<TypingHandler>();
const reactionHandlers = new Set<ReactionHandler>();

function getToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("access_token");
}

async function ensureConnection(): Promise<HubConnection | null> {
  const token = getToken();
  if (!token) return null;

  if (connection && connection.state !== HubConnectionState.Disconnected) {
    return connection;
  }

  connection = new HubConnectionBuilder()
    .withUrl(`${baseURL}/hubs/msg`, {
      accessTokenFactory: () => getToken() ?? "",
    })
    .withAutomaticReconnect([0, 2000, 5000, 15000, 30000])
    // Không log Warning — Next.js DevTools coi console.error của SignalR là Issues
    .configureLogging(LogLevel.None)
    .build();

  connection.on("messageReceived", (payload: MessageReceivedPayload) => {
    messageHandlers.forEach((h) => h(payload));
  });
  connection.on("messageEdited", (payload: MessageReceivedPayload) => {
    editHandlers.forEach((h) => h(payload));
  });
  connection.on("conversationUpdated", (payload: ConversationUpdatedPayload) => {
    convHandlers.forEach((h) => h(payload ?? { conversationId: "", reason: "unknown" }));
  });
  connection.on("ReceiveTypingStatus", (payload: TypingPayload) => {
    typingHandlers.forEach((h) => h(payload));
  });
  connection.on("reactionToggled", (payload: ReactionToggledDto) => {
    reactionHandlers.forEach((h) => h(payload));
  });

  if (connection.state === HubConnectionState.Disconnected) {
    try {
      await connection.start();
    } catch {
      const failed = connection;
      connection = null;
      void failed.stop().catch(() => {});
      return null;
    }
  }
  return connection;
}

function maybeStop() {
  if (
    messageHandlers.size === 0 &&
    editHandlers.size === 0 &&
    convHandlers.size === 0 &&
    typingHandlers.size === 0 &&
    reactionHandlers.size === 0 &&
    connection
  ) {
    const c = connection;
    connection = null;
    void c.stop();
  }
}

export function subscribeMsgReceived(handler: MessageHandler): () => void {
  messageHandlers.add(handler);
  void ensureConnection().catch(() => {});
  return () => {
    messageHandlers.delete(handler);
    maybeStop();
  };
}

export function subscribeMsgEdited(handler: MessageHandler): () => void {
  editHandlers.add(handler);
  void ensureConnection().catch(() => {});
  return () => {
    editHandlers.delete(handler);
    maybeStop();
  };
}

export function subscribeConversationUpdated(handler: ConvHandler): () => void {
  convHandlers.add(handler);
  void ensureConnection().catch(() => {});
  return () => {
    convHandlers.delete(handler);
    maybeStop();
  };
}

export function subscribeTyping(handler: TypingHandler): () => void {
  typingHandlers.add(handler);
  void ensureConnection().catch(() => {});
  return () => {
    typingHandlers.delete(handler);
    maybeStop();
  };
}

export function subscribeReactionToggled(handler: ReactionHandler): () => void {
  reactionHandlers.add(handler);
  void ensureConnection().catch(() => {});
  return () => {
    reactionHandlers.delete(handler);
    maybeStop();
  };
}

export async function joinConversation(conversationId: string) {
  const c = await ensureConnection();
  if (c) await c.invoke("JoinConversation", conversationId);
}

export async function leaveConversation(conversationId: string) {
  const c = await ensureConnection();
  if (c) await c.invoke("LeaveConversation", conversationId);
}

export async function sendTypingStatus(conversationId: string, isTyping: boolean) {
  const c = await ensureConnection();
  if (c) await c.invoke("SendTypingStatus", conversationId, isTyping);
}
