// pur-step89-helpers.ts
// Frontend helpers cho Bước 89:
//   UC_PUR_028 — Gửi PO cho nhà cung cấp (validatePoSendRequest)
//   UC_PUR_030 — Sửa PO phiên bản (validatePoRevision)
//   UC_PUR_031 — Theo dõi nhận hàng từng phần (formatPartialReceivingStatus)
//   UC_PUR_032 — Đóng / hủy PO (validatePoCloseOrCancel)

export function validatePoSendRequest(poStatus: string): { canSend: boolean; error?: string } {
  if (poStatus !== 'Approved') {
    return { canSend: false, error: 'Chỉ có thể gửi Đơn mua hàng (PO) đã phê duyệt (Approved) cho nhà cung cấp.' };
  }
  return { canSend: true };
}

export function validatePoRevision(poStatus: string): { canRevise: boolean; error?: string } {
  if (poStatus !== 'Sent' && poStatus !== 'Approved') {
    return { canRevise: false, error: 'Chỉ có thể điều chỉnh phiên bản PO ở trạng thái Approved hoặc Sent.' };
  }
  return { canRevise: true };
}

export function formatPartialReceivingStatus(
  orderedQty: number,
  receivedQty: number,
): { percentComplete: number; statusText: string } {
  if (orderedQty <= 0) {
    return { percentComplete: 0, statusText: 'Chưa giao hàng' };
  }
  const percentComplete = Math.min(100, Math.round((receivedQty / orderedQty) * 10000) / 100);
  let statusText = 'Chưa nhận hàng';
  if (percentComplete >= 100) {
    statusText = 'Đã nhận đủ 100%';
  } else if (percentComplete > 0) {
    statusText = `Nhận 1 phần (${percentComplete}%)`;
  }
  return { percentComplete, statusText };
}

export function validatePoCloseOrCancel(
  poStatus: string,
  receivedQty: number,
  action: 'Close' | 'Cancel',
): { canExecute: boolean; error?: string } {
  if (poStatus === 'Closed' || poStatus === 'Cancelled') {
    return { canExecute: false, error: `PO đã ở trạng thái ${poStatus}.` };
  }
  if (action === 'Cancel' && receivedQty > 0) {
    return { canExecute: false, error: 'PO đã phát sinh nhận hàng — không thể Hủy, hãy chọn Đóng đơn (Close).' };
  }
  return { canExecute: true };
}
