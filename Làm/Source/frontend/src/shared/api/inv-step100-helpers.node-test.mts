import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateLotTracking,
  calculateDaysToExpiry,
  validateExpiryForIssue,
  filterNearExpiryLots,
} from './inv-step100-helpers.ts';

test('UC_INV_043: validateLotTracking', () => {
  const valid = validateLotTracking('LOT-100');
  assert.equal(valid.isTracked, true);

  const empty = validateLotTracking('');
  assert.equal(empty.isTracked, false);
  assert.match(empty.error!, /Mã Lô/);
});

test('UC_INV_044: calculateDaysToExpiry', () => {
  const futureDate = new Date(Date.now() + 15 * 86400000).toISOString();
  const res = calculateDaysToExpiry(futureDate);
  assert.equal(res.isExpired, false);
  assert.equal(res.isNearExpiry, true);

  const pastDate = new Date(Date.now() - 5 * 86400000).toISOString();
  const expired = calculateDaysToExpiry(pastDate);
  assert.equal(expired.isExpired, true);
});

test('UC_INV_045: validateExpiryForIssue', () => {
  const pastDate = new Date(Date.now() - 5 * 86400000).toISOString();
  const check = validateExpiryForIssue(pastDate);
  assert.equal(check.canIssue, false);
  assert.match(check.reason!, /chặn xuất kho/);

  const futureDate = new Date(Date.now() + 60 * 86400000).toISOString();
  const validCheck = validateExpiryForIssue(futureDate);
  assert.equal(validCheck.canIssue, true);
});

test('UC_INV_048: filterNearExpiryLots', () => {
  const items = [
    { lot: 'L1', daysRemaining: 10 },
    { lot: 'L2', daysRemaining: 45 },
    { lot: 'L3', daysRemaining: -2 },
  ];
  const nearExpiry = filterNearExpiryLots(items, 30);
  assert.equal(nearExpiry.length, 1);
  assert.equal(nearExpiry[0].lot, 'L1');
});
