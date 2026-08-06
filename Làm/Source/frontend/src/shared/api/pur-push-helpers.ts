/** Pure helpers — PUR đẩy AP / INV / xuất PO (UC_PUR_033/037/043). FE mirror BE. */

/** Chỉ đẩy AP khi đã khớp 3 chiều, chưa Pushed và tổng > 0. */
export function canPushInvoiceToAp(
  matchStatus: string,
  apPushStatus: string,
  totalAmount: number,
): boolean {
  return matchStatus === "Matched" && apPushStatus !== "Pushed" && totalAmount > 0;
}

export function formatApPushMessage(code: string, totalAmount: number): string {
  return `Đã tạo + ghi sổ FIN AP cho ${code} (${totalAmount.toLocaleString("vi-VN")}).`;
}

/** Tone hiển thị trạng thái đẩy (AP / INV): Pushed = success, Failed = danger, None = muted. */
export function pushStatusTone(status: string): "success" | "danger" | "muted" {
  if (status === "Pushed") return "success";
  if (status === "Failed") return "danger";
  return "muted";
}

/** Chỉ xuất/in PO khi không Draft/Cancelled (khớp BE). */
export function canExportPo(status: string): boolean {
  return status !== "Draft" && status !== "Cancelled";
}

export function buildPoCsvFilename(code: string, version: number): string {
  return `${code}-v${version}.csv`;
}

/** Lấy lý do lỗi INV từ note GRN (BE append "INV lỗi: ..."). */
export function parseInvPushError(note: string | null | undefined): string | null {
  if (!note) return null;
  const idx = note.lastIndexOf("INV lỗi:");
  return idx < 0 ? null : note.slice(idx + "INV lỗi:".length).trim() || null;
}
