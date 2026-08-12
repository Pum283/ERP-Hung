// inv-step101-helpers.ts
// Frontend helpers cho Bước 101:
//   UC_INV_049 — Tạo phiếu kiểm kê (validateStocktakeCreate)
//   UC_INV_050 — Nhập số đếm thực tế (validateCountInput)
//   UC_INV_052 — Đối chiếu lệch kiểm kê (calculateStocktakeVariance)
//   UC_INV_053 — Duyệt điều chỉnh sau kiểm kê (validateStocktakeReview)

export function validateStocktakeCreate(warehouseId: string): { canCreate: boolean; error?: string } {
  if (!warehouseId || warehouseId.trim().length === 0) {
    return { canCreate: false, error: 'Phải chọn nhà kho cần kiểm kê.' };
  }
  return { canCreate: true };
}

export function validateCountInput(countQty: number): { isValid: boolean; error?: string } {
  if (isNaN(countQty) || countQty < 0) {
    return { isValid: false, error: 'Số lượng đếm thực tế phải >= 0.' };
  }
  return { isValid: true };
}

export function calculateStocktakeVariance(
  countQty: number,
  systemQty: number,
): { varianceQty: number; varianceType: 'Surplus' | 'Shortage' | 'Exact' } {
  const varianceQty = countQty - systemQty;
  let varianceType: 'Surplus' | 'Shortage' | 'Exact' = 'Exact';
  if (varianceQty > 0) varianceType = 'Surplus';
  else if (varianceQty < 0) varianceType = 'Shortage';

  return { varianceQty, varianceType };
}

export function validateStocktakeReview(status: string): { canReview: boolean; reason?: string } {
  if (status !== 'Draft') {
    return { canReview: false, reason: 'Chỉ có thể duyệt phiếu kiểm kê ở trạng thái Dự thảo (Draft).' };
  }
  return { canReview: true };
}
