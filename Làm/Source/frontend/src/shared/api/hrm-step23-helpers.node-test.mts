import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateShiftHeadcountPlan,
  validateDeptHeadcountPlan,
  calculateHeadcountGap,
  isHeadcountShortage,
} from './hrm-step23-helpers.ts';

// ─── UC_HRM_076: validateShiftHeadcountPlan ───

test('validateShiftHeadcountPlan - valid shift code returns valid', () => {
  const res = validateShiftHeadcountPlan('SH_MORNING', 10);
  assert.equal(res.valid, true);
});

test('validateShiftHeadcountPlan - empty shift code returns error', () => {
  const res = validateShiftHeadcountPlan('   ', 10);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Mã ca'));
});

test('validateShiftHeadcountPlan - shift code over 40 chars returns error', () => {
  const res = validateShiftHeadcountPlan('S'.repeat(41), 10);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('40 ký tự'));
});

test('validateShiftHeadcountPlan - negative headcount returns error', () => {
  const res = validateShiftHeadcountPlan('SH_MORNING', -5);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không âm'));
});

// ─── UC_HRM_077: validateDeptHeadcountPlan ───

test('validateDeptHeadcountPlan - valid deptId returns valid', () => {
  const res = validateDeptHeadcountPlan('dept-123', 15);
  assert.equal(res.valid, true);
});

test('validateDeptHeadcountPlan - empty deptId returns error', () => {
  const res = validateDeptHeadcountPlan('   ', 15);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Bộ phận'));
});

test('validateDeptHeadcountPlan - negative headcount returns error', () => {
  const res = validateDeptHeadcountPlan('dept-123', -2);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không âm'));
});

// ─── UC_HRM_078 & UC_HRM_079: Gap & Shortage ───

test('calculateHeadcountGap - planned 10 actual 3 returns gap 7', () => {
  const gap = calculateHeadcountGap(10, 3);
  assert.equal(gap, 7);
});

test('calculateHeadcountGap - planned 10 actual 12 returns gap -2', () => {
  const gap = calculateHeadcountGap(10, 12);
  assert.equal(gap, -2);
});

test('isHeadcountShortage - gap > 0 returns true', () => {
  assert.equal(isHeadcountShortage(10, 3), true);
});

test('isHeadcountShortage - gap <= 0 returns false', () => {
  assert.equal(isHeadcountShortage(10, 10), false);
  assert.equal(isHeadcountShortage(10, 12), false);
});
