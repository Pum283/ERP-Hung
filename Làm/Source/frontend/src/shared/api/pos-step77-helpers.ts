// pos-step77-helpers.ts
// Frontend helpers cho Bước 77:
//   UC_POS_009 — Danh mục nhóm sản phẩm (validateCategoryRequest, formatCategoryBadge)
//   UC_POS_010 — Danh mục sản phẩm bán (validateProductRequest, formatProductSummary)
//   UC_POS_012 — BOM / định mức nguyên liệu (validateBomLineRequest, formatBomLineSummary)
//   UC_POS_014 — Ngưng bán sản phẩm tạm thời (formatProductActiveStatus, toggleProductStatusEligibility)

export function validateCategoryRequest(
  code: string,
  name: string,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã nhóm sản phẩm không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên nhóm sản phẩm không được để trống.' };
  }
  return { isValid: true };
}

export function formatCategoryBadge(code: string, name: string, productCount: number): string {
  return `📁 [${code}] ${name} (${productCount} sản phẩm)`;
}

export function validateProductRequest(
  code: string,
  name: string,
  unit?: string,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã sản phẩm không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên sản phẩm không được để trống.' };
  }
  return { isValid: true };
}

export function formatProductSummary(code: string, name: string, unit: string, status: string): string {
  const icon = status === 'Active' ? '🏷️' : '⛔';
  return `${icon} [${code}] ${name} (${unit || 'Cái'}) - ${status === 'Active' ? 'Đang bán' : 'Tạm ngưng'}`;
}

export function validateBomLineRequest(
  materialCode: string,
  materialName: string,
  qty: number,
): { isValid: boolean; error?: string } {
  if (!materialCode || materialCode.trim().length === 0) {
    return { isValid: false, error: 'Mã nguyên liệu không được để trống.' };
  }
  if (!materialName || materialName.trim().length === 0) {
    return { isValid: false, error: 'Tên nguyên liệu không được để trống.' };
  }
  if (isNaN(qty) || qty <= 0) {
    return { isValid: false, error: 'Định mức nguyên liệu phải lớn hơn 0.' };
  }
  return { isValid: true };
}

export function formatBomLineSummary(materialName: string, qty: number, unit?: string): string {
  return `🧪 ${materialName}: ${qty} ${unit || 'đơn vị'}`;
}

export function formatProductActiveStatus(status: string): { label: string; actionText: string; color: string } {
  if (status === 'Suspended') {
    return { label: '⛔ Tạm ngưng bán', actionText: 'Mở bán lại', color: '#dc2626' };
  }
  return { label: '🟢 Đang mở bán', actionText: 'Tạm ngưng bán', color: '#16a34a' };
}

export function toggleProductStatusEligibility(status: string): { newStatus: 'Active' | 'Suspended' } {
  return { newStatus: status === 'Active' ? 'Suspended' : 'Active' };
}
