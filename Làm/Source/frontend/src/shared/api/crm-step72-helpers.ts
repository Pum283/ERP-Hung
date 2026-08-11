// crm-step72-helpers.ts
// Frontend helpers cho Bước 72:
//   UC_CRM_072 — Áp chính sách giá / bảng giá (formatPriceListBindingNotice)
//   UC_CRM_073 — Xin duyệt chiết khấu (validateDiscountApprovalRequest)
//   UC_CRM_074 — Gửi báo giá PDF/email (formatQuoteDispatchNotice)
//   UC_CRM_075 — Phiên bản báo giá (formatQuoteRevisionBadge)

export function formatPriceListBindingNotice(priceListName?: string): string {
  const plName = (priceListName || '').trim();
  return plName
    ? `🏷️ Đã áp dụng bảng giá: "${plName}" cho báo giá này.`
    : '🏷️ Đang sử dụng bảng giá mặc định.';
}

export function validateDiscountApprovalRequest(discountPercent: number, reason?: string): { isValid: boolean; error?: string; requiresApproval: boolean } {
  if (discountPercent < 0 || discountPercent > 100) {
    return { isValid: false, error: 'Tỷ lệ chiết khấu phải từ 0% đến 100%.', requiresApproval: false };
  }

  const requiresApproval = discountPercent > 15;
  if (requiresApproval && (!reason || reason.trim().length === 0)) {
    return { isValid: false, error: 'Mức chiết khấu > 15% bắt buộc nhập lý do xin duyệt.', requiresApproval: true };
  }

  return { isValid: true, requiresApproval };
}

export function formatQuoteDispatchNotice(channel: string, targetRecipient?: string): { title: string; icon: string } {
  const ch = (channel || '').trim().toLowerCase();
  const recipientNotice = targetRecipient ? ` tới ${targetRecipient}` : '';

  switch (ch) {
    case 'email':
      return { title: `✉️ Đã gửi Báo giá PDF qua Email${recipientNotice}.`, icon: 'email' };
    case 'zalo':
      return { title: `💬 Đã gửi Báo giá qua Zalo OA${recipientNotice}.`, icon: 'zalo' };
    case 'sms':
      return { title: `📱 Đã gửi link Báo giá qua SMS${recipientNotice}.`, icon: 'sms' };
    default:
      return { title: `📤 Đã phát hành Báo giá qua kênh ${channel}.`, icon: 'send' };
  }
}

export function formatQuoteRevisionBadge(versionNo: number): string {
  const v = versionNo || 1;
  return `v${v}.0 (Phiên bản ${v})`;
}
