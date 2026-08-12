import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateReservationRelease,
  calculateAvailableQty,
  formatBalanceSummary,
  validateMinMaxStockAlert,
} from './inv-step99-helpers.ts';

test('UC_INV_038: validateReservationRelease', () => {
  const valid = validateReservationRelease('Active');
  assert.equal(valid.canRelease, true);

  const released = validateReservationRelease('Released');
  assert.equal(released.canRelease, false);
  assert.match(released.reason!, /đã được giải phóng/);
});

test('UC_INV_039: calculateAvailableQty', () => {
  const { availableQty } = calculateAvailableQty(100, 20, 5);
  assert.equal(availableQty, 85); // 100 - 20 + 5
});

test('UC_INV_041: formatBalanceSummary', () => {
  const summary = formatBalanceSummary(50, 10, 5);
  assert.match(summary.summaryText, /Tồn kho: 50/);
  assert.match(summary.summaryText, /Đang giữ: 10/);
});

test('UC_INV_042: validateMinMaxStockAlert', () => {
  const belowMin = validateMinMaxStockAlert(5, 10, 100);
  assert.equal(belowMin.alertType, 'BelowMin');
  assert.match(belowMin.message!, /tối thiểu/);

  const aboveMax = validateMinMaxStockAlert(150, 10, 100);
  assert.equal(aboveMax.alertType, 'AboveMax');

  const normal = validateMinMaxStockAlert(50, 10, 100);
  assert.equal(normal.alertType, 'Normal');
});
