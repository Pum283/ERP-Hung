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

// ── Bước 153: SSO / Field ACL / Config versions / Push ──────────────────────

export type SysSsoProviderDto = {
  id: string;
  code: string;
  displayName: string;
  clientId: string;
  authorityUrl?: string | null;
  redirectUri: string;
  scopes: string;
  jitProvisioning: boolean;
  isActive: boolean;
  note?: string | null;
};

export type SysSsoProviderPublicDto = {
  code: string;
  displayName: string;
  authorizeUrl: string;
};

export type SysSensitiveFieldDto = {
  id: string;
  moduleCode: string;
  entityName: string;
  fieldKey: string;
  displayName: string;
  defaultMask: string;
  isActive: boolean;
};

export type SysRoleFieldPermissionDto = {
  id: string;
  roleId: string;
  sensitiveFieldId: string;
  fieldKey: string;
  displayName: string;
  access: string;
};

export type SysConfigVersionDto = {
  id: string;
  configKey: string;
  configValue: string;
  versionNumber: number;
  commitNote?: string | null;
  isCurrent: boolean;
  createdAt: string;
  createdByUserId?: string | null;
};

export type SysPushDeviceDto = {
  id: string;
  userId: string;
  platform: string;
  deviceToken: string;
  appVersion?: string | null;
  isValid: boolean;
  lastSeenAt: string;
};

export async function fetchSsoProviders() {
  const { data } = await api.get<Envelope<SysSsoProviderDto[]>>("/api/sys/sso/providers");
  return data.data;
}

export async function fetchPublicSsoProviders(tenantId?: string) {
  const q = tenantId ? `?tenantId=${encodeURIComponent(tenantId)}` : "";
  const { data } = await api.get<Envelope<SysSsoProviderPublicDto[]>>(`/api/sys/sso/providers/public${q}`);
  return data.data;
}

export async function upsertSsoProvider(body: {
  id?: string | null;
  code: string;
  displayName: string;
  clientId: string;
  clientSecret?: string | null;
  authorityUrl?: string | null;
  redirectUri: string;
  scopes?: string | null;
  jitProvisioning: boolean;
  isActive: boolean;
  note?: string | null;
}) {
  const { data } = await api.put<Envelope<SysSsoProviderDto>>("/api/sys/sso/providers", body);
  return data.data;
}

export async function startSso(providerCode: string, tenantId?: string) {
  const q = tenantId ? `?tenantId=${encodeURIComponent(tenantId)}` : "";
  const { data } = await api.post<Envelope<{ providerCode: string; state: string; authorizeUrl: string }>>(
    `/api/sys/sso/start/${encodeURIComponent(providerCode)}${q}`,
  );
  return data.data;
}

export async function completeSsoCallback(body: {
  providerCode: string;
  code?: string | null;
  state?: string | null;
  email?: string | null;
  subject?: string | null;
}, tenantId?: string) {
  const q = tenantId ? `?tenantId=${encodeURIComponent(tenantId)}` : "";
  const { data } = await api.post<Envelope<{
    accessToken: string;
    expiresAt: string;
    userId: string;
    username: string;
    displayName?: string | null;
  }>>(`/api/sys/sso/callback${q}`, body);
  return data.data;
}

export async function fetchSensitiveFields() {
  const { data } = await api.get<Envelope<SysSensitiveFieldDto[]>>("/api/sys/sensitive-fields");
  return data.data;
}

export async function upsertSensitiveField(body: {
  id?: string | null;
  moduleCode: string;
  entityName: string;
  fieldKey: string;
  displayName: string;
  defaultMask?: string | null;
  isActive: boolean;
}) {
  const { data } = await api.put<Envelope<SysSensitiveFieldDto>>("/api/sys/sensitive-fields", body);
  return data.data;
}

export async function fetchRoleFieldPermissions(roleId: string) {
  const { data } = await api.get<Envelope<SysRoleFieldPermissionDto[]>>(
    `/api/sys/roles/${roleId}/field-permissions`,
  );
  return data.data;
}

export async function upsertRoleFieldPermission(body: {
  roleId: string;
  sensitiveFieldId: string;
  access: string;
}) {
  const { data } = await api.put<Envelope<SysRoleFieldPermissionDto>>("/api/sys/role-field-permissions", body);
  return data.data;
}

export async function fetchConfigVersions(key: string) {
  const { data } = await api.get<Envelope<SysConfigVersionDto[]>>(
    `/api/sys/settings/${encodeURIComponent(key)}/versions`,
  );
  return data.data;
}

export async function upsertSettingVersioned(key: string, valueJson: string, commitNote?: string) {
  const { data } = await api.put<Envelope<{ ok: boolean }>>(
    `/api/sys/settings/${encodeURIComponent(key)}/versioned`,
    { key, valueJson, commitNote },
  );
  return data.data;
}

export async function rollbackConfigVersion(key: string, versionNumber: number, commitNote?: string) {
  const { data } = await api.post<Envelope<SysConfigVersionDto>>(
    `/api/sys/settings/${encodeURIComponent(key)}/rollback`,
    { key, versionNumber, commitNote },
  );
  return data.data;
}

export async function fetchMyPushDevices() {
  const { data } = await api.get<Envelope<SysPushDeviceDto[]>>("/api/sys/push/devices");
  return data.data;
}

export async function registerPushDevice(body: {
  platform: string;
  deviceToken: string;
  appVersion?: string | null;
}) {
  const { data } = await api.post<Envelope<SysPushDeviceDto>>("/api/sys/push/devices", body);
  return data.data;
}

export async function revokePushDevice(id: string) {
  const { data } = await api.delete<Envelope<{ ok: boolean }>>(`/api/sys/push/devices/${id}`);
  return data.data;
}

export async function sendTestPush(body: { userId?: string | null; title: string; body: string }) {
  const { data } = await api.post<Envelope<{ targetedDevices: number; deliveredStub: number; logRef?: string | null }>>(
    "/api/sys/push/test",
    body,
  );
  return data.data;
}

// ── Bước 154: Notif prefs / File scan / Bulk export / IP ────────────────────

export type SysNotificationPreferenceDto = {
  userId: string;
  channelInApp: boolean;
  channelEmail: boolean;
  channelSms: boolean;
  channelPush: boolean;
  muteAll: boolean;
  quietHoursStart?: string | null;
  quietHoursEnd?: string | null;
};

export type SysFileScanStatusDto = {
  fileObjectId: string;
  fileName: string;
  scanStatus: string;
  scannedAt?: string | null;
  threatName?: string | null;
  engine?: string | null;
};

export type SysBulkExportJobDto = {
  id: string;
  jobType: string;
  entityType: string;
  format?: string | null;
  status: string;
  rowCount: number;
  errorCount: number;
  errorDetails?: string | null;
  startedAt: string;
  completedAt?: string | null;
  actorId?: string | null;
  resultFileName?: string | null;
  expiresAt?: string | null;
};

export type SysIpRuleDto = {
  id: string;
  ipAddressOrCidr: string;
  ruleType: string;
  description: string;
  isActive: boolean;
};

export async function fetchMyNotificationPreferences() {
  const { data } = await api.get<Envelope<SysNotificationPreferenceDto>>("/api/sys/me/notification-preferences");
  return data.data;
}

export async function upsertMyNotificationPreferences(body: {
  channelInApp: boolean;
  channelEmail: boolean;
  channelSms: boolean;
  channelPush: boolean;
  muteAll: boolean;
  quietHoursStart?: string | null;
  quietHoursEnd?: string | null;
}) {
  const { data } = await api.put<Envelope<SysNotificationPreferenceDto>>("/api/sys/me/notification-preferences", body);
  return data.data;
}

export async function fetchSysFiles() {
  const { data } = await api.get<Envelope<{ id: string; fileName: string; sizeBytes: number; storageKey: string }[]>>(
    "/api/sys/files",
  );
  return data.data;
}

export async function scanSysFile(id: string, contentHint?: string) {
  const { data } = await api.post<Envelope<SysFileScanStatusDto>>(`/api/sys/files/${id}/scan`, { contentHint });
  return data.data;
}

export async function fetchFileScanStatus(id: string) {
  const { data } = await api.get<Envelope<SysFileScanStatusDto>>(`/api/sys/files/${id}/scan-status`);
  return data.data;
}

export async function startBulkExport(entityTypes: string[], format: string) {
  const { data } = await api.post<Envelope<SysBulkExportJobDto>>("/api/sys/export/bulk", { entityTypes, format });
  return data.data;
}

export async function fetchExportJobs(take = 50) {
  const { data } = await api.get<Envelope<SysBulkExportJobDto[]>>(`/api/sys/export/jobs?take=${take}`);
  return data.data;
}

export async function downloadExportJob(id: string) {
  const res = await api.get(`/api/sys/export/jobs/${id}/download`, { responseType: "blob" });
  return res.data as Blob;
}

export async function fetchIpRules() {
  const { data } = await api.get<Envelope<SysIpRuleDto[]>>("/api/sys/ip-rules");
  return data.data;
}

export async function upsertIpRule(body: {
  id?: string | null;
  ipAddressOrCidr: string;
  ruleType: string;
  description?: string | null;
  isActive: boolean;
}) {
  const { data } = await api.put<Envelope<SysIpRuleDto>>("/api/sys/ip-rules", body);
  return data.data;
}

export async function deleteIpRule(id: string) {
  const { data } = await api.delete<Envelope<{ ok: boolean }>>(`/api/sys/ip-rules/${id}`);
  return data.data;
}

export async function checkIpRule(ip: string) {
  const { data } = await api.post<Envelope<{ allowed: boolean; reason: string }>>("/api/sys/ip-rules/check", { ip });
  return data.data;
}

// ── Bước 155: Theme / Role home / Msg search / Mute ─────────────────────────

export type SysThemeDto = {
  tenantId: string;
  tenantName: string;
  logoUrl?: string | null;
  primaryColor?: string | null;
  accentColor?: string | null;
  faviconUrl?: string | null;
};

export type SysRoleHomeDto = {
  id: string;
  roleId: string;
  roleCode: string;
  roleName: string;
  landingPath: string;
  priority: number;
  isActive: boolean;
  note?: string | null;
};

export type SysMyHomeDto = {
  landingPath: string;
  matchedRoleCode?: string | null;
  priority?: number | null;
};

export type SysMessageSearchHitDto = {
  messageId: string;
  conversationId: string;
  conversationTitle?: string | null;
  senderUserId: string;
  senderDisplayName: string;
  bodyPreview: string;
  sentAt: string;
};

export type SysConversationMuteDto = {
  conversationId: string;
  muted: boolean;
  muteUntil?: string | null;
  effectivelyMuted: boolean;
};

export async function fetchTheme() {
  const { data } = await api.get<Envelope<SysThemeDto>>("/api/sys/theme");
  return data.data;
}

export async function fetchPublicTheme(tenantId?: string) {
  const q = tenantId ? `?tenantId=${encodeURIComponent(tenantId)}` : "";
  const { data } = await api.get<Envelope<SysThemeDto>>(`/api/sys/theme/public${q}`);
  return data.data;
}

export async function upsertTheme(body: {
  primaryColor?: string | null;
  accentColor?: string | null;
  faviconUrl?: string | null;
}) {
  const { data } = await api.put<Envelope<SysThemeDto>>("/api/sys/theme", body);
  return data.data;
}

export async function fetchRoleHomes() {
  const { data } = await api.get<Envelope<SysRoleHomeDto[]>>("/api/sys/role-homes");
  return data.data;
}

export async function upsertRoleHome(body: {
  id?: string | null;
  roleId: string;
  landingPath: string;
  priority: number;
  isActive: boolean;
  note?: string | null;
}) {
  const { data } = await api.put<Envelope<SysRoleHomeDto>>("/api/sys/role-homes", body);
  return data.data;
}

export async function deleteRoleHome(id: string) {
  const { data } = await api.delete<Envelope<{ ok: boolean }>>(`/api/sys/role-homes/${id}`);
  return data.data;
}

export async function fetchMyHome() {
  const { data } = await api.get<Envelope<SysMyHomeDto>>("/api/sys/me/home");
  return data.data;
}

export async function searchMessages(q: string, conversationId?: string, take = 20) {
  const params = new URLSearchParams({ q, take: String(take) });
  if (conversationId) params.set("conversationId", conversationId);
  const { data } = await api.get<Envelope<SysMessageSearchHitDto[]>>(
    `/api/sys/msg/messages/search?${params.toString()}`,
  );
  return data.data;
}

export async function fetchConversationMute(id: string) {
  const { data } = await api.get<Envelope<SysConversationMuteDto>>(`/api/sys/msg/conversations/${id}/mute`);
  return data.data;
}

export async function setConversationMute(id: string, body: { muted: boolean; muteUntil?: string | null }) {
  const { data } = await api.put<Envelope<SysConversationMuteDto>>(`/api/sys/msg/conversations/${id}/mute`, body);
  return data.data;
}

