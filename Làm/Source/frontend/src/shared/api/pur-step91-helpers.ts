// pur-step91-helpers.ts
// Frontend helpers cho Bước 91:
//   UC_PUR_040 — Nhập hóa đơn nhà cung cấp (validateVendorInvoiceCreate)
//   UC_PUR_041 — Đối soát 3 chiều PO–GRN–Invoice (formatThreeWayMatchStatus)
//   UC_PUR_043 — Đẩy công nợ sang FIN AP (validateApPushRequest)
//   UC_PUR_048 — Báo cáo mua theo nhà cung cấp / SP (validatePurchaseReportFilter)

export function validateVendorInvoiceCreate(
  vendorId: string,
  invoiceNumber: string,
  totalAmount: number,
): { isValid: boolean; error?: string } {
  if (!vendorId || vendorId.trim().length === 0) {
    return { isValid: false, error: 'Phải chọn nhà cung cấp phát hành hóa đơn.' };
  }
  if (!invoiceNumber || invoiceNumber.trim().length === 0) {
    return { isValid: false, error: 'Số hóa đơn NCC không được để trống.' };
  }
  if (isNaN(totalAmount) || totalAmount < 0) {
    return { isValid: false, error: 'Tổng tiền hóa đơn phải >= 0.' };
  }
  return { isValid: true };
}

export function formatThreeWayMatchStatus(matchStatus: string): { label: string; badgeStyle: 'success' | 'warning' | 'danger' | 'info' } {
  switch (matchStatus) {
    case 'Matched':
      return { label: '✅ Đã đối soát khớp', badgeStyle: 'success' };
    case 'Mismatch':
      return { label: '⚠️ Lệch số liệu', badgeStyle: 'danger' };
    case 'Pending':
      return { label: '⏳ Chờ đối soát', badgeStyle: 'warning' };
    default:
      return { label: matchStatus, badgeStyle: 'info' };
  }
}

export function validateApPushRequest(
  matchStatus: string,
  apPushStatus: string,
): { canPush: boolean; reason?: string } {
  if (matchStatus !== 'Matched') {
    return { canPush: false, reason: 'Hóa đơn phải được đối soát khớp (Matched) trước khi đẩy sang FIN AP.' };
  }
  if (apPushStatus === 'Pushed') {
    return { canPush: false, reason: 'Hóa đơn này đã được đẩy công nợ sang FIN AP trước đó.' };
  }
  return { canPush: true };
}

export function validatePurchaseReportFilter(
  fromDate?: string,
  toDate?: string,
): { isValid: boolean; error?: string } {
  if (fromDate && toDate && new Date(fromDate) > new Date(toDate)) {
    return { isValid: false, error: 'Từ ngày không được lớn hơn Đến ngày.' };
  }
  return { isValid: true };
}
