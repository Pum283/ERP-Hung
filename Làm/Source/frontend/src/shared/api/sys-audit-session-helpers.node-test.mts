import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateAuditLogQuery,
  validateAuditLogExportRequest,
  validateSessionPolicy,
  getActionBadgeColor,
  getChangeKindLabel,
  formatSessionDuration,
} from './sys-audit-session-helpers.ts';

// ─── validateAuditLogQuery ───

test('validateAuditLogQuery - valid params returns valid', () => {
  const res = validateAuditLogQuery({ page: 1, pageSize: 50 });
  assert.equal(res.valid, true);
});

test('validateAuditLogQuery - page < 1 returns error', () => {
  const res = validateAuditLogQuery({ page: 0 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('trang'));
});

test('validateAuditLogQuery - pageSize > 500 returns error', () => {
  const res = validateAuditLogQuery({ pageSize: 501 });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('500'));
});

test('validateAuditLogQuery - from > to returns error', () => {
  const res = validateAuditLogQuery({ from: '2026-01-10T00:00:00Z', to: '2026-01-01T00:00:00Z' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('kết thúc'));
});

test('validateAuditLogQuery - empty params are valid', () => {
  const res = validateAuditLogQuery({});
  assert.equal(res.valid, true);
});

// ─── validateAuditLogExportRequest ───

test('validateAuditLogExportRequest - from = to returns error', () => {
  const now = new Date().toISOString();
  const res = validateAuditLogExportRequest({ from: now, to: now });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('nhỏ hơn'));
});

test('validateAuditLogExportRequest - range > 366 days returns error', () => {
  const from = new Date('2025-01-01').toISOString();
  const to = new Date('2026-02-10').toISOString();
  const res = validateAuditLogExportRequest({ from, to });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('366'));
});

test('validateAuditLogExportRequest - valid 30-day range', () => {
  const from = new Date('2026-01-01').toISOString();
  const to = new Date('2026-01-31').toISOString();
  const res = validateAuditLogExportRequest({ from, to });
  assert.equal(res.valid, true);
});

// ─── validateSessionPolicy ───

test('validateSessionPolicy - sessionMinutes = 0 returns error', () => {
  const res = validateSessionPolicy({ sessionMinutes: 0, idleTimeoutMinutes: 0, maxConcurrentSessions: 5, forceLogoutOnPasswordChange: true });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('>= 1'));
});

test('validateSessionPolicy - sessionMinutes > 10080 returns error', () => {
  const res = validateSessionPolicy({ sessionMinutes: 10_081, idleTimeoutMinutes: 0, maxConcurrentSessions: 5, forceLogoutOnPasswordChange: true });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('10.080'));
});

test('validateSessionPolicy - idleTimeout > sessionMinutes returns error', () => {
  const res = validateSessionPolicy({ sessionMinutes: 60, idleTimeoutMinutes: 120, maxConcurrentSessions: 5, forceLogoutOnPasswordChange: true });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Idle timeout'));
});

test('validateSessionPolicy - maxConcurrentSessions = 25 returns error', () => {
  const res = validateSessionPolicy({ sessionMinutes: 120, idleTimeoutMinutes: 0, maxConcurrentSessions: 25, forceLogoutOnPasswordChange: false });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('1 – 20'));
});

test('validateSessionPolicy - valid policy returns valid', () => {
  const res = validateSessionPolicy({ sessionMinutes: 120, idleTimeoutMinutes: 30, maxConcurrentSessions: 5, forceLogoutOnPasswordChange: true });
  assert.equal(res.valid, true);
});

test('validateSessionPolicy - idleTimeout = 0 (disabled) is allowed', () => {
  const res = validateSessionPolicy({ sessionMinutes: 60, idleTimeoutMinutes: 0, maxConcurrentSessions: 3, forceLogoutOnPasswordChange: false });
  assert.equal(res.valid, true);
});

// ─── Display helpers ───

test('getActionBadgeColor - Create returns green', () => {
  assert.equal(getActionBadgeColor('Create'), '#10b981');
});

test('getActionBadgeColor - Delete returns red', () => {
  assert.equal(getActionBadgeColor('Delete'), '#ef4444');
});

test('getActionBadgeColor - Unknown returns grey', () => {
  assert.equal(getActionBadgeColor('SomeUnknown'), '#6b7280');
});

test('getChangeKindLabel - Added returns Vietnamese label', () => {
  const label = getChangeKindLabel('Added');
  assert.ok(label.includes('Thêm'));
});

test('formatSessionDuration - 45 minutes', () => {
  assert.equal(formatSessionDuration(45), '45 phút');
});

test('formatSessionDuration - 120 minutes = 2 giờ', () => {
  assert.equal(formatSessionDuration(120), '2 giờ');
});

test('formatSessionDuration - 90 minutes = 1 giờ 30 phút', () => {
  assert.equal(formatSessionDuration(90), '1 giờ 30 phút');
});
