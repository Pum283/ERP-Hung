// sys-step155-helpers.ts
// Bước 155:
//   UC_SYS_093 — Theme / logo
//   UC_SYS_094 — Trang chủ theo vai trò
//   UC_SYS_103 — Tìm kiếm tin nhắn
//   UC_SYS_104 — Tắt thông báo hội thoại

export function isValidHexColor(color?: string | null): boolean {
  if (!color || !color.trim()) return true; // optional
  return /^#([0-9A-Fa-f]{6}|[0-9A-Fa-f]{3})$/.test(color.trim());
}

export function validateThemeForm(input: {
  primaryColor?: string | null;
  accentColor?: string | null;
}): { isValid: boolean; error?: string } {
  if (!isValidHexColor(input.primaryColor)) {
    return { isValid: false, error: "PrimaryColor phải dạng #RGB/#RRGGBB." };
  }
  if (!isValidHexColor(input.accentColor)) {
    return { isValid: false, error: "AccentColor phải dạng #RGB/#RRGGBB." };
  }
  return { isValid: true };
}

export function validateLandingPath(path: string): { isValid: boolean; error?: string } {
  const p = (path || "").trim();
  if (!p.startsWith("/app")) return { isValid: false, error: "LandingPath phải bắt đầu bằng /app." };
  if (p.length > 200) return { isValid: false, error: "LandingPath tối đa 200 ký tự." };
  if (!/^\/app(\/[\w\-/]*)?$/.test(p)) {
    return { isValid: false, error: "LandingPath chứa ký tự không hợp lệ." };
  }
  return { isValid: true };
}

export function pickBestRoleHome(
  homes: { roleCode: string; landingPath: string; priority: number; isActive: boolean }[],
  myRoleCodes: string[],
): { landingPath: string; matchedRoleCode?: string } {
  const set = new Set(myRoleCodes.map((c) => c.toUpperCase()));
  const candidates = homes
    .filter((h) => h.isActive && set.has(h.roleCode.toUpperCase()))
    .sort((a, b) => a.priority - b.priority);
  if (!candidates.length) return { landingPath: "/app" };
  return { landingPath: candidates[0].landingPath, matchedRoleCode: candidates[0].roleCode };
}

export function validateMessageSearchQuery(q: string): { isValid: boolean; error?: string } {
  const s = (q || "").trim();
  if (s.length < 2) return { isValid: false, error: "Từ khóa tối thiểu 2 ký tự." };
  if (s.length > 200) return { isValid: false, error: "Từ khóa tối đa 200 ký tự." };
  return { isValid: true };
}

export function highlightSearchSnippet(body: string, query: string, max = 120): string {
  const b = body || "";
  const q = (query || "").trim();
  if (!q) return b.slice(0, max);
  const idx = b.toLowerCase().indexOf(q.toLowerCase());
  if (idx < 0) return b.slice(0, max) + (b.length > max ? "…" : "");
  const start = Math.max(0, idx - 20);
  const end = Math.min(b.length, idx + q.length + 40);
  let snip = (start > 0 ? "…" : "") + b.slice(start, end) + (end < b.length ? "…" : "");
  return snip;
}

export function isEffectivelyMuted(
  muted: boolean,
  muteUntil: string | null | undefined,
  nowMs: number,
): boolean {
  if (!muted) return false;
  if (!muteUntil) return true;
  const until = Date.parse(muteUntil);
  if (Number.isNaN(until)) return true;
  return until > nowMs;
}

export function validateMuteUntil(muted: boolean, muteUntil?: string | null): {
  isValid: boolean;
  error?: string;
} {
  if (!muted) return { isValid: true };
  if (!muteUntil) return { isValid: true };
  const t = Date.parse(muteUntil);
  if (Number.isNaN(t)) return { isValid: false, error: "MuteUntil không hợp lệ." };
  if (t <= Date.now()) return { isValid: false, error: "MuteUntil phải ở tương lai." };
  return { isValid: true };
}

export function applyThemeCssVars(primary?: string | null, accent?: string | null): Record<string, string> {
  const vars: Record<string, string> = {};
  if (primary && isValidHexColor(primary) && primary.trim()) {
    vars["--brand"] = primary.trim();
  }
  if (accent && isValidHexColor(accent) && accent.trim()) {
    vars["--accent"] = accent.trim();
  }
  return vars;
}
