/** Pure helpers — sync CRM promo → POS + báo cáo voucher (UC_CRM_036/038). */

export function mapCrmDiscountToPos(
  discountType: string,
): "Percent" | "Amount" | null {
  if (discountType === "Percentage") return "Percent";
  if (discountType === "FixedAmount") return "Amount";
  return null;
}

export function canSyncPromoToPos(discountType: string, discountValue: number): boolean {
  return mapCrmDiscountToPos(discountType) != null && discountValue > 0;
}

export function formatSyncToPosMessage(r: {
  posPromotionCode: string;
  created: boolean;
  vouchersSynced: number;
  vouchersSkipped: number;
}): string {
  const verb = r.created ? "Tạo" : "Cập nhật";
  return `${verb} POS ${r.posPromotionCode} · voucher ${r.vouchersSynced} (bỏ ${r.vouchersSkipped})`;
}

export function summarizeVoucherUsageReport(
  rows: { redeemCount: number; totalDiscount: number }[],
): { voucherCount: number; redeemTotal: number; discountTotal: number } {
  let redeemTotal = 0;
  let discountTotal = 0;
  for (const r of rows) {
    redeemTotal += r.redeemCount;
    discountTotal += r.totalDiscount;
  }
  return {
    voucherCount: rows.length,
    redeemTotal,
    discountTotal: Math.round(discountTotal * 100) / 100,
  };
}

export function rankUsageRows<T extends { redeemCount: number; voucherCode: string }>(
  rows: T[],
): T[] {
  return [...rows].sort((a, b) => {
    if (b.redeemCount !== a.redeemCount) return b.redeemCount - a.redeemCount;
    return a.voucherCode.localeCompare(b.voucherCode);
  });
}
