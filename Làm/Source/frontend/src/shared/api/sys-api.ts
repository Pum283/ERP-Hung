import { api } from "@/shared/api/client";

type Envelope<T> = { success: boolean; message?: string; data: T };

export type MenuItemDto = {
  id: string;
  code: string;
  parentId?: string | null;
  moduleCode: string;
  title: string;
  routePath?: string | null;
  permissionCode?: string | null;
  icon?: string | null;
  sortOrder: number;
};

export type OrgUnitDto = {
  id: string;
  code: string;
  name: string;
  parentId?: string | null;
  unitType: string;
  isActive: boolean;
};

export type DepartmentDto = {
  id: string;
  code: string;
  name: string;
  parentId?: string | null;
  orgUnitId: string;
  managerUserId?: string | null;
  isActive: boolean;
};

export type RoleDto = {
  id: string;
  code: string;
  name: string;
  bypassDataScope: boolean;
  isSystem: boolean;
  isActive: boolean;
  permissionIds: string[];
};

export type PermissionDto = {
  id: string;
  moduleCode: string;
  code: string;
  name: string;
  resource: string;
  action: string;
  description?: string | null;
  isActive?: boolean;
};

export async function fetchMyMenu() {
  const { data } = await api.get<Envelope<MenuItemDto[]>>("/api/sys/menu");
  return data.data;
}

export async function fetchOrgUnits() {
  const { data } = await api.get<Envelope<OrgUnitDto[]>>("/api/sys/org-units");
  return data.data;
}

export async function fetchDepartments() {
  const { data } = await api.get<Envelope<DepartmentDto[]>>("/api/sys/departments");
  return data.data;
}

export async function fetchRoles() {
  const { data } = await api.get<Envelope<RoleDto[]>>("/api/sys/roles");
  return data.data;
}

export async function fetchPermissions() {
  const { data } = await api.get<Envelope<PermissionDto[]>>("/api/sys/permissions");
  return data.data;
}

export type InviteUserResult = {
  userId: string; username: string; channel: string; target: string; logId: string; message: string;
};

/** UC_SYS_019 — mời user + gửi OTP qua Email/SMS stub. */
export async function inviteUser(body: {
  username: string; displayName?: string | null; email?: string | null; phone?: string | null;
  primaryOrgUnitId?: string | null; departmentId?: string | null; jobLevelId?: string | null;
}) {
  const { data } = await api.post<Envelope<InviteUserResult>>("/api/sys/users/invite", body);
  return data.data;
}

export type UserSessionDto = {
  id: string;
  sessionKey: string;
  ipAddress?: string | null;
  userAgent?: string | null;
  lastSeenAt: string;
  expiresAt: string;
  isRevoked: boolean;
};

export async function fetchUserSessions() {
  const { data } = await api.get<Envelope<UserSessionDto[]>>("/api/auth/sessions");
  return data.data;
}

export async function revokeUserSession(sessionId: string) {
  const { data } = await api.delete<Envelope<{ ok: boolean }>>(`/api/auth/sessions/${sessionId}`);
  return data.data;
}

export type TrustedDeviceDto = {
  id: string;
  deviceFingerprint: string;
  deviceName: string;
  ipAddress: string;
  lastUsedAt: string;
  expiresAt: string;
  isActive: boolean;
};

export async function fetchTrustedDevices() {
  const { data } = await api.get<Envelope<TrustedDeviceDto[]>>("/api/auth/trusted-devices");
  return data.data;
}

export async function registerTrustedDevice(body: { deviceFingerprint: string; deviceName: string }) {
  const { data } = await api.post<Envelope<TrustedDeviceDto>>("/api/auth/trusted-devices", body);
  return data.data;
}

export async function revokeTrustedDevice(id: string) {
  const { data } = await api.delete<Envelope<{ ok: boolean }>>(`/api/auth/trusted-devices/${id}`);
  return data.data;
}
