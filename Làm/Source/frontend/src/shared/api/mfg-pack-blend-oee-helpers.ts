export function formatOeePercentage(pct: number): string {
  return `${pct.toFixed(1)}% OEE`;
}

export function formatMixingRatio(ratio: number, tolerance: number): string {
  return `${ratio.toFixed(1)}% (±${tolerance}%)`;
}
