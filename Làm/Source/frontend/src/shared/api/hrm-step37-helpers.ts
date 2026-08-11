// hrm-step37-helpers.ts
// Frontend helpers cho Bước 37:
//   UC_HRM_139 — Ghi nhận quyết định khen thưởng (validateRewardDisciplineInput)
//   UC_HRM_140 — Ghi nhận quyết định kỷ luật (validateRewardDisciplineInput)
//   UC_HRM_141 — Theo dõi chấp hành & Áp dụng lương (formatRewardDisciplineStatus)
//   UC_HRM_143 — Lịch sử khen thưởng / kỷ luật (filterRewardDisciplineHistory & validateDecisionAttach)

export interface RewardDisciplineInput {
  employeeId: string;
  kind: string; // 'Reward' | 'Discipline'
  title: string;
  decisionDate: string;
  payrollImpactAmount?: number;
  payrollImpactKind?: string; // 'None' | 'Bonus' | 'Deduction' | 'Allowance'
}

export function validateRewardDisciplineInput(input: RewardDisciplineInput): { valid: boolean; error?: string } {
  if (!input.employeeId?.trim())
    return { valid: false, error: 'Vui lòng chọn nhân viên.' };

  const kind = (input.kind ?? '').trim();
  if (kind !== 'Reward' && kind !== 'Discipline')
    return { valid: false, error: 'Loại quyết định phải là Khen thưởng (Reward) hoặc Kỷ luật (Discipline).' };

  const title = (input.title ?? '').trim();
  if (!title)
    return { valid: false, error: 'Vui lòng nhập tiêu đề quyết định.' };

  if (title.length > 200)
    return { valid: false, error: 'Tiêu đề quyết định không được vượt quá 200 ký tự.' };

  const impactKind = (input.payrollImpactKind ?? 'None').trim();
  const validImpacts = ['None', 'Bonus', 'Deduction', 'Allowance'];
  if (!validImpacts.includes(impactKind))
    return { valid: false, error: 'Hình thức ảnh hưởng lương không hợp lệ.' };

  if (input.payrollImpactAmount !== undefined && input.payrollImpactAmount < 0)
    return { valid: false, error: 'Số tiền ảnh hưởng lương không được âm.' };

  return { valid: true };
}

export function validateDecisionAttach(storageKey: string): { valid: boolean; error?: string } {
  const key = (storageKey ?? '').trim();
  if (!key)
    return { valid: false, error: 'Vui lòng cung cấp mã lưu trữ file văn bản quyết định (storageKey).' };

  return { valid: true };
}

export interface DecisionHistoryEntry {
  id: string;
  employeeId: string;
  employeeName: string;
  kind: string;
  title: string;
  decisionDate: string;
  status: string;
}

export function filterRewardDisciplineHistory(
  items: DecisionHistoryEntry[],
  kindFilter?: string,
  employeeIdFilter?: string
): DecisionHistoryEntry[] {
  if (!items || items.length === 0) return [];
  return items.filter((item) => {
    if (kindFilter && item.kind !== kindFilter) return false;
    if (employeeIdFilter && item.employeeId !== employeeIdFilter) return false;
    return true;
  });
}

export function formatRewardDisciplineStatus(status: string): string {
  switch (status) {
    case 'Issued':
      return '📋 Đã ban hành';
    case 'Applied':
      return '✅ Đã áp dụng lương';
    case 'Cancelled':
      return '❌ Đã hủy bỏ';
    default:
      return status || 'Chưa xác định';
  }
}
