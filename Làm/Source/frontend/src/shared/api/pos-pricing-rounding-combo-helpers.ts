export interface CashRoundingResult {
  originalTotalVnd: number;
  roundedTotalVnd: number;
  roundingDifferenceVnd: number;
}

export function calculatePosCashRounding(totalVnd: number, interval = 500): CashRoundingResult {
  if (interval <= 0) interval = 500;
  const rounded = Math.round(totalVnd / interval) * interval;
  const diff = rounded - totalVnd;
  return {
    originalTotalVnd: totalVnd,
    roundedTotalVnd: rounded,
    roundingDifferenceVnd: diff,
  };
}

export function formatComboDiscountSavings(fixedComboPrice: number, originalSum: number): { savingsVnd: number; savingsPercent: number } {
  if (originalSum <= fixedComboPrice) {
    return { savingsVnd: 0, savingsPercent: 0 };
  }
  const savings = originalSum - fixedComboPrice;
  const pct = Math.round((savings / originalSum) * 100);
  return { savingsVnd: savings, savingsPercent: pct };
}
