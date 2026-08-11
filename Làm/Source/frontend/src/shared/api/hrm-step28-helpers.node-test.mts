import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateActualHours,
  validateBiometricDevice,
  validateGeoFence,
  validateFaceRecognitionConfig,
} from './hrm-step28-helpers.ts';

// ─── UC_HRM_097: validateActualHours ───

test('validateActualHours - valid hours returns valid', () => {
  const res = validateActualHours(168.5);
  assert.equal(res.valid, true);
});

test('validateActualHours - negative hours returns error', () => {
  const res = validateActualHours(-5);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 1,000'));
});

test('validateActualHours - excessive hours returns error', () => {
  const res = validateActualHours(1500);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 1,000'));
});

// ─── UC_HRM_098: validateBiometricDevice ───

test('validateBiometricDevice - valid device returns valid', () => {
  const res = validateBiometricDevice({
    code: 'BIO_MAIN',
    name: 'Máy Chấm Vân Tay Cổng Chính',
    deviceType: 'Fingerprint',
  });
  assert.equal(res.valid, true);
});

test('validateBiometricDevice - empty code returns error', () => {
  const res = validateBiometricDevice({ code: '   ', name: 'Máy Vân Tay' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('1 đến 40'));
});

test('validateBiometricDevice - invalid deviceType returns error', () => {
  const res = validateBiometricDevice({ code: 'BIO_1', name: 'Máy Vân Tay', deviceType: 'Bluetooth' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('không hợp lệ'));
});

// ─── UC_HRM_099: validateGeoFence ───

test('validateGeoFence - valid geo fence returns valid', () => {
  const res = validateGeoFence({
    name: 'Văn Phòng Trụ Sở Chính',
    latitude: 10.762622,
    longitude: 106.660172,
    radiusMeters: 150,
  });
  assert.equal(res.valid, true);
});

test('validateGeoFence - radius under 10m returns error', () => {
  const res = validateGeoFence({
    name: 'Điểm Chấm Nhỏ',
    latitude: 10.76,
    longitude: 106.66,
    radiusMeters: 5,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('10 đến 50,000'));
});

test('validateGeoFence - invalid latitude returns error', () => {
  const res = validateGeoFence({
    name: 'Điểm GPS Lỗi',
    latitude: 95,
    longitude: 106.66,
    radiusMeters: 100,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Vĩ độ'));
});

// ─── UC_HRM_100: validateFaceRecognitionConfig ───

test('validateFaceRecognitionConfig - valid policy returns valid', () => {
  const res = validateFaceRecognitionConfig({
    lateGraceMinutes: 15,
    lateDeductEveryMinutes: 30,
    lateDeductWorkUnit: 0.5,
    minConfidenceScore: 0.85,
  });
  assert.equal(res.valid, true);
});

test('validateFaceRecognitionConfig - graceMinutes over 240 returns error', () => {
  const res = validateFaceRecognitionConfig({
    lateGraceMinutes: 300,
    lateDeductEveryMinutes: 30,
    lateDeductWorkUnit: 0.5,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 240'));
});

test('validateFaceRecognitionConfig - confidence score under 0.5 returns error', () => {
  const res = validateFaceRecognitionConfig({
    lateGraceMinutes: 15,
    lateDeductEveryMinutes: 30,
    lateDeductWorkUnit: 0.5,
    minConfidenceScore: 0.3,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0.50 đến 0.99'));
});
