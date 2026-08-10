import test from 'node:test';
import assert from 'node:assert/strict';
import {
  isLockedStatus,
  validateStatusTransition,
  buildCsvExportFilename,
} from './hrm-profile-status-helpers.ts';

// ─── UC_HRM_027: isLockedStatus ───

test('isLockedStatus - returns true for Terminated, Resigned, Retired, Locked, Inactive', () => {
  assert.equal(isLockedStatus('Terminated'), true);
  assert.equal(isLockedStatus('Resigned'), true);
  assert.equal(isLockedStatus('Retired'), true);
  assert.equal(isLockedStatus('Locked'), true);
  assert.equal(isLockedStatus('Inactive'), true);
});

test('isLockedStatus - returns false for Active, Probation, New', () => {
  assert.equal(isLockedStatus('Active'), false);
  assert.equal(isLockedStatus('Probation'), false);
  assert.equal(isLockedStatus('New'), false);
});

// ─── UC_HRM_029 & UC_HRM_027: validateStatusTransition ───

test('validateStatusTransition - transition from New to Probation is valid', () => {
  const res = validateStatusTransition('New', 'Probation', '2026-08-10');
  assert.equal(res.valid, true);
  assert.equal(res.isLocking, false);
  assert.equal(res.isRehiring, false);
});

test('validateStatusTransition - transition to Terminated sets isLocking true', () => {
  const res = validateStatusTransition('Active', 'Terminated', '2026-08-10');
  assert.equal(res.valid, true);
  assert.equal(res.isLocking, true);
  assert.equal(res.isRehiring, false);
});

test('validateStatusTransition - transition from Terminated to Probation sets isRehiring true', () => {
  const res = validateStatusTransition('Terminated', 'Probation', '2026-08-10');
  assert.equal(res.valid, true);
  assert.equal(res.isLocking, false);
  assert.equal(res.isRehiring, true);
});

test('validateStatusTransition - same status returns error', () => {
  const res = validateStatusTransition('Active', 'Active');
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Active'));
});

// ─── UC_HRM_026: buildCsvExportFilename ───

test('buildCsvExportFilename - generates formatted filename with date', () => {
  const filename = buildCsvExportFilename();
  const todayStr = new Date().toISOString().split('T')[0];
  assert.equal(filename, `danh-sach-nhan-su_${todayStr}.csv`);
});
