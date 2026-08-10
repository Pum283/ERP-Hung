// hrm-profile-status-helpers.ts
// Frontend helpers cho Bước 13: UC_HRM_026 (Xuất Excel), UC_HRM_027 (Khóa hồ sơ đã nghỉ),
// UC_HRM_028 (Xem hồ sơ theo quyền), UC_HRM_029 (Chuyển trạng thái Thử việc)

export interface EmploymentStatusChangeForm {
  toStatus: 'New' | 'Probation' | 'Active' | 'Terminated' | 'Resigned' | 'Retired' | 'Locked';
  effectiveDate?: string | null;
  reason?: string | null;
}

export function isLockedStatus(status: string): boolean {
  const norm = (status || '').trim();
  return ['Terminated', 'Resigned', 'Retired', 'Locked', 'Inactive'].includes(norm);
}

export function validateStatusTransition(
  currentStatus: string,
  targetStatus: string,
  effectiveDate?: string | null
): { valid: boolean; error?: string; isLocking?: boolean; isRehiring?: boolean } {
  if (!targetStatus || targetStatus.trim().length === 0)
    return { valid: false, error: 'Trạng thái chuyển tới không được để trống.' };

  const currentLocked = isLockedStatus(currentStatus);
  const targetLocked = isLockedStatus(targetStatus);

  if (currentStatus === targetStatus)
    return { valid: false, error: `Hồ sơ đã ở trạng thái '${currentStatus}'.` };

  if (targetStatus === 'Probation' && !effectiveDate) {
    // Tự động lấy ngày hôm nay nếu không nhập
    effectiveDate = new Date().toISOString().split('T')[0];
  }

  return {
    valid: true,
    isLocking: targetLocked,
    isRehiring: currentLocked && !targetLocked,
  };
}

export function buildCsvExportFilename(prefix: string = 'danh-sach-nhan-su'): string {
  const dateStr = new Date().toISOString().split('T')[0];
  return `${prefix}_${dateStr}.csv`;
}
