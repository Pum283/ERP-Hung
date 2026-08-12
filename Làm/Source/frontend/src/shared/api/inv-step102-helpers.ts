// inv-step102-helpers.ts
// Frontend helpers cho Bước 102:
//   UC_INV_055 — Báo cáo kết quả kiểm kê (formatStocktakeSummary)
//   UC_INV_060 — Xem giá trị tồn (calculateInventoryValuation)
//   UC_INV_062 — Đẩy bút toán kho sang FIN (validateFinJournalPosting)
//   UC_INV_063 — Báo cáo giá trị tồn (formatValuationReportTitle)

export function formatStocktakeSummary(
  totalItems: number,
  surplusCount: number,
  shortageCount: number,
): { summary: string } {
  return {
    summary: `Tổng SKU đếm: ${totalItems} | Thừa: ${surplusCount} | Thiếu: ${shortageCount}`,
  };
}

export function calculateInventoryValuation(
  qty: number,
  unitCost: number,
): { totalValue: number } {
  const totalValue = Math.max(0, qty) * Math.max(0, unitCost);
  return { totalValue };
}

export function validateFinJournalPosting(docStatus: string): { canPostToFin: boolean; reason?: string } {
  if (docStatus !== 'Posted') {
    return { canPostToFin: false, reason: 'Chỉ có thể hạch toán bút toán FIN cho các chứng từ kho đã ghi sổ (Posted).' };
  }
  return { canPostToFin: true };
}

export function formatValuationReportTitle(
  warehouseName?: string,
  asOfDate?: string,
): { reportTitle: string } {
  const whText = warehouseName && warehouseName.trim().length > 0 ? warehouseName : 'Toàn bộ kho';
  const dateText = asOfDate && asOfDate.trim().length > 0 ? asOfDate : 'Hiện tại';
  return {
    reportTitle: `Báo cáo Giá trị Tồn kho [${whText}] - Thời điểm: ${dateText}`,
  };
}
