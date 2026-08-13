import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateReceivingDiscrepancy,
  determineDiscrepancySeverity,
} from './pur-reject-return-delivery-protocol-discrepancy-helpers.ts';

test('calculateReceivingDiscrepancy - calculates shortage/excess quantity and amount correctly', () => {
  const shortage = calculateReceivingDiscrepancy(100, 95, 240000);
  assert.equal(shortage.diffQty, 5);
  assert.equal(shortage.diffAmountVnd, 1200000);
  assert.equal(shortage.isShortage, true);
  assert.equal(shortage.isExcess, false);

  const excess = calculateReceivingDiscrepancy(100, 105, 240000);
  assert.equal(excess.diffQty, -5);
  assert.equal(excess.diffAmountVnd, 1200000);
  assert.equal(excess.isShortage, false);
  assert.equal(excess.isExcess, true);
});

test('determineDiscrepancySeverity - evaluates financial risk severity level', () => {
  assert.equal(determineDiscrepancySeverity(1200000), 'Minor');
  assert.equal(determineDiscrepancySeverity(15000000), 'Moderate');
  assert.equal(determineDiscrepancySeverity(80000000), 'Critical');
});
