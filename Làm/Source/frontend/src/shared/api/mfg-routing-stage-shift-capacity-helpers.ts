export function formatCycleTime(minutes: number): string {
  return `${minutes} Phút / SP`;
}

export function formatEfficiencyPercentage(pct: number): string {
  return `${pct.toFixed(0)}% Hiệu Suất`;
}
