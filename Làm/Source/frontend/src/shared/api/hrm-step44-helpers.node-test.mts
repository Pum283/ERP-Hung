import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validatePayrollLinePatchInput,
  validatePeriodLockEligibility,
  filterPayrollLines,
  formatPayslipSummary,
} from './hrm-step44-helpers.ts';

// ─── UC_HRM_168: validatePayrollLinePatchInput & filterPayrollLines ───

test('validatePayrollLinePatchInput - valid inputs return valid', () => {
  const res = validatePayrollLinePatchInput({ bonus: 1000000, deductionTotal: 500000, allowanceTotal: 2000000 });
  assert.equal(res.valid, true);
});

test('validatePayrollLinePatchInput - negative bonus returns error', () => {
  const res = validatePayrollLinePatchInput({ bonus: -500000 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('số âm'));
});

test('filterPayrollLines - filters lines by code or name', () => {
  const lines = [
    { employeeCode: 'EMP001', employeeName: 'Nguyễn Văn A' },
    { employeeCode: 'EMP002', employeeName: 'Trần Thị B' },
  ];

  const result = filterPayrollLines(lines, 'Trần');
  assert.equal(result.length, 1);
  assert.equal(result[0].employeeCode, 'EMP002');
});

// ─── UC_HRM_170: validatePeriodLockEligibility & formatPayslipSummary ───

test('validatePeriodLockEligibility - confirmed status returns valid', () => {
  const res = validatePeriodLockEligibility('Confirmed');
  assert.equal(res.valid, true);
});

test('validatePeriodLockEligibility - calculated status returns error', () => {
  const res = validatePeriodLockEligibility('Calculated');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('xác nhận bảng lương'));
});

test('formatPayslipSummary - formats payslip summary correctly', () => {
  const text = formatPayslipSummary('Lê Văn C', 20000000, 17500000);
  assert.ok(text.includes('Lê Văn C'));
  assert.ok(text.includes('17.500.000'));
});
