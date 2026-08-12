// log-step106-helpers.ts
// Frontend helpers cho Bước 106:
//   UC_LOG_014 — Cập nhật trạng thái vận đơn (validateStatusUpdate)
//   UC_LOG_017 — Ghi nhận giao thất bại (formatDeliveryFailureReason)
//   UC_LOG_021 — Ghi nhận số tiền COD (validateCodAmount)
//   UC_LOG_022 — Xác nhận đã thu COD (validateCodCollectConfirmation)

const ValidStatuses = new Set(['InTransit', 'Delivered', 'Failed', 'Dispatched', 'Ready', 'Picking', 'Confirmed']);

export function validateStatusUpdate(status: string): { isValid: boolean; error?: string } {
  if (!status || !ValidStatuses.has(status.trim())) {
    return { isValid: false, error: 'Trạng thái vận đơn không hợp lệ.' };
  }
  return { isValid: true };
}

export function formatDeliveryFailureReason(reason: string): { formattedReason: string } {
  const trimmed = reason ? reason.trim() : '';
  const text = trimmed.length > 0 ? trimmed : 'Không liên lạc được khách hàng';
  return { formattedReason: `Lý do giao thất bại: ${text}` };
}

export function validateCodAmount(amount: number): { isValid: boolean; error?: string } {
  if (isNaN(amount) || amount <= 0) {
    return { isValid: false, error: 'Số tiền COD phải lớn hơn 0.' };
  }
  return { isValid: true };
}

export function validateCodCollectConfirmation(codStatus: string): { canCollect: boolean; reason?: string } {
  if (codStatus !== 'Pending') {
    return { canCollect: false, reason: 'Chỉ có thể xác nhận thu khi khoản COD đang ở trạng thái Chờ thu (Pending).' };
  }
  return { canCollect: true };
}
