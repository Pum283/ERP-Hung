// inv-step94-helpers.ts
// Frontend helpers cho Bước 94:
//   UC_INV_008 — Import / export danh mục SP (validateCsvImportFile)
//   UC_INV_011 — Tạo kho (validateWarehouseCreate)
//   UC_INV_014 — Gán thủ kho / quyền (validateKeeperAssignment)
//   UC_INV_015 — Cấu hình FEFO / FIFO (validatePickingStrategy)

export function validateCsvImportFile(
  fileName: string,
  fileSize: number,
): { canImport: boolean; reason?: string } {
  if (!fileName || !fileName.toLowerCase().endsWith('.csv')) {
    return { canImport: false, reason: 'Tệp nhập dữ liệu phải có định dạng .csv.' };
  }
  if (fileSize <= 0) {
    return { canImport: false, reason: 'Tệp CSV không có dung lượng hoặc rỗng.' };
  }
  if (fileSize > 10 * 1024 * 1024) { // 10MB
    return { canImport: false, reason: 'Dung lượng tệp CSV không được vượt quá 10MB.' };
  }
  return { canImport: true };
}

export function validateWarehouseCreate(
  code: string,
  name: string,
): { isValid: boolean; error?: string } {
  if (!code || code.trim().length === 0) {
    return { isValid: false, error: 'Mã nhà kho không được để trống.' };
  }
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên nhà kho không được để trống.' };
  }
  return { isValid: true };
}

export function validateKeeperAssignment(
  warehouseId: string,
  userId: string,
): { isValid: boolean; error?: string } {
  if (!warehouseId || warehouseId.trim().length === 0) {
    return { isValid: false, error: 'Phải chọn nhà kho.' };
  }
  if (!userId || userId.trim().length === 0) {
    return { isValid: false, error: 'Phải chọn người dùng làm thủ kho.' };
  }
  return { isValid: true };
}

export function validatePickingStrategy(strategy: string): { isValid: boolean; error?: string } {
  const validStrategies = ['FEFO', 'FIFO', 'LIFO'];
  if (!strategy || !validStrategies.includes(strategy.toUpperCase())) {
    return { isValid: false, error: 'Chiến lược xuất kho phải là FEFO, FIFO hoặc LIFO.' };
  }
  return { isValid: true };
}
