// hrm-step23-helpers.ts
// Frontend helpers cho Bước 23:
//   UC_HRM_076 — Khai báo định biên theo ca (ShiftCode bắt buộc, max 40 ký tự)
//   UC_HRM_077 — Khai báo định biên theo bộ phận (DepartmentId bắt buộc)
//   UC_HRM_078 — So sánh thực tế vs định biên (Gap = Planned - Actual)
//   UC_HRM_079 — Cảnh báo thiếu người (Shortage = Gap > 0)

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_076 — Shift headcount plan validation
// ────────────────────────────────────────────────────────────────────────────

export function validateShiftHeadcountPlan(shiftCode: string, plannedHeadcount: number): { valid: boolean; error?: string } {
  const code = (shiftCode ?? '').trim();
  if (!code)
    return { valid: false, error: 'Cần nhập Mã ca làm việc khi định biên theo ca.' };

  if (code.length > 40)
    return { valid: false, error: 'Mã ca làm việc tối đa 40 ký tự.' };

  if (isNaN(plannedHeadcount) || plannedHeadcount < 0 || plannedHeadcount > 100000)
    return { valid: false, error: 'Định biên nhân sự phải là số không âm (≥ 0).' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_077 — Department headcount plan validation
// ────────────────────────────────────────────────────────────────────────────

export function validateDeptHeadcountPlan(departmentId: string, plannedHeadcount: number): { valid: boolean; error?: string } {
  if (!departmentId?.trim())
    return { valid: false, error: 'Vui lòng chọn Bộ phận khi định biên theo bộ phận.' };

  if (isNaN(plannedHeadcount) || plannedHeadcount < 0 || plannedHeadcount > 100000)
    return { valid: false, error: 'Định biên nhân sự phải là số không âm (≥ 0).' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_078 & UC_HRM_079 — Gap & Shortage calculation
// ────────────────────────────────────────────────────────────────────────────

export function calculateHeadcountGap(plannedHeadcount: number, actualHeadcount: number): number {
  return (plannedHeadcount ?? 0) - (actualHeadcount ?? 0);
}

export function isHeadcountShortage(plannedHeadcount: number, actualHeadcount: number): boolean {
  return calculateHeadcountGap(plannedHeadcount, actualHeadcount) > 0;
}
