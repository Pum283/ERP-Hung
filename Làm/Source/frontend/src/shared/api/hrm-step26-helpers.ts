// hrm-step26-helpers.ts
// Frontend helpers cho Bước 26:
//   UC_HRM_089 — Sao chép lịch ca (SourceFrom <= SourceTo, span <= 62 ngày, TargetStart bắt buộc)
//   UC_HRM_090 — Khóa sổ lịch ca theo kỳ (PeriodKey dạng yyyy-MM, OrgUnitId bắt buộc)
//   UC_HRM_091 — In / xuất lịch ca CSV
//   UC_HRM_092 — Tạo lệnh điều động nhân sự (EmployeeId bắt buộc, From != To, Reason 3-500 chars)

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_089 — Shift Schedule Copy validation
// ────────────────────────────────────────────────────────────────────────────

export interface ShiftCopyForm {
  sourceFrom: string;
  sourceTo: string;
  targetStart: string;
}

export function validateShiftCopy(form: ShiftCopyForm): { valid: boolean; error?: string } {
  if (!form.sourceFrom?.trim() || !form.sourceTo?.trim())
    return { valid: false, error: 'Vui lòng chọn khoảng ngày nguồn sao chép.' };

  if (!form.targetStart?.trim())
    return { valid: false, error: 'Vui lòng chọn ngày bắt đầu đích sao chép.' };

  const srcFrom = new Date(form.sourceFrom);
  const srcTo = new Date(form.sourceTo);
  if (srcTo < srcFrom)
    return { valid: false, error: 'Ngày kết thúc nguồn phải lớn hơn hoặc bằng ngày bắt đầu.' };

  const spanDays = Math.ceil((srcTo.getTime() - srcFrom.getTime()) / (1000 * 3600 * 24));
  if (spanDays > 62)
    return { valid: false, error: 'Chỉ sao chép tối đa 62 ngày một lần.' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_090 — Period Locking validation
// ────────────────────────────────────────────────────────────────────────────

export interface ShiftLockForm {
  orgUnitId: string;
  periodKey: string;
}

export function validateShiftLock(form: ShiftLockForm): { valid: boolean; error?: string } {
  if (!form.orgUnitId?.trim())
    return { valid: false, error: 'Vui lòng chọn đơn vị cần khóa sổ lịch ca.' };

  const key = (form.periodKey ?? '').trim();
  if (!/^\d{4}-\d{2}$/.test(key))
    return { valid: false, error: 'Kỳ khóa sổ phải có dạng yyyy-MM (ví dụ: 2026-08).' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_092 — Mobilization Order validation
// ────────────────────────────────────────────────────────────────────────────

export interface MobilizationOrderForm {
  employeeId: string;
  fromOrgUnitId: string;
  toOrgUnitId: string;
  startDate: string;
  reason: string;
}

export function validateMobilizationOrder(form: MobilizationOrderForm): { valid: boolean; error?: string } {
  if (!form.employeeId?.trim())
    return { valid: false, error: 'Vui lòng chọn nhân viên điều động.' };

  if (!form.fromOrgUnitId?.trim() || !form.toOrgUnitId?.trim())
    return { valid: false, error: 'Vui lòng chọn đơn vị đi và đơn vị đến.' };

  if (form.fromOrgUnitId.trim() === form.toOrgUnitId.trim())
    return { valid: false, error: 'Đơn vị đến phải khác đơn vị đi.' };

  if (!form.startDate?.trim())
    return { valid: false, error: 'Vui lòng chọn ngày hiệu lực điều động.' };

  const reason = (form.reason ?? '').trim();
  if (reason.length < 3 || reason.length > 500)
    return { valid: false, error: 'Lý do điều động từ 3 đến 500 ký tự.' };

  return { valid: true };
}
