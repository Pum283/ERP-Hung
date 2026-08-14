export function formatProgressPercent(pct: number): string {
  return `${pct.toFixed(0)}% Hoàn Thành`;
}

export function formatGanttStatusBadge(status: string): string {
  if (status === 'Completed') return 'bg-emerald-100 text-emerald-800 border-emerald-300';
  if (status === 'InProgress') return 'bg-blue-100 text-blue-800 border-blue-300';
  if (status === 'Delayed') return 'bg-rose-100 text-rose-800 border-rose-300';
  return 'bg-slate-100 text-slate-800 border-slate-300';
}
