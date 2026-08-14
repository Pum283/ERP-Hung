export function formatHourlyRate(rate: number): string {
  return `${rate.toLocaleString('vi-VN')} đ / Giờ`;
}

export function formatTravelFee(fee: number): string {
  return `${fee.toLocaleString('vi-VN')} đ / Lượt di chuyển`;
}
