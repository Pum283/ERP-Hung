export function formatBillableRepairAmount(amt: number, isWarranty: boolean): string {
  if (isWarranty) return 'Miễn Phí (Bảo Hành)';
  return `${amt.toLocaleString('vi-VN')} đ`;
}

export function formatStarRating(stars: number): string {
  return '★'.repeat(stars) + '☆'.repeat(5 - stars);
}
