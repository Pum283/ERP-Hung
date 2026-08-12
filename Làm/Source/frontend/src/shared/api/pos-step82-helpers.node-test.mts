import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateOpenShiftRequest,
  validateInitialCash,
  formatShiftRevenueSummary,
  validateCloseShiftRequest,
} from './pos-step82-helpers.ts';

test('UC_POS_042: validateOpenShiftRequest - valid request', () => {
  const res = validateOpenShiftRequest('STORE-01', false);
  assert.equal(res.canOpen, true);
});

test('UC_POS_042: validateOpenShiftRequest - active shift or missing store', () => {
  const res1 = validateOpenShiftRequest('', false);
  assert.equal(res1.canOpen, false);
  assert.match(res1.error!, /Phải chọn điểm bán/);

  const res2 = validateOpenShiftRequest('STORE-01', true);
  assert.equal(res2.canOpen, false);
  assert.match(res2.error!, /đang có ca mở/);
});

test('UC_POS_043: validateInitialCash', () => {
  const valid = validateInitialCash(500000);
  assert.equal(valid.isValid, true);

  const zeroCash = validateInitialCash(0);
  assert.equal(zeroCash.isValid, true);

  const negativeCash = validateInitialCash(-1000);
  assert.equal(negativeCash.isValid, false);
  assert.match(negativeCash.error!, /không được là số âm/);
});

test('UC_POS_045: formatShiftRevenueSummary', () => {
  const summary = formatShiftRevenueSummary(500000, 1200000, 800000, 15);
  assert.equal(summary.includes('15'), true);
  assert.equal(summary.includes('2.000.000'), true);
  assert.equal(summary.includes('1.700.000'), true);
});

test('UC_POS_046: validateCloseShiftRequest', () => {
  const valid = validateCloseShiftRequest('Open', 1700000);
  assert.equal(valid.canClose, true);

  const notOpen = validateCloseShiftRequest('Closed', 1700000);
  assert.equal(notOpen.canClose, false);

  const negativeCount = validateCloseShiftRequest('Open', -500);
  assert.equal(negativeCount.canClose, false);
});
