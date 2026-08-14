export function formatTotalHours(hours: number, ot: number): string {
  return `${hours}h (OT: +${ot}h)`;
}

export function formatOverrunPercent(pct: number): string {
  return `+${pct.toFixed(1)}% Vượt Ngân Sách`;
}
