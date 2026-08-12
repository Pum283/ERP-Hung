// mfg-step111-helpers.ts
// Bước 111:
//   UC_MFG_013 — Kế hoạch SX theo đơn hàng
//   UC_MFG_017 — Tạo lệnh sản xuất
//   UC_MFG_018 — Duyệt lệnh sản xuất
//   UC_MFG_019 — Phát hành / in phiếu

export function validatePlanSourceOrder(sourceOrderCode: string): { isValid: boolean; error?: string } {
  const so = (sourceOrderCode || "").trim();
  if (!so) return { isValid: false, error: "Mã đơn hàng (SO) không được để trống." };
  if (so.length > 40) return { isValid: false, error: "Mã đơn hàng tối đa 40 ký tự." };
  return { isValid: true };
}

export function canConfirmPlan(status: string, lineCount: number): { canConfirm: boolean; reason?: string } {
  if (status !== "Draft") return { canConfirm: false, reason: "Chỉ xác nhận KH ở trạng thái Draft." };
  if (lineCount <= 0) return { canConfirm: false, reason: "KH cần ít nhất 1 dòng thành phẩm / BTP." };
  return { canConfirm: true };
}

export function canCancelPlan(status: string, linkedActiveWoCount: number): { canCancel: boolean; reason?: string } {
  if (status === "Cancelled") return { canCancel: false, reason: "KH đã hủy." };
  if (status !== "Draft" && status !== "Confirmed") {
    return { canCancel: false, reason: "Chỉ hủy KH Draft hoặc Confirmed." };
  }
  if (linkedActiveWoCount > 0) {
    return { canCancel: false, reason: "Không hủy KH khi còn lệnh SX liên kết." };
  }
  return { canCancel: true };
}

export function validateWorkOrderCreate(
  itemId: string,
  qty: number,
  planId: string | null | undefined,
  planStatus: string | null | undefined,
): { isValid: boolean; error?: string } {
  if (!itemId || itemId.trim().length === 0) {
    return { isValid: false, error: "Phải chọn thành phẩm / bán thành phẩm." };
  }
  if (isNaN(qty) || qty <= 0) return { isValid: false, error: "Số lượng lệnh SX phải > 0." };
  if (planId && planStatus && planStatus !== "Confirmed") {
    return { isValid: false, error: "Chỉ gắn lệnh vào KH đã xác nhận (Confirmed)." };
  }
  return { isValid: true };
}

export function canApproveWorkOrder(status: string): { canApprove: boolean; reason?: string } {
  if (status !== "Draft") return { canApprove: false, reason: "Chỉ duyệt lệnh Draft." };
  return { canApprove: true };
}

export function canReleaseWorkOrder(status: string): { canRelease: boolean; reason?: string } {
  if (status !== "Approved") return { canRelease: false, reason: "Chỉ phát hành lệnh đã duyệt." };
  return { canRelease: true };
}

export function canPrintWorkOrder(status: string): { canPrint: boolean; reason?: string } {
  const blocked = status === "Draft" || status === "Approved" || status === "Cancelled";
  if (blocked) return { canPrint: false, reason: "Chỉ in phiếu khi lệnh đã phát hành." };
  return { canPrint: true };
}

export function formatWorkOrderSlip(input: {
  code: string;
  itemCode?: string | null;
  itemName?: string | null;
  qty: number;
  workshopName?: string | null;
  bomCode?: string | null;
  status: string;
}): string {
  return [
    "===== PHIẾU LỆNH SẢN XUẤT =====",
    `Mã LSX     : ${input.code}`,
    `Trạng thái : ${input.status}`,
    `Sản phẩm   : ${input.itemCode ?? "—"} — ${input.itemName ?? "—"}`,
    `Số lượng   : ${input.qty}`,
    `Xưởng      : ${input.workshopName ?? "—"}`,
    `BOM        : ${input.bomCode ?? "—"}`,
    "================================",
  ].join("\n");
}
