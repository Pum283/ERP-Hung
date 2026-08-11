// crm-step73-helpers.ts
// Frontend helpers cho Bước 73:
//   UC_CRM_076 — Hết hạn báo giá tự động (formatQuoteExpirationStatus, isQuoteExpiringSoon)
//   UC_CRM_077 — Chuyển báo giá thành đơn (formatOrderConversionNotice, validateQuoteForConversion)
//   UC_CRM_078 — In mẫu báo giá (generateQuotePrintTemplateHtml)
//   UC_CRM_079 — Tạo đơn hàng từ báo giá (formatSalesOrderSummary, formatOrderLineItemRow)

const BLOCKLIST_STATUSES = ['Expired', 'Rejected', 'Converted'] as const;

export function formatQuoteExpirationStatus(
  validUntil?: string | Date,
  status?: string,
): { statusLabel: string; isExpired: boolean; daysRemaining: number } {
  if (status === 'Expired') {
    return { statusLabel: '⏰ ĐÃ HẾT HẠN', isExpired: true, daysRemaining: 0 };
  }

  if (!validUntil) {
    return { statusLabel: '⏳ Đang hiệu lực', isExpired: false, daysRemaining: 99 };
  }

  const vTime = new Date(validUntil).getTime();
  if (isNaN(vTime)) {
    return { statusLabel: '⚠️ Ngày không hợp lệ', isExpired: false, daysRemaining: -1 };
  }

  const now = Date.now();
  const diffDays = Math.ceil((vTime - now) / (1000 * 3600 * 24));

  if (diffDays <= 0) {
    return { statusLabel: '⏰ QUÁ HẠN HIỆU LỰC', isExpired: true, daysRemaining: 0 };
  }

  return { statusLabel: `⏳ Còn ${diffDays} ngày hiệu lực`, isExpired: false, daysRemaining: diffDays };
}

export function isQuoteExpiringSoon(validUntil?: string | Date, thresholdDays: number = 3): boolean {
  if (!validUntil) return false;
  const vTime = new Date(validUntil).getTime();
  if (isNaN(vTime)) return false;
  const diffDays = Math.ceil((vTime - Date.now()) / (1000 * 3600 * 24));
  return diffDays > 0 && diffDays <= thresholdDays;
}

export function validateQuoteForConversion(
  status?: string,
  discountApprovalStatus?: string,
  lineCount?: number,
  totalAmount?: number,
): { canConvert: boolean; reason?: string } {
  if (!status) {
    return { canConvert: false, reason: 'Báo giá không có trạng thái.' };
  }
  if (BLOCKLIST_STATUSES.includes(status as typeof BLOCKLIST_STATUSES[number])) {
    return { canConvert: false, reason: `Báo giá ở trạng thái "${status}" — không thể chuyển đơn.` };
  }
  if (discountApprovalStatus === 'Pending') {
    return { canConvert: false, reason: 'Đang chờ duyệt chiết khấu — chưa thể chuyển đơn.' };
  }
  if ((lineCount ?? 0) === 0 && (totalAmount ?? 0) <= 0) {
    return { canConvert: false, reason: 'Báo giá trống (không có dòng hàng và tổng tiền = 0).' };
  }
  return { canConvert: true };
}

export function formatOrderConversionNotice(orderCode: string, quoteCode: string): string {
  return `🛒 Đã chuyển đổi Báo giá "${quoteCode}" thành Đơn hàng bán mới "${orderCode}".`;
}

export function generateQuotePrintTemplateHtml(
  quote: { code: string; customerName?: string; totalAmount: number; quoteDate?: string; validUntil?: string },
  companyName: string = 'CÔNG TY ERP HÙNG',
): string {
  const code = quote.code || 'QT-000';
  const cName = quote.customerName || 'Khách hàng';
  const total = (quote.totalAmount || 0).toLocaleString('vi-VN');
  const dateStr = quote.quoteDate ? new Date(quote.quoteDate).toLocaleDateString('vi-VN') : new Date().toLocaleDateString('vi-VN');
  const validStr = quote.validUntil ? new Date(quote.validUntil).toLocaleDateString('vi-VN') : 'N/A';

  return `
    <div class="quote-print-template" style="font-family: Arial, sans-serif; padding: 20px;">
      <h2 style="color: #1e3a8a;">${companyName}</h2>
      <h3>BÁO GIÁ SẢN PHẨM / DỊCH VỤ - ${code}</h3>
      <p>Ngày báo giá: <strong>${dateStr}</strong> | Hiệu lực đến: <strong>${validStr}</strong></p>
      <p>Kính gửi: <strong>${cName}</strong></p>
      <hr />
      <p>Tổng giá trị thanh toán: <strong>${total} VNĐ</strong></p>
      <p style="font-size: 0.9em; color: #666;">Báo giá có giá trị đến ngày ${validStr}.</p>
    </div>
  `.trim();
}

export function formatSalesOrderSummary(orderCode: string, totalAmount: number, status: string): string {
  const code = orderCode || 'SO-000';
  const total = (totalAmount || 0).toLocaleString('vi-VN');
  const statusIcon = status === 'Confirmed' ? '✅' : status === 'Draft' ? '📝' : status === 'Cancelled' ? '❌' : '📦';
  return `${statusIcon} Đơn hàng #${code} | Tổng tiền: ${total} VNĐ | Trạng thái: ${status}`;
}

export function formatOrderLineItemRow(
  lineNo: number,
  itemCode: string,
  itemName: string,
  quantity: number,
  unitPrice: number,
): string {
  const amount = (quantity * unitPrice).toLocaleString('vi-VN');
  return `${lineNo}. [${itemCode}] ${itemName} — SL: ${quantity} × ${unitPrice.toLocaleString('vi-VN')} = ${amount} VNĐ`;
}
