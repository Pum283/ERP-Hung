// crm-step65-helpers.ts
// Frontend helpers cho Bước 65:
//   UC_CRM_035 — Giới hạn lượt dùng voucher (formatVoucherStatusBadge, validateVoucherRedeemRequest)
//   UC_CRM_036 — Đồng bộ khuyến mại sang POS (formatVoucherStatusBadge)
//   UC_CRM_037 — Áp dụng khuyến mại trên báo giá (calculateDiscountPreview)
//   UC_CRM_038 — Báo cáo sử dụng voucher (formatVoucherReportSummary)

export function formatVoucherStatusBadge(status?: string, usageCount?: number, maxUsage?: number): { label: string; isAvailable: boolean } {
  const st = (status || '').trim();
  const count = usageCount || 0;
  const max = maxUsage || 1;

  if (st === 'Used' || count >= max) {
    return { label: '🔒 Đã hết lượt sử dụng', isAvailable: false };
  }
  if (st === 'Expired') {
    return { label: '⏰ Đã hết hạn', isAvailable: false };
  }
  if (st === 'Active' || st === 'Available') {
    return { label: `🎟️ Khả dụng (${count}/${max})`, isAvailable: true };
  }
  return { label: '❓ Chưa kích hoạt', isAvailable: false };
}

export function validateVoucherRedeemRequest(code: string): { isValid: boolean; error?: string } {
  const cleanCode = (code || '').trim();
  if (!cleanCode) {
    return { isValid: false, error: 'Mã voucher không được để trống.' };
  }
  if (cleanCode.length > 50) {
    return { isValid: false, error: 'Mã voucher tối đa 50 ký tự.' };
  }
  return { isValid: true };
}

export function calculateDiscountPreview(
  discountType: string,
  discountValue: number,
  subTotal: number,
  maxDiscount?: number
): number {
  if (isNaN(subTotal) || subTotal <= 0) return 0;

  let rawDiscount = 0;
  const type = (discountType || '').trim().toLowerCase();

  if (type === 'percentage' || type === 'percent') {
    rawDiscount = (subTotal * discountValue) / 100;
  } else if (type === 'fixedamount' || type === 'amount') {
    rawDiscount = discountValue;
  } else if (type === 'sameprice') {
    rawDiscount = Math.max(0, subTotal - discountValue);
  }

  if (maxDiscount && maxDiscount > 0) {
    rawDiscount = Math.min(rawDiscount, maxDiscount);
  }

  return Math.min(rawDiscount, subTotal);
}

export function formatVoucherReportSummary(totalGen: number, totalRedeemed: number): string {
  const pct = totalGen > 0 ? Math.round((totalRedeemed / totalGen) * 10000) / 100 : 0;
  return `📊 Đã dùng ${totalRedeemed.toLocaleString('vi-VN')} / ${totalGen.toLocaleString('vi-VN')} voucher (${pct}% tỷ lệ sử dụng)`;
}
