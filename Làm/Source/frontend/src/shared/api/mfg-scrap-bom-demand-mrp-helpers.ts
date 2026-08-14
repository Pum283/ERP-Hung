export function formatScrapPercentage(pct: number): string {
  return `+${pct.toFixed(1)}% Hao Hụt`;
}

export function formatGrossRequirement(net: number, pct: number): number {
  return Number((net * (1 + pct / 100)).toFixed(2));
}
