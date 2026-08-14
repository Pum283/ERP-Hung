export function formatPercentageBreakdown(pct: number): string {
  return `${pct.toFixed(1)}%`;
}

export function getPurposeColorIndicator(purpose: string): string {
  if (purpose.includes('Bán Hàng')) return 'text-blue-600 bg-blue-50 border-blue-200';
  if (purpose.includes('Dự Án')) return 'text-purple-600 bg-purple-50 border-purple-200';
  if (purpose.includes('Sản Xuất')) return 'text-amber-600 bg-amber-50 border-amber-200';
  if (purpose.includes('Kỹ Thuật')) return 'text-indigo-600 bg-indigo-50 border-indigo-200';
  return 'text-slate-600 bg-slate-50 border-slate-200';
}
