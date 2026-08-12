// pur-step87-helpers.ts
// Frontend helpers cho Bước 87:
//   UC_PUR_003 — Người liên hệ & điều khoản (validateVendorContact)
//   UC_PUR_009 — Gắn sản phẩm – nhà cung cấp (validateVendorProductMapping)
//   UC_PUR_014 — Tạo PR từ đơn vị (validatePurchaseRequestCreate)
//   UC_PUR_017 — Luồng duyệt PR (validatePrApproval)

export function validateVendorContact(
  name: string,
  email?: string,
  phone?: string,
): { isValid: boolean; error?: string } {
  if (!name || name.trim().length === 0) {
    return { isValid: false, error: 'Tên người liên hệ không được để trống.' };
  }
  if (email && email.trim().length > 0 && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
    return { isValid: false, error: 'Email người liên hệ không đúng định dạng.' };
  }
  if (phone && phone.trim().length > 0 && !/^[0-9\+\-\s]{8,15}$/.test(phone.trim())) {
    return { isValid: false, error: 'Số điện thoại không hợp lệ.' };
  }
  return { isValid: true };
}

export function validateVendorProductMapping(
  vendorId: string,
  skuCode: string,
  unitPrice: number,
): { isValid: boolean; error?: string } {
  if (!vendorId || vendorId.trim().length === 0) {
    return { isValid: false, error: 'Phải chọn nhà cung cấp.' };
  }
  if (!skuCode || skuCode.trim().length === 0) {
    return { isValid: false, error: 'Mã SKU sản phẩm không được để trống.' };
  }
  if (isNaN(unitPrice) || unitPrice <= 0) {
    return { isValid: false, error: 'Đơn giá mua tham chiếu phải > 0.' };
  }
  return { isValid: true };
}

export function validatePurchaseRequestCreate(
  departmentId?: string,
  lineCount: number = 0,
): { canCreate: boolean; reason?: string } {
  if (!departmentId || departmentId.trim().length === 0) {
    return { canCreate: false, reason: 'Phải chọn bộ phận / phòng ban yêu cầu mua hàng.' };
  }
  if (lineCount <= 0) {
    return { canCreate: false, reason: 'Yêu cầu mua hàng (PR) phải có ít nhất 1 dòng sản phẩm.' };
  }
  return { canCreate: true };
}

export function validatePrApproval(status: string): { canApprove: boolean; reason?: string } {
  if (status !== 'Submitted') {
    return { canApprove: false, reason: 'Chỉ có thể phê duyệt yêu cầu mua hàng (PR) ở trạng thái Đã gửi (Submitted).' };
  }
  return { canApprove: true };
}
