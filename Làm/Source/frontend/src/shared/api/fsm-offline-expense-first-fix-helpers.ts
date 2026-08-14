export function formatFtfrPercentage(rate: number): string {
  return `${rate.toFixed(1)}% FTFR`;
}

export function formatSettlementNet(net: number): string {
  return `${net.toLocaleString('vi-VN')} đ Quyết Toán`;
}
