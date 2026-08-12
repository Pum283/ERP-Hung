// inv-step98-helpers.ts
// Frontend helpers cho Bước 98:
//   UC_INV_033 — Xuất bên gửi / nhập bên nhận (validateTransferShipment)
//   UC_INV_035 — Theo dõi hàng đang chuyển (formatTransferStatusBadge)
//   UC_INV_036 — Chuyển từ kho trung tâm (validateCentralWarehouseDistribution)
//   UC_INV_037 — Giữ hàng theo đơn đã duyệt (validateReservationCreate)

export function validateTransferShipment(status: string): { canShip: boolean; reason?: string } {
  if (status !== 'Draft') {
    return { canShip: false, reason: 'Chỉ có thể xuất kho phiếu chuyển hàng ở trạng thái Dự thảo (Draft).' };
  }
  return { canShip: true };
}

export function formatTransferStatusBadge(status: string): { label: string; badgeStyle: 'info' | 'warning' | 'success' | 'danger' } {
  switch (status) {
    case 'Draft':
      return { label: '📝 Dự thảo', badgeStyle: 'info' };
    case 'InTransit':
      return { label: '🚚 Đang vận chuyển', badgeStyle: 'warning' };
    case 'Completed':
      return { label: '✅ Đã nhập kho đến', badgeStyle: 'success' };
    case 'Cancelled':
      return { label: '❌ Đã hủy', badgeStyle: 'danger' };
    default:
      return { label: status, badgeStyle: 'info' };
  }
}

export function validateCentralWarehouseDistribution(fromWarehouseCode: string): { isCentralWarehouse: boolean } {
  const code = fromWarehouseCode.toUpperCase();
  const isCentral = code.includes('CENTER') || code.includes('CENTRAL') || code.includes('TONG') || code.includes('MAIN');
  return { isCentralWarehouse: isCentral };
}

export function validateReservationCreate(
  warehouseId: string,
  skuId: string,
  qty: number,
): { isValid: boolean; error?: string } {
  if (!warehouseId || warehouseId.trim().length === 0) {
    return { isValid: false, error: 'Phải chọn nhà kho để giữ hàng.' };
  }
  if (!skuId || skuId.trim().length === 0) {
    return { isValid: false, error: 'Phải chọn SKU sản phẩm cần giữ hàng.' };
  }
  if (isNaN(qty) || qty <= 0) {
    return { isValid: false, error: 'Số lượng giữ hàng phải > 0.' };
  }
  return { isValid: true };
}
