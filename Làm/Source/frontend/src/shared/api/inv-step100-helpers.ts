// inv-step100-helpers.ts
// Frontend helpers cho Bước 100:
//   UC_INV_043 — Theo dõi tồn theo lô (validateLotTracking)
//   UC_INV_044 — Cảnh báo cận date / quá date (calculateDaysToExpiry)
//   UC_INV_045 — Chặn xuất hàng quá HSD (validateExpiryForIssue)
//   UC_INV_048 — Báo cáo hàng sắp hết hạn (filterNearExpiryLots)

export function validateLotTracking(lotCode?: string): { isTracked: boolean; error?: string } {
  if (!lotCode || lotCode.trim().length === 0) {
    return { isTracked: false, error: 'SKU quản lý theo lô yêu cầu nhập Mã Lô.' };
  }
  return { isTracked: true };
}

export function calculateDaysToExpiry(expiryDateStr: string): {
  daysRemaining: number;
  isExpired: boolean;
  isNearExpiry: boolean;
} {
  const expiry = new Date(expiryDateStr).getTime();
  const today = new Date().getTime();
  const diffTime = expiry - today;
  const daysRemaining = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

  const isExpired = daysRemaining < 0;
  const isNearExpiry = daysRemaining >= 0 && daysRemaining <= 30; // 30 ngày ngưỡng cận date mặc định

  return { daysRemaining, isExpired, isNearExpiry };
}

export function validateExpiryForIssue(expiryDateStr: string): { canIssue: boolean; reason?: string } {
  const { isExpired } = calculateDaysToExpiry(expiryDateStr);
  if (isExpired) {
    return { canIssue: false, reason: 'Lô hàng đã hết hạn sử dụng (quá HSD) — Hệ thống chặn xuất kho!' };
  }
  return { canIssue: true };
}

export function filterNearExpiryLots<T extends { daysRemaining: number }>(
  items: T[],
  thresholdDays: number = 30,
): T[] {
  return items.filter((item) => item.daysRemaining >= 0 && item.daysRemaining <= thresholdDays);
}
