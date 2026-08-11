import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateDailyHourlyRates,
  validatePayrollPeriodConfirm,
  calculateNetSalary,
  formatPayrollPeriodStatus,
} from './hrm-step41-helpers.ts';

// ─── UC_HRM_155: calculateDailyHourlyRates ───

test('calculateDailyHourlyRates - calculates daily and hourly rates accurately', () => {
  const res = calculateDailyHourlyRates(13000000, 26);
  assert.equal(res.dailyRate, 500000);
  assert.equal(res.hourlyRate, 62500);
});

test('calculateDailyHourlyRates - zero base salary returns zero rates', () => {
  const res = calculateDailyHourlyRates(0, 26);
  assert.equal(res.dailyRate, 0);
  assert.equal(res.hourlyRate, 0);
});

// ─── UC_HRM_156: validatePayrollPeriodConfirm & calculateNetSalary ───

test('validatePayrollPeriodConfirm - calculated status returns valid', () => {
  const res = validatePayrollPeriodConfirm('p-1', 'Calculated');
  assert.equal(res.valid, true);
});

test('validatePayrollPeriodConfirm - draft status returns error', () => {
  const res = validatePayrollPeriodConfirm('p-1', 'Draft');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('đã được tính toán'));
});

test('calculateNetSalary - calculates gross and net pay correctly', () => {
  const res = calculateNetSalary({
    attendancePay: 13000000,
    otPay: 1500000,
    allowanceTotal: 1000000,
    bonus: 500000,
    insuranceEmployee: 1365000,
    tax: 250000,
    deductionTotal: 0,
  });

  assert.equal(res.grossPay, 16000000);
  assert.equal(res.netPay, 14385000);
});

// ─── UC_HRM_158: formatPayrollPeriodStatus ───

test('formatPayrollPeriodStatus - returns correct status label', () => {
  assert.ok(formatPayrollPeriodStatus('Calculated').includes('Đã tính'));
  assert.ok(formatPayrollPeriodStatus('Confirmed').includes('Đã duyệt'));
});
