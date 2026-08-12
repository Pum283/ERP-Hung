// inv-step95-helpers.ts
// Frontend helpers cho Bước 95:
//   UC_INV_016 — Cho phép tồn âm hay không (validateNegativeStockSetting)
//   UC_INV_017 — Nhập từ mua hàng (validatePurchaseReceiptCreate)
//   UC_INV_018 — Nhập từ sản xuất (validateProductionReceiptCreate)
//   UC_INV_019 — Nhập điều chỉnh / kiểm kê (validateStocktakeAdjustment)

export function validateNegativeStockSetting(allowNegativeStock: boolean): { policyText: string } {
  return {
    policyText: allowNegativeStock
      ? 'Chấp nhận tồn âm (Cho phép xuất kho khi kho chưa cập nhật số lượng)'
      : 'Không cho phép tồn âm (Bắt buộc kiểm tra số dư tồn kho trước khi xuất)',
  };
}

export function validatePurchaseReceiptCreate(
  warehouseId: string,
  grnId: string,
): { canCreate: boolean; error?: string } {
  if (!warehouseId || warehouseId.trim().length === 0) {
    return { canCreate: false, error: 'Phải chọn nhà kho để nhập hàng.' };
  }
  if (!grnId || grnId.trim().length === 0) {
    return { canCreate: false, error: 'Phải chọn phiếu nhận hàng GRN làm căn cứ nhập kho.' };
  }
  return { canCreate: true };
}

export function validateProductionReceiptCreate(
  warehouseId: string,
  lineCount: number,
): { canCreate: boolean; error?: string } {
  if (!warehouseId || warehouseId.trim().length === 0) {
    return { canCreate: false, error: 'Phải chọn nhà kho thành phẩm nhập kho.' };
  }
  if (lineCount <= 0) {
    return { canCreate: false, error: 'Phiếu nhập sản xuất phải có ít nhất 1 dòng sản phẩm thành phẩm.' };
  }
  return { canCreate: true };
}

export function validateStocktakeAdjustment(
  countedQty: number,
  systemQty: number,
): { varianceQty: number; adjustmentType: 'Increase' | 'Decrease' | 'None' } {
  const varianceQty = countedQty - systemQty;
  let adjustmentType: 'Increase' | 'Decrease' | 'None' = 'None';
  if (varianceQty > 0) {
    adjustmentType = 'Increase'; // Thừa kho -> Nhập điều chỉnh tăng
  } else if (varianceQty < 0) {
    adjustmentType = 'Decrease'; // Thiếu kho -> Xuất điều chỉnh giảm
  }
  return { varianceQty, adjustmentType };
}
