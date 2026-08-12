// mfg-step110-helpers.ts
// Frontend helpers cho Bước 110:
//   UC_MFG_003 — Danh mục xưởng / dây chuyền (validateWorkshopUpsert)
//   UC_MFG_006 — Tạo BOM nhiều cấp (validateBomCreate)
//   UC_MFG_007 — Phiên bản BOM (validateBomActivation)
//   UC_MFG_008 — Định mức nguyên vật liệu (validateBomLineUpsert)

export function validateWorkshopUpsert(
  code: string,
  name: string,
  type: string,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã xưởng / dây chuyền không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên xưởng / dây chuyền không được để trống.' };
  }
  const wt = (type || '').trim();
  if (wt !== 'Workshop' && wt !== 'Line') {
    return { isValid: false, error: 'Loại phân xưởng phải là Workshop hoặc Line.' };
  }
  return { isValid: true };
}

export function validateBomCreate(
  parentItemId: string,
  version: string,
): { canCreate: boolean; error?: string } {
  if (!parentItemId || parentItemId.trim().length === 0) {
    return { canCreate: false, error: 'Phải chọn sản phẩm cha (FG/SFG) để lập định mức BOM.' };
  }
  if (!version || version.trim().length === 0) {
    return { canCreate: false, error: 'Phiên bản BOM không được để trống.' };
  }
  return { canCreate: true };
}

export function validateBomActivation(status: string, lineCount: number): { canActivate: boolean; reason?: string } {
  if (lineCount <= 0) {
    return { canActivate: false, reason: 'BOM cần ít nhất 1 dòng thành phần vật tư trước khi kích hoạt.' };
  }
  if (status === 'Obsolete') {
    return { canActivate: false, reason: 'Không thể kích hoạt phiên bản BOM đã lỗi thời (Obsolete).' };
  }
  return { canActivate: true };
}

export function validateBomLineUpsert(
  componentItemId: string,
  parentItemId: string,
  qty: number,
): { isValid: boolean; error?: string } {
  if (!componentItemId || componentItemId.trim().length === 0) {
    return { isValid: false, error: 'Phải chọn vật tư / bán thành phẩm thành phần.' };
  }
  if (componentItemId === parentItemId) {
    return { isValid: false, error: 'Thành phần BOM không thể trùng với sản phẩm cha (tự tham chiếu).' };
  }
  if (isNaN(qty) || qty <= 0) {
    return { isValid: false, error: 'Định mức số lượng vật tư phải > 0.' };
  }
  return { isValid: true };
}
