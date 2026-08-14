export function formatOffsetBalanceSummary(ar: number, ap: number, net: number): string {
  return `AR: ${ar.toLocaleString('vi-VN')} đ ↔ AP: ${ap.toLocaleString('vi-VN')} đ | Chênh lệch: ${net.toLocaleString('vi-VN')} đ`;
}

export function formatDunningLevelBadge(level: string): string {
  if (level === 'Level1_Reminder') return 'bg-amber-100 text-amber-800 border-amber-300';
  if (level === 'Level2_Warning') return 'bg-orange-100 text-orange-800 border-orange-300';
  return 'bg-rose-100 text-rose-800 border-rose-300';
}
