/** Pure helpers — POS sync catalog + in hóa đơn / báo cáo ca (UC_POS_015/037/048). */

/** Structural type khớp `PosSyncResult` trong pos-api (không import để test node chạy độc lập). */
export type CatalogSyncSummary = {
  productCount: number; createdCount: number; updatedCount: number;
  suspendedCount: number; syncedAt: string;
};

/** Thông điệp kết quả đồng bộ catalog từ back-office (mirror PosSyncResult BE). */
export function formatCatalogSyncMessage(r: CatalogSyncSummary): string {
  const parts = [`${r.productCount} SP`];
  if (r.createdCount > 0) parts.push(`${r.createdCount} tạo mới`);
  if (r.updatedCount > 0) parts.push(`${r.updatedCount} cập nhật`);
  if (r.suspendedCount > 0) parts.push(`${r.suspendedCount} suspend`);
  return `Đồng bộ INV→POS: ${parts.join(" · ")}`;
}

/** Chỉ in hóa đơn khi đơn Paid/Returned (khớp BE). */
export function canPrintReceipt(status: string): boolean {
  return status === "Paid" || status === "Returned";
}

/** Tên file hóa đơn — khớp BE `{code}-hoadon.txt`. */
export function buildReceiptFilename(saleCode: string): string {
  return `${saleCode}-hoadon.txt`;
}

/** Tên file báo cáo ca — khớp BE `{code}-baocao-ca.txt`. */
export function buildShiftReportFilename(shiftCode: string): string {
  return `${shiftCode}-baocao-ca.txt`;
}
