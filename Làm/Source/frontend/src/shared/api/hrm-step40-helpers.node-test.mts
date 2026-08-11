import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateSalaryGradeInput,
  validateEmployeeSalaryInput,
  validateAllowanceTypeInput,
  filterSalaryGrades,
  type SalaryGradeItem,
} from './hrm-step40-helpers.ts';

// ─── UC_HRM_152: validateSalaryGradeInput & filterSalaryGrades ───

test('validateSalaryGradeInput - valid grade returns valid', () => {
  const res = validateSalaryGradeInput({
    code: 'GR_01',
    name: 'Bậc 1 Senior',
    level: 1,
    baseAmount: 15000000,
  });
  assert.equal(res.valid, true);
});

test('validateSalaryGradeInput - negative base amount returns error', () => {
  const res = validateSalaryGradeInput({
    code: 'GR_01',
    name: 'Bậc 1 Senior',
    level: 1,
    baseAmount: -1000,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không được âm'));
});

test('filterSalaryGrades - sorts by level and filters active only', () => {
  const items: SalaryGradeItem[] = [
    { id: '1', code: 'GR_3', name: 'Bậc 3', level: 3, baseAmount: 20000000, isActive: true },
    { id: '2', code: 'GR_1', name: 'Bậc 1', level: 1, baseAmount: 10000000, isActive: true },
    { id: '3', code: 'GR_2', name: 'Bậc 2 (Inactive)', level: 2, baseAmount: 15000000, isActive: false },
  ];

  const filtered = filterSalaryGrades(items, true);
  assert.equal(filtered.length, 2);
  assert.equal(filtered[0].code, 'GR_1');
  assert.equal(filtered[1].code, 'GR_3');
});

// ─── UC_HRM_153: validateEmployeeSalaryInput ───

test('validateEmployeeSalaryInput - valid input returns valid', () => {
  const res = validateEmployeeSalaryInput({
    employeeId: 'emp-1',
    baseSalary: 12000000,
    effectiveFrom: '2026-08-01',
  });
  assert.equal(res.valid, true);
});

test('validateEmployeeSalaryInput - empty employeeId returns error', () => {
  const res = validateEmployeeSalaryInput({
    employeeId: '',
    baseSalary: 12000000,
    effectiveFrom: '2026-08-01',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('chọn nhân viên'));
});

// ─── UC_HRM_154: validateAllowanceTypeInput ───

test('validateAllowanceTypeInput - valid input returns valid', () => {
  const res = validateAllowanceTypeInput({
    code: 'ALLOW_PHONE',
    name: 'Phụ cấp điện thoại',
    defaultAmount: 500000,
    isTaxable: true,
  });
  assert.equal(res.valid, true);
});

test('validateAllowanceTypeInput - negative default amount returns error', () => {
  const res = validateAllowanceTypeInput({
    code: 'ALLOW_PHONE',
    name: 'Phụ cấp điện thoại',
    defaultAmount: -500,
    isTaxable: true,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không được âm'));
});
