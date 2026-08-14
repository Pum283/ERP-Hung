export function formatFrequencyLabel(freq: string): string {
  if (freq === 'Monthly') return 'Hàng Tháng';
  if (freq === 'Quarterly') return 'Hàng Quý';
  if (freq === 'SemiAnnual') return 'Nửa Năm / Lần';
  return 'Hàng Năm';
}

export function formatCompletionRate(rate: number): string {
  return `${rate.toFixed(1)}% Đúng Hạn`;
}
