export function formatWarrantyPeriod(months: number): string {
  return `${months} Tháng Bảo Hành`;
}

export function formatUtilizationPercent(pct: number): string {
  return `${pct.toFixed(1)}% Hiệu Suất Nguồn Lực`;
}
