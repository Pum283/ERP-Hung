"use client";

import { create } from "zustand";
import type { MeResponse, ScopeType } from "@/shared/auth/types";

type AuthState = {
  userId?: string;
  username?: string;
  displayName?: string | null;
  roles: string[];
  permissions: string[];
  enabledModules: string[];
  effectiveScopeType?: ScopeType;
  bypassDataScope: boolean;
  tenantLogoUrl?: string | null;
  tenantName?: string | null;
  setSession: (me: MeResponse, token: string) => void;
  patchTenantBrand: (logoUrl: string | null | undefined, tenantName?: string | null) => void;
  clear: () => void;
  hydrateFromStorage: () => void;
};

export const useAuthStore = create<AuthState>((set) => ({
  roles: [],
  permissions: [],
  enabledModules: [],
  bypassDataScope: false,
  setSession: (me, token) => {
    localStorage.setItem("access_token", token);
    set({
      userId: me.userId,
      username: me.username,
      displayName: me.displayName,
      roles: me.roles,
      permissions: me.permissions,
      enabledModules: me.enabledModules,
      effectiveScopeType: me.effectiveScopeType,
      bypassDataScope: me.bypassDataScope,
      tenantLogoUrl: me.tenantLogoUrl,
      tenantName: me.tenantName,
    });
  },
  patchTenantBrand: (logoUrl, tenantName) =>
    set((s) => ({
      tenantLogoUrl: logoUrl,
      tenantName: tenantName === undefined ? s.tenantName : tenantName,
    })),
  clear: () => {
    localStorage.removeItem("access_token");
    set({
      userId: undefined,
      username: undefined,
      displayName: undefined,
      roles: [],
      permissions: [],
      enabledModules: [],
      effectiveScopeType: undefined,
      bypassDataScope: false,
      tenantLogoUrl: undefined,
      tenantName: undefined,
    });
  },
  hydrateFromStorage: () => {
    // Token only; /me sẽ hydrate đầy đủ sau login hoặc bootstrap.
  },
}));
