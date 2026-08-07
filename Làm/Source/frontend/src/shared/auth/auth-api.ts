import { api } from "@/shared/api/client";
import type { LoginResponse, MeResponse } from "@/shared/auth/types";

type Envelope<T> = { success: boolean; message?: string; data: T };

export async function login(username: string, password: string) {
  const { data } = await api.post<Envelope<LoginResponse>>("/api/auth/login", {
    username,
    password,
  });
  const result = data.data;
  // Bắt buộc lưu token TRƯỚC /me — interceptor đọc localStorage
  if (typeof window !== "undefined" && result?.accessToken) {
    localStorage.setItem("access_token", result.accessToken);
  }
  return result;
}

export async function fetchMe() {
  const { data } = await api.get<Envelope<MeResponse>>("/api/auth/me");
  return data.data;
}

export async function logoutApi() {
  try {
    await api.post("/api/auth/logout");
  } catch {
    /* vẫn clear local */
  }
}

export async function changePassword(currentPassword: string, newPassword: string) {
  await api.post("/api/auth/change-password", { currentPassword, newPassword });
}

/** UC_SYS_004 — quên MK → OTP qua Email/SMS stub. */
export async function forgotPassword(usernameOrEmail: string) {
  const { data } = await api.post<Envelope<{ ok: boolean; message?: string }>>(
    "/api/auth/forgot-password",
    { usernameOrEmail },
  );
  return data.data;
}

/** UC_SYS_005 — đặt lại MK bằng OTP. */
export async function resetPasswordWithOtp(usernameOrEmail: string, otp: string, newPassword: string) {
  await api.post("/api/auth/reset-password", { usernameOrEmail, otp, newPassword });
}
