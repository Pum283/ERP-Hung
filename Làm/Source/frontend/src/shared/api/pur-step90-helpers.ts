// pur-step90-helpers.ts
// Frontend helpers cho Bước 90:
//   UC_PUR_033 — In / xuất PO (validatePoPrintRequest)
//   UC_PUR_034 — Tạo phiếu nhận hàng theo PO (validateGrnCreationFromPo)
//   UC_PUR_035 — Nhận hàng lệch số lượng / chất lượng (validateGrnItemInspection)
//   UC_PUR_037 — Đẩy nhập kho sang INV (validateGrnInventoryPush)

export function validatePoPrintRequest(poStatus: string): { canPrint: boolean; reason?: string } {
  if (poStatus === 'Draft') {
    return { canPrint: false, reason: 'Không thể in đơn mua hàng (PO) ở trạng thái Nháp (Draft).' };
  }
  return { canPrint: true };
}

export function validateGrnCreationFromPo(
  poStatus: string,
  orderedQty: number,
): { canCreate: boolean; error?: string } {
  if (poStatus !== 'Sent' && poStatus !== 'Approved') {
    return { canCreate: false, error: 'Chỉ có thể lập Phiếu nhận hàng (GRN) từ PO ở trạng thái Sent hoặc Approved.' };
  }
  if (orderedQty <= 0) {
    return { canCreate: false, error: 'PO rỗng không có số lượng đặt hàng.' };
  }
  return { canCreate: true };
}

export function validateGrnItemInspection(
  acceptedQty: number,
  rejectedQty: number,
  deliveredQty: number,
): { isValid: boolean; error?: string } {
  if (isNaN(acceptedQty) || acceptedQty < 0) {
    return { isValid: false, error: 'Số lượng chấp nhận (AcceptedQty) phải >= 0.' };
  }
  if (isNaN(rejectedQty) || rejectedQty < 0) {
    return { isValid: false, error: 'Số lượng từ chối/lỗi (RejectedQty) phải >= 0.' };
  }
  if (acceptedQty + rejectedQty !== deliveredQty) {
    return { isValid: false, error: `Tổng số lượng chấp nhận (${acceptedQty}) + từ chối (${rejectedQty}) phải bằng tổng số lượng giao (${deliveredQty}).` };
  }
  return { isValid: true };
}

export function validateGrnInventoryPush(grnStatus: string): { canPush: boolean; reason?: string } {
  if (grnStatus !== 'Posted') {
    return { canPush: false, reason: 'Phiếu nhận hàng (GRN) phải ở trạng thái Posted mới được đẩy nhập kho INV.' };
  }
  return { canPush: true };
}
