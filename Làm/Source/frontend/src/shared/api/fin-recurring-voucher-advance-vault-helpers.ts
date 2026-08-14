export function formatAdvanceRefundSummary(advance: number, settled: number, refund: number): string {
  return `Tạm ứng: ${advance.toLocaleString('vi-VN')} đ | Thực chi: ${settled.toLocaleString('vi-VN')} đ | Hoàn quỹ: ${refund.toLocaleString('vi-VN')} đ`;
}

export function formatFrequencyLabel(freq: string): string {
  if (freq === 'Monthly') return 'Hàng Tháng';
  if (freq === 'Quarterly') return 'Hàng Quý';
  if (freq === 'Annual') return 'Hàng Năm';
  return freq;
}
