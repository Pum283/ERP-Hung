// pos-step79-helpers.ts
// Frontend helpers cho Bước 79:
//   UC_POS_022 — Nhập mã voucher (validateVoucherCode, formatVoucherNotice)
//   UC_POS_024 — Giảm giá tay có quyền (validateManualDiscount, formatManualDiscountStatus)
//   UC_POS_026 — Mở đơn / chọn khu vực (validateOpenSaleRequest, formatAreaDisplay)
//   UC_POS_027 — Thêm / sửa / xóa sản phẩm (validateSaleLineInput, formatSaleLineRow)

export function validateVoucherCode(
  voucherCode: string,
): { isValid: boolean; error?: string } {
  const trimmed = (voucherCode || '').trim();
  if (trimmed.length === 0) {
    return { isValid: false, error: 'Mã voucher không được để trống.' };
  }
  if (trimmed.length < 3) {
    return { isValid: false, error: 'Mã voucher tối thiểu 3 ký tự.' };
  }
  return { isValid: true };
}

export function formatVoucherNotice(voucherCode: string, discountAmount: number): string {
  return `🎟️ Đã áp dụng Voucher [${voucherCode}] — Giảm ${discountAmount.toLocaleString('vi-VN')} VNĐ.`;
}

export function validateManualDiscount(
  discountType: string,
  value: number,
): { isValid: boolean; error?: string } {
  if (discountType !== 'Percent' && discountType !== 'Amount') {
    return { isValid: false, error: 'Loại giảm giá phải là Percent hoặc Amount.' };
  }
  if (isNaN(value) || value <= 0) {
    return { isValid: false, error: 'Giá trị giảm tay phải lớn hơn 0.' };
  }
  if (discountType === 'Percent' && value > 100) {
    return { isValid: false, error: 'Giảm giá phần trăm không được vượt quá 100%.' };
  }
  return { isValid: true };
}

export function formatManualDiscountStatus(status: string, value: number, type: string): { label: string; icon: string } {
  const typeText = type === 'Percent' ? `${value}%` : `${value.toLocaleString('vi-VN')} VNĐ`;
  switch (status) {
    case 'Approved':
      return { label: `✅ Giảm tay ${typeText} (Đã duyệt)`, icon: 'check-circle' };
    case 'Pending':
      return { label: `⏳ Giảm tay ${typeText} (Chờ quản lý duyệt)`, icon: 'clock' };
    case 'Rejected':
      return { label: `❌ Yêu cầu giảm tay bị từ chối`, icon: 'x-circle' };
    default:
      return { label: `Không áp dụng giảm tay`, icon: 'minus' };
  }
}

export function validateOpenSaleRequest(
  shiftId?: string,
): { isValid: boolean; error?: string } {
  if (!shiftId || shiftId.trim().length === 0) {
    return { isValid: false, error: 'Cần có ca bán đang mở để tạo đơn mới.' };
  }
  return { isValid: true };
}

export function formatAreaDisplay(areaName?: string): string {
  return areaName && areaName.trim().length > 0 ? `📍 Khu vực / Bàn: ${areaName}` : '🛍️ Đơn bán mang đi';
}

export function validateSaleLineInput(
  quantity: number,
  unitPrice: number,
): { isValid: boolean; error?: string } {
  if (isNaN(quantity) || quantity <= 0) {
    return { isValid: false, error: 'Số lượng sản phẩm phải lớn hơn 0.' };
  }
  if (isNaN(unitPrice) || unitPrice < 0) {
    return { isValid: false, error: 'Đơn giá không được âm.' };
  }
  return { isValid: true };
}

export function formatSaleLineRow(
  lineNo: number,
  productCode: string,
  productName: string,
  qty: number,
  unitPrice: number,
): string {
  const amount = (qty * unitPrice).toLocaleString('vi-VN');
  return `${lineNo}. [${productCode}] ${productName} x${qty} @ ${unitPrice.toLocaleString('vi-VN')} = ${amount} VNĐ`;
}
