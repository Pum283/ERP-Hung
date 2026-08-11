import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateShiftCopy,
  validateShiftLock,
  validateMobilizationOrder,
} from './hrm-step26-helpers.ts';

// ─── UC_HRM_089: validateShiftCopy ───

test('validateShiftCopy - valid source range & target returns valid', () => {
  const res = validateShiftCopy({ sourceFrom: '2026-08-01', sourceTo: '2026-08-07', targetStart: '2026-08-08' });
  assert.equal(res.valid, true);
});

test('validateShiftCopy - empty targetStart returns error', () => {
  const res = validateShiftCopy({ sourceFrom: '2026-08-01', sourceTo: '2026-08-07', targetStart: '   ' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('đích sao chép'));
});

test('validateShiftCopy - sourceTo < sourceFrom returns error', () => {
  const res = validateShiftCopy({ sourceFrom: '2026-08-10', sourceTo: '2026-08-01', targetStart: '2026-08-11' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('lớn hơn hoặc bằng'));
});

test('validateShiftCopy - span over 62 days returns error', () => {
  const res = validateShiftCopy({ sourceFrom: '2026-08-01', sourceTo: '2026-10-15', targetStart: '2026-10-16' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('62 ngày'));
});

// ─── UC_HRM_090: validateShiftLock ───

test('validateShiftLock - valid periodKey yyyy-MM returns valid', () => {
  const res = validateShiftLock({ orgUnitId: 'org-1', periodKey: '2026-08' });
  assert.equal(res.valid, true);
});

test('validateShiftLock - empty orgUnitId returns error', () => {
  const res = validateShiftLock({ orgUnitId: '   ', periodKey: '2026-08' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('đơn vị'));
});

test('validateShiftLock - invalid periodKey format returns error', () => {
  const res = validateShiftLock({ orgUnitId: 'org-1', periodKey: '202608' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('yyyy-MM'));
});

// ─── UC_HRM_092: validateMobilizationOrder ───

test('validateMobilizationOrder - valid order returns valid', () => {
  const res = validateMobilizationOrder({
    employeeId: 'emp-1',
    fromOrgUnitId: 'org-src',
    toOrgUnitId: 'org-dest',
    startDate: '2026-09-01',
    reason: 'Điều động hỗ trợ dự án mới',
  });
  assert.equal(res.valid, true);
});

test('validateMobilizationOrder - empty employeeId returns error', () => {
  const res = validateMobilizationOrder({
    employeeId: '   ',
    fromOrgUnitId: 'org-src',
    toOrgUnitId: 'org-dest',
    startDate: '2026-09-01',
    reason: 'Điều động hỗ trợ',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('nhân viên'));
});

test('validateMobilizationOrder - same from & to org units returns error', () => {
  const res = validateMobilizationOrder({
    employeeId: 'emp-1',
    fromOrgUnitId: 'org-1',
    toOrgUnitId: 'org-1',
    startDate: '2026-09-01',
    reason: 'Điều động hỗ trợ',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('khác đơn vị đi'));
});

test('validateMobilizationOrder - reason too short returns error', () => {
  const res = validateMobilizationOrder({
    employeeId: 'emp-1',
    fromOrgUnitId: 'org-src',
    toOrgUnitId: 'org-dest',
    startDate: '2026-09-01',
    reason: 'AB',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('3 đến 500'));
});
