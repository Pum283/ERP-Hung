export function calculatePosEarnedLoyaltyPoints(orderTotalVnd: number, rateVndPerPoint = 10000): number {
  if (rateVndPerPoint <= 0 || orderTotalVnd <= 0) return 0;
  return Math.floor(orderTotalVnd / rateVndPerPoint);
}

export function calculatePosRedeemPointsDiscount(pointsToRedeem: number, valuePerPointVnd = 1000): number {
  if (pointsToRedeem <= 0 || valuePerPointVnd <= 0) return 0;
  return pointsToRedeem * valuePerPointVnd;
}
