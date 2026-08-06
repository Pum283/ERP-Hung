"use client";

import { create } from "zustand";

export type DockChat = {
  conversationId: string;
  title: string;
  kind: string;
  peerUserId?: string | null;
  /** Chữ cái trên avatar mini */
  initials: string;
  minimized: boolean;
};

type MessengerState = {
  dropdownOpen: boolean;
  chats: DockChat[];
  setDropdownOpen: (open: boolean) => void;
  toggleDropdown: () => void;
  /** Mở cửa sổ chat (kiểu Facebook) */
  openChat: (chat: Omit<DockChat, "minimized">) => void;
  minimizeChat: (conversationId: string) => void;
  restoreChat: (conversationId: string) => void;
  closeChat: (conversationId: string) => void;
  toggleMinimize: (conversationId: string) => void;
};

const MAX_WINDOWS = 3;

function initialsFromTitle(title: string) {
  const parts = title.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "?";
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

export function chatInitials(title: string) {
  return initialsFromTitle(title);
}

export const useMessengerStore = create<MessengerState>((set, get) => ({
  dropdownOpen: false,
  chats: [],
  setDropdownOpen: (open) => set({ dropdownOpen: open }),
  toggleDropdown: () => set((s) => ({ dropdownOpen: !s.dropdownOpen })),
  openChat: (chat) => {
    const { chats } = get();
    const existing = chats.find((c) => c.conversationId === chat.conversationId);
    let next = existing
      ? chats.map((c) =>
          c.conversationId === chat.conversationId
            ? { ...c, ...chat, minimized: false }
            : c,
        )
      : [{ ...chat, minimized: false }, ...chats];

    // Giới hạn số cửa sổ mở (không minimized)
    const openIds = next.filter((c) => !c.minimized).map((c) => c.conversationId);
    if (openIds.length > MAX_WINDOWS) {
      const keep = new Set(openIds.slice(0, MAX_WINDOWS));
      next = next.map((c) =>
        !c.minimized && !keep.has(c.conversationId) ? { ...c, minimized: true } : c,
      );
    }
    set({ chats: next, dropdownOpen: false });
  },
  minimizeChat: (conversationId) =>
    set((s) => ({
      chats: s.chats.map((c) =>
        c.conversationId === conversationId ? { ...c, minimized: true } : c,
      ),
    })),
  restoreChat: (conversationId) => {
    const chat = get().chats.find((c) => c.conversationId === conversationId);
    if (!chat) return;
    get().openChat(chat);
  },
  closeChat: (conversationId) =>
    set((s) => ({
      chats: s.chats.filter((c) => c.conversationId !== conversationId),
    })),
  toggleMinimize: (conversationId) => {
    const chat = get().chats.find((c) => c.conversationId === conversationId);
    if (!chat) return;
    if (chat.minimized) get().restoreChat(conversationId);
    else get().minimizeChat(conversationId);
  },
}));
