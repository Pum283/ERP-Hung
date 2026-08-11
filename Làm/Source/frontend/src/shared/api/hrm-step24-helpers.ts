// hrm-step24-helpers.ts
// Frontend helpers cho Bước 24:
//   UC_HRM_080 — Duyệt thay đổi định biên
//   UC_HRM_081 — Tạo mẫu ca làm việc (Code 1-40 chars, Name 1-100 chars, BreakMinutes 0-600)
//   UC_HRM_082 — Xếp lịch ca nhân viên (EmployeeId, WorkShiftId, WorkDate)
//   UC_HRM_083 — Xếp lịch ca theo tuần / tháng (EmployeeIds non-empty, From <= To, range <= 62 days)

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_081 — Work Shift Template validation
// ────────────────────────────────────────────────────────────────────────────

export interface WorkShiftForm {
  code: string;
  name: string;
  breakMinutes: number;
}

export function validateWorkShiftTemplate(form: WorkShiftForm): { valid: boolean; error?: string } {
  const code = (form.code ?? '').trim();
  const name = (form.name ?? '').trim();

  if (!code || code.length > 40)
    return { valid: false, error: 'Mã ca làm việc từ 1 đến 40 ký tự.' };

  if (!name || name.length > 100)
    return { valid: false, error: 'Tên ca làm việc từ 1 đến 100 ký tự.' };

  if (isNaN(form.breakMinutes) || form.breakMinutes < 0 || form.breakMinutes > 600)
    return { valid: false, error: 'Thời gian nghỉ giữa ca từ 0 đến 600 phút.' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_082 — Single Shift Assignment validation
// ────────────────────────────────────────────────────────────────────────────

export interface SingleShiftAssignForm {
  employeeId: string;
  workShiftId: string;
  workDate: string;
}

export function validateSingleShiftAssign(form: SingleShiftAssignForm): { valid: boolean; error?: string } {
  if (!form.employeeId?.trim())
    return { valid: false, error: 'Vui lòng chọn nhân viên.' };

  if (!form.workShiftId?.trim())
    return { valid: false, error: 'Vui lòng chọn ca làm việc.' };

  if (!form.workDate?.trim())
    return { valid: false, error: 'Vui lòng chọn ngày làm việc.' };

  return { valid: true };
}

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_083 — Roster Range Shift Assignment validation
// ────────────────────────────────────────────────────────────────────────────

export interface ShiftAssignRangeForm {
  employeeIds: string[];
  workShiftId: string;
  from: string;
  to: string;
}

export function validateShiftAssignRange(form: ShiftAssignRangeForm): { valid: boolean; error?: string } {
  if (!form.employeeIds || form.employeeIds.length === 0)
    return { valid: false, error: 'Cần chọn ít nhất một nhân viên để xếp lịch ca.' };

  if (!form.workShiftId?.trim())
    return { valid: false, error: 'Vui lòng chọn ca làm việc.' };

  if (!form.from?.trim() || !form.to?.trim())
    return { valid: false, error: 'Vui lòng nhập khoảng ngày xếp ca.' };

  const fromDate = new Date(form.from);
  const toDate = new Date(form.to);
  if (toDate < fromDate)
    return { valid: false, error: 'Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.' };

  const diffDays = Math.ceil((toDate.getTime() - fromDate.getTime()) / (1000 * 3600 * 24));
  if (diffDays > 62)
    return { valid: false, error: 'Chỉ xếp lịch ca tối đa 62 ngày một lần.' };

  return { valid: true };
}
