import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateOverdueDays,
  calculatePassRatePct,
  calculateDropoutRatePct,
} from './lms-training-reports-helpers.ts';

test('calculateOverdueDays - calculates overdue days correctly', () => {
  const dueDate = '2026-08-01T00:00:00Z';
  const currentDate = '2026-08-10T00:00:00Z';
  const days = calculateOverdueDays(dueDate, currentDate);
  assert.equal(days, 9);
});

test('calculateOverdueDays - returns 0 when not overdue', () => {
  const dueDate = '2026-08-20T00:00:00Z';
  const currentDate = '2026-08-10T00:00:00Z';
  const days = calculateOverdueDays(dueDate, currentDate);
  assert.equal(days, 0);
});

test('calculatePassRatePct - excellent pass rate', () => {
  const res = calculatePassRatePct(18, 20); // 90%
  assert.equal(res.passRatePct, 90);
  assert.equal(res.gradeBadge, 'Excellent');
});

test('calculatePassRatePct - needs improvement pass rate', () => {
  const res = calculatePassRatePct(12, 20); // 60%
  assert.equal(res.passRatePct, 60);
  assert.equal(res.gradeBadge, 'NeedsImprovement');
});

test('calculateDropoutRatePct - high risk level', () => {
  const res = calculateDropoutRatePct(10, 40); // 25%
  assert.equal(res.dropoutRatePct, 25);
  assert.equal(res.riskLevel, 'High');
});
