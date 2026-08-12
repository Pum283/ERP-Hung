import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateHrmSyncEligibility,
  evaluateAssignmentScore,
  calculateCourseRevenue,
  checkAccountSharingViolation,
  AccountDevice,
} from './step163-helpers.js';

test('validateHrmSyncEligibility - active certificate with date is eligible', () => {
  const res = validateHrmSyncEligibility('Active', '2026-08-12');
  assert.equal(res.isEligible, true);
});

test('validateHrmSyncEligibility - revoked certificate is not eligible', () => {
  const res = validateHrmSyncEligibility('Revoked', '2026-08-12');
  assert.equal(res.isEligible, false);
  assert.equal(res.reason, 'Chứng chỉ đã bị thu hồi, không thể đồng bộ.');
});

test('validateHrmSyncEligibility - missing issuedAt is not eligible', () => {
  const res = validateHrmSyncEligibility('Active');
  assert.equal(res.isEligible, false);
});

test('evaluateAssignmentScore - score >= 90 receives distinction', () => {
  const res = evaluateAssignmentScore(95);
  assert.equal(res.isPass, true);
  assert.equal(res.grade, 'Xuất sắc');
  assert.equal(res.badgeColor, 'success');
});

test('evaluateAssignmentScore - score < 50 fails', () => {
  const res = evaluateAssignmentScore(40);
  assert.equal(res.isPass, false);
  assert.equal(res.grade, 'Cần sửa');
  assert.equal(res.badgeColor, 'danger');
});

test('calculateCourseRevenue - calculates gross revenue correctly', () => {
  const res = calculateCourseRevenue(2000000, 5);
  assert.equal(res.grossRevenue, 10000000);
  assert.equal(res.formattedVnd.includes('10'), true);
});

test('calculateCourseRevenue - zero enrollments yields zero revenue', () => {
  const res = calculateCourseRevenue(1500000, 0);
  assert.equal(res.grossRevenue, 0);
});

test('checkAccountSharingViolation - known device on normal IP is valid', () => {
  const devices: AccountDevice[] = [{ deviceId: 'DEV-1', ipAddress: '14.225.1.1' }];
  const res = checkAccountSharingViolation('DEV-1', '14.225.1.1', devices);
  assert.equal(res.isViolation, false);
});

test('checkAccountSharingViolation - suspicious IP triggers violation', () => {
  const devices: AccountDevice[] = [{ deviceId: 'DEV-1', ipAddress: '14.225.1.1' }];
  const res = checkAccountSharingViolation('DEV-1', '192.168.99.10', devices);
  assert.equal(res.isViolation, true);
  assert.equal(res.shouldForceLogout, true);
});

test('checkAccountSharingViolation - multiple active devices triggers violation', () => {
  const devices: AccountDevice[] = [
    { deviceId: 'DEV-1', ipAddress: '14.225.1.1' },
    { deviceId: 'DEV-2', ipAddress: '14.225.1.2' },
  ];
  const res = checkAccountSharingViolation('DEV-3', '14.225.1.3', devices);
  assert.equal(res.isViolation, true);
});
