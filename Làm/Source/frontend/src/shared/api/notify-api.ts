import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type AppNotificationDto = {
  id: string;
  title: string;
  body: string;
  link?: string | null;
  eventType?: string | null;
  isRead: boolean;
  createdAt: string;
};

export async function fetchNotifications() {
  const { data } = await api.get<Envelope<AppNotificationDto[]>>("/api/sys/notifications");
  return data.data;
}

export async function fetchNotificationUnreadCount() {
  const { data } = await api.get<Envelope<{ count: number }>>("/api/sys/notifications/unread-count");
  return data.data.count;
}

export async function markNotificationRead(id: string) {
  await api.post(`/api/sys/notifications/${id}/read`);
}

export async function markAllNotificationsRead() {
  await api.post("/api/sys/notifications/read-all");
}
