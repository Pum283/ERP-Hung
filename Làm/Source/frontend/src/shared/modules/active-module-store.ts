"use client";

import { create } from "zustand";

const STORAGE_KEY = "pums_active_module";

type ActiveModuleState = {
  activeModule: string | null;
  setActiveModule: (code: string) => void;
  hydrate: () => void;
};

export const useActiveModuleStore = create<ActiveModuleState>((set) => ({
  activeModule: null,
  setActiveModule: (code) => {
    const c = code.toUpperCase();
    if (typeof window !== "undefined") localStorage.setItem(STORAGE_KEY, c);
    set({ activeModule: c });
  },
  hydrate: () => {
    if (typeof window === "undefined") return;
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) set({ activeModule: saved.toUpperCase() });
  },
}));
