import test from 'node:test';
import assert from 'node:assert/strict';
import {
  calculateLateMinutes,
  calculatePenaltyDeduction,
  filterCompanyWideBoard,
  formatMissingAlertMessage,
  type CompanyBoardItem,
} from './hrm-step32-helpers.ts';

// ─── UC_HRM_115: calculateLateMinutes ───

test('calculateLateMinutes - on time returns 0', () => {
  const late = calculateLateMinutes('2026-08-11T08:00:00Z', '2026-08-11T07:55:00Z');
  assert.equal(late, 0);
});

test('calculateLateMinutes - late 45 minutes returns 45', () => {
  const late = calculateLateMinutes('2026-08-11T08:00:00Z', '2026-08-11T08:45:00Z');
  assert.equal(late, 45);
});

// ─── UC_HRM_116: calculatePenaltyDeduction ───

test('calculatePenaltyDeduction - late within grace returns zero deduction', () => {
  const res = calculatePenaltyDeduction(10, 15, 30, 0.25);
  assert.equal(res.deductedWorkUnit, 0);
  assert.equal(res.workUnit, 1.0);
});

test('calculatePenaltyDeduction - late 45m (grace 15m, step 30m, unit 0.25) deducts 0.25', () => {
  const res = calculatePenaltyDeduction(45, 15, 30, 0.25);
  assert.equal(res.deductedWorkUnit, 0.25);
  assert.equal(res.workUnit, 0.75);
});

test('calculatePenaltyDeduction - late 105m deducts 0.75', () => {
  const res = calculatePenaltyDeduction(105, 15, 30, 0.25);
  assert.equal(res.deductedWorkUnit, 0.75);
  assert.equal(res.workUnit, 0.25);
});

test('calculatePenaltyDeduction - extremely late capped at 1.0 deduction', () => {
  const res = calculatePenaltyDeduction(300, 15, 30, 0.5);
  assert.equal(res.deductedWorkUnit, 1.0);
  assert.equal(res.workUnit, 0.0);
});

// ─── UC_HRM_113: filterCompanyWideBoard ───

test('filterCompanyWideBoard - filters by keyword and orgUnit', () => {
  const records: CompanyBoardItem[] = [
    { id: '1', employeeCode: 'EMP_01', employeeName: 'Lê Văn A', orgUnitId: 'ou-1', orgUnitName: 'Vận Hành', workDate: '2026-08-11', status: 'Open' },
    { id: '2', employeeCode: 'EMP_02', employeeName: 'Phạm Thị B', orgUnitId: 'ou-2', orgUnitName: 'Kinh Doanh', workDate: '2026-08-11', status: 'Closed' },
  ];

  const res1 = filterCompanyWideBoard(records, 'Lê Văn');
  assert.equal(res1.length, 1);
  assert.equal(res1[0].id, '1');

  const res2 = filterCompanyWideBoard(records, undefined, 'ou-2');
  assert.equal(res2.length, 1);
  assert.equal(res2[0].id, '2');
});

// ─── UC_HRM_114: formatMissingAlertMessage ───

test('formatMissingAlertMessage - formats MissingCheckIn & MissingCheckout correctly', () => {
  const msg1 = formatMissingAlertMessage('MissingCheckIn', 'Nguyễn Văn C', '2026-08-11');
  assert.ok(msg1.includes('chưa check-in'));

  const msg2 = formatMissingAlertMessage('MissingCheckout', 'Phạm Văn D', '2026-08-11');
  assert.ok(msg2.includes('chưa check-out'));
});
