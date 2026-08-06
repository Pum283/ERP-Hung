/** Pure helpers — BT tự động FIN (UC_FIN_015) + đẩy giá thành MFG (UC_MFG_031). */

export function isAutoSource(source: string | null | undefined): boolean {
  return (source ?? "").localeCompare("Auto", undefined, { sensitivity: "accent" }) === 0;
}

export function formatAutoJournalFlash(code: string, source: string): string {
  return isAutoSource(source)
    ? `Đã tạo BT tự động ${code} (Source=Auto).`
    : `Đã tạo BT ${code}.`;
}

/** Có thể đẩy giá thành khi sheet Calculated. */
export function canPushMfgCost(status: string): boolean {
  return status === "Calculated";
}

export function formatMfgCostPushFlash(opts: {
  invSkuCode?: string | null;
  finJournalCode?: string | null;
  unitCost: number;
}): string {
  const parts = [`Đơn giá ${opts.unitCost.toLocaleString("vi-VN")}`];
  if (opts.invSkuCode) parts.push(`INV ${opts.invSkuCode}`);
  if (opts.finJournalCode) parts.push(`FIN ${opts.finJournalCode}`);
  else parts.push("chưa JE (TotalCost=0)");
  return `Đã đẩy giá thành: ${parts.join(" · ")}`;
}
