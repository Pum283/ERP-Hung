export function formatOnTimeRate(rate: number): string {
  return `${rate.toFixed(1)}%`;
}

export function formatWeightTons(kg: number): string {
  return `${(kg / 1000).toFixed(1)} Tấn`;
}
