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
