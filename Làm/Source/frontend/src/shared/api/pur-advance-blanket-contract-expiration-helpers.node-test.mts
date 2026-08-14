import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateBlanketContractRemaining,
  checkContractExpirationRisk,
} from './pur-advance-blanket-contract-expiration-helpers.ts';

test('calculateBlanketContractRemaining - calculates remaining capacity and consumed percentage', () => {
  const res = calculateBlanketContractRemaining(500000000, 320000000, 20000, 12800);
  assert.equal(res.remainingValue, 180000000);
  assert.equal(res.remainingQty, 7200);
  assert.equal(res.consumedPercentage, 64);
});

test('checkContractExpirationRisk - checks days left and expiring soon status', () => {
  const futureDate = new Date();
  futureDate.setDate(futureDate.getDate() + 15);
  const check = checkContractExpirationRisk(futureDate.toISOString(), 30);

  assert.equal(check.isExpiringSoon, true);
  assert.equal(check.isExpired, false);
});
