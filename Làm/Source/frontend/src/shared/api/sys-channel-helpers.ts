/** Pure helpers — SYS Email/SMS stub + forgot/invite (UC_SYS_004/019/060/061). */

export type ChannelKind = "Email" | "SMS";

/** Chọn kênh gửi: Email ưu tiên, SMS nếu chỉ có SĐT. */
export function preferMessageChannel(email?: string | null, phone?: string | null): ChannelKind | null {
  if (email?.trim()) return "Email";
  if (phone?.trim()) return "SMS";
  return null;
}

export function channelTarget(email?: string | null, phone?: string | null): string | null {
  const ch = preferMessageChannel(email, phone);
  if (ch === "Email") return email!.trim();
  if (ch === "SMS") return phone!.trim();
  return null;
}

/** Có thể mời khi có username + (email hoặc phone). */
export function canInviteUser(username: string, email?: string | null, phone?: string | null): boolean {
  return Boolean(username.trim()) && preferMessageChannel(email, phone) !== null;
}

export function formatInviteFlash(r: {
  username: string; channel: string; target: string; message?: string;
}): string {
  if (r.message?.trim()) return r.message.trim();
  return `Đã mời ${r.username} qua ${r.channel} → ${r.target}`;
}

export function formatForgotFlash(): string {
  return "Nếu tài khoản tồn tại, OTP đã được gửi qua Email/SMS.";
}

/** OTP 6 số — khớp BE RandomNumberGenerator 100000–999999. */
export function isValidOtpFormat(otp: string): boolean {
  return /^\d{6}$/.test(otp.trim());
}

export function loginModeTitle(mode: "login" | "forgot" | "reset"): string {
  if (mode === "forgot") return "Quên mật khẩu";
  if (mode === "reset") return "Đặt lại mật khẩu";
  return "Đăng nhập";
}
