export interface SkillItem {
  id: string;
  employeeId: string;
  skillName: string;
  proficiencyLevel: 'Basic' | 'Intermediate' | 'Advanced' | 'Expert' | string;
  certificateRef?: string | null;
}

export interface EmployeeMovementItem {
  id: string;
  status: 'Active' | 'OnLeave' | 'Terminated' | string;
  hireDate?: string | null;
  terminateDate?: string | null;
}

export interface CandidateImportRow {
  fullName: string;
  email?: string | null;
  phone?: string | null;
  jobPostingId: string;
}

/**
 * Validate cấp độ kỹ năng
 */
export function validateSkillProficiency(level: string): { isValid: boolean; normalized: string } {
  const valid = ['Basic', 'Intermediate', 'Advanced', 'Expert'];
  const found = valid.find((v) => v.toLowerCase() === level.trim().toLowerCase());
  return {
    isValid: !!found,
    normalized: found || 'Intermediate',
  };
}

/**
 * Tính toán thống kê biến động nhân sự
 */
export function calculateMovementStats(
  employees: EmployeeMovementItem[],
  fromDateStr: string,
  toDateStr: string
): {
  total: number;
  active: number;
  onLeave: number;
  terminated: number;
  joiners: number;
  leavers: number;
  turnoverRate: number;
} {
  const fromDate = new Date(fromDateStr);
  const toDate = new Date(toDateStr);

  const total = employees.length;
  const active = employees.filter((e) => e.status === 'Active').length;
  const onLeave = employees.filter((e) => e.status === 'OnLeave').length;
  const terminated = employees.filter((e) => e.status === 'Terminated').length;

  const joiners = employees.filter((e) => {
    if (!e.hireDate) return false;
    const d = new Date(e.hireDate);
    return d >= fromDate && d <= toDate;
  }).length;

  const leavers = employees.filter((e) => {
    if (!e.terminateDate) return false;
    const d = new Date(e.terminateDate);
    return d >= fromDate && d <= toDate;
  }).length;

  const turnoverRate = total > 0 ? Number(((leavers / total) * 100).toFixed(2)) : 0;

  return {
    total,
    active,
    onLeave,
    terminated,
    joiners,
    leavers,
    turnoverRate,
  };
}

/**
 * Formatter in mẫu hợp đồng
 */
export function renderContractTemplate(info: {
  contractNo: string;
  employeeName: string;
  employeeCode: string;
  contractType: string;
  startDate: string;
  endDate?: string | null;
  baseSalary?: number | null;
}): string {
  const salaryText = info.baseSalary ? `${info.baseSalary.toLocaleString('vi-VN')} VNĐ` : 'Thỏa thuận';
  return `HỢP ĐỒNG LAO ĐỘNG (${info.contractType.toUpperCase()})
Số: ${info.contractNo}
Bên A: Công ty ERP Hùng
Bên B: ${info.employeeName} (Mã NV: ${info.employeeCode})
Thời hạn: Từ ${info.startDate} đến ${info.endDate || 'Vô thời hạn'}
Mức lương chính: ${salaryText}`;
}

/**
 * Validate hàng ứng viên trong danh sách import
 */
export function validateBulkCandidateRow(row: CandidateImportRow): { isValid: boolean; error?: string } {
  if (!row.fullName || !row.fullName.trim()) {
    return { isValid: false, error: 'Họ tên ứng viên không được để trống.' };
  }
  if (!row.jobPostingId || !row.jobPostingId.trim()) {
    return { isValid: false, error: 'Chưa chọn vị trí tuyển dụng.' };
  }
  if (row.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(row.email.trim())) {
    return { isValid: false, error: 'Email không đúng định dạng.' };
  }
  return { isValid: true };
}
