export function formatExchangeRate(rate: number, code: string): string {
  if (code === 'VND') return '1 VNĐ (Đồng Tiền Cơ Sở)';
  return `1 ${code} = ${rate.toLocaleString('vi-VN')} VNĐ`;
}

export function formatCashFlowTypeBadge(type: string): string {
  if (type === 'Inflow') return 'bg-emerald-100 text-emerald-800 border-emerald-300';
  return 'bg-rose-100 text-rose-800 border-rose-300';
}
