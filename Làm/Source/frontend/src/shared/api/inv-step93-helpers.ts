// inv-step93-helpers.ts
// Frontend helpers cho Bước 93:
//   UC_INV_003 — Đơn vị tính & quy đổi (validateUnitConversion)
//   UC_INV_004 — Thuộc tính hàng (lô, serial, HSD) (validateLotSerialRequirement)
//   UC_INV_005 — Giá vốn / phương pháp tính giá (validateCostingMethod)
//   UC_INV_007 — Ngưng sử dụng SKU (validateSkuStatusChange)

export function validateUnitConversion(
  fromUomId: string,
  toUomId: string,
  conversionFactor: number,
): { isValid: boolean; error?: string } {
  if (!fromUomId || fromUomId.trim().length === 0) {
    return { isValid: false, error: 'Phải chọn ĐVT gốc.' };
  }
  if (!toUomId || toUomId.trim().length === 0) {
    return { isValid: false, error: 'Phải chọn ĐVT quy đổi.' };
  }
  if (fromUomId === toUomId) {
    return { isValid: false, error: 'ĐVT gốc và ĐVT quy đổi không được trùng nhau.' };
  }
  if (isNaN(conversionFactor) || conversionFactor <= 0) {
    return { isValid: false, error: 'Tỷ lệ quy đổi phải > 0.' };
  }
  return { isValid: true };
}

export function validateLotSerialRequirement(
  isLotTracked: boolean,
  isSerialTracked: boolean,
  isExpiryTracked: boolean,
): { flagsSummary: string } {
  const parts: string[] = [];
  if (isLotTracked) parts.push('Theo dõi Lô');
  if (isSerialTracked) parts.push('Theo dõi Serial');
  if (isExpiryTracked) parts.push('Theo dõi Hạn sử dụng');
  return { flagsSummary: parts.length > 0 ? parts.join(', ') : 'Hàng thông thường' };
}

export function validateCostingMethod(method?: string): { isValid: boolean; error?: string } {
  const validMethods = ['MovingAverage', 'FIFO', 'Standard'];
  if (!method || method.trim().length === 0) {
    return { isValid: true }; // mặc định hệ thống
  }
  if (!validMethods.includes(method)) {
    return { isValid: false, error: 'Phương pháp tính giá vốn phải là MovingAverage, FIFO hoặc Standard.' };
  }
  return { isValid: true };
}

export function validateSkuStatusChange(
  currentStatus: string,
  newStatus: string,
): { canChange: boolean; reason?: string } {
  if (currentStatus === newStatus) {
    return { canChange: false, reason: `SKU đã ở trạng thái ${newStatus}.` };
  }
  return { canChange: true };
}
