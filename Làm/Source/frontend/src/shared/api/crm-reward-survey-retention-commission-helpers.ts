export function calculateCommissionAmount(revenue: number, ratePercent: number): number {
  if (!revenue || revenue <= 0 || !ratePercent || ratePercent <= 0) return 0;
  return Math.round(revenue * (ratePercent / 100));
}

export interface RetentionHealthResult {
  statusLabel: string;
  badgeClass: string;
}

export function evaluateRetentionHealth(repeatPurchaseRate: number): RetentionHealthResult {
  if (repeatPurchaseRate >= 75) {
    return { statusLabel: 'Xuất sắc (Tỷ lệ giữ chân cao)', badgeClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
  }
  if (repeatPurchaseRate >= 50) {
    return { statusLabel: 'Trung bình (Cần chăm sóc thêm)', badgeClass: 'bg-amber-100 text-amber-800 border-amber-300' };
  }
  return { statusLabel: 'Cảnh báo (Tỷ lệ rời bỏ cao)', badgeClass: 'bg-rose-100 text-rose-800 border-rose-300' };
}

export function validateRedemptionRequest(pointsAvailable: number, pointsRequired: number): { isValid: boolean; error?: string } {
  if (isNaN(pointsRequired) || pointsRequired <= 0) {
    return { isValid: false, error: 'Số điểm đổi quà phải lớn hơn 0.' };
  }
  if (pointsAvailable < pointsRequired) {
    return { isValid: false, error: `Khách hàng không đủ điểm khả dụng (${pointsAvailable} pts < ${pointsRequired} pts).` };
  }
  return { isValid: true };
}
