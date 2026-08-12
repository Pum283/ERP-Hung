// inv-step96-helpers.ts
// Frontend helpers cho Bước 96:
//   UC_INV_020 — Nhập chuyển đến (validateTransferReceipt)
//   UC_INV_022 — Nhập theo lô / HSD / serial (validateLotExpiryInput)
//   UC_INV_024 — Xuất bán / giao hàng (validateSalesIssue)
//   UC_INV_025 — Xuất sản xuất (validateProductionIssue)

export function validateTransferReceipt(
  fromWarehouseId: string,
  toWarehouseId: string,
): { canReceipt: boolean; error?: string } {
  if (!fromWarehouseId || fromWarehouseId.trim().length === 0) {
    return { canReceipt: false, error: 'Chưa chọn kho xuất (kho đi).' };
  }
  if (!toWarehouseId || toWarehouseId.trim().length === 0) {
    return { canReceipt: false, error: 'Chưa chọn kho nhập (kho đến).' };
  }
  if (fromWarehouseId === toWarehouseId) {
    return { canReceipt: false, error: 'Kho xuất và kho nhập không được trùng nhau.' };
  }
  return { canReceipt: true };
}

export function validateLotExpiryInput(
  lotCode?: string,
  expiryDate?: string,
): { isValid: boolean; error?: string } {
  if (lotCode && lotCode.trim().length === 0) {
    return { isValid: false, error: 'Mã lô không được là chuỗi rỗng.' };
  }
  if (expiryDate && isNaN(Date.parse(expiryDate))) {
    return { isValid: false, error: 'Ngày hết hạn không đúng định dạng ngày tháng.' };
  }
  return { isValid: true };
}

export function validateSalesIssue(
  warehouseId: string,
  lineCount: number,
): { canIssue: boolean; error?: string } {
  if (!warehouseId || warehouseId.trim().length === 0) {
    return { canIssue: false, error: 'Phải chọn kho xuất bán hàng.' };
  }
  if (lineCount <= 0) {
    return { canIssue: false, error: 'Phiếu xuất bán phải có ít nhất 1 mặt hàng.' };
  }
  return { canIssue: true };
}

export function validateProductionIssue(
  warehouseId: string,
  lineCount: number,
): { canIssue: boolean; error?: string } {
  if (!warehouseId || warehouseId.trim().length === 0) {
    return { canIssue: false, error: 'Phải chọn kho xuất nguyên vật liệu sản xuất.' };
  }
  if (lineCount <= 0) {
    return { canIssue: false, error: 'Phiếu xuất sản xuất phải có ít nhất 1 loại NVL.' };
  }
  return { canIssue: true };
}
