// pos-step85-helpers.ts
// Frontend helpers cho Bước 85:
//   UC_POS_064 — Tỷ lệ hủy / giảm giá (calculateCancelDiscountRates, formatCancelDiscountNotice)
//   UC_POS_065 — Cost lý thuyết vs thực tế (calculateCostVariance)
//   UC_POS_066 — Top sản phẩm bán chạy (formatTopProductRank)
//   UC_POS_067 — So sánh điểm bán (formatStoreCompareRow)

export function calculateCancelDiscountRates(
  grossRevenue: number,
  totalCancelAmount: number,
  totalDiscountAmount: number,
): { cancelRatePct: number; discountRatePct: number } {
  if (grossRevenue <= 0) {
    return { cancelRatePct: 0, discountRatePct: 0 };
  }
  const cancelRatePct = Math.round((totalCancelAmount / grossRevenue) * 10000) / 100;
  const discountRatePct = Math.round((totalDiscountAmount / grossRevenue) * 10000) / 100;
  return { cancelRatePct, discountRatePct };
}

export function formatCancelDiscountNotice(cancelRatePct: number, discountRatePct: number): string {
  const cancelBadge = cancelRatePct > 5 ? '⚠️ High Cancel' : '✅ Normal Cancel';
  const discountBadge = discountRatePct > 15 ? '⚠️ High Discount' : '✅ Normal Discount';
  return `📊 Tỷ lệ hủy: ${cancelRatePct}% (${cancelBadge}) | Tỷ lệ chiết khấu: ${discountRatePct}% (${discountBadge})`;
}

export function calculateCostVariance(
  theoreticalCost: number,
  actualCost: number,
  tolerancePct: number = 5,
): { variance: number; variancePct: number; isWithinTolerance: boolean } {
  const variance = Math.round((actualCost - theoreticalCost) * 100) / 100;
  const variancePct = theoreticalCost > 0 ? Math.round((variance / theoreticalCost) * 10000) / 100 : 0;
  const isWithinTolerance = Math.abs(variancePct) <= tolerancePct;
  return { variance, variancePct, isWithinTolerance };
}

export function formatTopProductRank(
  rank: number,
  productName: string,
  qty: number,
  revenue: number,
): string {
  const medal = rank === 1 ? '🥇' : rank === 2 ? '🥈' : rank === 3 ? '🥉' : `#${rank}`;
  return `${medal} ${productName}: ${qty.toLocaleString('vi-VN')} món | ${revenue.toLocaleString('vi-VN')} VNĐ`;
}

export function formatStoreCompareRow(
  storeName: string,
  salesCount: number,
  revenue: number,
  compareRatioPct: number,
): string {
  return `🏬 ${storeName}: ${salesCount} đơn | DT: ${revenue.toLocaleString('vi-VN')} VNĐ (${compareRatioPct.toFixed(1)}% tổng chuỗi)`;
}
