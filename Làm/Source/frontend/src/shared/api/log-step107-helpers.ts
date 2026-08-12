// log-step107-helpers.ts
// Frontend helpers cho Bước 107:
//   UC_LOG_023 — Bàn giao tiền COD (validateCodHandoverCreate)
//   UC_LOG_024 — Đối soát 3 chiều COD (calculateHandoverVariance)
//   UC_LOG_026 — Xử lý lệch COD (validateVarianceResolutionNote)
//   UC_LOG_027 — Tạo phiếu hoàn về kho (validateReturnOrderCreate)

export function validateCodHandoverCreate(deliveryOrderIds: string[]): { canCreate: boolean; error?: string } {
  if (!deliveryOrderIds || deliveryOrderIds.length === 0) {
    return { canCreate: false, error: 'Phải chọn ít nhất 1 đơn giao hàng COD đã thu tiền.' };
  }
  return { canCreate: true };
}

export function calculateHandoverVariance(
  expectedAmount: number,
  remittedAmount: number,
): { varianceAmount: number; isMatched: boolean } {
  const varianceAmount = expectedAmount - remittedAmount;
  const isMatched = Math.abs(varianceAmount) < 0.01;
  return { varianceAmount, isMatched };
}

export function validateVarianceResolutionNote(note: string): { isValid: boolean; error?: string } {
  if (!note || note.trim().length < 3) {
    return { isValid: false, error: 'Ghi chú giải trình xử lý chênh lệch COD phải từ 3 ký tự trở lên.' };
  }
  return { isValid: true };
}

export function validateReturnOrderCreate(deliveryStatus: string): { canReturn: boolean; reason?: string } {
  const allowed = new Set(['Dispatched', 'InTransit', 'Failed', 'Delivered']);
  if (!allowed.has(deliveryStatus)) {
    return { canReturn: false, reason: 'Chỉ có thể hoàn trả các đơn đã xuất kho, đang giao, thất bại hoặc đã giao.' };
  }
  return { canReturn: true };
}
