// pur-step92-helpers.ts
// Frontend helpers cho Bước 92:
//   UC_PUR_051 — Open PR / Open PO aging (formatAgingBucket)
//   UC_PUR_052 — Xuất báo cáo mua hàng (validatePurCsvExport)
//   UC_INV_001 — Tạo / sửa SKU sản phẩm (validateSkuCreate)
//   UC_INV_002 — Phân nhóm hàng / ngành hàng (validateItemGroup)

export function formatAgingBucket(days: number): string {
  if (days <= 30) return '0 - 30 ngày';
  if (days <= 60) return '31 - 60 ngày';
  if (days <= 90) return '61 - 90 ngày';
  return 'Trên 90 ngày (> 90)';
}

export function validatePurCsvExport(
  reportType: string,
  rowCount: number,
): { canExport: boolean; reason?: string } {
  if (!reportType || reportType.trim().length === 0) {
    return { canExport: false, reason: 'Chưa chọn loại báo cáo mua hàng để xuất CSV.' };
  }
  if (rowCount <= 0) {
    return { canExport: false, reason: 'Báo cáo không có dữ liệu để xuất CSV.' };
  }
  return { canExport: true };
}

export function validateSkuCreate(
  code: string,
  name: string,
  uomCode: string,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã SKU không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên SKU không được để trống.' };
  }
  if (!uomCode || uomCode.trim().length === 0) {
    return { isValid: false, error: 'Phải chọn đơn vị tính (UOM) cơ bản cho SKU.' };
  }
  return { isValid: true };
}

export function validateItemGroup(
  code: string,
  name: string,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã nhóm hàng không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên nhóm hàng không được để trống.' };
  }
  return { isValid: true };
}
