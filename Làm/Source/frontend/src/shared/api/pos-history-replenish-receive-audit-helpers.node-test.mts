import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateQuickAuditDiscrepancy,
  validateReplenishmentItemsCount,
} from './pos-history-replenish-receive-audit-helpers.ts';

test('calculateQuickAuditDiscrepancy - calculates difference between actual and system stock', () => {
  const d1 = calculateQuickAuditDiscrepancy(24, 22);
  assert.equal(d1.diff, -2);
  assert.equal(d1.isMatch, false);

  const d2 = calculateQuickAuditDiscrepancy(10, 10);
  assert.equal(d2.diff, 0);
  assert.equal(d2.isMatch, true);
});

test('validateReplenishmentItemsCount - validates items array and total quantity', () => {
  assert.equal(validateReplenishmentItemsCount([]).isValid, false);
  const v = validateReplenishmentItemsCount([{ quantityRequested: 24 }, { quantityRequested: 10 }]);
  assert.equal(v.isValid, true);
  assert.equal(v.totalQty, 34);
});
