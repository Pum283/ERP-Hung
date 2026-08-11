// crm-step74-helpers.ts
// Frontend helpers cho Bước 74:
//   UC_CRM_081 — Cập nhật trạng thái đơn (formatOrderStatusBadge, getAvailableStatusTransitions)
//   UC_CRM_082 — Giữ tồn khi duyệt đơn (formatStockHoldNotice, validateStockHoldEligibility)
//   UC_CRM_083 — Tách / gộp đơn (validateSplitRequest, formatSplitOrderNotice, formatMergeOrderNotice)
//   UC_CRM_084 — Hủy đơn có kiểm soát (validateCancelRequest, formatCancelConfirmation)

const ORDER_STATUS_MAP: Record<string, { label: string; icon: string; color: string }> = {
  Draft:     { label: 'Nháp',         icon: '📝', color: '#6b7280' },
  Confirmed: { label: 'Đã xác nhận', icon: '✅', color: '#16a34a' },
  Holding:   { label: 'Đang giữ tồn', icon: '📦', color: '#2563eb' },
  Released:  { label: 'Đã xuất kho',  icon: '🚚', color: '#7c3aed' },
  Cancelled: { label: 'Đã hủy',       icon: '❌', color: '#dc2626' },
  Delivered: { label: 'Đã giao',      icon: '🎉', color: '#059669' },
};

const TERMINAL_STATUSES = ['Cancelled', 'Delivered'] as const;

export function formatOrderStatusBadge(status: string): { label: string; icon: string; color: string } {
  return ORDER_STATUS_MAP[status] ?? { label: status, icon: '❓', color: '#9ca3af' };
}

export function getAvailableStatusTransitions(currentStatus: string): string[] {
  switch (currentStatus) {
    case 'Draft':     return ['Confirmed'];
    case 'Confirmed': return ['Holding', 'Released', 'Delivered'];
    case 'Holding':   return ['Released', 'Delivered'];
    case 'Released':  return ['Delivered'];
    default:          return [];
  }
}

export function formatStockHoldNotice(
  stockHoldStatus: string,
  orderCode: string,
): { message: string; canHold: boolean } {
  switch (stockHoldStatus) {
    case 'Held':
      return { message: `📦 Đơn #${orderCode} đã giữ tồn kho thành công.`, canHold: false };
    case 'Released':
      return { message: `🔓 Tồn kho đơn #${orderCode} đã được giải phóng.`, canHold: false };
    default:
      return { message: `⏳ Đơn #${orderCode} chưa giữ tồn — có thể thực hiện giữ tồn.`, canHold: true };
  }
}

export function validateStockHoldEligibility(
  status: string,
  stockHoldStatus: string,
  lineCount: number,
): { eligible: boolean; reason?: string } {
  if (TERMINAL_STATUSES.includes(status as typeof TERMINAL_STATUSES[number])) {
    return { eligible: false, reason: `Đơn ở trạng thái "${status}" — không thể giữ tồn.` };
  }
  if (stockHoldStatus === 'Held') {
    return { eligible: false, reason: 'Đơn đã giữ tồn — không cần giữ lại.' };
  }
  if (lineCount === 0) {
    return { eligible: false, reason: 'Đơn chưa có dòng hàng — không thể giữ tồn.' };
  }
  return { eligible: true };
}

export function validateSplitRequest(
  lineIds: string[],
  totalLineCount: number,
  orderStatus: string,
): { isValid: boolean; error?: string } {
  if (TERMINAL_STATUSES.includes(orderStatus as typeof TERMINAL_STATUSES[number])) {
    return { isValid: false, error: `Đơn ở trạng thái "${orderStatus}" — không thể tách.` };
  }
  if (!lineIds || lineIds.length === 0) {
    return { isValid: false, error: 'Chọn ít nhất 1 dòng hàng để tách.' };
  }
  if (lineIds.length >= totalLineCount) {
    return { isValid: false, error: 'Không thể tách tất cả dòng hàng — đơn gốc sẽ trống.' };
  }
  return { isValid: true };
}

export function formatSplitOrderNotice(
  originalCode: string,
  newCode: string,
  movedLineCount: number,
): string {
  return `✂️ Đã tách ${movedLineCount} dòng hàng từ đơn "${originalCode}" → Đơn mới "${newCode}".`;
}

export function formatMergeOrderNotice(
  primaryCode: string,
  secondaryCode: string,
): string {
  return `🔗 Đã gộp đơn phụ "${secondaryCode}" vào đơn chính "${primaryCode}". Đơn phụ đã bị hủy.`;
}

export function validateCancelRequest(
  reason: string,
  orderStatus: string,
): { isValid: boolean; error?: string } {
  if (orderStatus === 'Delivered') {
    return { isValid: false, error: 'Đơn đã giao hàng — không thể hủy.' };
  }
  if (orderStatus === 'Cancelled') {
    return { isValid: false, error: 'Đơn đã hủy trước đó.' };
  }
  const trimmed = (reason || '').trim();
  if (trimmed.length === 0) {
    return { isValid: false, error: 'Bắt buộc nhập lý do hủy đơn.' };
  }
  if (trimmed.length > 500) {
    return { isValid: false, error: 'Lý do hủy tối đa 500 ký tự.' };
  }
  return { isValid: true };
}

export function formatCancelConfirmation(orderCode: string, reason: string): string {
  return `❌ Đơn hàng #${orderCode} đã hủy thành công.\nLý do: ${reason}`;
}
