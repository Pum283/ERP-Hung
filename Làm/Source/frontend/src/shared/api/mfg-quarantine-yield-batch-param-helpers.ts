export function formatYieldPercentage(rate: number): string {
  return `${rate.toFixed(1)}% Đạt Chuẩn`;
}

export function formatBatchQuantity(actual: number, planned: number): string {
  return `${actual} / ${planned} SP`;
}
