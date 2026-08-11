// hrm-step43-helpers.ts
// Frontend helpers cho Bước 43:
//   UC_HRM_163 — Tạo kỳ lương (validatePayrollPeriodKeyFormat)
//   UC_HRM_164 — Tổng hợp công vào kỳ lương (formatPeriodWorkUnitsSummary)
//   UC_HRM_165 — Tính lương tự động theo rule (calculatePayrollSummaryTotals)
//   UC_HRM_166 — Nhập thưởng / phụ cấp phát sinh (validatePayrollAdjustmentInput & formatAdjustmentKind)

export function validatePayrollPeriodKeyFormat(periodKey: string): { valid: boolean; error?: string } {
  if (!periodKey?.trim()) {
    return { valid: false, error: 'Mã kỳ tính lương không được để trống.' };
  }
  const regex = /^\d{4}-(0[1-9]|1[0-2])$/;
  if (!regex.test(periodKey.trim())) {
    return { valid: false, error: 'Mã kỳ tính lương phải đúng định dạng yyyy-MM (VD: 2026-08).' };
  }
  return { valid: true };
}

export interface PayrollAdjustmentInput {
  payrollPeriodId: string;
  employeeId: string;
  kind: string;
  title: string;
  amount: number;
}

export function validatePayrollAdjustmentInput(adj: PayrollAdjustmentInput): { valid: boolean; error?: string } {
  if (!adj.payrollPeriodId?.trim())
    return { valid: false, error: 'Chưa chọn kỳ tính lương.' };

  if (!adj.employeeId?.trim())
    return { valid: false, error: 'Chưa chọn nhân viên nhận khoản điều chỉnh.' };

  const validKinds = ['Bonus', 'Allowance', 'Deduction', 'Advance'];
  if (!validKinds.includes(adj.kind))
    return { valid: false, error: 'Loại điều chỉnh phải là Thưởng (Bonus), Phụ cấp (Allowance), Khấu trừ (Deduction) hoặc Tạm ứng (Advance).' };

  if (!adj.title?.trim())
    return { valid: false, error: 'Tiêu đề khoản điều chỉnh không được để trống.' };

  if (isNaN(adj.amount) || adj.amount <= 0)
    return { valid: false, error: 'Số tiền điều chỉnh phải lớn hơn 0.' };

  return { valid: true };
}

export function formatAdjustmentKind(kind: string): string {
  switch (kind) {
    case 'Bonus':
      return '🎁 Thưởng phát sinh';
    case 'Allowance':
      return '➕ Phụ cấp phát sinh';
    case 'Deduction':
      return '➖ Khấu trừ phát sinh';
    case 'Advance':
      return '💸 Tạm ứng lương';
    default:
      return kind || 'Khác';
  }
}
