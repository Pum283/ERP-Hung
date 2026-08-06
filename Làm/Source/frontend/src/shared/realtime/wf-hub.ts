"use client";

import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";

const baseURL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

export type InboxChangedPayload = {
  reason: string;
  taskId?: string | null;
};

type InboxHandler = (payload: InboxChangedPayload) => void;

let connection: HubConnection | null = null;
const handlers = new Set<InboxHandler>();

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
    .withUrl(`${baseURL}/hubs/wf`, {
      accessTokenFactory: () => getToken() ?? "",
    })
    .withAutomaticReconnect([0, 2000, 5000, 15000, 30000])
    // Không log Warning — Next.js DevTools coi console.error của SignalR là Issues
    .configureLogging(LogLevel.None)
    .build();

  connection.on("inboxChanged", (payload: InboxChangedPayload) => {
    handlers.forEach((h) => h(payload ?? { reason: "unknown" }));
  });

  connection.onclose(() => {
    /* reconnect handled by withAutomaticReconnect */
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

/** Đăng ký lắng nghe inbox WF (SignalR). Trả về hàm hủy. Không poll. */
export function subscribeWfInbox(handler: InboxHandler): () => void {
  handlers.add(handler);
  void ensureConnection().catch(() => {
    /* trang vẫn dùng GET một lần khi mount */
  });

  return () => {
    handlers.delete(handler);
    if (handlers.size === 0 && connection) {
      const c = connection;
      connection = null;
      void c.stop();
    }
  };
}
