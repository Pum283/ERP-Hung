// pos-step78-helpers.ts
// Frontend helpers cho Bước 78:
//   UC_POS_015 — Đồng bộ catalog từ back-office (formatCatalogSyncNotice, calculateCatalogSyncStats)
//   UC_POS_016 — Bảng giá theo điểm bán (validatePriceListRequest, formatPriceListBadge)
//   UC_POS_019 — Cấu hình thuế GTGT (validateTaxRateRequest, formatTaxRateBadge)
//   UC_POS_021 — Áp dụng chương trình khuyến mại (validatePromotionRequest, formatPromotionSummary)

export function formatCatalogSyncNotice(
  productCount: number,
  createdCount: number,
  updatedCount: number,
  suspendedCount: number,
): string {
  return `🔄 Đồng bộ catalog hoàn tất: ${productCount} SP tổng cộng (${createdCount} mới, ${updatedCount} cập nhật, ${suspendedCount} tạm ngưng).`;
}

export function validatePriceListRequest(
  code: string,
  name: string,
  storeId?: string,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã bảng giá không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên bảng giá không được để trống.' };
  }
  if (!storeId || storeId.trim().length === 0) {
    return { isValid: false, error: 'Vui lòng chọn điểm bán cho bảng giá.' };
  }
  return { isValid: true };
}

export function formatPriceListBadge(code: string, name: string, itemCount: number): string {
  return `🏷️ [${code}] ${name} (${itemCount} sản phẩm)`;
}

export function validateTaxRateRequest(
  code: string,
  name: string,
  ratePct: number,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã thuế GTGT không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên thuế GTGT không được để trống.' };
  }
  if (isNaN(ratePct) || ratePct < 0 || ratePct > 100) {
    return { isValid: false, error: 'Thuế suất GTGT phải từ 0% đến 100%.' };
  }
  return { isValid: true };
}

export function formatTaxRateBadge(name: string, ratePct: number, isDefault: boolean): string {
  const defaultTag = isDefault ? ' ⭐ (Mặc định)' : '';
  return `📊 ${name} [${ratePct}%]${defaultTag}`;
}

export function validatePromotionRequest(
  code: string,
  name: string,
  discountType: string,
  discountValue: number,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã chương trình khuyến mại không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên chương trình khuyến mại không được để trống.' };
  }
  if (discountType !== 'Percent' && discountType !== 'Amount') {
    return { isValid: false, error: 'Loại giảm giá phải là Percent hoặc Amount.' };
  }
  if (isNaN(discountValue) || discountValue <= 0) {
    return { isValid: false, error: 'Giá trị giảm giá phải lớn hơn 0.' };
  }
  if (discountType === 'Percent' && discountValue > 100) {
    return { isValid: false, error: 'Giảm giá theo phần trăm tối đa 100%.' };
  }
  return { isValid: true };
}

export function formatPromotionSummary(name: string, discountType: string, discountValue: number, minOrder: number): string {
  const valueStr = discountType === 'Percent' ? `${discountValue}%` : `${discountValue.toLocaleString('vi-VN')} VNĐ`;
  const minOrderStr = minOrder > 0 ? ` (Đơn từ ${minOrder.toLocaleString('vi-VN')} VNĐ)` : '';
  return `🎁 ${name}: Giảm ${valueStr}${minOrderStr}`;
}
