import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateLeaveFundUsageRate,
  calculateHeadcountVariance,
  validateLmsProgramInput,
  formatLmsProgramStatus,
} from './hrm-step47-helpers.ts';

// ─── UC_HRM_185: calculateLeaveFundUsageRate ───

test('calculateLeaveFundUsageRate - calculates usage percentage correctly', () => {
  const res = calculateLeaveFundUsageRate(12, 4);
  assert.equal(res.usageRatePct, 33.33);
  assert.equal(res.isOverLimit, false);
});

test('calculateLeaveFundUsageRate - detects over limit leave usage', () => {
  const res = calculateLeaveFundUsageRate(12, 14);
  assert.equal(res.usageRatePct, 116.67);
  assert.equal(res.isOverLimit, true);
});

// ─── UC_HRM_187: calculateHeadcountVariance ───

test('calculateHeadcountVariance - calculates variance and status label correctly', () => {
  const res = calculateHeadcountVariance(10, 8);
  assert.equal(res.variance, -2);
  assert.equal(res.fulfillmentPct, 80);
  assert.ok(res.statusLabel.includes('Thiếu 2 nhân sự'));
});

// ─── UC_LMS_001: validateLmsProgramInput & formatLmsProgramStatus ───

test('validateLmsProgramInput - valid input returns valid', () => {
  const res = validateLmsProgramInput({ code: 'PROG_01', name: 'Đào tạo An toàn', status: 'Active' });
  assert.equal(res.valid, true);
});

test('validateLmsProgramInput - empty code returns error', () => {
  const res = validateLmsProgramInput({ code: '', name: 'Tên hợp lệ', status: 'Active' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Mã chương trình'));
});

test('formatLmsProgramStatus - returns status label with icon', () => {
  assert.ok(formatLmsProgramStatus('Active').includes('Đang hoạt động'));
  assert.ok(formatLmsProgramStatus('Inactive').includes('Ngưng áp dụng'));
});
