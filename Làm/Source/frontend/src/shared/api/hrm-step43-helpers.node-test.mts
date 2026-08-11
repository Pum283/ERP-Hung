import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validatePayrollPeriodKeyFormat,
  validatePayrollAdjustmentInput,
  formatAdjustmentKind,
  type PayrollAdjustmentInput,
} from './hrm-step43-helpers.ts';

// ─── UC_HRM_163: validatePayrollPeriodKeyFormat ───

test('validatePayrollPeriodKeyFormat - valid yyyy-MM returns valid', () => {
  assert.equal(validatePayrollPeriodKeyFormat('2026-08').valid, true);
  assert.equal(validatePayrollPeriodKeyFormat('2026-12').valid, true);
});

test('validatePayrollPeriodKeyFormat - invalid format returns error', () => {
  assert.equal(validatePayrollPeriodKeyFormat('2026-13').valid, false);
  assert.equal(validatePayrollPeriodKeyFormat('20268').valid, false);
  assert.equal(validatePayrollPeriodKeyFormat('INVALID').valid, false);
});

// ─── UC_HRM_166: validatePayrollAdjustmentInput & formatAdjustmentKind ───

test('validatePayrollAdjustmentInput - valid adjustment returns valid', () => {
  const input: PayrollAdjustmentInput = {
    payrollPeriodId: 'p-1',
    employeeId: 'emp-1',
    kind: 'Bonus',
    title: 'Thưởng KPI xuất sắc',
    amount: 3000000,
  };
  assert.equal(validatePayrollAdjustmentInput(input).valid, true);
});

test('validatePayrollAdjustmentInput - invalid kind returns error', () => {
  const input: PayrollAdjustmentInput = {
    payrollPeriodId: 'p-1',
    employeeId: 'emp-1',
    kind: 'INVALID',
    title: 'Thưởng',
    amount: 3000000,
  };
  const res = validatePayrollAdjustmentInput(input);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Loại điều chỉnh'));
});

test('validatePayrollAdjustmentInput - zero amount returns error', () => {
  const input: PayrollAdjustmentInput = {
    payrollPeriodId: 'p-1',
    employeeId: 'emp-1',
    kind: 'Bonus',
    title: 'Thưởng',
    amount: 0,
  };
  const res = validatePayrollAdjustmentInput(input);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('lớn hơn 0'));
});

test('formatAdjustmentKind - formats kind labels correctly', () => {
  assert.ok(formatAdjustmentKind('Bonus').includes('Thưởng'));
  assert.ok(formatAdjustmentKind('Advance').includes('Tạm ứng'));
});
