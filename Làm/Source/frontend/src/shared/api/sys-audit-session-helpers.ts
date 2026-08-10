// sys-audit-session-helpers.ts
// Frontend helpers cho Bước 9: UC_SYS_078 (AuditLog), UC_SYS_080 (FieldDiff),
// UC_SYS_081 (Export AuditLog), UC_SYS_083 (Session Policy)

export interface AuditLogItem {
  id: string;
  entityType: string;
  entityId: string | null;
  action: string;
  beforeJson: string | null;
  afterJson: string | null;
  actorUserId: string | null;
  ipAddress: string | null;
  createdAt: string;
}

export interface FieldDiffItem {
  fieldName: string;
  oldValue: string | null;
  newValue: string | null;
  changeKind: 'Added' | 'Removed' | 'Modified' | 'Created' | 'Deleted';
}

export interface AuditLogQueryParams {
  entityType?: string;
  action?: string;
  actorUserId?: string;
  from?: string;   // ISO8601
  to?: string;     // ISO8601
  page?: number;
  pageSize?: number;
}

export interface SessionPolicyForm {
  sessionMinutes: number;
  idleTimeoutMinutes: number;
  maxConcurrentSessions: number;
  forceLogoutOnPasswordChange: boolean;
}

// ─── Audit Log Helpers ───

export function validateAuditLogQuery(params: AuditLogQueryParams): { valid: boolean; error?: string } {
  if (params.page !== undefined && params.page < 1)
    return { valid: false, error: 'Số trang phải >= 1.' };
  if (params.pageSize !== undefined && (params.pageSize < 1 || params.pageSize > 500))
    return { valid: false, error: 'Kích thước trang phải trong khoảng 1 – 500.' };
  if (params.from && params.to && new Date(params.from) > new Date(params.to))
    return { valid: false, error: 'Ngày bắt đầu không được lớn hơn ngày kết thúc.' };
  return { valid: true };
}

export function getActionBadgeColor(action: string): string {
  const map: Record<string, string> = {
    Create: '#10b981',
    Update: '#3b82f6',
    Delete: '#ef4444',
    Login:  '#f59e0b',
    Logout: '#8b5cf6',
  };
  return map[action] ?? '#6b7280';
}

export function getChangeKindLabel(kind: string): string {
  const map: Record<string, string> = {
    Added:    '+ Thêm mới',
    Removed:  '- Đã xoá',
    Modified: '~ Sửa đổi',
    Created:  '✚ Tạo mới',
    Deleted:  '✖ Đã xoá',
  };
  return map[kind] ?? kind;
}

// ─── Export AuditLog Helpers ───

export function validateAuditLogExportRequest(params: {
  from: string;
  to: string;
}): { valid: boolean; error?: string } {
  const from = new Date(params.from);
  const to   = new Date(params.to);

  if (isNaN(from.getTime()) || isNaN(to.getTime()))
    return { valid: false, error: 'Ngày không hợp lệ.' };

  if (from >= to)
    return { valid: false, error: 'Ngày bắt đầu phải nhỏ hơn ngày kết thúc.' };

  const daysDiff = (to.getTime() - from.getTime()) / (1000 * 60 * 60 * 24);
  if (daysDiff > 366)
    return { valid: false, error: 'Khoảng thời gian xuất tối đa 366 ngày.' };

  return { valid: true };
}

// ─── Session Policy Helpers ───

export function validateSessionPolicy(form: SessionPolicyForm): { valid: boolean; error?: string } {
  if (form.sessionMinutes < 1)
    return { valid: false, error: 'Thời gian phiên phải >= 1 phút.' };
  if (form.sessionMinutes > 10_080)
    return { valid: false, error: 'Thời gian phiên tối đa 10.080 phút (7 ngày).' };
  if (form.idleTimeoutMinutes < 0)
    return { valid: false, error: 'Thời gian idle timeout không được âm.' };
  if (form.idleTimeoutMinutes > 0 && form.idleTimeoutMinutes > form.sessionMinutes)
    return { valid: false, error: 'Idle timeout không được lớn hơn thời gian phiên.' };
  if (form.maxConcurrentSessions < 1 || form.maxConcurrentSessions > 20)
    return { valid: false, error: 'Số phiên đồng thời phải trong khoảng 1 – 20.' };
  return { valid: true };
}

export function formatSessionDuration(minutes: number): string {
  if (minutes < 60) return `${minutes} phút`;
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  return m > 0 ? `${h} giờ ${m} phút` : `${h} giờ`;
}
