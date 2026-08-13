import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateCommissionAmount,
  evaluateRetentionHealth,
  validateRedemptionRequest,
} from './crm-reward-survey-retention-commission-helpers.ts';

test('calculateCommissionAmount - calculates commission from revenue and rate', () => {
  assert.equal(calculateCommissionAmount(100000000, 2.5), 2500000);
  assert.equal(calculateCommissionAmount(0, 5.0), 0);
});

test('evaluateRetentionHealth - checks retention performance status', () => {
  const high = evaluateRetentionHealth(78);
  assert.equal(high.statusLabel.includes('Xuất sắc'), true);

  const low = evaluateRetentionHealth(45);
  assert.equal(low.statusLabel.includes('Cảnh báo'), true);
});

test('validateRedemptionRequest - validates available points balance', () => {
  assert.equal(validateRedemptionRequest(500, 800).isValid, false);
  assert.equal(validateRedemptionRequest(1000, 500).isValid, true);
});
