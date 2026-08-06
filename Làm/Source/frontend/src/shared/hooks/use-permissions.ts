"use client";

import { useCallback, useMemo } from "react";
import { useAuthStore } from "@/shared/auth/auth-store";

function normalize(code: string) {
  return code.trim().toLowerCase();
}

/** Digi-style access: Set permission codes + BypassDataScope. */
export function usePermissions() {
  const permissions = useAuthStore((s) => s.permissions);
  const bypass = useAuthStore((s) => s.bypassDataScope);

  const permissionSet = useMemo(() => {
    const set = new Set<string>();
    for (const code of permissions) set.add(normalize(code));
    return set;
  }, [permissions]);

  const can = useCallback(
    (code?: string | null) => {
      if (!code) return true;
      if (bypass) return true;
      return permissionSet.has(normalize(code));
    },
    [bypass, permissionSet]
  );

  const canAny = useCallback(
    (...codes: string[]) => codes.some((c) => can(c)),
    [can]
  );

  const canAll = useCallback(
    (...codes: string[]) => codes.every((c) => can(c)),
    [can]
  );

  return { permissions, can, canAny, canAll, bypass };
}

/** Alias Digi tester naming. */
export function useAccess() {
  const hooks = usePermissions();
  return {
    ...hooks,
    hasPermission: hooks.can,
    hasAnyPermission: (codes: string[]) => hooks.canAny(...codes),
    bypassDataScope: hooks.bypass,
  };
}
