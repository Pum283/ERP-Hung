import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateMixedPaymentBalance,
  calculateShiftCashNetBalance,
} from './pos-kitchen-mixed-pay-cross-sell-drawer-helpers.ts';

test('calculateMixedPaymentBalance - calculates total paid and remaining balance', () => {
  const res = calculateMixedPaymentBalance(150000, [
    { amountVnd: 50000 },
    { amountVnd: 100000 },
  ]);
  assert.equal(res.totalPaidVnd, 150000);
  assert.equal(res.balanceRemainingVnd, 0);
  assert.equal(res.isFullyPaid, true);
});

test('calculateShiftCashNetBalance - calculates drawer net float balance', () => {
  const balance = calculateShiftCashNetBalance(500000, 200000, 1000000);
  assert.equal(balance, 1300000);
});
