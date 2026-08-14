export function calculateOtdRating(
  onTimeRate: number
): 'Excellent' | 'Good' | 'Poor' {
  if (onTimeRate >= 95) return 'Excellent';
  if (onTimeRate >= 85) return 'Good';
  return 'Poor';
}

export function calculateRfqNegotiationSavings(
  initialBudget: number,
  awardedAmount: number
): { savingsAmount: number; savingsPercentage: number } {
  if (initialBudget <= 0) return { savingsAmount: 0, savingsPercentage: 0 };
  const savingsAmount = initialBudget - awardedAmount;
  const savingsPercentage = Math.round((savingsAmount / initialBudget) * 10000) / 100;
  return { savingsAmount, savingsPercentage };
}
