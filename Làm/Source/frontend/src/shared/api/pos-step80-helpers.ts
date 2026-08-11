// pos-step80-helpers.ts
// Frontend helpers cho Bước 80:
//   UC_POS_032 — Tạm tính / giữ đơn (validateHoldSaleRequest, formatSaleHoldBadge)
//   UC_POS_033 — Thanh toán tiền mặt (validateCashPayment, formatCashChange)
//   UC_POS_034 — Thanh toán chuyển khoản / QR (validateTransferPayment, formatTransferNotice)
//   UC_POS_035 — Thanh toán thẻ / ví điện tử (validateCardWalletPayment, formatPaymentMethodBadge)

export function validateHoldSaleRequest(
  itemCount: number,
  status: string,
): { canHold: boolean; reason?: string } {
  if (status !== 'Open') {
    return { canHold: false, reason: 'Chỉ có thể giữ đơn ở trạng thái Open.' };
  }
  if (itemCount <= 0) {
    return { canHold: false, reason: 'Đơn rỗng không có sản phẩm — không thể tạm giữ.' };
  }
  return { canHold: true };
}

export function formatSaleHoldBadge(status: string, note?: string): { label: string; style: string } {
  if (status === 'Held') {
    const noteText = note ? ` (${note})` : '';
    return { label: `⏸️ Đang giữ đơn${noteText}`, style: 'warning' };
  }
  return { label: '▶️ Đơn đang mở', style: 'info' };
}

export function validateCashPayment(
  givenCash: number,
  totalAmount: number,
): { isValid: boolean; change: number; error?: string } {
  if (isNaN(givenCash) || givenCash <= 0) {
    return { isValid: false, change: 0, error: 'Số tiền khách đưa phải lớn hơn 0.' };
  }
  if (givenCash < totalAmount) {
    return {
      isValid: false,
      change: 0,
      error: `Số tiền khách đưa chưa đủ (Thiếu ${(totalAmount - givenCash).toLocaleString('vi-VN')} VNĐ).`,
    };
  }
  const change = Math.max(0, givenCash - totalAmount);
  return { isValid: true, change };
}

export function formatCashChange(givenCash: number, change: number): string {
  return `💵 Tiền khách đưa: ${givenCash.toLocaleString('vi-VN')} VNĐ | Tiền thừa trả lại: ${change.toLocaleString('vi-VN')} VNĐ`;
}

export function validateTransferPayment(
  amount: number,
  totalAmount: number,
): { isValid: boolean; error?: string } {
  if (isNaN(amount) || amount <= 0) {
    return { isValid: false, error: 'Số tiền chuyển khoản phải lớn hơn 0.' };
  }
  if (amount > totalAmount) {
    return { isValid: false, error: 'Số tiền chuyển khoản không được vượt quá tổng đơn.' };
  }
  return { isValid: true };
}

export function formatTransferNotice(bankCode: string, refNo?: string): string {
  const ref = refNo ? ` - Mã GD: ${refNo}` : '';
  return `📱 Chuyển khoản qua ${bankCode}${ref}`;
}

export function validateCardWalletPayment(
  method: 'Card' | 'Wallet',
  amount: number,
  remainingAmount: number,
): { isValid: boolean; error?: string } {
  if (method !== 'Card' && method !== 'Wallet') {
    return { isValid: false, error: 'Phương thức phải là Card hoặc Wallet.' };
  }
  if (isNaN(amount) || amount <= 0) {
    return { isValid: false, error: 'Số tiền thanh toán phải lớn hơn 0.' };
  }
  if (amount > remainingAmount) {
    return { isValid: false, error: 'Số tiền vượt quá số tiền còn lại phải thanh toán.' };
  }
  return { isValid: true };
}

export function formatPaymentMethodBadge(method: string): { label: string; icon: string } {
  switch (method) {
    case 'Cash':
      return { label: 'Tiền mặt', icon: '💵' };
    case 'Transfer':
      return { label: 'Chuyển khoản / QR', icon: '📱' };
    case 'Card':
      return { label: 'Thẻ ngân hàng', icon: '💳' };
    case 'Wallet':
      return { label: 'Ví điện tử', icon: '👛' };
    default:
      return { label: 'Khác', icon: '❓' };
  }
}
