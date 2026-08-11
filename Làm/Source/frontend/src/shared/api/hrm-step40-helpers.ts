// hrm-step40-helpers.ts
// Frontend helpers cho Bước 40:
//   UC_HRM_151 — Báo cáo nghỉ việc / lý do (groupTurnoverByReason)
//   UC_HRM_152 — Tạo thang bậc lương (validateSalaryGradeInput & filterSalaryGrades)
//   UC_HRM_153 — Gán bậc lương theo nhân sự (validateEmployeeSalaryInput)
//   UC_HRM_154 — Cấu hình phụ cấp theo bậc lương (validateAllowanceTypeInput)

export interface SalaryGradeInput {
  code: string;
  name: string;
  level: number;
  baseAmount: number;
}

export function validateSalaryGradeInput(input: SalaryGradeInput): { valid: boolean; error?: string } {
  const code = (input.code ?? '').trim();
  if (!code || code.length > 40)
    return { valid: false, error: 'Mã bậc lương từ 1 đến 40 ký tự.' };

  const name = (input.name ?? '').trim();
  if (!name || name.length > 100)
    return { valid: false, error: 'Tên bậc lương từ 1 đến 100 ký tự.' };

  if (isNaN(input.baseAmount) || input.baseAmount < 0)
    return { valid: false, error: 'Mức lương cơ bản không được âm.' };

  return { valid: true };
}

export interface EmployeeSalaryInput {
  employeeId: string;
  salaryGradeId?: string;
  baseSalary: number;
  effectiveFrom: string;
}

export function validateEmployeeSalaryInput(input: EmployeeSalaryInput): { valid: boolean; error?: string } {
  if (!input.employeeId?.trim())
    return { valid: false, error: 'Vui lòng chọn nhân viên.' };

  if (isNaN(input.baseSalary) || input.baseSalary < 0)
    return { valid: false, error: 'Lương cơ bản không được âm.' };

  if (!input.effectiveFrom?.trim())
    return { valid: false, error: 'Vui lòng nhập ngày bắt đầu áp dụng lương.' };

  return { valid: true };
}

export interface AllowanceTypeInput {
  code: string;
  name: string;
  defaultAmount: number;
  isTaxable: boolean;
}

export function validateAllowanceTypeInput(input: AllowanceTypeInput): { valid: boolean; error?: string } {
  const code = (input.code ?? '').trim();
  if (!code || code.length > 40)
    return { valid: false, error: 'Mã phụ cấp từ 1 đến 40 ký tự.' };

  const name = (input.name ?? '').trim();
  if (!name || name.length > 100)
    return { valid: false, error: 'Tên loại phụ cấp từ 1 đến 100 ký tự.' };

  if (isNaN(input.defaultAmount) || input.defaultAmount < 0)
    return { valid: false, error: 'Số tiền phụ cấp mặc định không được âm.' };

  return { valid: true };
}

export interface SalaryGradeItem {
  id: string;
  code: string;
  name: string;
  level: number;
  baseAmount: number;
  isActive: boolean;
}

export function filterSalaryGrades(items: SalaryGradeItem[], activeOnly = false): SalaryGradeItem[] {
  if (!items || items.length === 0) return [];
  const list = activeOnly ? items.filter((g) => g.isActive) : [...items];
  return list.sort((a, b) => a.level - b.level);
}
