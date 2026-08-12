import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateReturnedItemInspection,
  validateRestockReceipt,
  calculateOnTimeDeliveryRate,
  calculateReturnFailureRate,
} from './log-step108-helpers.ts';

test('UC_LOG_028: validateReturnedItemInspection', () => {
  const valid = validateReturnedItemInspection(1, 5);
  assert.equal(valid.isValid, true);

  const overflow = validateReturnedItemInspection(6, 5);
  assert.equal(overflow.isValid, false);
});

test('UC_LOG_029: validateRestockReceipt', () => {
  const valid = validateRestockReceipt('WH-01');
  assert.equal(valid.canRestock, true);

  const empty = validateRestockReceipt('');
  assert.equal(empty.canRestock, false);
});

test('UC_LOG_034: calculateOnTimeDeliveryRate', () => {
  const rate = calculateOnTimeDeliveryRate(90, 100);
  assert.equal(rate.onTimeRatePct, 90);
});

test('UC_LOG_035: calculateReturnFailureRate', () => {
  const rate = calculateReturnFailureRate(5, 5, 100);
  assert.equal(rate.returnRatePct, 10);
});
