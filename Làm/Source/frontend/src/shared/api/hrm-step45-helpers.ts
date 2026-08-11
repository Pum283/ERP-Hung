// hrm-step45-helpers.ts
// Frontend helpers cho Bước 45:
//   UC_HRM_171 — Phiếu lương cá nhân (formatPayslipDetail)
//   UC_HRM_172 — Xuất bảng lương tổng hợp (parseCsvPayrollExport)
//   UC_HRM_173 — Xuất file chi lương ngân hàng (validateBankExportRow)
//   UC_HRM_175 — Báo cáo chi phí lương theo đơn vị (calculateCostByOrgTotals & formatCostByOrgRow)

export interface PayslipDetailItem {
  label: string;
  amount: number;
  type: 'income' | 'deduction' | 'summary';
}

export function formatPayslipDetail(
  baseSalary: number,
  attendancePay: number,
  otPay: number,
  allowanceTotal: number,
  bonus: number,
  insuranceEmployee: number,
  tax: number,
  deductionTotal: number,
  grossPay: number,
  netPay: number
): PayslipDetailItem[] {
  return [
    { label: 'Lương cơ bản', amount: baseSalary, type: 'income' },
    { label: 'Lương theo ngày công', amount: attendancePay, type: 'income' },
    { label: 'Lương tăng ca (OT)', amount: otPay, type: 'income' },
    { label: 'Phụ cấp', amount: allowanceTotal, type: 'income' },
    { label: 'Thưởng phát sinh', amount: bonus, type: 'income' },
    { label: 'Bảo hiểm NLĐ đóng', amount: insuranceEmployee, type: 'deduction' },
    { label: 'Thuế TNCN', amount: tax, type: 'deduction' },
    { label: 'Khấu trừ / Tạm ứng', amount: deductionTotal, type: 'deduction' },
    { label: 'Tổng thu nhập (Gross)', amount: grossPay, type: 'summary' },
    { label: 'Lương thực nhận (Net)', amount: netPay, type: 'summary' },
  ];
}

export interface BankExportRow {
  employeeCode: string;
  employeeName: string;
  amount: number;
  content: string;
}

export function validateBankExportRow(row: BankExportRow): { valid: boolean; error?: string } {
  if (!row.employeeCode?.trim())
    return { valid: false, error: 'Mã nhân viên không được để trống.' };
  if (!row.employeeName?.trim())
    return { valid: false, error: 'Tên nhân viên không được để trống.' };
  if (isNaN(row.amount) || row.amount <= 0)
    return { valid: false, error: 'Số tiền chi trả phải lớn hơn 0.' };
  if (!row.content?.trim())
    return { valid: false, error: 'Nội dung chuyển khoản không được để trống.' };
  return { valid: true };
}

export interface CostByOrgRow {
  orgUnitName: string;
  headCount: number;
  totalGross: number;
  totalNet: number;
  totalInsurance: number;
}

export function calculateCostByOrgTotals(rows: CostByOrgRow[]): { totalGross: number; totalNet: number; totalInsurance: number; totalHeadCount: number } {
  return {
    totalGross: rows.reduce((s, r) => s + r.totalGross, 0),
    totalNet: rows.reduce((s, r) => s + r.totalNet, 0),
    totalInsurance: rows.reduce((s, r) => s + r.totalInsurance, 0),
    totalHeadCount: rows.reduce((s, r) => s + r.headCount, 0),
  };
}

export function formatCostByOrgRow(row: CostByOrgRow): string {
  return `🏢 ${row.orgUnitName} | ${row.headCount} người | Gross: ${row.totalGross.toLocaleString('vi-VN')} | Net: ${row.totalNet.toLocaleString('vi-VN')} VNĐ`;
}
