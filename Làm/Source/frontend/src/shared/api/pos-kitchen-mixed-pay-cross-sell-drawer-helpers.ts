export interface MixedPaymentBalanceResult {
  orderTotalVnd: number;
  totalPaidVnd: number;
  balanceRemainingVnd: number;
  isFullyPaid: boolean;
}

export function calculateMixedPaymentBalance(
  orderTotalVnd: number,
  payments: { amountVnd: number }[]
): MixedPaymentBalanceResult {
  const totalPaid = (payments || []).reduce((acc, p) => acc + (p.amountVnd || 0), 0);
  const remaining = orderTotalVnd - totalPaid;
  return {
    orderTotalVnd,
    totalPaidVnd: totalPaid,
    balanceRemainingVnd: remaining > 0 ? remaining : 0,
    isFullyPaid: remaining <= 0,
  };
}

export function calculateShiftCashNetBalance(
  cashInTotal: number,
  cashOutTotal: number,
  initialFloat = 1000000
): number {
  return initialFloat + cashInTotal - cashOutTotal;
}
