// hrm-step35-helpers.ts
// Frontend helpers cho Bước 35:
//   UC_HRM_128 — Xác nhận bảng công (formatRecordConfirmStatus)
//   UC_HRM_130 — Cấu hình quỹ phép theo loại NS (validateLeaveEntitlementRule)
//   UC_HRM_131 — Cấp phát / điều chỉnh quỹ phép (validateLeaveBalanceAdjust & calculateRemainingLeave)
//   UC_HRM_133 — Duyệt đơn nghỉ đa cấp (validateLeaveCreateRequest)

export interface LeaveEntitlementInput {
  leaveTypeId: string;
  employeeTypeId?: string;
  daysPerYear: number;
}

export function validateLeaveEntitlementRule(input: LeaveEntitlementInput): { valid: boolean; error?: string } {
  if (!input.leaveTypeId?.trim())
    return { valid: false, error: 'Vui lòng chọn loại nghỉ phép.' };

  if (isNaN(input.daysPerYear) || input.daysPerYear < 0 || input.daysPerYear > 366)
    return { valid: false, error: 'Số ngày quỹ phép/năm phải từ 0 đến 366 ngày.' };

  return { valid: true };
}

export interface LeaveBalanceAdjustInput {
  employeeId: string;
  leaveTypeId: string;
  year: number;
  entitled: number;
}

export function validateLeaveBalanceAdjust(input: LeaveBalanceAdjustInput): { valid: boolean; error?: string } {
  if (!input.employeeId?.trim())
    return { valid: false, error: 'Vui lòng chọn nhân viên.' };

  if (!input.leaveTypeId?.trim())
    return { valid: false, error: 'Vui lòng chọn loại nghỉ phép.' };

  if (isNaN(input.entitled) || input.entitled < 0 || input.entitled > 366)
    return { valid: false, error: 'Số ngày phép được hưởng phải từ 0 đến 366 ngày.' };

  return { valid: true };
}

export interface LeaveCreateInput {
  leaveTypeId: string;
  fromDate: string;
  toDate: string;
  days: number;
  reason?: string;
}

export function validateLeaveCreateRequest(input: LeaveCreateInput): { valid: boolean; error?: string } {
  if (!input.leaveTypeId?.trim())
    return { valid: false, error: 'Vui lòng chọn loại nghỉ phép.' };

  if (!input.fromDate?.trim() || !input.toDate?.trim())
    return { valid: false, error: 'Vui lòng chọn từ ngày và đến ngày.' };

  if (input.toDate < input.fromDate)
    return { valid: false, error: 'Đến ngày phải lớn hơn hoặc bằng từ ngày.' };

  if (isNaN(input.days) || input.days <= 0)
    return { valid: false, error: 'Số ngày xin nghỉ phải lớn hơn 0.' };

  return { valid: true };
}

export function calculateRemainingLeave(entitled: number, used: number): number {
  if (isNaN(entitled) || entitled < 0) return 0;
  if (isNaN(used) || used < 0) return entitled;
  return Math.max(0, entitled - used);
}
