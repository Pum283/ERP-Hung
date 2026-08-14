export function formatVariancePercentage(pct: number): string {
  const sign = pct > 0 ? '+' : '';
  return `${sign}${pct.toFixed(1)}% Lệch`;
}

export function getQcResultBadge(result: string): { label: string; colorClass: string } {
  switch (result) {
    case 'Pass':
      return { label: 'Đạt Tiêu Chuẩn (Pass)', colorClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' };
    case 'Fail':
      return { label: 'Không Đạt (Fail)', colorClass: 'bg-rose-100 text-rose-800 border-rose-300' };
    case 'ConditionalPass':
    default:
      return { label: 'Đạt Có Điều Kiện', colorClass: 'bg-amber-100 text-amber-800 border-amber-300' };
  }
}
