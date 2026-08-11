// hrm-step34-helpers.ts
// Frontend helpers cho Bước 34:
//   UC_HRM_122 — Duyệt / từ chối điều chỉnh (validateAdjustDecision)
//   UC_HRM_123 — Ghi nhận vi phạm đi trễ (filterLateViolations)
//   UC_HRM_126 — Khóa bảng công theo kỳ (validatePeriodLockKey)
//   UC_HRM_127 — Mở khóa bảng công có kiểm soát (formatPeriodLockStatus)

export function validatePeriodLockKey(periodKey: string): { valid: boolean; error?: string } {
  const key = (periodKey ?? '').trim();
  if (!key)
    return { valid: false, error: 'Vui lòng nhập kỳ khóa công (yyyy-MM).' };

  if (!/^\d{4}\-(0[1-9]|1[0-2])$/.test(key))
    return { valid: false, error: 'Kỳ khóa công phải đúng định dạng yyyy-MM (ví dụ: 2026-08).' };

  return { valid: true };
}

export function validateAdjustDecision(requestId: string, approve: boolean): { valid: boolean; error?: string } {
  if (!requestId?.trim())
    return { valid: false, error: 'Mã phiếu điều chỉnh không hợp lệ.' };

  if (typeof approve !== 'boolean')
    return { valid: false, error: 'Quyết định phê duyệt không hợp lệ.' };

  return { valid: true };
}

export interface AttendanceRecordItem {
  id: string;
  employeeCode: string;
  employeeName: string;
  lateMinutes: number;
  deductedWorkUnit: number;
}

export function filterLateViolations(records: AttendanceRecordItem[], minLateMinutes = 1): AttendanceRecordItem[] {
  if (!records || records.length === 0) return [];
  return records.filter((r) => r.lateMinutes >= minLateMinutes);
}

export function formatPeriodLockStatus(isLocked: boolean, lockerName?: string, lockedAtIso?: string): string {
  if (isLocked) {
    const locker = lockerName || 'Quản trị viên';
    const dateStr = lockedAtIso ? new Date(lockedAtIso).toLocaleDateString('vi-VN') : '';
    return `🔒 Đã khóa bởi ${locker}${dateStr ? ` ngày ${dateStr}` : ''}`;
  }
  return '🔓 Đang mở (cho phép chỉnh sửa)';
}
