// pos-step84-helpers.ts
// Frontend helpers cho Bước 84:
//   UC_POS_059 — Đồng bộ doanh thu ca sang FIN (validateFinSyncRequest)
//   UC_POS_061 — Doanh thu theo giờ / ngày / ca (formatHourlyRevenueRow)
//   UC_POS_062 — Doanh thu theo sản phẩm (formatProductRevenueRow)
//   UC_POS_063 — Doanh thu theo thu ngân (formatCashierRevenueRow)

export function validateFinSyncRequest(
  shiftStatus: string,
  paidCount: number,
): { canSync: boolean; reason?: string } {
  if (shiftStatus !== 'Closed') {
    return { canSync: false, reason: 'Chỉ có thể đồng bộ sang FIN cho ca bán đã đóng (Closed).' };
  }
  if (paidCount <= 0) {
    return { canSync: false, reason: 'Ca bán không có đơn thanh toán nào — không cần đồng bộ.' };
  }
  return { canSync: true };
}

export function formatHourlyRevenueRow(
  hourLabel: string,
  salesCount: number,
  revenueAmount: number,
): string {
  return `⏰ Kèo giờ ${hourLabel}: ${salesCount} đơn | Doanh thu: ${revenueAmount.toLocaleString('vi-VN')} VNĐ`;
}

export function formatProductRevenueRow(
  productCode: string,
  productName: string,
  qty: number,
  totalAmount: number,
): string {
  return `📦 [${productCode}] ${productName}: Slg bán: ${qty.toLocaleString('vi-VN')} | Tổng tiền: ${totalAmount.toLocaleString('vi-VN')} VNĐ`;
}

export function formatCashierRevenueRow(
  cashierName: string,
  salesCount: number,
  totalRevenue: number,
): string {
  const avgPerSale = salesCount > 0 ? Math.round(totalRevenue / salesCount) : 0;
  return `👤 Thu ngân: ${cashierName} | Số đơn: ${salesCount} | Tổng DT: ${totalRevenue.toLocaleString('vi-VN')} VNĐ | TB/đơn: ${avgPerSale.toLocaleString('vi-VN')} VNĐ`;
}
