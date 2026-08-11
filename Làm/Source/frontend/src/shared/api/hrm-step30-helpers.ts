// hrm-step30-helpers.ts
// Frontend helpers cho Bước 30:
//   UC_HRM_105 — Cấu hình quên check-out (forgotCheckoutHours 1-48h)
//   UC_HRM_106 — Cấu hình thời hạn xin điều chỉnh (adjustDeadlineDays 0-60d)
//   UC_HRM_107 — Cấu hình làm thêm giờ OT (enableOt boolean, otAfterMinutes 0-480m)
//   UC_HRM_108 — Cấu hình ca đêm / ngày lễ (enableNightShiftRule & enableHolidayRule)

export function validateForgotCheckoutConfig(forgotCheckoutHours: number): { valid: boolean; error?: string } {
  if (isNaN(forgotCheckoutHours) || forgotCheckoutHours < 1 || forgotCheckoutHours > 48)
    return { valid: false, error: 'Thời gian tự động đánh dấu quên check-out phải từ 1 đến 48 giờ.' };
  return { valid: true };
}

export function validateAdjustDeadlineConfig(adjustDeadlineDays: number): { valid: boolean; error?: string } {
  if (isNaN(adjustDeadlineDays) || adjustDeadlineDays < 0 || adjustDeadlineDays > 60)
    return { valid: false, error: 'Thời hạn xin điều chỉnh công phải từ 0 đến 60 ngày.' };
  return { valid: true };
}

export interface OvertimeConfigInput {
  enableOt: boolean;
  otAfterMinutes: number;
}

export function validateOvertimeConfig(input: OvertimeConfigInput): { valid: boolean; error?: string } {
  if (typeof input.enableOt !== 'boolean')
    return { valid: false, error: 'Trạng thái tính OT không hợp lệ.' };

  if (input.enableOt) {
    if (isNaN(input.otAfterMinutes) || input.otAfterMinutes < 0 || input.otAfterMinutes > 480)
      return { valid: false, error: 'Số phút làm thêm sau ca để tính OT phải từ 0 đến 480 phút.' };
  }

  return { valid: true };
}

export interface NightShiftHolidayConfigInput {
  enableNightShiftRule: boolean;
  enableHolidayRule: boolean;
}

export function validateNightShiftHolidayConfig(input: NightShiftHolidayConfigInput): { valid: boolean; error?: string } {
  if (typeof input.enableNightShiftRule !== 'boolean')
    return { valid: false, error: 'Cấu hình ca đêm không hợp lệ.' };

  if (typeof input.enableHolidayRule !== 'boolean')
    return { valid: false, error: 'Cấu hình ngày lễ không hợp lệ.' };

  return { valid: true };
}
