export function formatPrtAccountStatus(status: string): string {
  switch (status?.trim()?.toLowerCase()) {
    case "active":
      return "Hoạt động";
    case "pending":
      return "Chờ kích hoạt";
    case "locked":
      return "Đã khóa";
    default:
      return status || "Khởi tạo";
  }
}

export function isValidPrtResetToken(token?: string | null): boolean {
  if (!token) return false;
  const cleaned = token.trim();
  return cleaned.length >= 6 && /^[a-zA-Z0-9_-]+$/.test(cleaned);
}

export function formatPrtArSummaryText(summary: { openAmount: number; openInvoiceCount: number; paidYtd: number }): string {
  const amt = summary.openAmount.toLocaleString("vi-VN");
  const paid = summary.paidYtd.toLocaleString("vi-VN");
  return `Nợ chưa trả: ${amt} đ (${summary.openInvoiceCount} HĐ) · Đã thanh toán YTD: ${paid} đ`;
}
