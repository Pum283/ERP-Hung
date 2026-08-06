"use client";

import type { ReactNode } from "react";
import { usePermissions } from "@/shared/hooks/use-permissions";

/** Digi-style: ẩn UI theo permission code. */
export function CanAccess({
  permission,
  anyOf,
  children,
  fallback = null,
}: {
  permission?: string;
  anyOf?: string[];
  children: ReactNode;
  fallback?: ReactNode;
}) {
  const { can, canAny } = usePermissions();

  if (anyOf?.length) {
    return canAny(...anyOf) ? <>{children}</> : <>{fallback}</>;
  }
  if (permission && !can(permission)) {
    return <>{fallback}</>;
  }
  return <>{children}</>;
}
