/** Pure helpers — CRM giữ tồn INV + đẩy kho/LOG (UC_CRM_082/088). FE mirror BE. */

/** Giữ tồn được khi đơn chưa hủy/giao và chưa Held. */
export function canHoldStock(status: string, stockHoldStatus: string): boolean {
  return status !== "Cancelled" && status !== "Delivered" && stockHoldStatus !== "Held";
}

/** Đẩy kho được khi đơn đã xác nhận (không Draft/Cancelled) và chưa Pushed. */
export function canPushWarehouse(status: string, warehousePushStatus: string): boolean {
  return status !== "Draft" && status !== "Cancelled" && warehousePushStatus !== "Pushed";
}

export function holdStatusTone(status: string): "brand" | "muted" | "danger" {
  if (status === "Held") return "brand";
  if (status === "Failed") return "danger";
  return "muted";
}

export function warehousePushTone(status: string): "success" | "danger" | "muted" {
  if (status === "Pushed") return "success";
  if (status === "Failed") return "danger";
  return "muted";
}

/** BE append "Giữ tồn RV-... (n/m dòng)" vào note đơn. */
export function parseReservationRef(note: string | null | undefined): string | null {
  const m = (note ?? "").match(/Giữ tồn (RV-[\w-]+)/);
  return m ? m[1] : null;
}

/** BE append "LOG DG-..." vào note đơn. */
export function parseLogDeliveryRef(note: string | null | undefined): string | null {
  const m = (note ?? "").match(/LOG (DG-[\w-]+)/);
  return m ? m[1] : null;
}

export function canReturnOrder(status: string): boolean {
  return status === "Confirmed" || status === "Delivered";
}

export function canSplitOrder(status: string, lineCount: number): boolean {
  return status !== "Delivered" && status !== "Cancelled" && lineCount > 1;
}

export function canMergeOrders(status1: string, status2: string): boolean {
  return status1 !== "Delivered" && status1 !== "Cancelled" && status2 !== "Delivered" && status2 !== "Cancelled";
}

export function canLinkContract(status: string): boolean {
  return status !== "Cancelled";
}
