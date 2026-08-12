export interface ShiftImportRow {
  employeeCode: string;
  workShiftCode: string;
  workDate: string;
  note?: string | null;
}

export interface PenaltyItem {
  id: string;
  employeeId: string;
  reason: string;
  penaltyType: string;
  amount: number;
  status: 'Pending' | 'Applied' | 'Cancelled' | string;
}

/**
 * Validate hàng dữ liệu import lịch ca
 */
export function validateShiftImportRow(row: ShiftImportRow): { isValid: boolean; error?: string } {
  if (!row.employeeCode || !row.employeeCode.trim()) {
    return { isValid: false, error: 'Mã nhân viên không được để trống.' };
  }
  if (!row.workShiftCode || !row.workShiftCode.trim()) {
    return { isValid: false, error: 'Mã ca làm việc không được để trống.' };
  }
  if (!row.workDate || !/^\d{4}-\d{2}-\d{2}$/.test(row.workDate.trim())) {
    return { isValid: false, error: 'Ngày làm việc không hợp lệ (định dạng YYYY-MM-DD).' };
  }
  return { isValid: true };
}

/**
 * Chuẩn hóa loại vi phạm phạt
 */
export function normalizePenaltyType(type: string): { isValid: boolean; normalized: string } {
  const valid = ['LateArrival', 'EarlyLeave', 'RegulationBreach', 'SafetyViolation', 'Other'];
  const found = valid.find((v) => v.toLowerCase() === type.trim().toLowerCase());
  return {
    isValid: !!found,
    normalized: found || 'LateArrival',
  };
}

/**
 * Tính tổng số tiền phạt
 */
export function calculatePayrollPenaltyTotal(penalties: PenaltyItem[]): number {
  return penalties
    .filter((p) => p.status !== 'Cancelled')
    .reduce((acc, p) => acc + (Number(p.amount) || 0), 0);
}

/**
 * Xem trước bút toán kế toán lương đồng bộ FIN
 */
export function generateFinJePreview(
  grossSalary: number,
  netSalary: number,
  penalties: number
): {
  debitAccount: string;
  creditAccountSalary: string;
  creditAccountPenalty: string;
  isBalanced: boolean;
} {
  return {
    debitAccount: `TK 642 - Chi phí quản lý (Lương): ${grossSalary.toLocaleString('vi-VN')} VNĐ`,
    creditAccountSalary: `TK 334 - Phải trả NLĐ (Lương thực nhận): ${netSalary.toLocaleString('vi-VN')} VNĐ`,
    creditAccountPenalty: `TK 338/Phạt - Thu trừ lương phạt: ${penalties.toLocaleString('vi-VN')} VNĐ`,
    isBalanced: true,
  };
}
