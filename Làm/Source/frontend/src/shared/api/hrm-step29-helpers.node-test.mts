import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateDeviceSyncRequest,
  validateGeoFenceLocation,
  validateLateGraceRules,
  validateLateDeductionScale,
} from './hrm-step29-helpers.ts';

// ─── UC_HRM_101: validateDeviceSyncRequest ───

test('validateDeviceSyncRequest - valid items returns valid', () => {
  const res = validateDeviceSyncRequest([
    { employeeCode: 'EMP_01', punchedAt: '2026-08-11T08:00:00Z', punchType: 'In', deviceCode: 'DEV_1' },
    { employeeCode: 'EMP_02', punchedAt: '2026-08-11T17:00:00Z', punchType: 'Out', deviceCode: 'DEV_1' },
  ]);
  assert.equal(res.valid, true);
});

test('validateDeviceSyncRequest - empty array returns error', () => {
  const res = validateDeviceSyncRequest([]);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không được rỗng'));
});

test('validateDeviceSyncRequest - missing employeeCode returns error', () => {
  const res = validateDeviceSyncRequest([
    { employeeCode: '  ', punchedAt: '2026-08-11T08:00:00Z', punchType: 'In' },
  ]);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('thiếu mã nhân viên'));
});

test('validateDeviceSyncRequest - invalid punchType returns error', () => {
  const res = validateDeviceSyncRequest([
    { employeeCode: 'EMP_01', punchedAt: '2026-08-11T08:00:00Z', punchType: 'UNKNOWN' },
  ]);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không hợp lệ'));
});

// ─── UC_HRM_102: validateGeoFenceLocation ───

test('validateGeoFenceLocation - valid location returns valid', () => {
  const res = validateGeoFenceLocation({
    name: 'Chi Nhánh Tây Ninh',
    latitude: 11.31,
    longitude: 106.09,
    radiusMeters: 250,
  });
  assert.equal(res.valid, true);
});

test('validateGeoFenceLocation - empty name returns error', () => {
  const res = validateGeoFenceLocation({
    name: '  ',
    latitude: 11.31,
    longitude: 106.09,
    radiusMeters: 250,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('1 đến 100'));
});

test('validateGeoFenceLocation - radius under 10m returns error', () => {
  const res = validateGeoFenceLocation({
    name: 'Chi Nhánh',
    latitude: 11.31,
    longitude: 106.09,
    radiusMeters: 5,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('10 đến 50,000'));
});

// ─── UC_HRM_103: validateLateGraceRules ───

test('validateLateGraceRules - valid grace period returns valid', () => {
  const res = validateLateGraceRules(15);
  assert.equal(res.valid, true);
});

test('validateLateGraceRules - grace over 240m returns error', () => {
  const res = validateLateGraceRules(300);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 240'));
});

// ─── UC_HRM_104: validateLateDeductionScale ───

test('validateLateDeductionScale - valid scale returns valid', () => {
  const res = validateLateDeductionScale({
    lateDeductEveryMinutes: 30,
    lateDeductWorkUnit: 0.25,
  });
  assert.equal(res.valid, true);
});

test('validateLateDeductionScale - zero deductEveryMinutes returns error', () => {
  const res = validateLateDeductionScale({
    lateDeductEveryMinutes: 0,
    lateDeductWorkUnit: 0.25,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('1 đến 480'));
});

test('validateLateDeductionScale - deductWorkUnit over 1.0 returns error', () => {
  const res = validateLateDeductionScale({
    lateDeductEveryMinutes: 30,
    lateDeductWorkUnit: 1.5,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 1.0'));
});
