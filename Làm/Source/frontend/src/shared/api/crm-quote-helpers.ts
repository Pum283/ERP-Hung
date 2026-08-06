/** Pure helpers — gửi / xuất báo giá CRM (UC_CRM_074). */

export function canSendQuote(status: string, discountApprovalStatus: string): boolean {
  if (status === "Converted" || status === "Rejected" || status === "Expired") return false;
  if (discountApprovalStatus === "Pending") return false;
  return true;
}

export function buildQuoteFilename(quoteCode: string): string {
  return `${quoteCode}-baogia.txt`;
}

export function formatQuoteSendFlash(channel: "Email" | "Pdf", code: string): string {
  return channel === "Email"
    ? `Đã xếp hàng gửi email báo giá ${code} (nội dung text thật + notification).`
    : `Đã xuất/gửi PDF-text báo giá ${code}.`;
}

export function parseQuoteSendLog(note: string | null | undefined): string | null {
  if (!note) return null;
  const email = note.match(/EMAIL→([^\s@]+@[^\s]+)/);
  if (email) return `Email → ${email[1]}`;
  if (note.includes("PDF/TEXT")) return "Đã xuất file text/PDF";
  return null;
}
