// inv-step97-helpers.ts
// Frontend helpers cho Bước 97:
//   UC_INV_026 — Xuất nội bộ / tiêu hao (validateInternalIssue)
//   UC_INV_029 — Xuất theo FEFO tự động (validateFefoPicking)
//   UC_INV_030 — Xuất điều chỉnh (validateAdjustmentIssue)
//   UC_INV_031 — Tạo phiếu chuyển kho (validateTransferCreate)

export function validateInternalIssue(
  warehouseId: string,
  reason: string,
): { canIssue: boolean; error?: string } {
  if (!warehouseId || warehouseId.trim().length === 0) {
    return { canIssue: false, error: 'Phải chọn kho xuất nội bộ.' };
  }
  if (!reason || reason.trim().length === 0) {
    return { canIssue: false, error: 'Phải ghi rõ lý do xuất nội bộ / tiêu hao.' };
  }
  return { canIssue: true };
}

export function validateFefoPicking(
  requestedQty: number,
  lotAvailableQty: number,
): { canFefoPick: boolean; reason?: string } {
  if (isNaN(requestedQty) || requestedQty <= 0) {
    return { canFefoPick: false, reason: 'Số lượng yêu cầu xuất kho phải > 0.' };
  }
  if (lotAvailableQty < requestedQty) {
    return { canFefoPick: false, reason: 'Số lượng lô cận date không đủ đáp ứng, cần nhặt thêm lô tiếp theo.' };
  }
  return { canFefoPick: true };
}

export function validateAdjustmentIssue(
  warehouseId: string,
  reason: string,
): { canIssue: boolean; error?: string } {
  if (!warehouseId || warehouseId.trim().length === 0) {
    return { canIssue: false, error: 'Phải chọn kho xuất điều chỉnh.' };
  }
  if (!reason || reason.trim().length === 0) {
    return { canIssue: false, error: 'Phải ghi rõ nguyên nhân xuất điều chỉnh (hư hỏng, mất mát, vv).' };
  }
  return { canIssue: true };
}

export function validateTransferCreate(
  fromWarehouseId: string,
  toWarehouseId: string,
): { isValid: boolean; error?: string } {
  if (!fromWarehouseId || fromWarehouseId.trim().length === 0) {
    return { isValid: false, error: 'Chưa chọn kho xuất phát (FromWarehouse).' };
  }
  if (!toWarehouseId || toWarehouseId.trim().length === 0) {
    return { isValid: false, error: 'Chưa chọn kho đích (ToWarehouse).' };
  }
  if (fromWarehouseId === toWarehouseId) {
    return { isValid: false, error: 'Kho xuất phát và kho đích không được trùng nhau.' };
  }
  return { isValid: true };
}
