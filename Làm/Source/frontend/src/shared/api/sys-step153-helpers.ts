// sys-step153-helpers.ts
// Bước 153:
//   UC_SYS_009 — SSO / OAuth
//   UC_SYS_031 — Quyền trường nhạy cảm
//   UC_SYS_058 — Phiên bản cấu hình
//   UC_SYS_062 — Push notification mobile

export function validateSsoProviderForm(input: {
  code: string;
  displayName: string;
  clientId: string;
  redirectUri: string;
}): { isValid: boolean; error?: string } {
  const code = (input.code || "").trim();
  const name = (input.displayName || "").trim();
  const clientId = (input.clientId || "").trim();
  const redirect = (input.redirectUri || "").trim();
  if (!code) return { isValid: false, error: "Mã IdP bắt buộc." };
  if (code.length > 40) return { isValid: false, error: "Mã IdP tối đa 40 ký tự." };
  if (!name) return { isValid: false, error: "Tên hiển thị bắt buộc." };
  if (!clientId) return { isValid: false, error: "ClientId bắt buộc." };
  if (!redirect) return { isValid: false, error: "RedirectUri bắt buộc." };
  return { isValid: true };
}

export function buildDevSsoCode(email: string, subject: string): string {
  const e = (email || "").trim().toLowerCase();
  const s = (subject || "").trim() || e;
  return `dev:${e}|${s}`;
}

export function isAllowedFieldAccess(access: string): boolean {
  const a = (access || "").trim().toLowerCase();
  return a === "none" || a === "masked" || a === "read" || a === "write";
}

export function rankFieldAccess(access: string): number {
  switch ((access || "").trim().toLowerCase()) {
    case "write":
      return 3;
    case "read":
      return 2;
    case "masked":
      return 1;
    default:
      return 0;
  }
}

export function mostPermissiveAccess(accesses: string[]): string {
  if (!accesses.length) return "None";
  let best = "None";
  let bestRank = -1;
  for (const a of accesses) {
    const r = rankFieldAccess(a);
    if (r > bestRank) {
      bestRank = r;
      best = ["None", "Masked", "Read", "Write"][r] ?? "None";
    }
  }
  return best;
}

export function applyFieldMaskUi(rawValue: string | null | undefined, access: string): string {
  const a = (access || "None").trim().toLowerCase();
  if (a === "none" || a === "hide") return "••••";
  if (a === "masked" || a === "mask") {
    const v = rawValue ?? "";
    if (!v) return "";
    if (v.length <= 4) return "•".repeat(v.length);
    return v.slice(0, 2) + "•".repeat(Math.max(2, v.length - 4)) + v.slice(-2);
  }
  return rawValue ?? "";
}

export function validateConfigKey(key: string): { isValid: boolean; error?: string } {
  const k = (key || "").trim();
  if (!k) return { isValid: false, error: "ConfigKey bắt buộc." };
  if (k.length > 100) return { isValid: false, error: "ConfigKey tối đa 100 ký tự." };
  return { isValid: true };
}

export function canRollbackVersion(versions: { versionNumber: number; isCurrent: boolean }[], target: number): {
  canRollback: boolean;
  reason?: string;
} {
  if (target <= 0) return { canRollback: false, reason: "VersionNumber phải > 0." };
  const hit = versions.find((v) => v.versionNumber === target);
  if (!hit) return { canRollback: false, reason: "Phiên bản không tồn tại." };
  if (hit.isCurrent) return { canRollback: false, reason: "Đã là phiên bản hiện tại." };
  return { canRollback: true };
}

export function validatePushDevice(platform: string, deviceToken: string): { isValid: boolean; error?: string } {
  const p = (platform || "").trim().toLowerCase();
  if (!["fcm", "apns", "web"].includes(p)) {
    return { isValid: false, error: "Platform phải là Fcm|Apns|Web." };
  }
  const t = (deviceToken || "").trim();
  if (t.length < 8) return { isValid: false, error: "DeviceToken tối thiểu 8 ký tự." };
  if (t.length > 500) return { isValid: false, error: "DeviceToken tối đa 500 ký tự." };
  return { isValid: true };
}

export function formatPushPlatformLabel(platform: string): string {
  switch ((platform || "").trim().toLowerCase()) {
    case "fcm":
      return "Android (FCM)";
    case "apns":
      return "iOS (APNs)";
    case "web":
      return "Web Push";
    default:
      return platform || "—";
  }
}
