import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateOffboardingCreateRequest,
  validateNoticePeriodConfig,
  calculateNoticeDaysLeft,
  formatNoticeStatus,
} from './hrm-step38-helpers.ts';

// ─── UC_HRM_144: validateOffboardingCreateRequest ───

test('validateOffboardingCreateRequest - valid dates returns valid', () => {
  const res = validateOffboardingCreateRequest({
    employeeId: 'emp-1',
    requestDate: '2026-08-01',
    lastWorkingDay: '2026-09-15',
  });
  assert.equal(res.valid, true);
});

test('validateOffboardingCreateRequest - last working day before request date returns error', () => {
  const res = validateOffboardingCreateRequest({
    employeeId: 'emp-1',
    requestDate: '2026-08-15',
    lastWorkingDay: '2026-08-01',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('lớn hơn hoặc bằng'));
});

// ─── UC_HRM_145: validateNoticePeriodConfig & calculateNoticeDaysLeft ───

test('validateNoticePeriodConfig - valid days returns valid', () => {
  const res = validateNoticePeriodConfig(30);
  assert.equal(res.valid, true);
});

test('validateNoticePeriodConfig - days over 365 returns error', () => {
  const res = validateNoticePeriodConfig(400);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 365'));
});

test('calculateNoticeDaysLeft - calculates correct days difference', () => {
  const days = calculateNoticeDaysLeft('2026-08-01', '2026-08-31');
  assert.equal(days, 30);
});

// ─── UC_HRM_146: formatNoticeStatus ───

test('formatNoticeStatus - formats notice satisfied label correctly', () => {
  assert.ok(formatNoticeStatus(true, 35, 30).includes('Đảm bảo'));
  assert.ok(formatNoticeStatus(false, 15, 30).includes('thiếu'));
});
