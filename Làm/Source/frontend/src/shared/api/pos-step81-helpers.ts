// pos-step81-helpers.ts
// Frontend helpers cho Bước 81:
//   UC_POS_037 — In hóa đơn (validateInvoicePrintRequest, formatInvoiceHeader)
//   UC_POS_038 — Hủy sản phẩm (validateCancelLineRequest)
//   UC_POS_039 — Hủy cả bill (validateCancelBillRequest)
//   UC_POS_040 — Trả hàng / hoàn tiền (validateReturnRefundRequest, formatRefundSummary)

export function validateInvoicePrintRequest(
  saleStatus: string,
  lineCount: number,
): { canPrint: boolean; reason?: string } {
  if (saleStatus !== 'Completed' && saleStatus !== 'Paid') {
    return { canPrint: false, reason: 'Chỉ có thể in hóa đơn chính thức cho đơn hàng đã thanh toán hoặc hoàn tất.' };
  }
  if (lineCount <= 0) {
    return { canPrint: false, reason: 'Đơn hàng rỗng không thể in hóa đơn.' };
  }
  return { canPrint: true };
}

export function formatInvoiceHeader(storeName: string, saleCode: string): string {
  return `🧾 HÓA ĐƠN BÁN HÀNG - ${storeName.toUpperCase()} | Mã đơn: ${saleCode}`;
}

export function validateCancelLineRequest(
  saleStatus: string,
  quantity: number,
): { canCancel: boolean; reason?: string } {
  if (saleStatus === 'Completed' || saleStatus === 'Cancelled') {
    return { canCancel: false, reason: `Đơn hàng ở trạng thái ${saleStatus} không được hủy sản phẩm trực tiếp.` };
  }
  if (quantity <= 0) {
    return { canCancel: false, reason: 'Số lượng sản phẩm hủy phải lớn hơn 0.' };
  }
  return { canCancel: true };
}

export function validateCancelBillRequest(
  saleStatus: string,
  reason?: string,
): { canCancel: boolean; error?: string } {
  if (saleStatus === 'Completed' || saleStatus === 'Cancelled') {
    return { canCancel: false, error: `Không thể hủy đơn ở trạng thái ${saleStatus}.` };
  }
  if (!reason || reason.trim().length === 0) {
    return { canCancel: false, error: 'Phải nhập lý do khi hủy toàn bộ bill bán hàng.' };
  }
  return { canCancel: true };
}

export function validateReturnRefundRequest(
  saleStatus: string,
  refundAmount: number,
  paidAmount: number,
): { canRefund: boolean; error?: string } {
  if (saleStatus !== 'Completed' && saleStatus !== 'Paid') {
    return { canRefund: false, error: 'Chỉ có thể thực hiện trả hàng / hoàn tiền trên đơn đã hoàn tất thanh toán.' };
  }
  if (isNaN(refundAmount) || refundAmount <= 0) {
    return { canRefund: false, error: 'Số tiền hoàn trả phải lớn hơn 0.' };
  }
  if (refundAmount > paidAmount) {
    return { canRefund: false, error: 'Số tiền hoàn trả không được lớn hơn tổng số tiền đã thanh toán.' };
  }
  return { canRefund: true };
}

export function formatRefundSummary(originalAmount: number, refundAmount: number): string {
  const remaining = Math.max(0, originalAmount - refundAmount);
  return `🔄 Giá trị đơn ban đầu: ${originalAmount.toLocaleString('vi-VN')} VNĐ | Đã hoàn: ${refundAmount.toLocaleString('vi-VN')} VNĐ | Còn lại: ${remaining.toLocaleString('vi-VN')} VNĐ`;
}
