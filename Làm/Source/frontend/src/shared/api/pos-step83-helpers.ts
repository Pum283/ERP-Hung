// pos-step83-helpers.ts
// Frontend helpers cho Bước 83:
//   UC_POS_047 — Đối soát lệch quỹ (calculateCashVariance, formatVarianceNotice)
//   UC_POS_048 — In báo cáo ca (validateShiftReportPrint)
//   UC_POS_054 — Trừ tồn theo BOM khi bán (calculateBomMaterialRequirement)
//   UC_POS_055 — Cảnh báo hết / sắp hết (formatStockAlertBadge)

export function calculateCashVariance(
  expectedCash: number,
  closingCashCounted: number,
): { variance: number; isBalanced: boolean; status: 'Balanced' | 'Over' | 'Short' } {
  const variance = Math.round(closingCashCounted - expectedCash);
  if (variance === 0) {
    return { variance: 0, isBalanced: true, status: 'Balanced' };
  }
  return {
    variance,
    isBalanced: false,
    status: variance > 0 ? 'Over' : 'Short',
  };
}

export function formatVarianceNotice(variance: number): string {
  if (variance === 0) {
    return '✅ Tiền đếm khớp 100% với dự kiến trong ca.';
  }
  if (variance > 0) {
    return `⚠️ Thừa tiền quỹ: +${variance.toLocaleString('vi-VN')} VNĐ.`;
  }
  return `❌ Thiếu tiền quỹ: ${variance.toLocaleString('vi-VN')} VNĐ.`;
}

export function validateShiftReportPrint(shiftStatus: string): { canPrint: boolean; reason?: string } {
  if (shiftStatus !== 'Closed' && shiftStatus !== 'Open') {
    return { canPrint: false, reason: 'Trạng thái ca không hợp lệ để in báo cáo.' };
  }
  return { canPrint: true };
}

export function calculateBomMaterialRequirement(saleQty: number, bomRatio: number): number {
  if (saleQty <= 0 || bomRatio <= 0) return 0;
  return Math.round(saleQty * bomRatio * 1000) / 1000;
}

export function formatStockAlertBadge(
  alertType: string,
  skuCode: string,
  qty: number,
): { label: string; badgeStyle: 'danger' | 'warning' | 'info' } {
  switch (alertType) {
    case 'OutOfStock':
      return { label: `⛔ [${skuCode}] Hết hàng (Tồn: ${qty})`, badgeStyle: 'danger' };
    case 'BelowMin':
      return { label: `⚠️ [${skuCode}] Dưới mức tối thiểu (Tồn: ${qty})`, badgeStyle: 'warning' };
    case 'NearReorder':
      return { label: `📦 [${skuCode}] Chạm ngưỡng đặt hàng (Tồn: ${qty})`, badgeStyle: 'info' };
    default:
      return { label: `ℹ️ [${skuCode}] Tồn kho bình thường (${qty})`, badgeStyle: 'info' };
  }
}
