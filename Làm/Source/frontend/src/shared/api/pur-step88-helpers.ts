// pur-step88-helpers.ts
// Frontend helpers cho Bước 88:
//   UC_PUR_018 — Từ chối / trả lại PR (validatePrRejection)
//   UC_PUR_019 — Theo dõi trạng thái PR (formatPrStatusBadge)
//   UC_PUR_026 — Tạo PO từ PR/RFQ (validatePoFromPrCreation)
//   UC_PUR_027 — Duyệt PO theo hạn mức (validatePoLimitApproval)

export function validatePrRejection(
  status: string,
  reason?: string,
): { canReject: boolean; error?: string } {
  if (status !== 'Submitted') {
    return { canReject: false, error: 'Chỉ có thể từ chối / trả lại Yêu cầu mua hàng ở trạng thái Submitted.' };
  }
  if (!reason || reason.trim().length === 0) {
    return { canReject: false, error: 'Phải nhập lý do từ chối / trả lại PR.' };
  }
  return { canReject: true };
}

export function formatPrStatusBadge(status: string): { label: string; badgeStyle: 'info' | 'warning' | 'success' | 'danger' } {
  switch (status) {
    case 'Draft':
      return { label: '📝 Nháp', badgeStyle: 'info' };
    case 'Submitted':
      return { label: '⏳ Chờ duyệt', badgeStyle: 'warning' };
    case 'Approved':
      return { label: '✅ Đã duyệt', badgeStyle: 'success' };
    case 'Rejected':
      return { label: '❌ Từ chối', badgeStyle: 'danger' };
    case 'Returned':
      return { label: '↩️ Trả lại', badgeStyle: 'warning' };
    case 'Ordered':
      return { label: '🛒 Đã tạo PO', badgeStyle: 'success' };
    default:
      return { label: status, badgeStyle: 'info' };
  }
}

export function validatePoFromPrCreation(
  prStatus: string,
  vendorId?: string,
): { canCreate: boolean; error?: string } {
  if (prStatus !== 'Approved') {
    return { canCreate: false, error: 'Chỉ có thể tạo Đơn mua hàng (PO) từ PR đã được phê duyệt (Approved).' };
  }
  if (!vendorId || vendorId.trim().length === 0) {
    return { canCreate: false, error: 'Phải chọn Nhà cung cấp để phát hành PO.' };
  }
  return { canCreate: true };
}

export function validatePoLimitApproval(
  poAmount: number,
  userApprovalLimit: number,
): { isWithinLimit: boolean; error?: string } {
  if (isNaN(poAmount) || poAmount <= 0) {
    return { isWithinLimit: false, error: 'Giá trị PO không hợp lệ.' };
  }
  if (userApprovalLimit > 0 && poAmount > userApprovalLimit) {
    return {
      isWithinLimit: false,
      error: `Giá trị PO (${poAmount.toLocaleString('vi-VN')} VNĐ) vượt quá hạn mức duyệt của bạn (${userApprovalLimit.toLocaleString('vi-VN')} VNĐ).`,
    };
  }
  return { isWithinLimit: true };
}
