// hrm-step39-helpers.ts
// Frontend helpers cho Bước 39:
//   UC_HRM_147 — Checklist bàn giao (calculateHandoverProgress)
//   UC_HRM_148 — Thu hồi quyền hệ thống (formatRevokeStatus)
//   UC_HRM_149 — Quyết toán phép / lương nghỉ việc (validateFinalSettlementInput)
//   UC_HRM_150 — Phỏng vấn nghỉ việc & Hoàn tất (validateExitInterviewNotes & formatOffboardingStatus)

export interface HandoverChecklistItem {
  key: string;
  label: string;
  done: boolean;
}

export function calculateHandoverProgress(items: HandoverChecklistItem[]): { completed: number; total: number; percentage: number } {
  if (!items || items.length === 0) return { completed: 0, total: 0, percentage: 0 };
  const completed = items.filter((i) => i.done).length;
  const total = items.length;
  const percentage = Math.round((completed / total) * 100);
  return { completed, total, percentage };
}

export interface FinalSettlementInput {
  leaveDaysRemaining: number;
  leaveSettlementAmount: number;
  finalPayEstimate: number;
  settlementNote?: string;
}

export function validateFinalSettlementInput(input: FinalSettlementInput): { valid: boolean; error?: string } {
  if (isNaN(input.leaveSettlementAmount) || input.leaveSettlementAmount < 0)
    return { valid: false, error: 'Số tiền thanh toán phép tồn không được âm.' };

  if (isNaN(input.finalPayEstimate) || input.finalPayEstimate < 0)
    return { valid: false, error: 'Tổng tiền thực nhận dự kiến không được âm.' };

  return { valid: true };
}

export function validateExitInterviewNotes(notes: string): { valid: boolean; error?: string } {
  const text = (notes ?? '').trim();
  if (!text)
    return { valid: false, error: 'Vui lòng nhập ghi chú phỏng vấn nghỉ việc.' };

  if (text.length < 5 || text.length > 1000)
    return { valid: false, error: 'Ghi chú phỏng vấn nghỉ việc phải từ 5 đến 1000 ký tự.' };

  return { valid: true };
}

export function formatOffboardingStatus(status: string): string {
  switch (status) {
    case 'Draft':
      return '📝 Nháp';
    case 'Submitted':
      return '📤 Đã nộp đơn';
    case 'Approved':
      return '✅ Đã duyệt đơn';
    case 'InProgress':
      return '⏳ Đang làm thủ tục bàn giao';
    case 'Completed':
      return '🏁 Đã hoàn tất nghỉ việc';
    case 'Rejected':
      return '❌ Bị từ chối';
    case 'Cancelled':
      return '🚫 Đã hủy đơn';
    default:
      return status || 'Chưa xác định';
  }
}
