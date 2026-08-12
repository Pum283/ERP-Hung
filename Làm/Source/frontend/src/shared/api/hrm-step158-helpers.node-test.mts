import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateShiftImportRow,
  normalizePenaltyType,
  calculatePayrollPenaltyTotal,
  generateFinJePreview,
  ShiftImportRow,
  PenaltyItem,
} from './hrm-step158-helpers.js';

test('validateShiftImportRow - valid row passes', () => {
  const row: ShiftImportRow = {
    employeeCode: 'EMP001',
    workShiftCode: 'HC_01',
    workDate: '2026-08-15',
  };
  const res = validateShiftImportRow(row);
  assert.equal(res.isValid, true);
});

test('validateShiftImportRow - missing employee code fails', () => {
  const row: ShiftImportRow = {
    employeeCode: '  ',
    workShiftCode: 'HC_01',
    workDate: '2026-08-15',
  };
  const res = validateShiftImportRow(row);
  assert.equal(res.isValid, false);
  assert.equal(res.error, 'Mã nhân viên không được để trống.');
});

test('validateShiftImportRow - missing work shift code fails', () => {
  const row: ShiftImportRow = {
    employeeCode: 'EMP001',
    workShiftCode: '',
    workDate: '2026-08-15',
  };
  const res = validateShiftImportRow(row);
  assert.equal(res.isValid, false);
  assert.equal(res.error, 'Mã ca làm việc không được để trống.');
});

test('validateShiftImportRow - invalid date format fails', () => {
  const row: ShiftImportRow = {
    employeeCode: 'EMP001',
    workShiftCode: 'HC_01',
    workDate: '15/08/2026',
  };
  const res = validateShiftImportRow(row);
  assert.equal(res.isValid, false);
  assert.equal(res.error, 'Ngày làm việc không hợp lệ (định dạng YYYY-MM-DD).');
});

test('normalizePenaltyType - valid penalty type returns normalized string', () => {
  assert.deepEqual(normalizePenaltyType('latearrival'), { isValid: true, normalized: 'LateArrival' });
  assert.deepEqual(normalizePenaltyType('RegulationBreach'), { isValid: true, normalized: 'RegulationBreach' });
});

test('normalizePenaltyType - invalid penalty type falls back to LateArrival', () => {
  const res = normalizePenaltyType('UnknownType');
  assert.equal(res.isValid, false);
  assert.equal(res.normalized, 'LateArrival');
});

test('calculatePayrollPenaltyTotal - sums active penalties and excludes cancelled', () => {
  const list: PenaltyItem[] = [
    { id: '1', employeeId: 'emp1', reason: 'Late', penaltyType: 'LateArrival', amount: 100000, status: 'Pending' },
    { id: '2', employeeId: 'emp1', reason: 'Safety', penaltyType: 'SafetyViolation', amount: 500000, status: 'Applied' },
    { id: '3', employeeId: 'emp1', reason: 'Cancelled', penaltyType: 'Other', amount: 200000, status: 'Cancelled' },
  ];

  const total = calculatePayrollPenaltyTotal(list);
  assert.equal(total, 600000); // 100k + 500k
});

test('calculatePayrollPenaltyTotal - empty list returns zero', () => {
  assert.equal(calculatePayrollPenaltyTotal([]), 0);
});

test('generateFinJePreview - returns preview object with isBalanced true', () => {
  const preview = generateFinJePreview(20000000, 18000000, 300000);
  assert.equal(preview.isBalanced, true);
  assert.match(preview.debitAccount, /642/);
  assert.match(preview.creditAccountSalary, /334/);
});

test('generateFinJePreview - formatting handles zero values', () => {
  const preview = generateFinJePreview(0, 0, 0);
  assert.equal(preview.isBalanced, true);
});
