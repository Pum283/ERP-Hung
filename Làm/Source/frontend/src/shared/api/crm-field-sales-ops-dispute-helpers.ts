export interface SeverityBadgeResult {
  label: string;
  badgeClass: string;
}

export function evaluateComplaintSeverityBadge(severity: string): SeverityBadgeResult {
  switch (severity?.toLowerCase()) {
    case 'critical':
      return { label: 'Rất nghiêm trọng (Critical)', badgeClass: 'bg-rose-100 text-rose-800 border-rose-300' };
    case 'high':
      return { label: 'Nghiêm trọng (High)', badgeClass: 'bg-amber-100 text-amber-800 border-amber-300' };
    case 'medium':
      return { label: 'Trung bình (Medium)', badgeClass: 'bg-blue-100 text-blue-800 border-blue-300' };
    default:
      return { label: 'Thấp (Low)', badgeClass: 'bg-slate-100 text-slate-800 border-slate-300' };
  }
}

export function calculateReconciliationMatchRate(matched: number, total: number): number {
  if (!total || total <= 0) return 100;
  const rate = (matched / total) * 100;
  return Math.round(rate * 10) / 10;
}

export function validateComplaintForm(orderId: string, reason: string): { isValid: boolean; error?: string } {
  if (!orderId || !orderId.trim()) {
    return { isValid: false, error: 'Mã đơn hàng khiếu nại không được để trống.' };
  }
  if (!reason || !reason.trim() || reason.trim().length < 5) {
    return { isValid: false, error: 'Nội dung khiếu nại phải chứa ít nhất 5 ký tự.' };
  }
  return { isValid: true };
}
