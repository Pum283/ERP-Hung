// log-step104-helpers.ts
// Frontend helpers cho Bước 104:
//   UC_INV_070 — Xuất báo cáo kho Excel / CSV (validateValuationCsvExport)
//   UC_LOG_001 — Danh mục đơn vị vận chuyển (validateCarrierUpsert)
//   UC_LOG_006 — Tạo lệnh giao từ đơn hàng (validateDeliveryOrderCreate)
//   UC_LOG_008 — Tách lệnh giao nhiều đợt (validateBatchSplit)

export function validateValuationCsvExport(warehouseId?: string): { canExport: boolean } {
  return { canExport: true };
}

export function validateCarrierUpsert(
  code: string,
  name: string,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã ĐVVC không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên ĐVVC không được để trống.' };
  }
  return { isValid: true };
}

export function validateDeliveryOrderCreate(
  sourceOrderCode: string,
  customerName: string,
): { canCreate: boolean; error?: string } {
  if (!sourceOrderCode || sourceOrderCode.trim().length === 0) {
    return { canCreate: false, error: 'Mã đơn hàng nguồn (SO) không được để trống.' };
  }
  if (!customerName || customerName.trim().length === 0) {
    return { canCreate: false, error: 'Tên khách hàng không được để trống.' };
  }
  return { canCreate: true };
}

export function validateBatchSplit(
  qtyToSplit: number,
  originalQty: number,
): { canSplit: boolean; error?: string } {
  if (isNaN(qtyToSplit) || qtyToSplit <= 0) {
    return { canSplit: false, error: 'Số lượng tách đợt phải > 0.' };
  }
  if (qtyToSplit >= originalQty) {
    return { canSplit: false, error: 'Số lượng tách phải nhỏ hơn tổng số lượng dòng đơn gốc.' };
  }
  return { canSplit: true };
}
