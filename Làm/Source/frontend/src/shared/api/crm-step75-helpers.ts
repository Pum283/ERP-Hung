// crm-step75-helpers.ts
// Frontend helpers cho Bước 75:
//   UC_CRM_085 — Trả hàng / điều chỉnh đơn (validateReturnOrderRequest, formatOrderReturnNotice)
//   UC_CRM_086 — Gắn hợp đồng (validateLinkContractRequest, formatContractLinkNotice)
//   UC_CRM_087 — Theo dõi thanh toán (validateOrderPayment, formatOrderPaymentSummary)
//   UC_CRM_088 — Đẩy đơn sang kho / giao vận (validatePushToWarehouseEligibility, formatWarehousePushNotice)

const ALLOWED_PAY_METHODS = ['Cash', 'Transfer', 'Card', 'Other'] as const;

export function validateReturnOrderRequest(
  reason: string,
  orderStatus: string,
): { isValid: boolean; error?: string } {
  if (orderStatus === 'Cancelled') {
    return { isValid: false, error: 'Đơn hàng đã hủy — không thể thực hiện trả hàng.' };
  }
  if (orderStatus === 'Draft') {
    return { isValid: false, error: 'Đơn hàng nháp chưa phát sinh giao dịch — không thể trả hàng.' };
  }
  const trimmed = (reason || '').trim();
  if (trimmed.length === 0) {
    return { isValid: false, error: 'Bắt buộc nhập lý do trả hàng / điều chỉnh.' };
  }
  if (trimmed.length > 500) {
    return { isValid: false, error: 'Lý do trả hàng tối đa 500 ký tự.' };
  }
  return { isValid: true };
}

export function formatOrderReturnNotice(orderCode: string, reason: string): string {
  return `↩️ Đã tiếp nhận yêu cầu trả hàng / điều chỉnh cho đơn #${orderCode}.\nLý do: ${reason}`;
}

export function validateLinkContractRequest(
  contractId?: string,
): { isValid: boolean; error?: string } {
  if (!contractId || contractId.trim().length === 0) {
    return { isValid: false, error: 'Vui lòng chọn hợp đồng để gắn với đơn hàng.' };
  }
  return { isValid: true };
}

export function formatContractLinkNotice(orderCode: string, contractCode?: string): string {
  const code = contractCode ? `"${contractCode}"` : 'hợp đồng chỉ định';
  return `📜 Đơn hàng #${orderCode} đã được liên kết với hợp đồng ${code}.`;
}

export function validateOrderPayment(
  amount: number,
  remainingAmount: number,
  method: string,
): { isValid: boolean; error?: string } {
  if (isNaN(amount) || amount <= 0) {
    return { isValid: false, error: 'Số tiền thanh toán phải lớn hơn 0.' };
  }
  if (amount > remainingAmount + 0.01) {
    return { isValid: false, error: `Số tiền thanh toán vượt quá số tiền còn lại (${remainingAmount.toLocaleString('vi-VN')} VNĐ).` };
  }
  const normalizedMethod = (method || '').trim();
  if (!ALLOWED_PAY_METHODS.some(m => m.toLowerCase() === normalizedMethod.toLowerCase())) {
    return { isValid: false, error: 'Phương thức thanh toán không hợp lệ (Cash | Transfer | Card | Other).' };
  }
  return { isValid: true };
}

export function formatOrderPaymentSummary(
  paidAmount: number,
  totalAmount: number,
): { paidText: string; remainText: string; percent: number; status: 'Unpaid' | 'Partial' | 'Paid' } {
  const total = Math.max(0, totalAmount);
  const paid = Math.max(0, Math.min(paidAmount, total));
  const remain = Math.max(0, total - paid);
  const percent = total > 0 ? Math.min(100, Math.round((paid / total) * 100)) : 100;

  let status: 'Unpaid' | 'Partial' | 'Paid' = 'Unpaid';
  if (paid >= total && total > 0) {
    status = 'Paid';
  } else if (paid > 0) {
    status = 'Partial';
  }

  return {
    paidText: `${paid.toLocaleString('vi-VN')} VNĐ`,
    remainText: `${remain.toLocaleString('vi-VN')} VNĐ`,
    percent,
    status,
  };
}

export function validatePushToWarehouseEligibility(
  orderStatus: string,
  lineCount: number,
): { canPush: boolean; reason?: string } {
  if (orderStatus === 'Cancelled') {
    return { canPush: false, reason: 'Đơn hàng đã hủy — không thể đẩy kho.' };
  }
  if (orderStatus === 'Draft') {
    return { canPush: false, reason: 'Cần xác nhận đơn hàng trước khi đẩy sang kho/giao vận.' };
  }
  if (lineCount <= 0) {
    return { canPush: false, reason: 'Đơn hàng chưa có dòng sản phẩm nào — không thể đẩy kho.' };
  }
  return { canPush: true };
}

export function formatWarehousePushNotice(
  orderCode: string,
  warehousePushStatus: string,
): { label: string; icon: string; style: string } {
  switch (warehousePushStatus) {
    case 'Pushed':
      return { label: `🚚 Đơn #${orderCode} đã đẩy kho / tạo lệnh giao vận.`, icon: 'truck', style: 'success' };
    case 'Failed':
      return { label: `⚠️ Đẩy kho đơn #${orderCode} thất bại. Vui lòng thử lại.`, icon: 'alert', style: 'error' };
    default:
      return { label: `📦 Đơn #${orderCode} chưa đẩy kho.`, icon: 'package', style: 'info' };
  }
}
