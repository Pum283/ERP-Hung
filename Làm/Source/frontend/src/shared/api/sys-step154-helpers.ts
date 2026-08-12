// sys-step154-helpers.ts
// Bước 154:
//   UC_SYS_064 — Tùy chọn thông báo cá nhân
//   UC_SYS_071 — Quét virus / bảo mật file
//   UC_SYS_077 — Xuất dữ liệu hàng loạt
//   UC_SYS_082 — Quản lý IP allow/deny

export const LOCKED_SECURITY_EVENTS = [
  "security.login_failed",
  "security.password_changed",
  "security.account_locked",
  "sys.ip_blocked",
] as const;

export function validateQuietHours(
  start?: string | null,
  end?: string | null,
): { isValid: boolean; error?: string } {
  const s = (start || "").trim();
  const e = (end || "").trim();
  if (!s && !e) return { isValid: true };
  if (!s || !e) return { isValid: false, error: "Quiet hours phải đủ Start và End." };
  const re = /^([01]\d|2[0-3]):[0-5]\d$/;
  if (!re.test(s) || !re.test(e)) return { isValid: false, error: "Quiet hours phải dạng HH:mm." };
  return { isValid: true };
}

export function shouldDeliverInApp(input: {
  muteAll: boolean;
  channelInApp: boolean;
  quietHoursStart?: string | null;
  quietHoursEnd?: string | null;
  eventType: string;
  utcMinutes: number; // minutes from midnight UTC
}): boolean {
  if (LOCKED_SECURITY_EVENTS.includes(input.eventType as (typeof LOCKED_SECURITY_EVENTS)[number])) {
    return true;
  }
  if (input.muteAll || !input.channelInApp) return false;
  const s = (input.quietHoursStart || "").trim();
  const e = (input.quietHoursEnd || "").trim();
  if (!s || !e) return true;
  const toMin = (hhmm: string) => {
    const [h, m] = hhmm.split(":").map(Number);
    return h * 60 + m;
  };
  const start = toMin(s);
  const end = toMin(e);
  const t = input.utcMinutes;
  if (start <= end) return !(t >= start && t < end);
  return !(t >= start || t < end);
}

export function isInfectedScanStatus(status: string): boolean {
  return (status || "").trim().toLowerCase() === "infected";
}

export function canDownloadFile(scanStatus: string): { canDownload: boolean; reason?: string } {
  const s = (scanStatus || "").trim().toLowerCase();
  if (s === "infected") return { canDownload: false, reason: "File bị nhiễm mã độc." };
  if (s === "scanning") return { canDownload: false, reason: "File đang quét." };
  return { canDownload: true };
}

export function validateBulkExportRequest(entityTypes: string[], format: string): {
  isValid: boolean;
  error?: string;
} {
  const types = (entityTypes || []).map((t) => t.trim()).filter(Boolean);
  if (types.length === 0) return { isValid: false, error: "Chọn ít nhất 1 loại dữ liệu." };
  if (types.length > 10) return { isValid: false, error: "Tối đa 10 loại dữ liệu." };
  const allowed = new Set(["Users", "Files", "AuditLogs"]);
  for (const t of types) {
    if (!allowed.has(t)) return { isValid: false, error: `Loại '${t}' chưa hỗ trợ.` };
  }
  if (format !== "Csv" && format !== "Pdf") {
    return { isValid: false, error: "Format chỉ hỗ trợ Csv|Pdf." };
  }
  return { isValid: true };
}

export function isValidIpOrCidr(value: string): boolean {
  const v = (value || "").trim();
  const m = v.match(/^(\d{1,3}(?:\.\d{1,3}){3})(?:\/(\d{1,2}))?$/);
  if (!m) return false;
  const octets = m[1].split(".").map(Number);
  if (octets.some((n) => n < 0 || n > 255)) return false;
  if (m[2] != null) {
    const p = Number(m[2]);
    if (p < 0 || p > 32) return false;
  }
  return true;
}

export function validateIpRuleForm(input: {
  ipAddressOrCidr: string;
  ruleType: string;
}): { isValid: boolean; error?: string } {
  if (!isValidIpOrCidr(input.ipAddressOrCidr)) {
    return { isValid: false, error: "IP/CIDR không hợp lệ." };
  }
  const t = (input.ruleType || "").trim().toLowerCase();
  if (t !== "allow" && t !== "deny") {
    return { isValid: false, error: "RuleType phải là Allow|Deny." };
  }
  return { isValid: true };
}

export function formatScanStatusLabel(status: string): string {
  switch ((status || "").toLowerCase()) {
    case "clean":
      return "Sạch";
    case "infected":
      return "Nhiễm độc";
    case "scanning":
      return "Đang quét";
    case "pending":
      return "Chờ quét";
    default:
      return status || "—";
  }
}
