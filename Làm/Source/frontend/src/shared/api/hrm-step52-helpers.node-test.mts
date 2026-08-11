import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateVoucherCode,
  calculateCourseFinalPrice,
  formatCourseEnrollmentStatus,
  calculateCourseProgressPercentage,
} from './hrm-step52-helpers.ts';

// ─── UC_LMS_032: validateVoucherCode ───

test('validateVoucherCode - FREE voucher returns 100% discount', () => {
  const res = validateVoucherCode('FREE');
  assert.equal(res.valid, true);
  assert.equal(res.discountPct, 100);
});

test('validateVoucherCode - invalid voucher returns error', () => {
  const res = validateVoucherCode('INVALID_CODE');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không hợp lệ'));
});

// ─── UC_LMS_031: calculateCourseFinalPrice ───

test('calculateCourseFinalPrice - calculates price with 20% voucher', () => {
  const price = calculateCourseFinalPrice(500000, 'OFF20');
  assert.equal(price, 400000);
});

test('calculateCourseFinalPrice - FREE voucher makes price zero', () => {
  const price = calculateCourseFinalPrice(500000, 'FREE');
  assert.equal(price, 0);
});

// ─── UC_LMS_033: formatCourseEnrollmentStatus ───

test('formatCourseEnrollmentStatus - returns status label with icon', () => {
  assert.ok(formatCourseEnrollmentStatus('Unlocked').includes('Đã mở khóa'));
  assert.ok(formatCourseEnrollmentStatus('Completed').includes('Đã hoàn thành'));
});

// ─── UC_LMS_034: calculateCourseProgressPercentage ───

test('calculateCourseProgressPercentage - calculates progress percentage correctly', () => {
  const res = calculateCourseProgressPercentage(8, 10);
  assert.equal(res.progressPct, 80);
  assert.equal(res.isCompleted, false);
});
