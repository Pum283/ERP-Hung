export function calculateReceivingDiscrepancy(
  orderedQty: number,
  receivedQty: number,
  unitPriceVnd: number
): { diffQty: number; diffAmountVnd: number; isShortage: boolean; isExcess: boolean } {
  const diffQty = orderedQty - receivedQty;
  const diffAmountVnd = Math.abs(diffQty) * unitPriceVnd;
  return {
    diffQty,
    diffAmountVnd,
    isShortage: diffQty > 0,
    isExcess: diffQty < 0,
  };
}

export function determineDiscrepancySeverity(
  discrepancyAmountVnd: number
): 'Minor' | 'Moderate' | 'Critical' {
  if (discrepancyAmountVnd >= 50000000) return 'Critical';
  if (discrepancyAmountVnd >= 5000000) return 'Moderate';
  return 'Minor';
}
