import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateTransferForm,
  calculateProbationStatusBadge,
} from './hrm-transfer-probation-helpers.ts';

// ─── UC_HRM_034: validateTransferForm ───

test('validateTransferForm - valid transfer returns true', () => {
  const res = validateTransferForm('ORG_1', 'DEPT_1', 'JT_1', 'JL_1', {
    orgUnitId: 'ORG_2',
    departmentId: 'DEPT_2',
    effectiveDate: '2026-08-15',
    reason: 'Điều chuyển công tác',
  });
  assert.equal(res.valid, true);
});

test('validateTransferForm - same org/dept/title returns error', () => {
  const res = validateTransferForm('ORG_1', 'DEPT_1', 'JT_1', 'JL_1', {
    orgUnitId: 'ORG_1',
    departmentId: 'DEPT_1',
    jobTitleId: 'JT_1',
    jobLevelId: 'JL_1',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('phải khác'));
});

test('validateTransferForm - invalid date format returns error', () => {
  const res = validateTransferForm('ORG_1', 'DEPT_1', 'JT_1', 'JL_1', {
    orgUnitId: 'ORG_2',
    effectiveDate: 'INVALID_DATE',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('định dạng'));
});

// ─── UC_HRM_036: calculateProbationStatusBadge ───

test('calculateProbationStatusBadge - <= 3 days returns critical badge', () => {
  const badge = calculateProbationStatusBadge(2);
  assert.equal(badge.severity, 'critical');
  assert.ok(badge.text.includes('Rất gấp'));
});

test('calculateProbationStatusBadge - <= 7 days returns warning badge', () => {
  const badge = calculateProbationStatusBadge(5);
  assert.equal(badge.severity, 'warning');
  assert.ok(badge.text.includes('Cần xử lý'));
});

test('calculateProbationStatusBadge - > 7 days returns info badge', () => {
  const badge = calculateProbationStatusBadge(12);
  assert.equal(badge.severity, 'info');
});
