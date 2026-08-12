// mfg-step109-helpers.ts
// Frontend helpers cho Bước 109:
//   UC_LOG_038 — Báo cáo COD tồn / đã nộp (formatCodReportSummary)
//   UC_LOG_039 — Dashboard giao vận (calculateLogisticsDashboardKpi)
//   UC_MFG_001 — Danh mục thành phẩm / bán thành phẩm (validateFinishedGoodItem)
//   UC_MFG_002 — Danh mục nguyên vật liệu (validateRawMaterialItem)

export function formatCodReportSummary(
  pendingAmt: number,
  collectedAmt: number,
  remittedAmt: number,
): { summaryText: string } {
  return {
    summaryText: `Chờ thu: ${pendingAmt.toLocaleString('vi-VN')} đ | Đã thu: ${collectedAmt.toLocaleString('vi-VN')} đ | Đã bàn giao: ${remittedAmt.toLocaleString('vi-VN')} đ`,
  };
}

export function calculateLogisticsDashboardKpi(
  deliveredCount: number,
  inTransitCount: number,
  failedCount: number,
): { totalOrders: number; successRatePct: number } {
  const totalOrders = deliveredCount + inTransitCount + failedCount;
  if (totalOrders === 0) return { totalOrders: 0, successRatePct: 0 };
  const rate = (deliveredCount / totalOrders) * 100;
  return {
    totalOrders,
    successRatePct: Math.round(rate * 100) / 100,
  };
}

export function validateFinishedGoodItem(
  code: string,
  name: string,
  itemType: string,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã sản phẩm không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên sản phẩm không được để trống.' };
  }
  const type = (itemType || '').trim().toUpperCase();
  if (type !== 'FG' && type !== 'SFG') {
    return { isValid: false, error: 'Loại sản phẩm phải là TP (FG) hoặc Bán thành phẩm (SFG).' };
  }
  return { isValid: true };
}

export function validateRawMaterialItem(
  code: string,
  name: string,
  standardCost?: number,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã nguyên vật liệu không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên nguyên vật liệu không được để trống.' };
  }
  if (standardCost !== undefined && (isNaN(standardCost) || standardCost < 0)) {
    return { isValid: false, error: 'Giá chuẩn của NVL phải >= 0.' };
  }
  return { isValid: true };
}
