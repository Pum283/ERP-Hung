import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatBillableRepairAmount,
  formatStarRating,
} from './fsm-repair-feedback-reopen-finance-helpers.ts';

test('formatBillableRepairAmount - formats billable amount with warranty check', () => {
  assert.equal(formatBillableRepairAmount(0, true), 'Miễn Phí (Bảo Hành)');
  assert.equal(formatBillableRepairAmount(1250000, false), '1.250.000 đ');
});

test('formatStarRating - generates star string representation', () => {
  assert.equal(formatStarRating(5), '★★★★★');
  assert.equal(formatStarRating(4), '★★★★☆');
});
