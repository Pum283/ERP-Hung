// hrm-step44-helpers.ts
// Frontend helpers cho Bước 44:
//   UC_HRM_167 — Nhập khấu trừ / tạm ứng (validateDeductionAdvanceAmount)
//   UC_HRM_168 — Xem / chỉnh bảng lương chi tiết (validatePayrollLinePatchInput & filterPayrollLines)
//   UC_HRM_169 — Xác nhận bảng lương (validatePeriodConfirmEligibility)
//   UC_HRM_170 — Khóa sổ kỳ lương (validatePeriodLockEligibility & formatPayslipSummary)

export interface PayrollLinePatchInput {
  bonus?: number;
  deductionTotal?: number;
  allowanceTotal?: number;
  note?: string;
}

export function validatePayrollLinePatchInput(input: PayrollLinePatchInput): { valid: boolean; error?: string } {
  if (input.bonus !== undefined && (isNaN(input.bonus) || input.bonus < 0))
    return { valid: false, error: 'Mức tiền thưởng không được là số âm.' };

  if (input.deductionTotal !== undefined && (isNaN(input.deductionTotal) || input.deductionTotal < 0))
    return { valid: false, error: 'Tổng khấu trừ không được là số âm.' };

  if (input.allowanceTotal !== undefined && (isNaN(input.allowanceTotal) || input.allowanceTotal < 0))
    return { valid: false, error: 'Tổng phụ cấp không được là số âm.' };

  return { valid: true };
}

export function validatePeriodLockEligibility(status: string): { valid: boolean; error?: string } {
  if (status === 'Locked')
    return { valid: false, error: 'Kỳ lương này đã được khóa sổ từ trước.' };

  if (status !== 'Confirmed')
    return { valid: false, error: 'Chỉ có thể khóa sổ kỳ lương sau khi đã duyệt xác nhận bảng lương.' };

  return { valid: true };
}

export interface PayrollLineSearchItem {
  employeeCode: string;
  employeeName: string;
}

export function filterPayrollLines<T extends PayrollLineSearchItem>(lines: T[], query: string): T[] {
  if (!query?.trim()) return lines;
  const q = query.trim().toLowerCase();
  return lines.filter(
    (l) => l.employeeCode.toLowerCase().includes(q) || l.employeeName.toLowerCase().includes(q)
  );
}

export function formatPayslipSummary(employeeName: string, grossPay: number, netPay: number): string {
  const formattedNet = netPay.toLocaleString('vi-VN');
  return `👤 ${employeeName} | Lương thực nhận: ${formattedNet} VNĐ (Tổng thu nhập Gross: ${grossPay.toLocaleString('vi-VN')} VNĐ)`;
}
