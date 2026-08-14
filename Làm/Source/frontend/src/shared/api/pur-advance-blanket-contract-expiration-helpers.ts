export function calculateBlanketContractRemaining(
  totalValue: number,
  consumedValue: number,
  totalQty: number,
  consumedQty: number
): { remainingValue: number; remainingQty: number; consumedPercentage: number } {
  const remainingValue = Math.max(0, totalValue - consumedValue);
  const remainingQty = Math.max(0, totalQty - consumedQty);
  const consumedPercentage = totalValue > 0 ? Math.round((consumedValue / totalValue) * 10000) / 100 : 0;

  return {
    remainingValue,
    remainingQty,
    consumedPercentage,
  };
}

export function checkContractExpirationRisk(
  expirationDateIso: string,
  warningDaysThreshold: number = 30
): { daysLeft: number; isExpiringSoon: boolean; isExpired: boolean } {
  const exp = new Date(expirationDateIso);
  const now = new Date();
  const diffMs = exp.getTime() - now.getTime();
  const daysLeft = Math.ceil(diffMs / (1000 * 60 * 60 * 24));

  return {
    daysLeft,
    isExpiringSoon: daysLeft > 0 && daysLeft <= warningDaysThreshold,
    isExpired: daysLeft <= 0,
  };
}
