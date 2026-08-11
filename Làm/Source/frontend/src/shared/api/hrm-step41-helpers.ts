// hrm-step41-helpers.ts
// Frontend helpers cho Bước 41:
//   UC_HRM_155 — Đơn giá giờ / ngày nhân viên (calculateDailyHourlyRates)
//   UC_HRM_156 — Quản lý lương thực tế chi trả (calculateNetSalary & validatePayrollPeriodConfirm)
//   UC_HRM_157 — Danh mục phụ cấp (filterAllowanceTypes)
//   UC_HRM_158 — Rule phụ cấp theo ca (formatPayrollPeriodStatus)

export function calculateDailyHourlyRates(baseSalary: number, standardWorkDays = 26): { dailyRate: number; hourlyRate: number } {
  if (isNaN(baseSalary) || baseSalary <= 0 || isNaN(standardWorkDays) || standardWorkDays <= 0) {
    return { dailyRate: 0, hourlyRate: 0 };
  }
  const dailyRate = Math.round(baseSalary / standardWorkDays);
  const hourlyRate = Math.round(dailyRate / 8);
  return { dailyRate, hourlyRate };
}

export function validatePayrollPeriodConfirm(periodId: string, status: string): { valid: boolean; error?: string } {
  if (!periodId?.trim())
    return { valid: false, error: 'Mã kỳ tính lương không hợp lệ.' };

  if (status === 'Locked')
    return { valid: false, error: 'Kỳ tính lương đã khóa sổ, không thể thay đổi.' };

  if (status !== 'Calculated' && status !== 'Confirmed')
    return { valid: false, error: 'Chỉ có thể xác nhận kỳ lương đã được tính toán.' };

  return { valid: true };
}

export interface NetSalaryComponents {
  attendancePay: number;
  otPay: number;
  allowanceTotal: number;
  bonus: number;
  insuranceEmployee: number;
  tax: number;
  deductionTotal: number;
}

export function calculateNetSalary(c: NetSalaryComponents): { grossPay: number; netPay: number } {
  const grossPay = (c.attendancePay || 0) + (c.otPay || 0) + (c.allowanceTotal || 0) + (c.bonus || 0);
  const totalDeductions = (c.insuranceEmployee || 0) + (c.tax || 0) + (c.deductionTotal || 0);
  const netPay = Math.max(0, grossPay - totalDeductions);
  return { grossPay, netPay };
}

export function formatPayrollPeriodStatus(status: string): string {
  switch (status) {
    case 'Draft':
      return '📝 Chưa tính lương';
    case 'Calculated':
      return '📊 Đã tính bảng lương';
    case 'Confirmed':
      return '✅ Đã duyệt chi trả';
    case 'Locked':
      return '🔒 Đã khóa kỳ lương';
    default:
      return status || 'Chưa xác định';
  }
}
