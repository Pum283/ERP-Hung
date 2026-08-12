// inv-step103-helpers.ts
// Frontend helpers cho Bước 103:
//   UC_INV_064 — Xuất nhập tồn theo kỳ (formatPeriodSummaryTitle)
//   UC_INV_065 — Thẻ kho / lịch sử sản phẩm (formatStockCardRow)
//   UC_INV_067 — Hàng dưới min / trên max (validateStockLevelThreshold)
//   UC_INV_069 — Dashboard tồn & cảnh báo (formatDashboardWidgetStats)

export function formatPeriodSummaryTitle(fromDate: string, toDate: string): { title: string } {
  return {
    title: `Báo cáo Xuất Nhập Tồn (${fromDate} ➔ ${toDate})`,
  };
}

export function formatStockCardRow(
  date: string,
  docCode: string,
  inQty: number,
  outQty: number,
  balance: number,
): { formattedRowText: string } {
  return {
    formattedRowText: `[${date}] Phiếu: ${docCode} | Nhập: +${inQty} | Xuất: -${outQty} | Tồn cuối: ${balance}`,
  };
}

export function validateStockLevelThreshold(
  qtyOnHand: number,
  minQty?: number,
  maxQty?: number,
): { status: 'LOW' | 'HIGH' | 'OK'; message?: string } {
  if (minQty !== undefined && minQty !== null && qtyOnHand < minQty) {
    return { status: 'LOW', message: `Số lượng tồn thực tế (${qtyOnHand}) dưới ngưỡng tối thiểu (${minQty}).` };
  }
  if (maxQty !== undefined && maxQty !== null && qtyOnHand > maxQty) {
    return { status: 'HIGH', message: `Số lượng tồn thực tế (${qtyOnHand}) vượt ngưỡng tối đa (${maxQty}).` };
  }
  return { status: 'OK' };
}

export function formatDashboardWidgetStats(
  totalSkus: number,
  lowStockCount: number,
  nearExpiryCount: number,
): { widgetTitle: string; warningCount: number } {
  const warningCount = lowStockCount + nearExpiryCount;
  return {
    widgetTitle: `Tổng SKU: ${totalSkus} (${warningCount} cảnh báo cần xử lý)`,
    warningCount,
  };
}
