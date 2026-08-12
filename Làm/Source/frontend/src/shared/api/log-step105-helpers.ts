// log-step105-helpers.ts
// Frontend helpers cho Bước 105:
//   UC_LOG_009 — Pick list / soạn hàng (validatePickCompletion)
//   UC_LOG_011 — In vận đơn / phiếu giao (validateWaybillPrint)
//   UC_LOG_012 — Hủy / hoàn lệnh giao (validateDeliveryCancellation)
//   UC_LOG_013 — Phân công tài xế / đơn vị vận chuyển (validateDriverAssignment)

export function validatePickCompletion(
  pickedQty: number,
  orderedQty: number,
): { isComplete: boolean; error?: string } {
  if (isNaN(pickedQty) || pickedQty < 0) {
    return { isComplete: false, error: 'Số lượng đếm soạn hàng phải >= 0.' };
  }
  if (pickedQty > orderedQty) {
    return { isComplete: false, error: 'Số lượng soạn kho vượt quá số lượng đặt trên lệnh giao.' };
  }
  const isComplete = pickedQty === orderedQty;
  return { isComplete };
}

export function validateWaybillPrint(status: string): { canPrint: boolean; reason?: string } {
  if (status === 'Draft' || status === 'Cancelled') {
    return { canPrint: false, reason: 'Không thể in vận đơn cho đơn giao hàng ở trạng thái Dự thảo hoặc Đã hủy.' };
  }
  return { canPrint: true };
}

export function validateDeliveryCancellation(status: string): { canCancel: boolean; reason?: string } {
  if (status === 'Delivered' || status === 'Returned' || status === 'Cancelled') {
    return { canCancel: false, reason: 'Đơn giao hàng đã hoàn thành, hoàn trả hoặc đã bị hủy từ trước.' };
  }
  return { canCancel: true };
}

export function validateDriverAssignment(
  driverName?: string,
  carrierId?: string,
): { hasAssignee: boolean; message?: string } {
  const hasDriver = !!(driverName && driverName.trim().length > 0);
  const hasCarrier = !!(carrierId && carrierId.trim().length > 0);
  if (!hasDriver && !hasCarrier) {
    return { hasAssignee: false, message: 'Chưa chọn Tài xế hoặc Đơn vị vận chuyển (ĐVVC).' };
  }
  return { hasAssignee: true };
}
