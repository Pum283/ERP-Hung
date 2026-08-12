// log-step108-helpers.ts
// Frontend helpers cho Bước 108:
//   UC_LOG_028 — Kiểm đếm hàng hoàn (validateReturnedItemInspection)
//   UC_LOG_029 — Nhập kho hàng hoàn (validateRestockReceipt)
//   UC_LOG_034 — Tỷ lệ giao đúng hạn (calculateOnTimeDeliveryRate)
//   UC_LOG_035 — Tỷ lệ hoàn / thất bại (calculateReturnFailureRate)

export function validateReturnedItemInspection(
  damagedQty: number,
  totalQty: number,
): { isValid: boolean; error?: string } {
  if (isNaN(damagedQty) || damagedQty < 0) {
    return { isValid: false, error: 'Số lượng hỏng/hư hại phải >= 0.' };
  }
  if (damagedQty > totalQty) {
    return { isValid: false, error: 'Số lượng hư hại vượt quá tổng số lượng hoàn trả.' };
  }
  return { isValid: true };
}

export function validateRestockReceipt(warehouseId: string): { canRestock: boolean; error?: string } {
  if (!warehouseId || warehouseId.trim().length === 0) {
    return { canRestock: false, error: 'Phải chọn nhà kho nhập lại hàng hoàn.' };
  }
  return { canRestock: true };
}

export function calculateOnTimeDeliveryRate(
  onTimeCount: number,
  totalDelivered: number,
): { onTimeRatePct: number } {
  if (totalDelivered <= 0) return { onTimeRatePct: 0 };
  const rate = (onTimeCount / totalDelivered) * 100;
  return { onTimeRatePct: Math.min(100, Math.max(0, Math.round(rate * 100) / 100)) };
}

export function calculateReturnFailureRate(
  failedCount: number,
  returnedCount: number,
  totalShipments: number,
): { returnRatePct: number } {
  if (totalShipments <= 0) return { returnRatePct: 0 };
  const rate = ((failedCount + returnedCount) / totalShipments) * 100;
  return { returnRatePct: Math.min(100, Math.max(0, Math.round(rate * 100) / 100)) };
}
