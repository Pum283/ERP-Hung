import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateLeaveEntitlementRule,
  validateLeaveBalanceAdjust,
  validateLeaveCreateRequest,
  calculateRemainingLeave,
} from './hrm-step35-helpers.ts';

// ─── UC_HRM_130: validateLeaveEntitlementRule ───

test('validateLeaveEntitlementRule - valid rule returns valid', () => {
  const res = validateLeaveEntitlementRule({ leaveTypeId: 'lt-1', daysPerYear: 12 });
  assert.equal(res.valid, true);
});

test('validateLeaveEntitlementRule - days over 366 returns error', () => {
  const res = validateLeaveEntitlementRule({ leaveTypeId: 'lt-1', daysPerYear: 400 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('0 đến 366'));
});

// ─── UC_HRM_131: validateLeaveBalanceAdjust & calculateRemainingLeave ───

test('validateLeaveBalanceAdjust - valid balance returns valid', () => {
  const res = validateLeaveBalanceAdjust({ employeeId: 'emp-1', leaveTypeId: 'lt-1', year: 2026, entitled: 15 });
  assert.equal(res.valid, true);
});

test('calculateRemainingLeave - calculates entitled minus used correctly', () => {
  assert.equal(calculateRemainingLeave(12, 3), 9);
  assert.equal(calculateRemainingLeave(12, 15), 0); // Floored at 0
});

// ─── UC_HRM_133: validateLeaveCreateRequest ───

test('validateLeaveCreateRequest - valid request returns valid', () => {
  const res = validateLeaveCreateRequest({
    leaveTypeId: 'lt-1',
    fromDate: '2026-08-15',
    toDate: '2026-08-17',
    days: 3,
  });
  assert.equal(res.valid, true);
});

test('validateLeaveCreateRequest - toDate < fromDate returns error', () => {
  const res = validateLeaveCreateRequest({
    leaveTypeId: 'lt-1',
    fromDate: '2026-08-20',
    toDate: '2026-08-15',
    days: 2,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('lớn hơn hoặc bằng'));
});

test('validateLeaveCreateRequest - zero days returns error', () => {
  const res = validateLeaveCreateRequest({
    leaveTypeId: 'lt-1',
    fromDate: '2026-08-15',
    toDate: '2026-08-15',
    days: 0,
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('lớn hơn 0'));
});
