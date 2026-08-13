"use client";

import { useEffect } from "react";
import { fetchPublicTheme, fetchTheme } from "@/shared/api/sys-api";
import { applyThemeToDocument } from "@/shared/api/sys-theme-role-home-msg-helpers";
import { useAuthStore } from "@/shared/auth/auth-store";

/**
 * Áp màu Primary/Accent từ trang Branding (--brand / --accent + biến dẫn xuất)
 * lên toàn bộ app (login + shell).
 */
export function ThemeBootstrap() {
  const token = useAuthStore((s) => s.accessToken);

  useEffect(() => {
    let cancelled = false;
    void (async () => {
      try {
        const theme = token ? await fetchTheme() : await fetchPublicTheme();
        if (cancelled || !theme) return;
        applyThemeToDocument(theme.primaryColor, theme.accentColor, theme.faviconUrl);
      } catch {
        /* giữ brand-kit.css mặc định */
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [token]);

  return null;
}
