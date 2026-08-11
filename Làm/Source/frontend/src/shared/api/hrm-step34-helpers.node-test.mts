import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validatePeriodLockKey,
  validateAdjustDecision,
  filterLateViolations,
  formatPeriodLockStatus,
  type AttendanceRecordItem,
} from './hrm-step34-helpers.ts';

// ─── UC_HRM_126: validatePeriodLockKey ───

test('validatePeriodLockKey - valid format YYYY-MM returns valid', () => {
  const res = validatePeriodLockKey('2026-08');
  assert.equal(res.valid, true);
});

test('validatePeriodLockKey - invalid format returns error', () => {
  const res = validatePeriodLockKey('08-2026');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('yyyy-MM'));
});

test('validatePeriodLockKey - invalid month returns error', () => {
  const res = validatePeriodLockKey('2026-15');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('yyyy-MM'));
});

// ─── UC_HRM_122: validateAdjustDecision ───

test('validateAdjustDecision - valid decision returns valid', () => {
  const res = validateAdjustDecision('req-123', true);
  assert.equal(res.valid, true);
});

test('validateAdjustDecision - empty requestId returns error', () => {
  const res = validateAdjustDecision('', true);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không hợp lệ'));
});

// ─── UC_HRM_123: filterLateViolations ───

test('filterLateViolations - filters records with late minutes', () => {
  const records: AttendanceRecordItem[] = [
    { id: '1', employeeCode: 'EMP_01', employeeName: 'Lê Văn A', lateMinutes: 0, deductedWorkUnit: 0 },
    { id: '2', employeeCode: 'EMP_02', employeeName: 'Phạm Thị B', lateMinutes: 45, deductedWorkUnit: 0.25 },
  ];

  const violations = filterLateViolations(records);
  assert.equal(violations.length, 1);
  assert.equal(violations[0].employeeCode, 'EMP_02');
});

// ─── UC_HRM_127: formatPeriodLockStatus ───

test('formatPeriodLockStatus - returns correct lock status label', () => {
  const lockedStr = formatPeriodLockStatus(true, 'Nguyễn Quản Lý', '2026-08-01T10:00:00Z');
  assert.ok(lockedStr.includes('Đã khóa bởi Nguyễn Quản Lý'));

  const unlockedStr = formatPeriodLockStatus(false);
  assert.ok(unlockedStr.includes('Đang mở'));
});
