import test from 'node:test';
import assert from 'node:assert/strict';
import {
  evaluateOutcomeStatusBadge,
  calculateOnSiteOrderTotal,
  validateDemandEntry,
} from './crm-field-visit-outcome-order-helpers.ts';

test('evaluateOutcomeStatusBadge - formats outcome status badges', () => {
  const succ = evaluateOutcomeStatusBadge('successful');
  assert.equal(succ.label, 'Thành công');
  assert.equal(succ.badgeClass.includes('emerald'), true);

  const fail = evaluateOutcomeStatusBadge('unsuccessful');
  assert.equal(fail.label, 'Không thành công');
  assert.equal(fail.badgeClass.includes('rose'), true);
});

test('calculateOnSiteOrderTotal - calculates item totals accurately', () => {
  const items = [
    { qty: 10, price: 500000 },
    { qty: 2, price: 1500000 },
  ];
  assert.equal(calculateOnSiteOrderTotal(items), 8000000);
});

test('validateDemandEntry - validates required fields', () => {
  assert.equal(validateDemandEntry('', 10).isValid, false);
  assert.equal(validateDemandEntry('Phân bón', 0).isValid, false);
  assert.equal(validateDemandEntry('Phân bón', 10).isValid, true);
});
