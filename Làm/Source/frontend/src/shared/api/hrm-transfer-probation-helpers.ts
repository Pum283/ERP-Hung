// hrm-transfer-probation-helpers.ts
// Frontend helpers cho Bước 14: UC_HRM_032 (Nghỉ việc), UC_HRM_033 (Lịch sử biến động),
// UC_HRM_034 (Điều chuyển đơn vị / bộ phận), UC_HRM_036 (Cảnh báo sắp hết hạn thử việc)

export interface EmployeeTransferForm {
  orgUnitId?: string | null;
  departmentId?: string | null;
  jobTitleId?: string | null;
  jobLevelId?: string | null;
  effectiveDate?: string | null;
  reason?: string | null;
}

export function validateTransferForm(
  currentOrgId: string,
  currentDeptId?: string | null,
  currentJobTitleId?: string | null,
  currentJobLevelId?: string | null,
  form?: EmployeeTransferForm
): { valid: boolean; error?: string } {
  if (!form) return { valid: false, error: 'Dữ liệu điều chuyển không hợp lệ.' };

  const targetOrgId = form.orgUnitId || currentOrgId;
  const targetDeptId = form.departmentId ?? currentDeptId;
  const targetJtId = form.jobTitleId ?? currentJobTitleId;
  const targetJlId = form.jobLevelId ?? currentJobLevelId;

  if (
    targetOrgId === currentOrgId &&
    targetDeptId === currentDeptId &&
    targetJtId === currentJobTitleId &&
    targetJlId === currentJobLevelId
  ) {
    return { valid: false, error: 'Thông tin Đơn vị, Bộ phận hoặc Chức danh mới phải khác với thông tin hiện tại.' };
  }

  if (form.effectiveDate && isNaN(Date.parse(form.effectiveDate))) {
    return { valid: false, error: 'Ngày hiệu lực điều chuyển không đúng định dạng.' };
  }

  return { valid: true };
}

export function calculateProbationStatusBadge(daysRemaining: number): { text: string; severity: 'critical' | 'warning' | 'info' } {
  if (daysRemaining <= 3) {
    return { text: `Còn ${daysRemaining} ngày (Rất gấp)`, severity: 'critical' };
  }
  if (daysRemaining <= 7) {
    return { text: `Còn ${daysRemaining} ngày (Cần xử lý)`, severity: 'warning' };
  }
  return { text: `Còn ${daysRemaining} ngày`, severity: 'info' };
}
