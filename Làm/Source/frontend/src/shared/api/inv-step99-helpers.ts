// inv-step99-helpers.ts
// Frontend helpers cho Bước 99:
//   UC_INV_038 — Giải phóng giữ hàng (validateReservationRelease)
//   UC_INV_039 — Xem tồn thực tế (calculateAvailableQty)
//   UC_INV_041 — Xem tồn đang giữ / đang chuyển (formatBalanceSummary)
//   UC_INV_042 — Cảnh báo không đủ tồn (validateMinMaxStockAlert)

export function validateReservationRelease(status: string): { canRelease: boolean; reason?: string } {
  if (status === 'Released') {
    return { canRelease: false, reason: 'Lệnh giữ hàng đã được giải phóng trước đó.' };
  }
  if (status === 'Cancelled') {
    return { canRelease: false, reason: 'Lệnh giữ hàng đã bị hủy.' };
  }
  return { canRelease: true };
}

export function calculateAvailableQty(
  onHand: number,
  reserved: number,
  inTransit: number,
): { availableQty: number } {
  const availableQty = Math.max(0, onHand - reserved + inTransit);
  return { availableQty };
}

export function formatBalanceSummary(
  onHand: number,
  reserved: number,
  inTransit: number,
): { summaryText: string } {
  return {
    summaryText: `Tồn kho: ${onHand} | Đang giữ: ${reserved} | Đang chuyển: ${inTransit}`,
  };
}

export function validateMinMaxStockAlert(
  qtyAvailable: number,
  minQty?: number,
  maxQty?: number,
): { alertType: 'BelowMin' | 'AboveMax' | 'Normal'; message?: string } {
  if (minQty !== undefined && minQty !== null && qtyAvailable < minQty) {
    return { alertType: 'BelowMin', message: `Tồn kho khả dụng (${qtyAvailable}) dưới định mức tối thiểu (${minQty}).` };
  }
  if (maxQty !== undefined && maxQty !== null && qtyAvailable > maxQty) {
    return { alertType: 'AboveMax', message: `Tồn kho khả dụng (${qtyAvailable}) vượt định mức tối đa (${maxQty}).` };
  }
  return { alertType: 'Normal' };
}
