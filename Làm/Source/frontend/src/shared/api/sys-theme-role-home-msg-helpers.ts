// sys-theme-role-home-msg-helpers.ts
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

  const p = primary?.trim();
  if (p && isValidHexColor(p)) {
    const hex = normalizeHex(p);
    vars["--brand"] = hex;
    vars["--brand-hover"] = mixHex(hex, "#000000", 0.18);
    vars["--brand-strong"] = mixHex(hex, "#000000", 0.32);
    vars["--brand-muted"] = hexToRgba(hex, 0.12);
    vars["--brand-foreground"] = "#ffffff";
    vars["--ring"] = hex;
    vars["--sidebar-active-bg"] = vars["--brand-muted"];
    vars["--sidebar-active-fg"] = vars["--brand-strong"];
    vars["--login-panel"] = mixHex(hex, "#000000", 0.55);
    vars["--login-panel-fg"] = "#f0f9ff";
    vars["--login-panel-muted"] = mixHex(hex, "#ffffff", 0.45);
  }

  const a = accent?.trim();
  if (a && isValidHexColor(a)) {
    const hex = normalizeHex(a);
    vars["--accent"] = hex;
    vars["--accent-foreground"] = mixHex(hex, "#000000", 0.45);
    vars["--accent-muted"] = hexToRgba(hex, 0.18);
  }

  return vars;
}

/** Ghi CSS variables + favicon lên document (browser only). */
export function applyThemeToDocument(
  primary?: string | null,
  accent?: string | null,
  faviconUrl?: string | null,
): void {
  if (typeof document === "undefined") return;
  const vars = applyThemeCssVars(primary, accent);
  Object.entries(vars).forEach(([k, v]) => document.documentElement.style.setProperty(k, v));
  if (faviconUrl) {
    let link = document.querySelector("link[rel='icon']") as HTMLLinkElement | null;
    if (!link) {
      link = document.createElement("link");
      link.rel = "icon";
      document.head.appendChild(link);
    }
    link.href = faviconUrl;
  }
}

function normalizeHex(input: string): string {
  const h = input.trim();
  if (/^#[0-9A-Fa-f]{3}$/.test(h)) {
    return `#${h[1]}${h[1]}${h[2]}${h[2]}${h[3]}${h[3]}`.toUpperCase();
  }
  return h.toUpperCase();
}

function parseRgb(hex: string): { r: number; g: number; b: number } | null {
  const n = normalizeHex(hex).replace("#", "");
  if (!/^[0-9A-Fa-f]{6}$/.test(n)) return null;
  return {
    r: parseInt(n.slice(0, 2), 16),
    g: parseInt(n.slice(2, 4), 16),
    b: parseInt(n.slice(4, 6), 16),
  };
}

function toHex(r: number, g: number, b: number): string {
  const c = (n: number) => Math.max(0, Math.min(255, Math.round(n))).toString(16).padStart(2, "0");
  return `#${c(r)}${c(g)}${c(b)}`.toUpperCase();
}

/** Trộn màu hex với màu đích theo tỉ lệ amount (0..1 về phía toward). */
function mixHex(from: string, toward: string, amount: number): string {
  const a = parseRgb(from);
  const b = parseRgb(toward);
  if (!a || !b) return from;
  const t = Math.max(0, Math.min(1, amount));
  return toHex(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t);
}

function hexToRgba(hex: string, alpha: number): string {
  const rgb = parseRgb(hex);
  if (!rgb) return hex;
  return `rgba(${rgb.r}, ${rgb.g}, ${rgb.b}, ${alpha})`;
}
