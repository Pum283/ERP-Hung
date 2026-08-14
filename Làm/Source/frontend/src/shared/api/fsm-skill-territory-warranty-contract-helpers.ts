export function formatWarrantyDaysRemaining(days: number): string {
  if (days <= 0) return 'Đã Hết Hạn Bảo Hành';
  return `Còn ${days} ngày bảo hành`;
}

export function formatContractValue(val: number): string {
  return `${val.toLocaleString('vi-VN')} đ / Năm`;
}
