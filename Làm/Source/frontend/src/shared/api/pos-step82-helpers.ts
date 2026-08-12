// pos-step82-helpers.ts
// Frontend helpers cho Bước 82:
//   UC_POS_042 — Mở ca thu ngân (validateOpenShiftRequest)
//   UC_POS_043 — Nhập tiền đầu ca (validateInitialCash)
//   UC_POS_045 — Xem doanh thu trong ca (formatShiftRevenueSummary)
//   UC_POS_046 — Đóng ca & đếm quỹ (validateCloseShiftRequest)

export function validateOpenShiftRequest(
  storeId?: string,
  hasActiveShift: boolean = false,
): { canOpen: boolean; error?: string } {
  if (!storeId || storeId.trim().length === 0) {
    return { canOpen: false, error: 'Phải chọn điểm bán (Store) trước khi mở ca.' };
  }
  if (hasActiveShift) {
    return { canOpen: false, error: 'Thu ngân / máy POS này đang có ca mở chưa đóng.' };
  }
  return { canOpen: true };
}

export function validateInitialCash(cashAmount: number): { isValid: boolean; error?: string } {
  if (isNaN(cashAmount) || cashAmount < 0) {
    return { isValid: false, error: 'Số tiền đầu ca (tiền lẻ) không được là số âm.' };
  }
  return { isValid: true };
}

export function formatShiftRevenueSummary(
  openingCash: number,
  cashTotal: number,
  cardWalletTotal: number,
  totalSalesCount: number,
): string {
  const totalRev = cashTotal + cardWalletTotal;
  const expectedCashInDrawer = openingCash + cashTotal;
  return `📊 Số lượng đơn: ${totalSalesCount} | Doanh thu: ${totalRev.toLocaleString('vi-VN')} VNĐ (Tiền mặt: ${cashTotal.toLocaleString('vi-VN')} | Thẻ/Ví/CK: ${cardWalletTotal.toLocaleString('vi-VN')}) | Tiền mặt dự kiến trong quỹ: ${expectedCashInDrawer.toLocaleString('vi-VN')} VNĐ`;
}

export function validateCloseShiftRequest(
  shiftStatus: string,
  closingCashCounted: number,
): { canClose: boolean; error?: string } {
  if (shiftStatus !== 'Open') {
    return { canClose: false, error: 'Ca bán không ở trạng thái mở (Open) — không thể đóng ca.' };
  }
  if (isNaN(closingCashCounted) || closingCashCounted < 0) {
    return { canClose: false, error: 'Số tiền thực tế đếm trong quỹ không được âm.' };
  }
  return { canClose: true };
}
